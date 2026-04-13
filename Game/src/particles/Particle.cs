using SFML.System;
using SFML.Graphics;
using SFML.Audio;

public class Particle : Character {
    private static Dictionary<string, Texture> textures_local = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => textures_local; protected set => textures_local = value ?? new Dictionary<string, Texture>();}
    private static Dictionary<string, SoundBuffer> sounds_local = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => sounds_local; protected set => sounds_local = value ?? new Dictionary<string, SoundBuffer>();}

    public Particle(string initialState, float startX, float startY, int facing)
        : base("Particle", initialState, startX, startY, Data.GetPath("Assets/particles/Particle")) {
            this.facing = facing;
            this.own_light = Color.White;
        }
    public Particle() : base("Particle", "", 0, 0, Data.GetPath("Assets/particles/Particle")) {}

    public override void Load() {
        var a = Data.LoadAnimationDat(Path.Combine(this.folder_path, "animations.dat"));

        var animations = new Dictionary<string, State> {
            {"SALighting", new State(a["SAGathering"], "SALighting_tail", 60)},
            {"SALighting_tail", new State(a["SALighting"], "Remove", 60)},
            {"SABlink", new State(a["SAGathering"], "SABlink_tail", 60)},
            {"SABlink_tail", new State(a["SABlink"], "Remove", 20)},
            {"Shungoku", new State(a["Shungoku"], "Remove", 15)},
            {"Shungoku_text", new State(a["Shungoku_text"], "Remove", 15)},
        };

        this.states = animations;
    }
    
    public override void Render(bool drawHitboxes = false) {
        if (!this.render) return;
        
        // Render sprite
        Sprite temp_sprite = this.GetCurrentSprite();
        temp_sprite.Position = new Vector2f(this.body.Position.X - (temp_sprite.GetLocalBounds().Width / 2 * this.facing), this.body.Position.Y - temp_sprite.GetLocalBounds().Height / 2);
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