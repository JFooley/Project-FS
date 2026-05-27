public class MidnightDuel : Stage {
    public MidnightDuel()
        : base("The Midnight Duel", 362, 800, 336, Data.GetPath("assets/stages/The Midnight Duel"))
    {
        this.AmbientLight = new SFML.Graphics.Color(205, 220, 195, 255);
    }

    public override void LoadStage() {
        Frame[] frames = Enumerable.Range(0, 4).Select(i => new Frame(i, len: 7)).ToArray();

        var animations = new Dictionary<string, State> {
            { "Default", new State(F(frames), "Default")},
        };

        this.states = animations;
    }
}