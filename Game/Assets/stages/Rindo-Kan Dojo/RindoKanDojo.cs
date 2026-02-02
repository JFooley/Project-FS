using Stage_Space;
using Animation_Space;


public class RindoKanDojo : Stage {
    public RindoKanDojo()
        : base("Rindo-Kan Dojo", 496, 895, 475, "Assets/stages/Rindo-Kan Dojo", Program.thumbs["kan_thumb"])
    {
        this.AmbientLight = new SFML.Graphics.Color(215, 210, 180, 255);
    }

    public override void LoadStage() {
        var animations = new Dictionary<string, State> {
            { "Default", new State(new List<Frame> {new Frame(0, 60)}, "Default", 1)},
        };

        this.states = animations;
    }
}