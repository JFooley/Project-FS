public class TheSavana : Stage {
    public TheSavana()
        : base("The Savana", 530, 512, 512, Data.GetPath("Assets/stages/The Savana"))
    {
        this.AmbientLight = new SFML.Graphics.Color(200, 165, 145, 255);
    }

    public override void LoadStage() {
        Frame[] frames = Enumerable.Range(0, 16).Select(i => new Frame(i, len: 4)).ToArray();

        var animations = new Dictionary<string, State> {
            { "Default", new State(F(frames), "Default")},
        };

        this.states = animations;
    }
}