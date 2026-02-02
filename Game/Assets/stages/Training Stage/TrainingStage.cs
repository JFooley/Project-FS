using Stage_Space;
using Animation_Space;

public class TrainingStage : Stage {
    public TrainingStage()
        : base("Training Stage", 473, 544, 450, "Assets/stages/Training Stage", Program.thumbs["training_thumb"])
    {
        this.AmbientLight = new SFML.Graphics.Color(255, 255, 225, 255);
    }

    public override void LoadStage() {
        var animations = new Dictionary<string, State> {
            { "Default", new State(new List<Frame> {new Frame(0, len: 60)}, "Default")},
        };

        this.states = animations;
    }
}