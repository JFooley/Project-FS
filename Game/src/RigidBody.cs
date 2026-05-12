using SFML.System;

public class RigidBody {
    public Vector2f last_position;
    public Vector2f position;
    public Vector2f velocity;
    public Vector2f acceleration;
    private float gravity = Config.gravity;
    private float friction => gravity * 0.5F;

    public RigidBody(float X = 0, float Y = 0) {
        this.position = new Vector2f(X, Y);
    }

    public void Update(Character player) {
        this.last_position = position;

        this.velocity.X += this.acceleration.X;
        this.velocity.Y += this.acceleration.Y;

        this.position.X += this.velocity.X;
        this.position.Y += this.velocity.Y;

        this.CheckGravity(player);
        this.CheckFriction(player);

        this.acceleration.X = 0;
        this.acceleration.Y = 0;
    }

    private void CheckGravity(Character player) {
        if (this.position.Y < player.floor_line && player.state.has_gravity) {
            this.velocity.Y += this.gravity;
        } else {
            this.velocity.Y = 0;
            this.position.Y = player.floor_line;
        }
    }

    private void CheckFriction(Character player) {
        if (this.velocity.X != 0 && this.position.Y == player.floor_line) {
            this.velocity.X = Math.Abs(this.velocity.X) > this.friction ? this.velocity.X + (this.friction * -Math.Sign(this.velocity.X)) : 0;
        } 
    }

    public void SetVelocity(Character player, float X = 0, float Y = 0, bool raw_set = false, bool keep_X = false, bool keep_Y = false) {
        if (raw_set) {
            this.velocity.X = keep_X ? this.velocity.X : X * player.facing;
            this.velocity.Y = keep_Y ? this.velocity.Y : -Y;
        } else {
            this.velocity.X = keep_X ? this.velocity.X : X * player.facing;
            this.velocity.Y = keep_Y ? this.velocity.Y : -this.CalcularForcaY(Y);
        }
    }

    public void AddVelocity(Character player, float X = 0, float Y = 0, bool raw_set = false) {
        if (raw_set) {
            this.velocity.X += X * player.facing;
            this.velocity.Y += -Y;
        } else {
            this.velocity.X += X * player.facing;
            this.velocity.Y += -this.CalcularForcaY(Y);
        }
    }

    public void SetForce(Character player, float X = 0, float Y = 0, int T = 0, bool keep_X = false, bool keep_Y = false) {
        this.acceleration.X = keep_X ? this.acceleration.X : X * player.facing;
        this.acceleration.Y = keep_Y ? this.acceleration.Y : -Y;
    }

    public void AddForce(Character player, float X = 0, float Y = 0, int T = 0) {
        this.acceleration.X += X * player.facing;
        this.acceleration.Y += -Y;
    }

    private float CalcularForcaY(float deltaY) {
        if (deltaY <= 0) return 0;

        float forcaY = (float)Math.Sqrt(2 * this.gravity * deltaY);

        return forcaY;
    }

}
