public class NightAlley : Stage {
    public NightAlley()
        : base("Night Alley", 409, 640, 384, Data.GetPath("Assets/stages/Night Alley"))
    {
        this.AmbientLight = new SFML.Graphics.Color(180, 180, 230, 255);
    }

    public override void LoadStage() {
        Frame[] frames = Enumerable.Range(0, 202).Select(i => new Frame(i)).ToArray();

        var animations = new Dictionary<string, State> {
            { "Default", new State(F(frames), "Default", 60)},
        };

        this.states = animations;
    }
}