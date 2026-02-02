using Stage_Space;
using Animation_Space;


public class NYCSubway : Stage {
    
    public NYCSubway()
        : base("NYC Subway", 528, 912, 512, "Assets/stages/NYC Subway", Program.thumbs["subway_thumb"])
    {
        this.AmbientLight = new SFML.Graphics.Color(255, 255, 220, 255);
    }

    public override void LoadStage() {
        var day = new List<Frame> {
            new Frame(10, len: 3),
            new Frame(11, len: 3),
            new Frame(12, len: 3),
            new Frame(13, len: 3),
            new Frame(14, len: 3),
            new Frame(15, len: 3),
            new Frame(16, len: 3),
            new Frame(17, len: 3),
            new Frame(18, len: 3),
            new Frame(19, len: 3),
            new Frame(110, len: 3),
            new Frame(111, len: 3),
            new Frame(112, len: 3),
            new Frame(113, len: 3),
            new Frame(114, len: 3),
            new Frame(115, len: 3),
            new Frame(116, len: 3),
            new Frame(117, len: 3),
            new Frame(118, len: 3),
            new Frame(119, len: 3),
            new Frame(120, len: 3),
            new Frame(121, len: 3),
            new Frame(122, len: 3),
            new Frame(123, len: 3),
            new Frame(124, len: 3),
            new Frame(125, len: 3),
            new Frame(126, len: 3),
            new Frame(127, len: 3),
            new Frame(128, len: 3),
            new Frame(129, len: 3),
            new Frame(130, len: 3),
            new Frame(131, len: 3),
            new Frame(132, len: 3),
            new Frame(133, len: 3),
            new Frame(134, len: 3),
            new Frame(135, len: 3),
            new Frame(136, len: 3),
            new Frame(137, len: 3),
            new Frame(138, len: 3),
            new Frame(139, len: 3),
            new Frame(140, len: 3),
            new Frame(141, len: 3),
            new Frame(142, len: 3),
            new Frame(143, len: 3)
        };

        var animations = new Dictionary<string, State> {
            { "day", new State(day, "day")},
        };

        this.CurrentState = "day";

        this.states = animations;
    }
}