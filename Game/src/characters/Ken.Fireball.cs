using SFML.System;
using SFML.Graphics;
using SFML.Audio;

public class Fireball : Character {
    private static Dictionary<string, Texture> textures_local = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => textures_local; protected set => textures_local = value ?? new Dictionary<string, Texture>();}
    
    private static Dictionary<string, SoundBuffer> sounds_local = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => sounds_local; protected set => sounds_local = value ?? new Dictionary<string, SoundBuffer>();}
    
    private static Dictionary<string, List<Frame>> animations_local = new Dictionary<string, List<Frame>>();
    public override Dictionary<string, List<Frame>> animations { get => animations_local; protected set => animations_local = value ?? new Dictionary<string, List<Frame>>();}

    public Fireball(string initialState, int life_points, float startX, float startY, int team, int facing)
        : base("Ken-Fireball", initialState, startX, startY, Data.GetPath("Assets/characters/Ken-Fireball"), 1) {
            this.player_index = team;
            this.facing = facing;
            this.life_points = new Vector2i(life_points, 0);
            this.shadow_size = 1;
            this.own_light = new Color(255, 255, 255, 255);
    }
    public Fireball()
        : base("Ken-Fireball", "", 0, 0, Data.GetPath("Assets/characters/Ken-Fireball"), 1) {
            this.shadow_size = 1;
            this.own_light = new Color(255, 255, 255, 255);
    }
    public override void Load() {
        var a = this.animations;
    
        this.states = new Dictionary<string, State> {
            {"Ken1", new State(a["KenFireballFrames"], "Ken1", 7, can_harm: true)},
            {"Ken2", new State(a["KenFireballFrames"], "Ken2", 7, can_harm: true)},
            {"Ken3", new State(a["KenFireballFrames"], "Ken3", 7, glow: true, can_harm: true)},
            {"KenExit", new State(a["KenFireballFinal"], "Remove", 7)},
        };
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
                Character.Push(target: target, self: this, Config.light_pushback);
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
                Character.Push(target: target, self: this, Config.light_pushback);
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
                Character.Push(target: target, self: this, Config.light_pushback);
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