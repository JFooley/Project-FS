using Stage_Space;
using Animation_Space;


public class JapanFields : Stage {
    public JapanFields()
        : base("Japan Fields", 540, 896, 511, "Assets/stages/Japan Fields", Program.thumbs["japan_thumb"])
    {
        this.AmbientLight = new SFML.Graphics.Color(250, 250, 230, 255);
    }

    public override void LoadStage() {
        var animations = new Dictionary<string, State> {
            { "Default", new State(new List<Frame> {new Frame(0, len: 60)}, "Default")},
        };

        this.states = animations;
    }
}