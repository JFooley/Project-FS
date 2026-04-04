using SFML.System;
using SFML.Graphics;
using SFML.Audio;

public class Fireball : Character {
    private static Dictionary<string, Texture> textures_local = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => textures_local; protected set => textures_local = value ?? new Dictionary<string, Texture>();}
    private static Dictionary<string, SoundBuffer> sounds_local = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => sounds_local; protected set => sounds_local = value ?? new Dictionary<string, SoundBuffer>();}

    public Fireball(string initialState, int life_points, float startX, float startY, int team, int facing)
        : base("Fireball", initialState, startX, startY, Data.GetPath("Assets/characters/Ken/Fireball"), 1) {
            this.player_index = team;
            this.facing = facing;
            this.life_points = new Vector2i(life_points, 0);
            this.shadow_size = 1;
            this.own_light = new Color(255, 255, 255, 255);
        }
    public Fireball() : base("Fireball", "", 0, 0, Data.GetPath("Assets/characters/Ken/Fireball"), 1) {}

    public override void Load() {
        // Animations
        var kenFB0 = new GenericBox(0, 139, 115, 163, 143);
        var kenFB1 = new GenericBox(1, 139, 115, 163, 143);
        
        var KenFireballFrames = new List<Frame> { 
            new FrameData("fireball-1 (1)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (2)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (3)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (4)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (5)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (6)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (7)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (8)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (9)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (10)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (11)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (12)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (13)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (14)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (15)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (16)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (17)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (18)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (19)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (20)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData("fireball-1 (21)", 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
        };

        var KenFireballFinal = new List<Frame> {
            new FrameData("fireball-0 (1)", 0, 0, len: 2, boxes: new List<GenericBox> {}),
            new FrameData("fireball-0 (2)", 0, 0, len: 2, boxes: new List<GenericBox> {}),
            new FrameData("fireball-0 (3)", 0, 0, len: 2, boxes: new List<GenericBox> {}),
            new FrameData("fireball-0 (4)", 0, 0, len: 2, boxes: new List<GenericBox> {}),
            new FrameData("fireball-0 (5)", 0, 0, len: 2, boxes: new List<GenericBox> {}),
            new FrameData("fireball-0 (6)", 0, 0, len: 2, boxes: new List<GenericBox> {}),
        };

        // States
        var animations = new Dictionary<string, State> {
            {"Ken1", new State(KenFireballFrames, "Ken1", 7, can_harm: true)},
            {"Ken2", new State(KenFireballFrames, "Ken2", 7, can_harm: true)},
            {"Ken3", new State(KenFireballFrames, "Ken3", 7, glow: true, can_harm: true)},
            {"KenExit", new State(KenFireballFinal, "Remove", 7)},
        };

        this.states = animations;
    }
    public override void Behave() {
        base.Behave();
        if (this.state.post_state == "Remove" && this.current_animation.ended) {
            this.remove = true;
            this.current_animation.Reset();
        }

        if (Math.Abs(Camera.X - this.visual_position.X) > Config.RenderWidth) this.remove = true;  
        
        switch (this.current_state) {
            case "Ken1":
                this.SetVelocity(X: 4);
                if (this.life_points.X <= 0) {
                    this.ChangeState("KenExit");
                    this.SetVelocity(raw_set: true);
                    break;
                }
                break;

            case "Ken2":
                this.SetVelocity(X: 5);
                if (this.life_points.X <= 0) {
                    this.ChangeState("KenExit");
                    this.SetVelocity(raw_set: true);
                    break;
                }
                break;
            
            case "Ken3":
                this.SetVelocity(X: 6);
                if (this.life_points.X <= 0) {
                    this.ChangeState("KenExit");
                    this.SetVelocity(raw_set: true);
                    break;
                }
                break;

            case "Remove":
                this.remove = true;
                break;
                
            default:
                break;
        }
    }
    public override int ImposeBehavior(Character target) {
        int hit = Character.NOTHING;

        if (target.name == "Fireball") {
            target.life_points.X -= 1;
            return Character.HIT;
        };

        switch (this.current_state) {
            case "Ken1":
                this.SetVelocity(raw_set: true);

                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 0);
                    target.BlockStun(this, 20, raw_value: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 63, 48);
                    target.Stun(this, 30, raw_value: true);
                }

                if (this.player_index == 0) Character.AddAuraPoints(target: target, self: stage.character_A, hit);
                else Character.AddAuraPoints(target: target, self: stage.character_B, hit);
                break;

            case "Ken2":
                this.SetVelocity(raw_set: true);

                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 0);
                    target.BlockStun(this, 20, raw_value: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 125, 78);
                    target.Stun(this, 30, raw_value: true);
                }

                if (this.player_index == 0) Character.AddAuraPoints(target: target, self: stage.character_A, hit);
                else Character.AddAuraPoints(target: target, self: stage.character_B, hit);
                break;
            
            case "Ken3":
                this.SetVelocity(raw_set: true);

                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 0);
                    target.BlockStun(this, 20, raw_value: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 80, 60);
                    target.Stun(this, 30, raw_value: true);
                }

                if (this.player_index == 0) Character.AddAuraPoints(target: target, self: stage.character_A, hit);
                else Character.AddAuraPoints(target: target, self: stage.character_B, hit);
                break;

            default:
                this.remove = true;
                break;
        }
        return hit;
    }
    public override int DefineColisionType(Character target) {
        this.life_points.X -= 1;
        return base.DefineColisionType(target);
    }


}