using SFML.Graphics;
using SFML.Audio;
using SFML.System;

public class Hitspark : Character {
    private static Dictionary<string, Texture> textures_local = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => textures_local; protected set => textures_local = value ?? new Dictionary<string, Texture>();}
    
    private static Dictionary<string, SoundBuffer> sounds_local = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => sounds_local; protected set => sounds_local = value ?? new Dictionary<string, SoundBuffer>();}

    private static Dictionary<string, Frame[]> _shared_animations = new Dictionary<string, Frame[]>();
    public override Dictionary<string, Frame[]> animations { get => _shared_animations; protected set => _shared_animations = value ?? new Dictionary<string, Frame[]>();}

    public Hitspark(string initialState, float startX, float startY, int facing)
        : base("Hitspark", initialState, startX, startY, Data.GetPath("Assets/particles/Hitspark")) {
            this.facing = facing;
            this.own_light = Color.White;
        }
    public Hitspark() : base("Hitspark", "", 0, 0, Data.GetPath("Assets/particles/Hitspark")) {}

    public override void Load() {
        var a = this.animations;
        
        this.states = new Dictionary<string, State> {
            {"HitLight", new State(F(a["HitLight"]), "Remove")},
            {"HitMedium", new State(F(a["HitMedium"]), "Remove")},
            {"HitHeavy", new State(F(a["HitHeavy"]), "Remove")},
            {"Parry", new State(F(a["Parry"]), "Remove")},
            {"Block", new State(F(a["Block"]), "Remove")},
            {"Grab", new State(F(a["Grab"]), "Remove")},
            {"Tech", new State(F(a["Tech"]), "Remove")},

        };
    }

    public override void Behave() {        
        if (this.state.post_state == "Remove" && this.current_animation.ended) {
            this.remove = true;
            this.current_animation.Reset();
        }
    }
}