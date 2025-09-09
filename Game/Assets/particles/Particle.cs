using Character_Space;
using Animation_Space;
using SFML.System;
using SFML.Graphics;
using Stage_Space;
using SFML.Audio;

public class Particle : Character {
    private static Dictionary<string, Texture> textures_local = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => textures_local; protected set => textures_local = value ?? new Dictionary<string, Texture>();}
    private static Dictionary<string, SoundBuffer> sounds_local = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => sounds_local; protected set => sounds_local = value ?? new Dictionary<string, SoundBuffer>();}

    public Particle(string initialState, float startX, float startY, int facing, Stage stage = null)
        : base("Particle", initialState, startX, startY, "Assets/particles/Particle", stage) {
            this.facing = facing;
        }
    public Particle() : base("Particle", "", 0, 0, "Assets/particles/Particle", null) {}

    public override void Load() {
        // Animations

        var SAGathering = new List<FrameData> {
            new FrameData(11, 0, 0, new List<GenericBox> {}, "super art"),
            new FrameData(12, 0, 0, new List<GenericBox> {}),
            new FrameData(13, 0, 0, new List<GenericBox> {}),
            new FrameData(14, 0, 0, new List<GenericBox> {}),
            new FrameData(15, 0, 0, new List<GenericBox> {}),
            new FrameData(16, 0, 0, new List<GenericBox> {}),
            new FrameData(17, 0, 0, new List<GenericBox> {}),
            new FrameData(18, 0, 0, new List<GenericBox> {}),
            new FrameData(19, 0, 0, new List<GenericBox> {}),
            new FrameData(110, 0, 0, new List<GenericBox> {}),
            new FrameData(111, 0, 0, new List<GenericBox> {}),
            new FrameData(112, 0, 0, new List<GenericBox> {}),
            new FrameData(113, 0, 0, new List<GenericBox> {}),
            new FrameData(114, 0, 0, new List<GenericBox> {}),
            new FrameData(115, 0, 0, new List<GenericBox> {}),
            new FrameData(116, 0, 0, new List<GenericBox> {}),
            new FrameData(117, 0, 0, new List<GenericBox> {}),
            new FrameData(118, 0, 0, new List<GenericBox> {}),
            new FrameData(119, 0, 0, new List<GenericBox> {}),
            new FrameData(120, 0, 0, new List<GenericBox> {}),
            new FrameData(121, 0, 0, new List<GenericBox> {}),
            new FrameData(122, 0, 0, new List<GenericBox> {}),
            new FrameData(123, 0, 0, new List<GenericBox> {}),
            new FrameData(124, 0, 0, new List<GenericBox> {}),
            new FrameData(125, 0, 0, new List<GenericBox> {}),
            new FrameData(126, 0, 0, new List<GenericBox> {}),
            new FrameData(127, 0, 0, new List<GenericBox> {}),
            new FrameData(128, 0, 0, new List<GenericBox> {}),
            new FrameData(129, 0, 0, new List<GenericBox> {}),
            new FrameData(130, 0, 0, new List<GenericBox> {}),
            new FrameData(131, 0, 0, new List<GenericBox> {}),
            new FrameData(132, 0, 0, new List<GenericBox> {}),
        };

        var SALighting = new List<FrameData> { 
            new FrameData(133, 0, 0, new List<GenericBox> {}),
            new FrameData(134, 0, 0, new List<GenericBox> {}),
            new FrameData(135, 0, 0, new List<GenericBox> {}),
            new FrameData(136, 0, 0, new List<GenericBox> {}),
            new FrameData(137, 0, 0, new List<GenericBox> {}),
            new FrameData(138, 0, 0, new List<GenericBox> {}),
            new FrameData(139, 0, 0, new List<GenericBox> {}),
            new FrameData(140, 0, 0, new List<GenericBox> {}),
            new FrameData(141, 0, 0, new List<GenericBox> {}),
            new FrameData(142, 0, 0, new List<GenericBox> {}),
            new FrameData(143, 0, 0, new List<GenericBox> {}),
            new FrameData(144, 0, 0, new List<GenericBox> {}),
            new FrameData(145, 0, 0, new List<GenericBox> {}),
            new FrameData(146, 0, 0, new List<GenericBox> {}),
            new FrameData(147, 0, 0, new List<GenericBox> {}),
            new FrameData(148, 0, 0, new List<GenericBox> {}),
            new FrameData(149, 0, 0, new List<GenericBox> {}),
            new FrameData(150, 0, 0, new List<GenericBox> {}),
            new FrameData(151, 0, 0, new List<GenericBox> {}),
            new FrameData(152, 0, 0, new List<GenericBox> {}),
            new FrameData(153, 0, 0, new List<GenericBox> {}),
            new FrameData(154, 0, 0, new List<GenericBox> {}),
        };

        var SABlink = new List<FrameData> {
            new FrameData(29, 0, 0, new List<GenericBox> {}),
            new FrameData(210, 0, 0, new List<GenericBox> {}),
            new FrameData(211, 0, 0, new List<GenericBox> {}),
            new FrameData(212, 0, 0, new List<GenericBox> {}),
            new FrameData(213, 0, 0, new List<GenericBox> {}),
            new FrameData(214, 0, 0, new List<GenericBox> {}),
            new FrameData(215, 0, 0, new List<GenericBox> {}),
            new FrameData(216, 0, 0, new List<GenericBox> {}),
            new FrameData(217, 0, 0, new List<GenericBox> {}),
            new FrameData(218, 0, 0, new List<GenericBox> {}),
            new FrameData(219, 0, 0, new List<GenericBox> {}),
            new FrameData(220, 0, 0, new List<GenericBox> {}),
            new FrameData(221, 0, 0, new List<GenericBox> {}),
        };

        var Shungoku = new List<FrameData> {
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(30, 0, 0, new List<GenericBox> {}),
            new FrameData(31, 0, 0, new List<GenericBox> {}),
            new FrameData(32, 0, 0, new List<GenericBox> {}),
            new FrameData(33, 0, 0, new List<GenericBox> {}),
            new FrameData(34, 0, 0, new List<GenericBox> {}),
            new FrameData(35, 0, 0, new List<GenericBox> {}),
            new FrameData(36, 0, 0, new List<GenericBox> {}),
            new FrameData(37, 0, 0, new List<GenericBox> {}),
            new FrameData(38, 0, 0, new List<GenericBox> {}),
            new FrameData(39, 0, 0, new List<GenericBox> {}),
            new FrameData(310, 0, 0, new List<GenericBox> {}),
            new FrameData(311, 0, 0, new List<GenericBox> {}),
            new FrameData(312, 0, 0, new List<GenericBox> {}),
            new FrameData(313, 0, 0, new List<GenericBox> {}),
            new FrameData(314, 0, 0, new List<GenericBox> {}),
            new FrameData(315, 0, 0, new List<GenericBox> {}),
            new FrameData(316, 0, 0, new List<GenericBox> {}, "explosão"),
            new FrameData(317, 0, 0, new List<GenericBox> {}),
            new FrameData(318, 0, 0, new List<GenericBox> {}),
            new FrameData(319, 0, 0, new List<GenericBox> {}),
            new FrameData(320, 0, 0, new List<GenericBox> {}),
            new FrameData(321, 0, 0, new List<GenericBox> {}),
            new FrameData(322, 0, 0, new List<GenericBox> {}),
            new FrameData(323, 0, 0, new List<GenericBox> {}),
            new FrameData(324, 0, 0, new List<GenericBox> {}),
            new FrameData(325, 0, 0, new List<GenericBox> {}),
            new FrameData(326, 0, 0, new List<GenericBox> {}),
        };

        var Shungoku_text = new List<FrameData> {
            new FrameData(511, 0, 0, new List<GenericBox> {}),
            new FrameData(512, 0, 0, new List<GenericBox> {}),
            new FrameData(513, 0, 0, new List<GenericBox> {}),
            new FrameData(514, 0, 0, new List<GenericBox> {}),
            new FrameData(515, 0, 0, new List<GenericBox> {}),
            new FrameData(516, 0, 0, new List<GenericBox> {}),
            new FrameData(516, 0, 0, new List<GenericBox> {}),
            new FrameData(516, 0, 0, new List<GenericBox> {}),
            new FrameData(516, 0, 0, new List<GenericBox> {}),
            new FrameData(516, 0, 0, new List<GenericBox> {}),
            new FrameData(517, 0, 0, new List<GenericBox> {}),
            new FrameData(518, 0, 0, new List<GenericBox> {}),
            new FrameData(519, 0, 0, new List<GenericBox> {}),
            new FrameData(520, 0, 0, new List<GenericBox> {}),
            new FrameData(521, 0, 0, new List<GenericBox> {}),
        };
        
        // States
        var animations = new Dictionary<string, State> {
            {"SALighting", new State(SAGathering, "SALighting_tail", 60)},
            {"SALighting_tail", new State(SALighting, "Remove", 60)},
            {"SABlink", new State(SAGathering, "SABlink_tail", 60)},
            {"SABlink_tail", new State(SABlink, "Remove", 20)},
            {"Shungoku", new State(Shungoku, "Remove", 15)},
            {"Shungoku_text", new State(Shungoku_text, "Remove", 15)},
        };

        this.states = animations;
    }
    
    public override void Render(bool drawHitboxes = false) {
        if (!this.render) return;
        
        // Render sprite
        Sprite temp_sprite = this.GetCurrentSprite();
        temp_sprite.Position = new Vector2f(this.body.Position.X - (temp_sprite.GetLocalBounds().Width / 2 * this.facing), this.body.Position.Y - temp_sprite.GetLocalBounds().Height / 2);
        temp_sprite.Scale = new Vector2f(this.size_ratio * this.facing, this.size_ratio);
        Program.window.Draw(temp_sprite);

        // Play sounds
        this.PlayFrameSound();
    }
    
    public override void DoBehave() {        
        if (this.state.post_state == "Remove" && this.current_animation.ended) {
            this.remove = true;
            this.current_animation.Reset();
        }
    }
}