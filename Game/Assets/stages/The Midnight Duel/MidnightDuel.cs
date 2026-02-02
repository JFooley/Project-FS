using Stage_Space;
using Animation_Space;


public class MidnightDuel : Stage {
    public MidnightDuel()
        : base("The Midnight Duel", 362, 800, 336, "Assets/stages/The Midnight Duel", Program.thumbs["duel_thumb"])
    {
        this.AmbientLight = new SFML.Graphics.Color(205, 220, 195, 255);
    }

    public override void LoadStage() {
        var defaultFrames = new List<Frame> {
            new Frame(0, len: 7),
            new Frame(1, len: 7),
            new Frame(2, len: 7),
            new Frame(3, len: 7)
        };

        var animations = new Dictionary<string, State> {
            { "Default", new State(defaultFrames, "Default")},
        };

        this.states = animations;
    }
}