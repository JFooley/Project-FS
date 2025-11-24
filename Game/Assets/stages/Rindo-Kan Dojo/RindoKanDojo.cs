using Stage_Space;
using Animation_Space;


public class RindoKanDojo : Stage {
    public RindoKanDojo()
        : base("Rindo-Kan Dojo", 496, 895, 475, "Assets/stages/Rindo-Kan Dojo", Program.thumbs["kan_thumb"])
    {
        this.AmbientLight = new SFML.Graphics.Color(215, 210, 180, 255);
    }

    public override void LoadStage() {
        var defaultFrames = new List<int> {0};
        List<string> frames = defaultFrames.Select(i => i.ToString()).ToList();

        var animations = new Dictionary<string, State> {
            { "Default", new State(frames, "Default", 1)},
        };

        this.states = animations;
    }
}