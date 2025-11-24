using Stage_Space;
using Animation_Space;


public class MidnightDuel : Stage {
    public MidnightDuel()
        : base("The Midnight Duel", 362, 800, 336, "Assets/stages/The Midnight Duel", Program.thumbs["duel_thumb"])
    {
        this.AmbientLight = new SFML.Graphics.Color(205, 220, 195, 255);
    }

    public override void LoadStage() {
        var defaultFrames = new List<int> {0, 1, 2, 3};
        List<string> frames = defaultFrames.Select(i => i.ToString()).ToList();

        var animations = new Dictionary<string, State> {
            { "Default", new State(frames, "Default", 8)},
        };

        this.states = animations;
    }
}