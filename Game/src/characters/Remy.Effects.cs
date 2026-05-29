using SFML.System;
using SFML.Graphics;
using SFML.Audio;

public class RemyEffects : Character {
    private static Dictionary<string, Texture> textures_local = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => textures_local; protected set => textures_local = value ?? new Dictionary<string, Texture>();}
    
    private static Dictionary<string, SoundBuffer> sounds_local = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => sounds_local; protected set => sounds_local = value ?? new Dictionary<string, SoundBuffer>();}
    
    private static Dictionary<string, Frame[]> animations_local = new Dictionary<string, Frame[]>();
    public override Dictionary<string, Frame[]> animations { get => animations_local; protected set => animations_local = value ?? new Dictionary<string, Frame[]>();}

    private Remy character;

    public RemyEffects(Remy character, string initialState, float startX, float startY, int facing)
        : base("Remy-Effects", initialState, startX, startY, Data.GetPath("assets/characters/Remy-Effects"), 1) {
            this.character = character;
            this.player_index = character.player_index;
            this.facing = facing;
            this.life_points = new Vector2i(1, 1);
            this.shadow_size = 1;
            this.own_light = new Color(255, 255, 255, 255);
    }
    public RemyEffects()
        : base("Remy-Effects", "", 0, 0, Data.GetPath("assets/characters/Remy-Effects"), 1) {
            this.shadow_size = 1;
            this.own_light = new Color(255, 255, 255, 255);
    }
    public override void Load() {
        var a = this.animations;
    
        this.states = new Dictionary<string, State> {
            {"Cut", new State(F(a["sp1_cut"]), "Remove", 7, change_on_end: false, has_gravity: false)},
            {"Dust", new State(F(a["sp1_dust"]), "Remove", 7, change_on_end: false, has_gravity: false)},
            {"FireballEXHigh", new State(F(a["sp2_fb"]), "Remove", 7, change_on_end: false, has_gravity: false, will_hit: true, glow: true, hitstop: "")},
            {"FireballEXLow", new State(F(a["sp2_fb"]), "Remove", 7, low: true, change_on_end: false, has_gravity: false, will_hit: true, glow: true, hitstop: "")},
            {"FireballHigh", new State(F(a["sp2_fb"]), "Remove", 7, change_on_end: false, has_gravity: false, will_hit: true)},
            {"FireballLow", new State(F(a["sp2_fb"]), "Remove", 7, low: true, change_on_end: false, has_gravity: false, will_hit: true)},
            {"FireballDizzy", new State(F(a["sp2_fb_dizzy"]), "Remove", 7, change_on_end: false, has_gravity: false, will_hit: true)},
            {"FireballHit", new State(F(a["sp2_hit"]), "Remove", 7, change_on_end: false, has_gravity: false)},
        };
    }
    public override void Behave() {
        base.Behave();
        if (this.state.post_state == "Remove" && this.current_animation.ended) {
            this.remove = true;
            this.current_animation.Reset();
        }
        
        if (Math.Abs(Camera.X - this.visual_position.X) > Config.RenderWidth) this.remove = true;  

        if (this.current_state == "Cut") {
            this.body.position = new Vector2f(this.character.body.position.X, this.character.body.position.Y);
            
        } else if (this.life_points.X <= 0 && (this.current_state == "FireballHigh" || this.current_state == "FireballEXLow" || this.current_state == "FireballLow" || this.current_state == "FireballDizzy" || this.current_state == "FireballEXHigh")) {
            this.ChangeState("FireballHit");
        }

    }
    public override int ImposeBehavior(Character target) {
        int hit = Character.NOTHING;

        if (this.next_state != null) return hit;

        Character.Push(target: target, self: this, Config.light_pushback);
        this.character.has_hit = true;

        switch (this.current_state) {
            case "FireballHigh":
                if (target.isBlocking(this)) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 0);
                    target.BlockStun(this, 20, raw_value: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 35, 30);
                    target.Stun(this, 30, raw_value: true);
                }
                Character.AddAuraPoints(target: target, self: this.character, hit);
                break;
            
            case "FireballLow":
                if (target.isBlockingLow(this)) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 0);
                    target.BlockStun(this, 20, raw_value: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 35, 30);
                    target.Stun(this, 30, raw_value: true);
                }
                Character.AddAuraPoints(target: target, self: this.character, hit);
                break;

            case "FireballDizzy":
                if (target.isBlocking(this)) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 0);
                    target.BlockStun(this, 20, raw_value: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 50, 30);
                    target.Stun(this, 30, raw_value: true);
                }
                Character.AddAuraPoints(target: target, self: this.character, hit);
                break;

            case "FireballEXHigh":
                if (target.isBlocking(this)) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 20, 0);
                    target.BlockStun(this, 20, raw_value: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 27, 30);
                    target.Stun(this, 30, raw_value: true);
                }
                break;

            case "FireballEXLow":
                if (target.isBlockingLow(this)) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 20, 0);
                    target.BlockStun(this, 20, raw_value: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 27, 30);
                    target.Stun(this, 30, raw_value: true);
                }                
                break;  
        }

        return hit;
    }
    public override int DefineColisionType(Character target) {
        this.life_points.X -= 1;
        return base.DefineColisionType(target);
    }
}