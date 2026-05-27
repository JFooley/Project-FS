using SFML.System;
using SFML.Graphics;
using SFML.Audio;

public class KenEffects : Character {
    private static Dictionary<string, Texture> textures_local = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => textures_local; protected set => textures_local = value ?? new Dictionary<string, Texture>();}
    
    private static Dictionary<string, SoundBuffer> sounds_local = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => sounds_local; protected set => sounds_local = value ?? new Dictionary<string, SoundBuffer>();}
    
    private static Dictionary<string, Frame[]> animations_local = new Dictionary<string, Frame[]>();
    public override Dictionary<string, Frame[]> animations { get => animations_local; protected set => animations_local = value ?? new Dictionary<string, Frame[]>();}

    private Ken character;

    public KenEffects(Ken character, string initialState, int life_points, float startX, float startY, int facing)
        : base("Ken-Effects", initialState, startX, startY, Data.GetPath("assets/characters/Ken-Effects"), 1) {
            this.character = character;
            this.player_index = character.player_index;
            this.facing = facing;
            this.life_points = new Vector2i(life_points, life_points);
            this.shadow_size = 1;
            this.own_light = new Color(255, 255, 255, 255);
    }
    public KenEffects()
        : base("Ken-Effects", "", 0, 0, Data.GetPath("assets/characters/Ken-Effects"), 1) {
            this.shadow_size = 1;
            this.own_light = new Color(255, 255, 255, 255);
    }
    public override void Load() {
        var a = this.animations;
    
        this.states = new Dictionary<string, State> {
            { "Ken1", new State(F(a["KenFireball"]), "Ken1", 7, will_hit: true, has_gravity: false)},
            { "Ken2", new State(F(a["KenFireball"]), "Ken2", 7, will_hit: true, has_gravity: false)},
            { "Ken3", new State(F(a["KenFireball"]), "Ken3", 7, glow: true, will_hit: true, has_gravity: false)},
            { "KenExit", new State(F(a["KenFireballFinal"]), "Remove", 7, has_gravity: false)},
            { "Fire", new State(F(a["fire"]), "Remove", 7, has_gravity: false)}
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
            
            case "Fire":
                this.body.position = character.body.position;
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

        if (this.next_state != null) return hit;

        this.character.has_hit = true;

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

                Character.AddAuraPoints(target: target, self: this.character, hit);
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

                Character.AddAuraPoints(target: target, self: this.character, hit);
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

                Character.AddAuraPoints(target: target, self: this.character, hit);
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