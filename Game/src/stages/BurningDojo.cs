public class BurningDojo : Stage {
    public BurningDojo()
        : base("Burning Dojo", 369, 800, 336, Data.GetPath("Assets/stages/Burning Dojo"))
    {
        this.AmbientLight = new SFML.Graphics.Color(190, 160, 150, 255);
    }

    public override void LoadStage() {
        Frame[] frames = Enumerable.Range(0, 8).Select(i => new Frame(i, len: 4)).ToArray();

        this.states = new Dictionary<string, State> {
            { "Default", new State(F(frames), "Default")},
        };
    }
}