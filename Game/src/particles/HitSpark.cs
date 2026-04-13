using SFML.Graphics;
using SFML.Audio;

public class Hitspark : Character {
    private static Dictionary<string, Texture> textures_local = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => textures_local; protected set => textures_local = value ?? new Dictionary<string, Texture>();}
    private static Dictionary<string, SoundBuffer> sounds_local = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => sounds_local; protected set => sounds_local = value ?? new Dictionary<string, SoundBuffer>();}

    public Hitspark(string initialState, float startX, float startY, int facing)
        : base("Hitspark", initialState, startX, startY, Data.GetPath("Assets/particles/Hitspark")) {
            this.facing = facing;
            this.own_light = Color.White;
        }
    public Hitspark() : base("Hitspark", "", 0, 0, Data.GetPath("Assets/particles/Hitspark")) {}

    public override void Load() {
        var a = Data.LoadAnimationDat(Path.Combine(this.folder_path, "animations.dat"));
        
        this.states = new Dictionary<string, State> {
            {"Hit1", new State(a["Hit1"], "Remove")},
            {"Hit2", new State(a["Hit2"], "Remove")},
            {"Hit3", new State(a["Hit3"], "Remove")},
            {"Parry", new State(a["Parry"], "Remove")},
            {"Block", new State(a["Block"], "Remove")},
        };
    }

    public override void Behave() {        
        if (this.state.post_state == "Remove" && this.current_animation.ended) {
            this.remove = true;
            this.current_animation.Reset();
        }
    }
}