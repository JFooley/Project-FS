using Character_Space;
using Animation_Space;
using SFML.System;
using Stage_Space;
using SFML.Graphics;
using SFML.Audio;
using System.Windows.Forms;

public class Fireball : Character {
    private static Dictionary<string, Texture> textures_local = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => textures_local; protected set => textures_local = value ?? new Dictionary<string, Texture>();}
    private static Dictionary<string, SoundBuffer> sounds_local = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => sounds_local; protected set => sounds_local = value ?? new Dictionary<string, SoundBuffer>();}

    public Fireball(string initialState, int life_points, float startX, float startY, int team, int facing)
        : base("Fireball", initialState, startX, startY, "Assets/particles/Fireball", 1) {
            this.player_index = team;
            this.facing = facing;
            this.life_points = new Vector2i(life_points, 0);
            this.shadow_size = 0;
            this.own_light = new Color(255, 255, 255, 255);
        }
    public Fireball() : base("Fireball", "", 0, 0, "Assets/particles/Fireball", 1) {}

    public override void Load() {
        // Animations
        var kenFB0 = new GenericBox(0, 139, 115, 163, 143);
        var kenFB1 = new GenericBox(1, 139, 115, 163, 143);
        
        var KenFireballFrames = new List<FrameData> { 
            new FrameData(21, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(22, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(23, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(24, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(25, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(26, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(27, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(28, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(29, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(210, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(211, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(212, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(213, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(214, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(215, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(216, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(217, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(218, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(219, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(220, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
            new FrameData(221, 0, 0, len: 2, boxes: new List<GenericBox> {kenFB1, kenFB0}, hasHit: false),
        };

        var KenFireballFinal = new List<FrameData> {
            new FrameData(12, 0, 0, len: 2, boxes: new List<GenericBox> {}),
            new FrameData(13, 0, 0, len: 2, boxes: new List<GenericBox> {}),
            new FrameData(14, 0, 0, len: 2, boxes: new List<GenericBox> {}),
            new FrameData(15, 0, 0, len: 2, boxes: new List<GenericBox> {}),
            new FrameData(16, 0, 0, len: 2, boxes: new List<GenericBox> {}),
            new FrameData(17, 0, 0, len: 2, boxes: new List<GenericBox> {}),
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
    public override void DoBehave() {
        base.DoBehave();
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
                    target.BlockStun(this, 20, force: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 63, 48);
                    target.Stun(this, 30, force: true);
                }

                if (this.player_index == 0) Character.AddAuraPoints(target: target, self: stage.character_A, hit);
                else Character.AddAuraPoints(target: target, self: stage.character_B, hit);
                break;

            case "Ken2":
                this.SetVelocity(raw_set: true);

                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 0);
                    target.BlockStun(this, 20, force: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 125, 78);
                    target.Stun(this, 30, force: true);
                }

                if (this.player_index == 0) Character.AddAuraPoints(target: target, self: stage.character_A, hit);
                else Character.AddAuraPoints(target: target, self: stage.character_B, hit);
                break;
            
            case "Ken3":
                this.SetVelocity(raw_set: true);

                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 0);
                    target.BlockStun(this, 20, force: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 80, 60);
                    target.Stun(this, 30, force: true);
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