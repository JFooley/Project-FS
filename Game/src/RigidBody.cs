using SFML.System;

public class RigidBody {
    public Vector2f last_position;
    public Vector2f position;
    public Vector2f velocity;
    public Vector2f acceleration;
    public float gravity = Config.gravity;
    private float friction => gravity * 0.5F;

    private Character player;

    public RigidBody(float X = 0, float Y = 0) {
        this.position = new Vector2f(X, Y);
    }

    public void Update(Character player) {
        if (this.player == null) this.player = player;
        this.last_position = position;

        this.CheckGravity();

        this.velocity.X += this.acceleration.X;
        this.velocity.Y += this.acceleration.Y;

        this.position.X += this.velocity.X;
        this.position.Y -= this.velocity.Y;

        this.CheckFloor();
        this.CheckFriction();

        this.acceleration.X = 0;
        this.acceleration.Y = 0;
    }

    private void CheckGravity() {
        if (this.player.state.has_gravity) {
            this.acceleration.Y -= this.gravity;
        } 
    }
    private void CheckFriction() {
        if (this.velocity.X != 0 && this.velocity.Y <= 0 && this.position.Y == this.player.floor_line) {
            this.velocity.X = Math.Abs(this.velocity.X) > this.friction ? this.velocity.X + (this.friction * -Math.Sign(this.velocity.X)) : 0;
        } 
    }
    private void CheckFloor() {
        this.position.Y = Math.Min(this.player.floor_line, this.position.Y);

        if (this.position.Y == this.player.floor_line) {
            this.velocity.Y = 0;
        }
    }

    public void SetVelocity(float X = 0, float Y = 0, bool raw_set = false, bool fixed_height = false, bool keep_X = false, bool keep_Y = false) {
        if (raw_set) {
            this.velocity.X = keep_X ? this.velocity.X : X * this.player.facing;
            this.velocity.Y = keep_Y ? this.velocity.Y : Y;
        } else {
            this.velocity.X = keep_X ? this.velocity.X : X * this.player.facing;
            this.velocity.Y = keep_Y ? this.velocity.Y : this.CalcularForcaY(Y, fixed_height: fixed_height);
        }
    }
    public void AddVelocity(float X = 0, float Y = 0, bool raw_set = false, bool fixed_height = false) {
        if (raw_set) {
            this.velocity.X += X * this.player.facing;
            this.velocity.Y += Y;
        } else {
            this.velocity.X += X * this.player.facing;
            this.velocity.Y += this.CalcularForcaY(Y, fixed_height: fixed_height);
        }
    }
    
    public void SetAcceleration(float X = 0, float Y = 0, bool keep_X = false, bool keep_Y = false, bool raw_set = false, bool fixed_height = false) {
        if (raw_set) {
            this.acceleration.X = keep_X ? this.acceleration.X : X * this.player.facing;
            this.acceleration.Y = keep_Y ? this.acceleration.Y : Y;
        } else {
            this.acceleration.X = keep_X ? this.acceleration.X : X * this.player.facing;
            this.acceleration.Y = keep_Y ? this.acceleration.Y : this.CalcularForcaY(Y, fixed_height: fixed_height);
        }
    }
    public void AddAcceleration(float X = 0, float Y = 0) {
        this.acceleration.X += X * this.player.facing;
        this.acceleration.Y += Y;
    }

    private float CalcularForcaY(float deltaY, bool fixed_height = false) {
        if (fixed_height) deltaY = deltaY - (this.player.floor_line - this.position.Y);

        if (deltaY <= 0) return 0;

        float forcaY = (float)Math.Sqrt(2 * this.gravity * deltaY);

        return forcaY;
    }

}
