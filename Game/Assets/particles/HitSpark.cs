using Character_Space;
using Animation_Space;
using Stage_Space;
using SFML.Graphics;
using SFML.Audio;

public class Hitspark : Character {
    private static Dictionary<string, Texture> textures_local = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => textures_local; protected set => textures_local = value ?? new Dictionary<string, Texture>();}
    private static Dictionary<string, SoundBuffer> sounds_local = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => sounds_local; protected set => sounds_local = value ?? new Dictionary<string, SoundBuffer>();}

    public Hitspark(string initialState, float startX, float startY, int facing, Stage stage = null)
        : base("Hitspark", initialState, startX, startY, "Assets/particles/Hitspark", stage) {
            this.facing = facing;
            this.own_light = Color.White;
        }
    public Hitspark() : base("Hitspark", "", 0, 0, "Assets/particles/Hitspark", null) {}

    public override void Load() {
        // Animations
        var Hit1 = new List<FrameData> { 
            new FrameData(11, 0, 0, new List<GenericBox> {}, "on_hit_1"),
            new FrameData(12, 0, 0, new List<GenericBox> {}),
            new FrameData(13, 0, 0, new List<GenericBox> {}),
            new FrameData(14, 0, 0, new List<GenericBox> {}),
            new FrameData(15, 0, 0, new List<GenericBox> {}),
            new FrameData(16, 0, 0, new List<GenericBox> {}),
            new FrameData(17, 0, 0, new List<GenericBox> {}),
            new FrameData(18, 0, 0, new List<GenericBox> {}),
            new FrameData(19, 0, 0, new List<GenericBox> {}),
            new FrameData(110, 0, 0, new List<GenericBox> {}),
        };

        var Hit2 = new List<FrameData> { 
            new FrameData(21, 0, 0, new List<GenericBox> {}, "on_hit_2"),
            new FrameData(22, 0, 0, new List<GenericBox> {}),
            new FrameData(23, 0, 0, new List<GenericBox> {}),
            new FrameData(24, 0, 0, new List<GenericBox> {}),
            new FrameData(25, 0, 0, new List<GenericBox> {}),
            new FrameData(26, 0, 0, new List<GenericBox> {}),
            new FrameData(27, 0, 0, new List<GenericBox> {}),
            new FrameData(28, 0, 0, new List<GenericBox> {}),
            new FrameData(29, 0, 0, new List<GenericBox> {}),
            new FrameData(210, 0, 0, new List<GenericBox> {}),
        };

        var Hit3 = new List<FrameData> { 
            new FrameData(41, 0, 0, new List<GenericBox> {}, "on_hit_3"),
            new FrameData(42, 0, 0, new List<GenericBox> {}),
            new FrameData(43, 0, 0, new List<GenericBox> {}),
            new FrameData(44, 0, 0, new List<GenericBox> {}),
            new FrameData(45, 0, 0, new List<GenericBox> {}),
            new FrameData(46, 0, 0, new List<GenericBox> {}),
            new FrameData(47, 0, 0, new List<GenericBox> {}),
            new FrameData(48, 0, 0, new List<GenericBox> {}),
            new FrameData(49, 0, 0, new List<GenericBox> {}),
            new FrameData(410, 0, 0, new List<GenericBox> {}),
        };

        var Block = new List<FrameData> {
            new FrameData(31, 0, 0, new List<GenericBox> {}, "on_block"),
            new FrameData(32, 0, 0, new List<GenericBox> {}),
            new FrameData(33, 0, 0, new List<GenericBox> {}),
            new FrameData(34, 0, 0, new List<GenericBox> {}),
            new FrameData(35, 0, 0, new List<GenericBox> {}),
            new FrameData(36, 0, 0, new List<GenericBox> {}),
        };

        var Parry = new List<FrameData> {
            new FrameData("parry1", 0, 0, new List<GenericBox> {}),
            new FrameData("parry2", 0, 0, new List<GenericBox> {}, "parry"),
            new FrameData("parry3", 0, 0, new List<GenericBox> {}),
            new FrameData("parry4", 0, 0, new List<GenericBox> {}),
            new FrameData("parry5", 0, 0, new List<GenericBox> {}),
            new FrameData("parry6", 0, 0, new List<GenericBox> {}),
            new FrameData("parry7", 0, 0, new List<GenericBox> {}),
            new FrameData("parry8", 0, 0, new List<GenericBox> {}),
            new FrameData("parry9", 0, 0, new List<GenericBox> {}),
        };
        
        // States
        var animations = new Dictionary<string, State> {
            {"Hit1", new State(Hit1, "Remove", 60)},
            {"Hit2", new State(Hit2, "Remove", 60)},
            {"Hit3", new State(Hit3, "Remove", 60)},
            {"Parry", new State(Parry, "Remove", 20)},
            {"Block", new State(Block, "Remove", 20)},
        };

        this.states = animations;
    }

    public override void DoBehave() {        
        if (this.state.post_state == "Remove" && this.current_animation.ended) {
            this.remove = true;
            this.current_animation.Reset();
        }
    }
}