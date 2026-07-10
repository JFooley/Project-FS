using SFML.System;
using SFML.Graphics;
using SFML.Audio;

public class Super : Character {
    private static Dictionary<string, Texture> textures_local = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => textures_local; protected set => textures_local = value ?? new Dictionary<string, Texture>();}
    
    private static Dictionary<string, SoundBuffer> sounds_local = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => sounds_local; protected set => sounds_local = value ?? new Dictionary<string, SoundBuffer>();}
    
    private static Dictionary<string, Frame[]> _shared_animations = new Dictionary<string, Frame[]>();
    public override Dictionary<string, Frame[]> animations { get => _shared_animations; protected set => _shared_animations = value ?? new Dictionary<string, Frame[]>();}

    private Vector2f initial_pos;

    public Super(string initialState, float startX, float startY, int facing)
        : base("Super", initialState, startX, startY, Data.GetPath("assets/particles/Super")) {
            this.facing = facing;
            this.own_light = Color.White;
            this.initial_pos = new Vector2f(startX, startY);
        }
    public Super() : base("Super", "", 0, 0, Data.GetPath("assets/particles/Super")) {}

    public override void Load() {
        var a = this.animations;

        this.states = new Dictionary<string, State> {
            {"SALighting", new State(F(a["SAGathering"]), "SALighting_tail_1")},
            {"SALighting_tail_1", new State(F(a["SALighting_horizontal"]), "SALighting_tail_2")},
            {"SALighting_tail_2", new State(F(a["SALighting_vertical"]), "Remove")},
            {"SABlink", new State(F(a["SAGathering"]), "SABlink_tail")},
            {"SABlink_tail", new State(F(a["SABlink"]), "Remove")}
        };
    }

    public override void Update() {
        base.Update();

        switch (this.current_state) {
            case "SALighting_tail_1":
                this.body.position.Y = this.initial_pos.Y;
                this.body.position.X = this.current_animation.on_last_frame ? this.initial_pos.X : Camera.X;

                break;

            case "SALighting_tail_2":
                this.body.position.Y = this.current_animation.on_last_frame ? this.initial_pos.Y : Camera.Y;
                this.body.position.X = this.initial_pos.X;
                break;

            default:
                this.body.position = this.initial_pos;
                break;
        }       

    }

    public override void Render(bool drawHitboxes = false) {
        if (!this.render) return;
        
        // Render sprite
        Sprite temp_sprite = this.GetCurrentSprite();
        temp_sprite.Position = new Vector2f(this.body.position.X - (temp_sprite.GetLocalBounds().Width / 2 * this.facing), this.body.position.Y - temp_sprite.GetLocalBounds().Height / 2);
        temp_sprite.Scale = new Vector2f(this.facing, 1f);
        Program.window.Draw(temp_sprite);

        // Play sounds
        this.PlayFrameSound();
    }
    
    public override void Behave() {        
        if (this.state.post_state == "Remove" && this.current_animation.ended) {
            this.remove = true;
            this.current_animation.Reset();
        }
    }
}