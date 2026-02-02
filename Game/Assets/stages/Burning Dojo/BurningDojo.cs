using Stage_Space;
using Animation_Space;

public class BurningDojo : Stage {
    public BurningDojo()
        : base("Burning Dojo", 369, 800, 336, "Assets/stages/Burning Dojo", Program.thumbs["dojo_thumb"])
    {
        this.AmbientLight = new SFML.Graphics.Color(190, 160, 150, 255);
    }

    public override void LoadStage() {
        List<Frame> frames = new List<Frame> {
            new Frame(0, len: 4),
            new Frame(1, len: 4),
            new Frame(2, len: 4),
            new Frame(3, len: 4),
            new Frame(4, len: 4),
            new Frame(5, len: 4),
            new Frame(6, len: 4),
            new Frame(7, len: 4)
        };

        this.states = new Dictionary<string, State> {
            { "Default", new State(frames, "Default")},
        };
    }
}