using Stage_Space;
using Animation_Space;


public class BurningDojo : Stage {
    public BurningDojo()
        : base("Burning Dojo", 369, 800, 336, "Assets/stages/Burning Dojo", Program.thumbs["dojo_thumb"])
    {
        this.AmbientLight = new SFML.Graphics.Color(255, 230, 210, 255);
    }

    public override void LoadStage() {
        List<string> frames = new List<int> {0, 1, 2, 3, 4, 5, 6, 7}.Select(i => i.ToString()).ToList();

        this.states = new Dictionary<string, State> {
            { "Default", new State(frames, "Default", 15)},
        };
    }
}