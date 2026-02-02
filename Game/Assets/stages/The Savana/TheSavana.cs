using Stage_Space;
using Animation_Space;

public class TheSavana : Stage {
    public TheSavana()
        : base("The Savana", 530, 512, 512, "Assets/stages/The Savana", Program.thumbs["savana_thumb"])
    {
        this.AmbientLight = new SFML.Graphics.Color(200, 165, 145, 255);
    }

    public override void LoadStage() {
        var defaultFrames = new List<Frame> { 
            new Frame(0, len: 4),
            new Frame(1, len: 4),
            new Frame(2, len: 4),
            new Frame(3, len: 4),
            new Frame(4, len: 4),
            new Frame(5, len: 4),
            new Frame(6, len: 4),
            new Frame(7, len: 4),
            new Frame(8, len: 4),
            new Frame(9, len: 4),
            new Frame(10, len: 4),
            new Frame(11, len: 4),
            new Frame(12, len: 4),
            new Frame(13, len: 4),
            new Frame(14, len: 4),
            new Frame(15, len: 4)
        };

        var animations = new Dictionary<string, State> {
            { "Default", new State(defaultFrames, "Default")},
        };

        this.states = animations;
    }
}