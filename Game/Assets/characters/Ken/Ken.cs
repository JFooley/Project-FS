using SFML.System;
using Character_Space;
using Animation_Space;
using Input_Space;
using System.IO.Compression;
using Stage_Space;
using SFML.Graphics;
using SFML.Audio;
using UI_space;

public class Ken : Character {
    private static Dictionary<string, Texture> _shared_textures = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => _shared_textures; protected set => _shared_textures = value ?? new Dictionary<string, Texture>();}
    private static Dictionary<string, SoundBuffer> _shared_sounds = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => _shared_sounds; protected set => _shared_sounds = value ?? new Dictionary<string, SoundBuffer>();}
    private static Texture _shared_palette;
    public override Texture palette {get => _shared_palette; protected set => _shared_palette = value;}

    private int tatso_speed = 4;
    private Fireball current_fireball;
// Constructors
    public Ken(string initialState, int startX, int startY)
        : base("Ken", initialState, startX, startY, "Assets/characters/Ken")
    {
        this.life_points = new Vector2i(1000, 1000);
        this.dizzy_points = new Vector2i(500, 500);
        this.aura_points = new Vector2i(0, 100);

        this.dash_speed = 8;
        this.move_speed = 3;
    } 
    public Ken() : base("Ken", "Assets/characters/Ken") { }

    public override Character Copy() {
        var obj = new Ken("Intro", 0, 0);
        obj.Load();
        return obj;
    }
    public override void Load() {
        // Hurtboxes
        var pushbox = new GenericBox(2, 125 - this.push_box_width, 110, 125 + this.push_box_width, 195);
        var airPuxbox = new GenericBox(2, 125 - this.push_box_width, 80, 125 + this.push_box_width, 156);

        // Animations
        var introFrames = new List<Frame> {
            new FrameData(15568, 0, 0, len: 36, boxes: new List<GenericBox> { pushbox,}),
            new FrameData(15569, 0, 0, len: 6, boxes: new List<GenericBox> { pushbox,}),
            new FrameData(15570, 0, 0, len: 6, boxes: new List<GenericBox> { pushbox,}),
            new FrameData(15571, 0, 0, len: 6, boxes: new List<GenericBox> { pushbox,}),
            new FrameData(15572, 0, 0, len: 6, boxes: new List<GenericBox> { pushbox,}),
            new FrameData(15573, 0, 0, len: 6, boxes: new List<GenericBox> { pushbox,}, sound: "ykesse"),
            new FrameData(15574, 0, 0, len: 6, boxes: new List<GenericBox> { pushbox,}),
            new FrameData(15575, 0, 0, len: 6, boxes: new List<GenericBox> { pushbox,}),
        };

        var idleFrames = new List<Frame> {
            new FrameData("14657", 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 116, 90, 138, 110) }),
            new FrameData("14658", 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 118, 91, 139, 109) }),
            new FrameData("14659", 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 118, 87, 138, 105) }),
            new FrameData("14660", 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 118, 85, 139, 104) }),
            new FrameData("14661", 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 116, 83, 140, 102) }),
            new FrameData("14662", 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 117, 83, 140, 102) }),
            new FrameData("14663", 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 117, 82, 139, 101) }),
            new FrameData("14664", 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 117, 85, 139, 104) }),
            new FrameData("14665", 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 118, 83, 139, 104) }),
            new FrameData("14666", 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 117, 88, 140, 108) }),
        };
    
        var OnBlockFrames = new List<Frame> {
            new FrameData(14751, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 91, 129, 107), new GenericBox(1, 95, 104, 145, 123), new GenericBox(1, 97, 123, 139, 160), new GenericBox(1, 88, 159, 154, 194) }),
            new FrameData(14752, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 113, 93, 129, 109), new GenericBox(1, 96, 102, 142, 128), new GenericBox(1, 100, 128, 135, 156), new GenericBox(1, 90, 155, 148, 194) }),
            new FrameData(14753, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 91, 125, 106), new GenericBox(1, 90, 102, 136, 128), new GenericBox(1, 95, 128, 134, 153), new GenericBox(1, 87, 154, 149, 194) }),
            new FrameData(14754, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 112, 91, 127, 106), new GenericBox(1, 94, 103, 139, 127), new GenericBox(1, 90, 128, 136, 151), new GenericBox(1, 89, 152, 152, 195) }),
        };

        var OnBlockLowFrames = new List<Frame> {
            new FrameData(14755, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 116, 124, 137, 139), new GenericBox(1, 96, 133, 142, 152), new GenericBox(1, 98, 153, 157, 174), new GenericBox(1, 84, 174, 169, 196) }),
            new FrameData(14756, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 112, 123, 132, 140), new GenericBox(1, 97, 132, 144, 151), new GenericBox(1, 100, 151, 154, 171), new GenericBox(1, 89, 170, 164, 195) }),
            new FrameData(14757, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 111, 125, 127, 139), new GenericBox(1, 95, 132, 143, 155), new GenericBox(1, 98, 155, 166, 177), new GenericBox(1, 86, 176, 167, 195) }),
            new FrameData(14758, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 123, 125, 139), new GenericBox(1, 92, 133, 141, 154), new GenericBox(1, 98, 155, 161, 175), new GenericBox(1, 87, 175, 166, 196) }),
            new FrameData(14759, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 123, 125, 139), new GenericBox(1, 94, 133, 140, 149), new GenericBox(1, 97, 149, 154, 171), new GenericBox(1, 87, 171, 165, 195) }),
        };

        var OnHitFrames = new List<Frame> {
            new FrameData(14807, 0, 0, len: 2, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 111, 92, 127, 105), new GenericBox(1, 89, 107, 140, 135), new GenericBox(1, 92, 135, 141, 161), new GenericBox(1, 98, 161, 152, 194) }),
            new FrameData(14808, 0, 0, len: 2, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 95, 93, 112, 107), new GenericBox(1, 85, 105, 139, 134), new GenericBox(1, 93, 134, 149, 162), new GenericBox(1, 93, 163, 158, 196) }),
            new FrameData(14809, 0, 0, len: 2, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 82, 97, 99, 112), new GenericBox(1, 81, 113, 132, 140), new GenericBox(1, 90, 141, 149, 166), new GenericBox(1, 93, 166, 162, 195) }),
            new FrameData(14810, 0, 0, len: 2, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 67, 107, 87, 123), new GenericBox(1, 76, 110, 144, 141), new GenericBox(1, 92, 141, 146, 168), new GenericBox(1, 91, 168, 164, 195) }),
            new FrameData(14811, 0, 0, len: 2, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 86, 108, 148, 151), new GenericBox(1, 91, 151, 176, 194) }),
            new FrameData(14812, 0, 0, len: 2, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 100, 113, 116, 130), new GenericBox(1, 87, 117, 129, 157), new GenericBox(1, 128, 123, 154, 140), new GenericBox(1, 92, 147, 174, 195) }),
            new FrameData(14813, 0, 0, len: 2, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 109, 101, 127, 118), new GenericBox(1, 89, 116, 142, 144), new GenericBox(1, 90, 144, 154, 165), new GenericBox(1, 88, 164, 157, 195) }),
            new FrameData(14814, 0, 0, len: 2, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 115, 96, 135, 113), new GenericBox(1, 96, 107, 147, 139), new GenericBox(1, 95, 140, 146, 159), new GenericBox(1, 90, 158, 154, 194) }),
        };
        
        var OnHitLowFrames = new List<Frame> {
            new FrameData(14860, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 117, 122, 135, 138), new GenericBox(1, 89, 135, 139, 151), new GenericBox(1, 92, 152, 147, 174), new GenericBox(1, 78, 174, 156, 196) }),
            new FrameData(14861, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 84, 123, 103, 137), new GenericBox(1, 67, 135, 123, 154), new GenericBox(1, 85, 155, 149, 176), new GenericBox(1, 84, 176, 166, 195) }),
            new FrameData(14862, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 69, 127, 95, 145), new GenericBox(1, 68, 140, 115, 159), new GenericBox(1, 85, 158, 141, 178), new GenericBox(1, 85, 178, 162, 195) }),
            new FrameData(14863, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 58, 139, 78, 155), new GenericBox(1, 65, 141, 112, 162), new GenericBox(1, 89, 163, 132, 178), new GenericBox(1, 82, 179, 162, 194) }),
            new FrameData(14864, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 85, 128, 102, 140), new GenericBox(1, 86, 132, 130, 152), new GenericBox(1, 82, 153, 132, 171), new GenericBox(1, 82, 170, 156, 194) }),
        };

        var parryFrames = new List<Frame> {
            new FrameData("15034", 0.0f, 0.0f, len: 1, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}, sound: "golpe_grito_4"),
            new FrameData("15033", 0.0f, 0.0f, len: 8, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData("15034", 0.0f, 0.0f, len: 1, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
        };

        var airParryFrames = new List<Frame> {
            new FrameData("14760", 0.0f, 0.0f, len: 3, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 95, 75, 155, 155) }),
            new FrameData("14761", 0.0f, 0.0f, len: 3, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 95, 75, 155, 155) }),
            new FrameData("14764", 0.0f, 0.0f, len: 3, boxes:new List<GenericBox> { pushbox, new GenericBox(1, 95, 75, 155, 155) }),
        };

        // Normals
        var LPFrames = new List<Frame> { 
            new FrameData(15008, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 105, 97, 144, 154), new GenericBox(1, 136, 103, 162, 129), new GenericBox(1, 91, 96, 125, 121), new GenericBox(1, 120, 87, 141, 105), new GenericBox(1, 87, 137, 162, 194), pushbox}, sound: "golpe_1"),
            new FrameData(15009, 0, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 145, 100, 206, 119), new GenericBox(1, 142, 103, 204, 116), new GenericBox(1, 120, 87, 142, 105), new GenericBox(1, 88, 97, 116, 121), new GenericBox(1, 105, 98, 146, 153), new GenericBox(1, 88, 148, 158, 194), pushbox}),
            new FrameData(15010, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 144, 99, 200, 119), new GenericBox(1, 139, 102, 199, 117), new GenericBox(1, 105, 96, 145, 154), new GenericBox(1, 120, 86, 145, 107), new GenericBox(1, 91, 96, 124, 122), new GenericBox(1, 89, 146, 159, 195), pushbox}),
            new FrameData(15011, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 119, 87, 144, 107), new GenericBox(1, 139, 100, 178, 126), new GenericBox(1, 93, 99, 123, 122), new GenericBox(1, 105, 99, 145, 151), new GenericBox(1, 90, 145, 157, 194), pushbox}),
            new FrameData(15012, 0, 0, len: 5, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 120, 88, 142, 108), new GenericBox(1, 105, 98, 143, 153), new GenericBox(1, 139, 101, 160, 128), new GenericBox(1, 93, 99, 126, 122), new GenericBox(1, 88, 147, 155, 196), pushbox}),
        };
        
        var lowLPFrames = new List<Frame> {
            new FrameData(15152, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 124, 126, 143, 143), new GenericBox(1, 99, 132, 152, 152), new GenericBox(1, 98, 153, 156, 170), new GenericBox(1, 87, 169, 160, 196) }),
            new FrameData(15153, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 128, 126, 143, 144), new GenericBox(1, 99, 130, 149, 150), new GenericBox(1, 99, 151, 159, 172), new GenericBox(1, 90, 172, 164, 196), new GenericBox(1, 150, 135, 180, 149), new GenericBox(0, 165, 132, 200, 151) }),
            new FrameData(15154, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 126, 126, 145, 142), new GenericBox(1, 100, 130, 173, 150), new GenericBox(1, 101, 150, 158, 168), new GenericBox(1, 89, 166, 165, 196), new GenericBox(0, 171, 132, 201, 153) }),
            new FrameData(15155, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 125, 126, 144, 144), new GenericBox(1, 98, 132, 154, 151), new GenericBox(1, 101, 152, 157, 173), new GenericBox(1, 90, 174, 161, 196) }),
        };

        var airLPFrames = new List<Frame> {
            new FrameData(15216, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 140, 84, 162, 103), new GenericBox(1, 119, 90, 158, 124), new GenericBox(1, 89, 119, 142, 159) }),
            new FrameData(15217, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 138, 74, 170, 106), new GenericBox(1, 115, 92, 155, 128), new GenericBox(1, 90, 115, 137, 157) }),
            new FrameData(15218, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 142, 94, 160, 112), new GenericBox(1, 119, 92, 158, 126), new GenericBox(1, 92, 112, 139, 157), new GenericBox(0, 150, 104, 174, 135) }),
            new FrameData(15219, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(0, 155, 103, 175, 133), new GenericBox(1, 117, 91, 153, 128), new GenericBox(1, 96, 114, 138, 149) }),
            new FrameData(15220, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 96, 112, 139, 149), new GenericBox(1, 118, 91, 153, 123), new GenericBox(0, 153, 102, 176, 134) }),
            new FrameData(15221, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(0, 151, 101, 175, 133), new GenericBox(1, 116, 92, 152, 127), new GenericBox(1, 96, 114, 137, 152) }),
        };

        var MPFrames = new List<Frame> {
            new FrameData(15013, 0, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 100, 100, 146, 153), new GenericBox(1, 119, 89, 140, 106), new GenericBox(1, 85, 102, 111, 126), new GenericBox(1, 137, 102, 162, 129), new GenericBox(1, 91, 143, 154, 195), pushbox}, sound: "golpe_2"),
            new FrameData(15014, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 121, 90, 142, 108), new GenericBox(1, 105, 98, 147, 153), new GenericBox(1, 143, 116, 164, 134), new GenericBox(1, 84, 100, 110, 115), new GenericBox(1, 92, 139, 157, 195), pushbox}),
            new FrameData(15015, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 125, 92, 148, 108), new GenericBox(1, 108, 108, 148, 157), new GenericBox(1, 146, 118, 161, 132), new GenericBox(1, 90, 102, 123, 119), new GenericBox(1, 93, 144, 164, 196), pushbox}),
            new FrameData(15016, 0, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 111, 104, 154, 157), new GenericBox(1, 127, 94, 154, 113), new GenericBox(1, 94, 143, 162, 195), pushbox, new GenericBox(1, 153, 109, 209, 123), new GenericBox(0, 153, 107, 212, 124) }),
            new FrameData(15017, 0, 0, len: 8, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 163, 110, 204, 124), new GenericBox(1, 110, 103, 150, 161), new GenericBox(1, 129, 95, 152, 110), new GenericBox(1, 142, 107, 201, 122), new GenericBox(1, 91, 148, 160, 195), pushbox}),
            new FrameData(15018, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 106, 102, 145, 158), new GenericBox(1, 137, 105, 173, 128), new GenericBox(1, 123, 92, 146, 109), new GenericBox(1, 92, 144, 157, 195) }),
            new FrameData(15019, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 106, 99, 142, 156), new GenericBox(1, 96, 102, 156, 130), new GenericBox(1, 123, 90, 146, 106), new GenericBox(1, 89, 144, 160, 195), pushbox}),
        };

        var lowMPFrames = new List<Frame> {
            new FrameData(15164, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 133, 120, 150, 139), new GenericBox(1, 109, 124, 156, 146), new GenericBox(1, 106, 146, 163, 169), new GenericBox(1, 91, 169, 164, 196) }),
            new FrameData(15165, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 131, 117, 151, 133), new GenericBox(1, 106, 124, 153, 147), new GenericBox(1, 105, 148, 158, 171), new GenericBox(1, 95, 171, 162, 195) }),
            new FrameData(15166, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 129, 107, 150, 125), new GenericBox(1, 104, 112, 154, 137), new GenericBox(1, 104, 138, 156, 166), new GenericBox(1, 96, 166, 158, 195), new GenericBox(0, 151, 125, 179, 155), new GenericBox(0, 143, 155, 169, 181) }),
            new FrameData(15167, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 154, 83, 170, 111), new GenericBox(0, 170, 111, 185, 141), new GenericBox(0, 124, 61, 148, 83), new GenericBox(1, 107, 96, 131, 112), new GenericBox(1, 100, 104, 143, 126), new GenericBox(1, 107, 127, 144, 154), new GenericBox(1, 95, 153, 162, 195) }),
            new FrameData(15168, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 110, 99, 128, 112), new GenericBox(1, 101, 106, 143, 131), new GenericBox(1, 109, 131, 147, 159), new GenericBox(1, 97, 157, 160, 195), new GenericBox(0, 130, 70, 151, 94) }),
            new FrameData(15169, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 97, 131, 114), new GenericBox(1, 98, 111, 145, 130), new GenericBox(1, 104, 129, 145, 159), new GenericBox(1, 93, 159, 161, 195), new GenericBox(0, 131, 72, 153, 91) }),
            new FrameData(15170, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 113, 98, 134, 116), new GenericBox(1, 99, 110, 149, 137), new GenericBox(1, 102, 135, 155, 167), new GenericBox(1, 91, 166, 164, 197) }),
            new FrameData(15171, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 113, 106, 135, 122), new GenericBox(1, 103, 117, 149, 139), new GenericBox(1, 104, 138, 157, 167), new GenericBox(1, 91, 167, 161, 195) }),
            new FrameData(15172, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 111, 132, 125), new GenericBox(1, 97, 121, 147, 150), new GenericBox(1, 99, 149, 156, 174), new GenericBox(1, 87, 173, 163, 196) }),
        };

        var airMPFrames = new List<Frame> {
            new FrameData(15222, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 116, 78, 136, 92), new GenericBox(1, 97, 91, 150, 119), new GenericBox(1, 97, 119, 152, 164) }),
            new FrameData(15233, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 121, 81, 139, 95), new GenericBox(1, 100, 90, 151, 124), new GenericBox(1, 96, 123, 152, 159) }),
            new FrameData(15234, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 130, 83, 147, 100), new GenericBox(1, 106, 92, 144, 123), new GenericBox(1, 90, 123, 151, 161) }),
            new FrameData(15235, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 135, 86, 155, 105), new GenericBox(1, 115, 88, 150, 126), new GenericBox(1, 91, 124, 150, 156) }),
            new FrameData(15236, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(0, 163, 110, 202, 142), new GenericBox(1, 116, 87, 156, 124), new GenericBox(1, 89, 123, 154, 155) }),
            new FrameData(15237, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 116, 89, 156, 123), new GenericBox(1, 87, 122, 154, 154), new GenericBox(0, 163, 110, 199, 141) }),
            new FrameData(15238, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 126, 82, 144, 99), new GenericBox(1, 109, 88, 151, 122), new GenericBox(1, 93, 122, 156, 159) }),
            new FrameData(15239, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 122, 77, 144, 96), new GenericBox(1, 106, 88, 146, 131), new GenericBox(1, 100, 130, 155, 171) }),
        };

        var backMPframes = new List<Frame> {
            new FrameData(15021, 4, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 97, 146, 158), new GenericBox(1, 88, 150, 164, 195) }, sound: "golpe_grito_2"),
            new FrameData(15022, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 105, 96, 142, 160), new GenericBox(1, 84, 116, 109, 135), new GenericBox(1, 84, 149, 163, 194), pushbox }),
            new FrameData(15023, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 113, 92, 136, 111), new GenericBox(1, 103, 103, 146, 159), new GenericBox(1, 85, 145, 161, 196), new GenericBox(1, 85, 110, 114, 127) }),
            new FrameData(15024, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 106, 95, 148, 156), new GenericBox(1, 94, 117, 111, 135), new GenericBox(1, 144, 101, 182, 113), new GenericBox(0, 144, 96, 191, 119), new GenericBox(0, 171, 93, 199, 121), pushbox, new GenericBox(1, 85, 150, 162, 195) }),
            new FrameData(15025, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 166, 92, 196, 116), new GenericBox(1, 95, 96, 153, 143), new GenericBox(1, 143, 99, 189, 112), new GenericBox(1, 82, 142, 161, 193) }),
            new FrameData(15026, 0, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 99, 146, 156), new GenericBox(1, 132, 94, 167, 119), new GenericBox(1, 79, 144, 159, 194), pushbox }),
            new FrameData(15027, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 97, 98, 147, 158), new GenericBox(1, 85, 152, 161, 196) }),
            new FrameData(15028, 0, 0, len: 6, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 106, 95, 128, 114), new GenericBox(1, 91, 110, 143, 162), new GenericBox(1, 85, 150, 162, 194) }),
            new FrameData(15029, -0.5f, 0, len: 8, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 116, 93, 138, 111), new GenericBox(1, 98, 104, 148, 154), new GenericBox(1, 92, 140, 167, 195) }),
        };

        var cl_HPFrames = new List<Frame> {
            new FrameData(15053, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 122, 92, 141, 110), new GenericBox(1, 93, 104, 146, 145), new GenericBox(1, 146, 107, 162, 119), new GenericBox(1, 100, 145, 156, 195) }, sound: "golpe_3"),
            new FrameData(15054, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 125, 94, 143, 110), new GenericBox(1, 106, 106, 143, 159), new GenericBox(1, 98, 153, 154, 195), new GenericBox(1, 144, 106, 158, 120), new GenericBox(1, 93, 115, 106, 148) }, sound: "golpe_grito_3"),
            new FrameData(15055, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 130, 92, 145, 109), new GenericBox(1, 109, 103, 145, 155), new GenericBox(1, 100, 151, 153, 195) }),
            new FrameData(15056, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 130, 91, 150, 108), new GenericBox(1, 113, 103, 151, 153), new GenericBox(1, 151, 113, 170, 150), new GenericBox(1, 102, 108, 115, 125), new GenericBox(1, 98, 148, 154, 195) }),
            new FrameData(15057, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 165, 100, 185, 125), new GenericBox(1, 129, 89, 149, 107), new GenericBox(1, 113, 102, 154, 152), new GenericBox(1, 102, 110, 120, 129), new GenericBox(1, 100, 148, 155, 195) }),
            new FrameData(15058, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 144, 76, 166, 116), new GenericBox(1, 125, 90, 144, 107), new GenericBox(1, 115, 105, 150, 156), new GenericBox(1, 97, 148, 154, 195) }),
            new FrameData(15059, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 80, 152, 153), new GenericBox(1, 96, 143, 154, 195), new GenericBox(1, 102, 116, 114, 129) }),
            new FrameData(15060, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 122, 90, 141, 107), new GenericBox(1, 140, 76, 165, 120), new GenericBox(1, 113, 104, 149, 153), new GenericBox(1, 95, 146, 154, 195) }),
            new FrameData(15061, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 142, 73, 164, 115), new GenericBox(1, 120, 88, 140, 104), new GenericBox(1, 109, 104, 146, 156), new GenericBox(1, 95, 150, 154, 195) }),
            new FrameData(15062, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 121, 87, 138, 104), new GenericBox(1, 141, 79, 161, 116), new GenericBox(1, 107, 102, 144, 154), new GenericBox(1, 95, 146, 152, 195) }),
            new FrameData(15063, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 121, 87, 138, 104), new GenericBox(1, 105, 99, 146, 153), new GenericBox(1, 145, 98, 162, 122), new GenericBox(1, 96, 148, 153, 195) }),
            new FrameData(15064, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 122, 89, 138, 103), new GenericBox(1, 104, 101, 143, 153), new GenericBox(1, 143, 110, 158, 130) }),
            new FrameData(15065, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 144, 151, 195), new GenericBox(1, 118, 92, 137, 108), new GenericBox(1, 101, 102, 147, 145) }),
            new FrameData(15066, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 120, 92, 135, 105), new GenericBox(1, 100, 102, 144, 151), new GenericBox(1, 93, 146, 153, 195) }),
        };

        var LKFrames = new List<Frame> {
            new FrameData(15104, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 124, 91, 145, 109), new GenericBox(1, 104, 96, 141, 155), new GenericBox(1, 139, 114, 156, 131), new GenericBox(1, 93, 144, 155, 195), pushbox}, sound: "golpe_1"),
            new FrameData(15105, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 127, 92, 149, 107), new GenericBox(1, 105, 100, 153, 153), new GenericBox(1, 124, 152, 161, 193), new GenericBox(1, 149, 129, 171, 155) }),
            new FrameData(15106, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 125, 91, 149, 105), new GenericBox(1, 107, 100, 153, 147), new GenericBox(1, 121, 147, 157, 194), new GenericBox(1, 153, 129, 172, 157), pushbox, new GenericBox(0, 186, 154, 207, 181), new GenericBox(0, 171, 139, 187, 169) }),
            new FrameData(15107, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 126, 91, 149, 105), new GenericBox(1, 104, 97, 155, 146), new GenericBox(1, 123, 140, 157, 193), new GenericBox(1, 151, 129, 174, 160) }),
            new FrameData(15108, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 128, 91, 148, 109), new GenericBox(1, 104, 99, 154, 150), new GenericBox(1, 127, 130, 163, 194), pushbox}),
            new FrameData(15109, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 128, 90, 148, 106), new GenericBox(1, 107, 99, 156, 154), new GenericBox(1, 108, 153, 159, 193) }),
            new FrameData(15110, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 122, 90, 141, 107), new GenericBox(1, 100, 97, 139, 153), new GenericBox(1, 137, 105, 156, 132), new GenericBox(1, 97, 143, 155, 195), pushbox}),
        };

        var lowLKFrames = new List<Frame> {
            new FrameData(15189, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 120, 124, 138, 141), new GenericBox(1, 97, 132, 151, 152), new GenericBox(1, 98, 153, 157, 175), new GenericBox(1, 85, 175, 164, 197) }),
            new FrameData(15190, 2.75f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 119, 122, 138, 139), new GenericBox(1, 98, 129, 155, 151), new GenericBox(1, 101, 151, 164, 175), new GenericBox(1, 76, 174, 163, 195) }),
            new FrameData(15191, 2.25f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 125, 130, 142), new GenericBox(1, 97, 134, 156, 157), new GenericBox(1, 96, 157, 169, 176), new GenericBox(1, 87, 176, 169, 191) }),
            new FrameData(15192, 0.75f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 137, 129, 148), new GenericBox(1, 89, 146, 140, 168), new GenericBox(1, 102, 168, 160, 191), new GenericBox(1, 161, 178, 199, 192), new GenericBox(0, 193, 167, 227, 197) }),
            new FrameData(15193, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 104, 134, 125, 149), new GenericBox(1, 88, 143, 147, 168), new GenericBox(1, 101, 168, 162, 193), new GenericBox(1, 161, 176, 189, 193), new GenericBox(0, 188, 169, 215, 196) }),
            new FrameData(15194, 0.0f, 0.0f, len: 6, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 103, 134, 126, 150), new GenericBox(1, 87, 143, 156, 169), new GenericBox(1, 102, 169, 167, 193), new GenericBox(1, 166, 178, 197, 195) }),
            new FrameData(15195,-0.75f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 127, 130, 145), new GenericBox(1, 93, 137, 141, 160), new GenericBox(1, 103, 159, 166, 179), new GenericBox(1, 100, 178, 167, 195) }),
            new FrameData(15190,-2.75f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 119, 122, 138, 139), new GenericBox(1, 98, 129, 155, 151), new GenericBox(1, 101, 151, 164, 175), new GenericBox(1, 76, 174, 163, 195) }),
            new FrameData(15205, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { new GenericBox(1, 117, 117, 140, 134), new GenericBox(1, 95, 126, 154, 151), new GenericBox(1, 81, 150, 165, 173), new GenericBox(1, 71, 173, 163, 197) }),
        };

        var airLKFrames = new List<Frame> {
            new FrameData(15269, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 119, 83, 142, 101), new GenericBox(1, 100, 90, 155, 153) }),
            new FrameData(15270, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 121, 85, 143, 101), new GenericBox(1, 102, 91, 156, 150) }),
            new FrameData(15271, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 127, 86, 147, 105), new GenericBox(1, 100, 89, 144, 146), new GenericBox(1, 145, 98, 173, 143), new GenericBox(0, 168, 104, 215, 154) }),
            new FrameData(15272, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(0, 165, 105, 210, 153), new GenericBox(1, 137, 96, 169, 142), new GenericBox(1, 95, 89, 142, 143) }),
            new FrameData(15273, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 95, 86, 142, 143), new GenericBox(1, 139, 94, 170, 143), new GenericBox(0, 166, 105, 208, 149) }),
            new FrameData(15274, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(0, 168, 105, 208, 150), new GenericBox(1, 138, 96, 169, 146), new GenericBox(1, 94, 87, 147, 144) }),
            new FrameData(15275, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 104, 86, 161, 122), new GenericBox(1, 105, 122, 184, 162) }),
        };

        var MKFrames = new List<Frame> {
            new FrameData(15136, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 117, 92, 137, 109), new GenericBox(1, 103, 102, 141, 123), new GenericBox(1, 106, 123, 142, 152), new GenericBox(1, 89, 151, 153, 195) }),
            new FrameData(15137, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 118, 93, 137, 106), new GenericBox(1, 104, 103, 142, 118), new GenericBox(1, 108, 117, 140, 144), new GenericBox(1, 83, 145, 157, 195) }),
            new FrameData(15138, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 116, 92, 140, 108), new GenericBox(1, 103, 100, 141, 123), new GenericBox(1, 107, 123, 143, 151), new GenericBox(1, 98, 151, 154, 194) }),
            new FrameData(15072, 8, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 102, 143, 150), new GenericBox(1, 82, 107, 100, 132), new GenericBox(1, 138, 124, 170, 150), new GenericBox(1, 114, 143, 148, 193), new GenericBox(1, 106, 92, 131, 106), pushbox}, sound: "golpe_3"),
            new FrameData(15073, 2, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 102, 93, 121, 106), new GenericBox(1, 91, 104, 132, 120), new GenericBox(1, 109, 122, 142, 144), new GenericBox(1, 112, 145, 136, 194), new GenericBox(0, 177, 96, 198, 114), new GenericBox(0, 160, 109, 182, 126), new GenericBox(0, 144, 120, 163, 137) }),
            new FrameData(15074, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 186, 95, 219, 120), new GenericBox(0, 162, 106, 189, 129), new GenericBox(0, 145, 117, 171, 140), pushbox , new GenericBox(1, 91, 91, 148, 140), new GenericBox(1, 112, 140, 139, 194), new GenericBox(1, 70, 120, 92, 140) }),
            new FrameData(15075, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 174, 118, 204, 144), new GenericBox(0, 145, 123, 174, 146), pushbox, new GenericBox(1, 93, 94, 148, 139), new GenericBox(1, 112, 137, 144, 193), new GenericBox(1, 79, 128, 101, 148) }),
            new FrameData(15076, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 91, 131, 105), new GenericBox(1, 94, 96, 154, 140), new GenericBox(1, 135, 138, 176, 163), new GenericBox(1, 107, 141, 140, 193), pushbox}),
            new FrameData(15077, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 92, 149, 145), new GenericBox(1, 106, 139, 144, 194) }),
            new FrameData(15078, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 102, 88, 124, 106), new GenericBox(1, 90, 100, 150, 132), new GenericBox(1, 100, 133, 141, 194), pushbox}),
            new FrameData(15079, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 89, 131, 107), new GenericBox(1, 92, 101, 144, 148), new GenericBox(1, 96, 148, 145, 195) }),
            new FrameData(15080, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 117, 88, 137, 105), new GenericBox(1, 108, 100, 142, 151), new GenericBox(1, 95, 100, 153, 130), new GenericBox(1, 98, 143, 156, 195), pushbox}),
        };

        var lowMKFrames = new List<Frame> {
            new FrameData(15196, 6.25f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 113, 119, 137, 135), new GenericBox(1, 102, 129, 152, 153), new GenericBox(1, 100, 154, 159, 177), new GenericBox(1, 61, 175, 161, 196) }),
            new FrameData(15197, 0.75f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 119, 122, 138, 135), new GenericBox(1, 104, 130, 150, 155), new GenericBox(1, 105, 156, 159, 175), new GenericBox(1, 105, 175, 160, 193) }),
            new FrameData(15198, 0.25f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 123, 136, 137), new GenericBox(1, 102, 134, 147, 155), new GenericBox(1, 103, 155, 154, 173), new GenericBox(1, 97, 173, 170, 193), new GenericBox(0, 159, 171, 215, 193) }),
            new FrameData(15199, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 125, 133, 139), new GenericBox(1, 100, 134, 141, 153), new GenericBox(1, 103, 154, 147, 170), new GenericBox(1, 100, 170, 169, 193) }),
            new FrameData(15200, 1.25f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 112, 121, 132, 138), new GenericBox(1, 97, 131, 149, 153), new GenericBox(1, 96, 152, 175, 177), new GenericBox(1, 102, 176, 152, 195) }),
            new FrameData(15201, 0.0f, 0.0f, len: 5, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 120, 130, 146, 149), new GenericBox(1, 102, 136, 153, 160), new GenericBox(1, 98, 159, 168, 179), new GenericBox(1, 101, 178, 141, 195) }),
            new FrameData(15202, -0.75f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 124, 134, 143, 151), new GenericBox(1, 101, 137, 157, 159), new GenericBox(1, 99, 159, 171, 178), new GenericBox(1, 101, 178, 153, 196) }),
            new FrameData(15203, 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 105, 123, 130, 141), new GenericBox(1, 93, 131, 155, 153), new GenericBox(1, 96, 153, 160, 173), new GenericBox(1, 104, 174, 152, 195) }),
            new FrameData(15204, -2.5f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 119, 133, 137), new GenericBox(1, 93, 128, 158, 154), new GenericBox(1, 97, 154, 163, 175), new GenericBox(1, 101, 174, 157, 193) }),
            new FrameData(15205, -2f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 118, 117, 140, 135), new GenericBox(1, 101, 125, 149, 148), new GenericBox(1, 90, 147, 163, 171), new GenericBox(1, 75, 169, 161, 197) }),
        };

        var airMKFrames = new List<Frame> {
            new FrameData(15276, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 122, 83, 142, 99), new GenericBox(1, 101, 92, 146, 124), new GenericBox(1, 104, 124, 154, 174) }),
            new FrameData(15277, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 117, 81, 136, 96), new GenericBox(1, 102, 89, 160, 122), new GenericBox(1, 103, 121, 170, 156) }),
            new FrameData(15278, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 109, 78, 130, 94), new GenericBox(1, 101, 89, 140, 122), new GenericBox(1, 101, 118, 204, 148) }),
            new FrameData(15279, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 110, 78, 130, 94), new GenericBox(1, 100, 89, 136, 123), new GenericBox(1, 102, 115, 184, 147), new GenericBox(0, 181, 112, 224, 151) }),
            new FrameData(15280, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(0, 182, 112, 225, 148), new GenericBox(1, 111, 78, 132, 95), new GenericBox(1, 101, 89, 139, 121), new GenericBox(1, 102, 116, 183, 144) }),
            new FrameData(15281, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 114, 79, 131, 95), new GenericBox(1, 102, 90, 138, 119), new GenericBox(1, 105, 114, 168, 147) }),
            new FrameData(15282, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 118, 80, 136, 98), new GenericBox(1, 100, 92, 142, 123), new GenericBox(1, 106, 121, 165, 163) }),
        };

        var BackMKFrames = new List<Frame> {
            new FrameData(15118, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 110, 86, 134, 103), new GenericBox(1, 100, 98, 141, 147), new GenericBox(1, 106, 132, 152, 193), new GenericBox(1, 88, 101, 157, 127), pushbox}, sound:"golpe_3"),
            new FrameData(15119, 3, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 76, 88, 154, 128), new GenericBox(1, 106, 116, 159, 194) }, sound: "golpe_grito_5"),
            new FrameData(15120, 5, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 106, 59, 137, 193), new GenericBox(1, 88, 91, 122, 129), new GenericBox(1, 68, 107, 95, 130), pushbox}),
            new FrameData(15121, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 64, 140, 193), new GenericBox(1, 85, 89, 143, 134), new GenericBox(1, 75, 108, 93, 127) }),
            new FrameData(15122, 0, 0, len: 6, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 67, 144, 193), pushbox, new GenericBox(1, 86, 92, 147, 134) }),
            new FrameData(15123, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 140, 68, 170, 97), new GenericBox(1, 94, 90, 151, 140), new GenericBox(1, 119, 140, 146, 193) }),            
            new FrameData(15124, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 164, 101, 211, 129), new GenericBox(0, 144, 113, 171, 138), new GenericBox(1, 93, 92, 148, 145), new GenericBox(1, 81, 111, 105, 147), new GenericBox(1, 116, 137, 151, 193), pushbox}),
            new FrameData(15125, -1, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 176, 132, 222, 160), new GenericBox(0, 152, 126, 183, 150), new GenericBox(1, 103, 95, 154, 149), new GenericBox(1, 122, 141, 162, 193), new GenericBox(1, 66, 111, 102, 148), pushbox}, hasHit: false),
            new FrameData(15126, -3, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 165, 149, 191, 190), new GenericBox(1, 109, 95, 163, 152), new GenericBox(1, 79, 112, 109, 150), new GenericBox(1, 131, 150, 163, 193), pushbox}, hasHit: false),
            new FrameData(15127, -4, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 106, 98, 148, 148), new GenericBox(1, 122, 91, 148, 109), new GenericBox(1, 140, 101, 164, 126), new GenericBox(1, 86, 102, 115, 142), new GenericBox(1, 111, 136, 160, 194) }),
            new FrameData(15128, -3, 0, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 106, 99, 147, 156), new GenericBox(1, 120, 89, 144, 107), new GenericBox(1, 94, 101, 163, 133), new GenericBox(1, 93, 145, 160, 194) }),
        };

        // Movement
        var walkingForwardFrames = new List<Frame> {
            new FrameData("14671", this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 119, 87, 140, 104) }),
            new FrameData("14672", this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 120, 88, 139, 107) }),
            new FrameData("14673", this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 119, 89, 140, 106) }),
            new FrameData("14674", this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 118, 85, 139, 103) }),
            new FrameData("14675", this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 115, 85, 141, 105) }),
            new FrameData("14676", this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 117, 84, 140, 102) }),
            new FrameData("14677", this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 118, 82, 139, 101) }),
            new FrameData("14678", this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 117, 82, 140, 102) }),
            new FrameData("14679", this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 119, 82, 139, 101) }),
            new FrameData("14680", this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 118, 82, 141, 105) }),
            new FrameData("14681", this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 114, 84, 144, 109) }),
        };

        var walkingBackwardFrames = new List<Frame> {
            new FrameData("14683", -this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 118, 87, 142, 107) }),
            new FrameData("14684", -this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 120, 87, 140, 106) }),
            new FrameData("14685", -this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 121, 88, 142, 106) }),
            new FrameData("14686", -this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 119, 85, 141, 103) }),
            new FrameData("14687", -this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 120, 84, 142, 104) }),
            new FrameData("14688", -this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 119, 83, 142, 101) }),
            new FrameData("14689", -this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 119, 83, 142, 100) }),
            new FrameData("14690", -this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 120, 82, 141, 101) }),
            new FrameData("14691", -this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 120, 81, 142, 103) }),
            new FrameData("14692", -this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 119, 84, 142, 102) }),
            new FrameData("14693", -this.move_speed, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195), new GenericBox(1, 119, 84, 142, 102) }),
        };

        var dashForwardFrames = new List<Frame> {
            new FrameData(14697,  0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData(14768, 12, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData(14768,  8, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData(14769,  8, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData(14769, 12, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData(14770,  8, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData(14771,  0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData(14772,  0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
        };

        var dashBackwardFrames = new List<Frame> {
            new FrameData(14697,  0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData(14774, -4, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData(14775,-20, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData(14776, -2, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData(14777,  0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
            new FrameData(14778,  0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 95, 95, 155, 195)}),
        };

        var landingFrames = new List<Frame> {
            new FrameData(14698, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 122, 130, 139, 143), new GenericBox(1, 100, 133, 140, 150), new GenericBox(1, 103, 150, 157, 169), new GenericBox(1, 94, 169, 160, 196) }),
            new FrameData(14697, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 125, 109, 144, 124), new GenericBox(1, 101, 117, 145, 137), new GenericBox(1, 106, 138, 141, 160), new GenericBox(1, 92, 158, 160, 194) }),
        };

        var jumpFrames = new List<Frame> {
            new FrameData(14720, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 99, 85, 149, 155)}, sound: "golpe_2"),
            new FrameData(14721, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 99, 85, 149, 155)}),
            new FrameData(14722, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 99, 85, 149, 155)}),
            new FrameData(14723, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 99, 85, 149, 155)}),
            new FrameData(14724, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 99, 85, 149, 155)}),
            new FrameData(14725, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 99, 85, 149, 155)}),
            new FrameData(14726, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 99, 85, 149, 155)}),
            new FrameData(14727, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 99, 85, 149, 155)}),
            new FrameData(14728, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 99, 85, 149, 155)}),
            new FrameData(14729, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 99, 85, 149, 155)}),
            new FrameData(14730, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 99, 85, 149, 155)}),
        };

        var jumpForward = new List<Frame> {
            new FrameData(14732, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 126, 82, 143, 99), new GenericBox(1, 96, 95, 144, 123), new GenericBox(1, 105, 123, 155, 158), new GenericBox(1, 101, 156, 135, 190) }, sound: "golpe_2"),
            new FrameData(14733, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 149, 96, 165, 113), new GenericBox(1, 102, 97, 158, 145), new GenericBox(1, 95, 145, 124, 180) }),
            new FrameData(14734, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 144, 115, 164, 138), new GenericBox(1, 95, 101, 152, 150) }),
            new FrameData(14735, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 93, 104, 155, 147) }),
            new FrameData(14736, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 96, 99, 153, 149) }),
            new FrameData(14737, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 90, 97, 147, 148) }),
            new FrameData(14738, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 88, 96, 149, 145) }),
            new FrameData(14739, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 84, 91, 136, 141), new GenericBox(1, 136, 107, 162, 168) }),
            new FrameData(14740, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 114, 82, 135, 99), new GenericBox(1, 93, 90, 137, 125), new GenericBox(1, 107, 115, 155, 144), new GenericBox(1, 120, 143, 152, 191) }),
            new FrameData(14741, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 119, 80, 138, 98), new GenericBox(1, 93, 94, 140, 124), new GenericBox(1, 107, 123, 149, 185) }),
        };

        var jumpFallingFrames = new List<Frame> {
            new FrameData(14744, 0, 0, len: 3, boxes: new List<GenericBox> {pushbox, new GenericBox(1, 121, 80, 141, 96), new GenericBox(1, 100, 89, 142, 124), new GenericBox(1, 106, 123, 154, 161), new GenericBox(1, 105, 160, 138, 192) }),
            new FrameData(14743, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 117, 79, 137, 95), new GenericBox(1, 95, 89, 138, 122), new GenericBox(1, 107, 122, 155, 149), new GenericBox(1, 106, 149, 139, 193) }),
            new FrameData(14742, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 119, 78, 137, 93), new GenericBox(1, 95, 89, 142, 125), new GenericBox(1, 109, 126, 152, 158), new GenericBox(1, 109, 159, 138, 194) }),
        };

        var JumpBackward = new List<Frame> {
            new FrameData(14741, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 119, 80, 138, 98), new GenericBox(1, 93, 94, 140, 124), new GenericBox(1, 107, 123, 149, 185) }, sound: "golpe_2"),
            new FrameData(14740, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 114, 82, 135, 99), new GenericBox(1, 93, 90, 137, 125), new GenericBox(1, 107, 115, 155, 144), new GenericBox(1, 120, 143, 152, 191) }),
            new FrameData(14739, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 84, 91, 136, 141), new GenericBox(1, 136, 107, 162, 168) }),
            new FrameData(14738, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 88, 96, 149, 145) }),
            new FrameData(14737, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 90, 97, 147, 148) }),
            new FrameData(14736, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 96, 99, 153, 149) }),
            new FrameData(14735, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 93, 104, 155, 147) }),
            new FrameData(14734, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 144, 115, 164, 138), new GenericBox(1, 95, 101, 152, 150) }),
            new FrameData(14733, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 149, 96, 165, 113), new GenericBox(1, 102, 97, 158, 145), new GenericBox(1, 95, 145, 124, 180) }),
            new FrameData(14732, 0, 0, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 126, 82, 143, 99), new GenericBox(1, 96, 95, 144, 123), new GenericBox(1, 105, 123, 155, 158), new GenericBox(1, 101, 156, 135, 190) })
        };

        var crouchingInFrames = new List<Frame> {
            new FrameData(14696, 0, 0, new List<GenericBox> { pushbox, new GenericBox(1, 102, 99, 143, 154), new GenericBox(1, 132, 105, 156, 134), new GenericBox(1, 94, 101, 130, 122), new GenericBox(1, 117, 89, 140, 107), new GenericBox(1, 90, 137, 158, 195), pushbox }),
            new FrameData(14697, 0, 0, new List<GenericBox> { pushbox, new GenericBox(1, 103, 115, 147, 164), new GenericBox(1, 98, 116, 135, 145), new GenericBox(1, 134, 120, 159, 152), new GenericBox(1, 91, 150, 162, 195), pushbox }),
            new FrameData(14698, 0, 0, new List<GenericBox> { pushbox, new GenericBox(1, 98, 132, 163, 196), pushbox}),
            new FrameData(14699, 0, 0, new List<GenericBox> { pushbox, new GenericBox(1, 96, 131, 160, 195), pushbox}),
        };

        var crouchingFrames = new List<Frame> {
            new FrameData(14704, 0, 0, len: 15, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 131, 160, 195), pushbox}), // 0
            new FrameData(14705, 0, 0, len: 15, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 131, 160, 195), pushbox}), // 1
            new FrameData(14706, 0, 0, len: 15, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 131, 160, 195), pushbox}), // 2
            new FrameData(14707, 0, 0, len: 15, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 131, 160, 195), pushbox}), // 3
        };

        // Specials
        var hadukenFrames = new List<Frame> {
            new FrameData(15328, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 128, 95, 146, 109), new GenericBox(1, 103, 103, 153, 125), new GenericBox(1, 102, 126, 152, 151), new GenericBox(1, 89, 153, 173, 195) }, sound: "haduken"),
            new FrameData(15329, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 134, 105, 149, 119), new GenericBox(1, 106, 108, 145, 134), new GenericBox(1, 99, 134, 149, 159), new GenericBox(1, 77, 158, 170, 195) }),
            new FrameData(15330, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 137, 106, 152, 121), new GenericBox(1, 107, 107, 153, 131), new GenericBox(1, 104, 132, 142, 157), new GenericBox(1, 76, 156, 164, 195) }),
            new FrameData(15331, 0.0f, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 139, 110, 155, 123), new GenericBox(1, 108, 114, 175, 137), new GenericBox(1, 105, 137, 149, 163), new GenericBox(1, 73, 163, 164, 195) }),
            new FrameData(15332, 0.0f, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 134, 109, 156, 127), new GenericBox(1, 109, 113, 182, 139), new GenericBox(1, 100, 139, 150, 162), new GenericBox(1, 71, 162, 159, 195) }),
            new FrameData(15333, 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 135, 109, 152, 123), new GenericBox(1, 106, 112, 177, 139), new GenericBox(1, 101, 139, 162, 159), new GenericBox(1, 70, 159, 163, 196) }),
            new FrameData(15334, 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 134, 103, 151, 120), new GenericBox(1, 107, 115, 176, 134), new GenericBox(1, 107, 134, 163, 161), new GenericBox(1, 71, 161, 163, 195) }),
            new FrameData(15335, 0.0f, 0.0f, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 129, 101, 152, 122), new GenericBox(1, 111, 114, 174, 137), new GenericBox(1, 102, 136, 167, 162), new GenericBox(1, 70, 161, 167, 196) }),
            new FrameData(15336, 0.0f, 0.0f, len: 6, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 129, 102, 151, 121), new GenericBox(1, 112, 113, 172, 135), new GenericBox(1, 107, 134, 166, 162), new GenericBox(1, 71, 161, 167, 195) }),
            new FrameData(15337, 0.0f, 0.0f, len: 10, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 133, 105, 150, 119), new GenericBox(1, 111, 115, 174, 134), new GenericBox(1, 108, 135, 166, 162), new GenericBox(1, 71, 161, 166, 195) }),
            new FrameData(15338, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 131, 102, 150, 118), new GenericBox(1, 107, 108, 152, 129), new GenericBox(1, 102, 129, 146, 158), new GenericBox(1, 72, 160, 167, 195) }),
            new FrameData(15339, 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 130, 92, 147, 108), new GenericBox(1, 105, 99, 153, 119), new GenericBox(1, 105, 119, 144, 153), new GenericBox(1, 91, 154, 155, 195) }),
        };

        var lightShoryFrames = new List<Frame> {
            new FrameData(15344, 0, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 144, 122, 161, 140), new GenericBox(1, 107, 130, 151, 167), new GenericBox(1, 107, 155, 162, 193) }),
            new FrameData(15345, 0, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 139, 116, 160, 136), new GenericBox(1, 107, 120, 155, 164), new GenericBox(1, 98, 164, 155, 195) }, sound: "shory"),
            new FrameData(15346, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 154, 125, 177, 143), new GenericBox(0, 153, 153, 170, 171), new GenericBox(1, 109, 108, 151, 152), new GenericBox(1, 109, 153, 152, 172), new GenericBox(1, 97, 171, 151, 195) }, hasHit: false),
            new FrameData(15347, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 74, 127, 90), new GenericBox(1, 102, 83, 141, 102), new GenericBox(1, 109, 103, 144, 127), new GenericBox(1, 113, 128, 133, 161), new GenericBox(0, 134, 35, 151, 72), new GenericBox(0, 144, 97, 173, 145)}),
            new FrameData(15348, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 83, 127, 97), new GenericBox(1, 101, 97, 145, 130), new GenericBox(1, 111, 124, 151, 152), new GenericBox(1, 110, 152, 133, 187) }),
            new FrameData(15349, 0, 0, len: 5, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 83, 127, 97), new GenericBox(1, 101, 97, 145, 130), new GenericBox(1, 111, 124, 151, 152), new GenericBox(1, 110, 152, 133, 187) }),
        };

        var heavyShoryFrames = new List<Frame> {
            new FrameData(15344, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 144, 122, 161, 140), new GenericBox(1, 107, 130, 151, 167), new GenericBox(1, 107, 155, 162, 193) }),
            new FrameData(15345, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 139, 116, 160, 136), new GenericBox(1, 107, 120, 155, 164), new GenericBox(1, 98, 164, 155, 195) }, sound: "shory"),
            new FrameData(15346, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 154, 125, 177, 143), new GenericBox(0, 153, 153, 170, 171), new GenericBox(1, 109, 108, 151, 152), new GenericBox(1, 109, 153, 152, 172), new GenericBox(1, 97, 171, 151, 195) }, hasHit: false),
            new FrameData(15347, 0, 0, len: 5, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 74, 127, 90), new GenericBox(1, 102, 83, 141, 102), new GenericBox(1, 109, 103, 144, 127), new GenericBox(1, 113, 128, 133, 161), new GenericBox(0, 134, 35, 151, 72), new GenericBox(0, 144, 97, 173, 145) }, hasHit: false),
            new FrameData(15348, 0, 0, len: 5, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 132, 47, 153, 71), new GenericBox(0, 151, 119, 164, 136), new GenericBox(1, 109, 82, 128, 99), new GenericBox(1, 100, 94, 147, 120), new GenericBox(1, 108, 120, 149, 146), new GenericBox(1, 109, 146, 133, 191) }),
            new FrameData(15349, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 83, 127, 97), new GenericBox(1, 101, 97, 145, 130), new GenericBox(1, 111, 124, 151, 152), new GenericBox(1, 110, 152, 133, 187) }),
        };

        var EXShoryFrames = new List<Frame> {
            new FrameData(15344, 0, 0, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 144, 122, 161, 140), new GenericBox(1, 107, 130, 151, 167), new GenericBox(1, 107, 155, 162, 193) }),
            new FrameData(15345, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 139, 116, 160, 136), new GenericBox(1, 107, 120, 155, 164), new GenericBox(1, 98, 164, 155, 195) }, sound: "shory"),
            new FrameData(15346, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 154, 125, 177, 143), new GenericBox(0, 153, 153, 170, 171), new GenericBox(1, 109, 108, 151, 152), new GenericBox(1, 109, 153, 152, 172), new GenericBox(1, 97, 171, 151, 195) }, hasHit: false),
            new FrameData(15347, 0, 0, len: 8, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 74, 127, 90), new GenericBox(1, 102, 83, 141, 102), new GenericBox(1, 109, 103, 144, 127), new GenericBox(1, 113, 128, 133, 161), new GenericBox(0, 134, 35, 151, 72), new GenericBox(0, 144, 97, 173, 145) }, hasHit: false),
            new FrameData(15348, 0, 0, len: 8, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 132, 47, 153, 71), new GenericBox(0, 151, 119, 164, 136), new GenericBox(1, 109, 82, 128, 99), new GenericBox(1, 100, 94, 147, 120), new GenericBox(1, 108, 120, 149, 146), new GenericBox(1, 109, 146, 133, 191) }),
            new FrameData(15349, 0, 0, len: 5, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 83, 127, 97), new GenericBox(1, 101, 97, 145, 130), new GenericBox(1, 111, 124, 151, 152), new GenericBox(1, 110, 152, 133, 187) }),
        };

        var shoryFallingFrames = new List<Frame> {
            new FrameData(15350, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 104, 83, 125, 99), new GenericBox(1, 100, 91, 141, 127), new GenericBox(1, 107, 127, 144, 157), new GenericBox(1, 112, 157, 137, 186) }),
            new FrameData(15351, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 105, 85, 123, 99), new GenericBox(1, 105, 90, 141, 128), new GenericBox(1, 105, 127, 143, 151), new GenericBox(1, 108, 151, 141, 177) }),
            new FrameData(15352, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 83, 128, 98), new GenericBox(1, 103, 93, 142, 135), new GenericBox(1, 87, 134, 136, 157), new GenericBox(1, 104, 157, 137, 175) }),
            new FrameData(15353, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 95, 142, 130), new GenericBox(1, 111, 83, 129, 96), new GenericBox(1, 88, 130, 135, 154), new GenericBox(1, 103, 155, 137, 176) }),
            new FrameData(15354, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 112, 84, 129, 99), new GenericBox(1, 105, 96, 147, 129), new GenericBox(1, 91, 130, 141, 155), new GenericBox(1, 93, 155, 138, 182) }),
            new FrameData(15355, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 112, 84, 129, 99), new GenericBox(1, 105, 96, 147, 129), new GenericBox(1, 91, 130, 141, 155), new GenericBox(1, 93, 155, 138, 182) }),
        };

        var heavyTatsoFrames = new List<Frame> {
            new FrameData(15356, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 122, 88, 138, 102), new GenericBox(1, 108, 99, 143, 122), new GenericBox(1, 105, 121, 143, 143), new GenericBox(1, 81, 145, 148, 195) }, sound: "tatso", hasHit: false),
            new FrameData(15357, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 110, 89, 127, 107), new GenericBox(1, 97, 103, 135, 123), new GenericBox(1, 111, 123, 162, 149), new GenericBox(1, 114, 148, 139, 194) }),
            new FrameData(15358, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 82, 126, 95), new GenericBox(1, 103, 93, 140, 115), new GenericBox(1, 110, 115, 141, 138), new GenericBox(1, 97, 137, 150, 170) }),
            new FrameData(15359, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 82, 131, 98), new GenericBox(1, 102, 93, 140, 112), new GenericBox(1, 109, 112, 140, 136), new GenericBox(1, 109, 136, 136, 158) }),

            new FrameData(15456, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 79, 133, 96), new GenericBox(1, 100, 90, 137, 109), new GenericBox(1, 102, 109, 134, 136), new GenericBox(1, 99, 136, 120, 174) }, hasHit: false),
            new FrameData(15457, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 80, 136, 98), new GenericBox(1, 100, 90, 139, 115), new GenericBox(1, 109, 115, 176, 134), new GenericBox(1, 99, 134, 130, 169), new GenericBox(0, 143, 113, 197, 137) }),
            new FrameData(15458, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 81, 128, 95), new GenericBox(1, 93, 91, 142, 111), new GenericBox(1, 102, 111, 168, 136), new GenericBox(1, 101, 136, 132, 168), new GenericBox(0, 154, 116, 197, 137) }),
            new FrameData(15459, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 104, 80, 127, 96), new GenericBox(1, 92, 93, 139, 111), new GenericBox(1, 102, 111, 146, 135), new GenericBox(1, 111, 135, 136, 167) }),
            new FrameData(15460, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 81, 127, 96), new GenericBox(1, 100, 94, 137, 118), new GenericBox(1, 67, 117, 134, 138), new GenericBox(1, 107, 138, 136, 168), new GenericBox(0, 43, 113, 86, 138) }),
            new FrameData(15461, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 111, 79, 133, 97), new GenericBox(1, 100, 89, 143, 112), new GenericBox(1, 78, 113, 134, 137), new GenericBox(1, 102, 136, 129, 165), new GenericBox(0, 55, 112, 100, 138) }),

            new FrameData(15456, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 79, 133, 96), new GenericBox(1, 100, 90, 137, 109), new GenericBox(1, 102, 109, 134, 136), new GenericBox(1, 99, 136, 120, 174) }, hasHit: false),
            new FrameData(15457, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 80, 136, 98), new GenericBox(1, 100, 90, 139, 115), new GenericBox(1, 109, 115, 176, 134), new GenericBox(1, 99, 134, 130, 169), new GenericBox(0, 143, 113, 197, 137) }),
            new FrameData(15458, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 81, 128, 95), new GenericBox(1, 93, 91, 142, 111), new GenericBox(1, 102, 111, 168, 136), new GenericBox(1, 101, 136, 132, 168), new GenericBox(0, 154, 116, 197, 137) }),
            new FrameData(15459, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 104, 80, 127, 96), new GenericBox(1, 92, 93, 139, 111), new GenericBox(1, 102, 111, 146, 135), new GenericBox(1, 111, 135, 136, 167) }),
            new FrameData(15460, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 81, 127, 96), new GenericBox(1, 100, 94, 137, 118), new GenericBox(1, 67, 117, 134, 138), new GenericBox(1, 107, 138, 136, 168), new GenericBox(0, 43, 113, 86, 138) }),
            new FrameData(15461, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 111, 79, 133, 97), new GenericBox(1, 100, 89, 143, 112), new GenericBox(1, 78, 113, 134, 137), new GenericBox(1, 102, 136, 129, 165), new GenericBox(0, 55, 112, 100, 138) }),

            new FrameData(15456, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 79, 133, 96), new GenericBox(1, 100, 90, 137, 109), new GenericBox(1, 102, 109, 134, 136), new GenericBox(1, 99, 136, 120, 174) }, hasHit: false),
            new FrameData(15457, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 80, 136, 98), new GenericBox(1, 100, 90, 139, 115), new GenericBox(1, 109, 115, 176, 134), new GenericBox(1, 99, 134, 130, 169), new GenericBox(0, 143, 113, 197, 137) }),
            new FrameData(15458, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 81, 128, 95), new GenericBox(1, 93, 91, 142, 111), new GenericBox(1, 102, 111, 168, 136), new GenericBox(1, 101, 136, 132, 168), new GenericBox(0, 154, 116, 197, 137) }),
            new FrameData(15459, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 104, 80, 127, 96), new GenericBox(1, 92, 93, 139, 111), new GenericBox(1, 102, 111, 146, 135), new GenericBox(1, 111, 135, 136, 167) }),
            new FrameData(15460, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 81, 127, 96), new GenericBox(1, 100, 94, 137, 118), new GenericBox(1, 67, 117, 134, 138), new GenericBox(1, 107, 138, 136, 168), new GenericBox(0, 43, 113, 86, 138) }),
            new FrameData(15461, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 111, 79, 133, 97), new GenericBox(1, 100, 89, 143, 112), new GenericBox(1, 78, 113, 134, 137), new GenericBox(1, 102, 136, 129, 165), new GenericBox(0, 55, 112, 100, 138) }),

            new FrameData(15366, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 82, 132, 99), new GenericBox(1, 103, 92, 147, 110), new GenericBox(1, 107, 110, 137, 134), new GenericBox(1, 95, 133, 154, 157) }),
            new FrameData(15367, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 112, 82, 131, 98), new GenericBox(1, 100, 93, 140, 114), new GenericBox(1, 106, 114, 139, 136), new GenericBox(1, 100, 135, 140, 164) }),
            new FrameData(15368, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 86, 132, 101), new GenericBox(1, 100, 95, 143, 116), new GenericBox(1, 105, 116, 140, 139), new GenericBox(1, 97, 138, 152, 178) }),
        };

        var lightTatsoFrames = new List<Frame> {
            new FrameData(15356, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 122, 88, 138, 102), new GenericBox(1, 108, 99, 143, 122), new GenericBox(1, 105, 121, 143, 143), new GenericBox(1, 81, 145, 148, 195) }, sound: "tatso", hasHit: false),
            new FrameData(15357, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 110, 89, 127, 107), new GenericBox(1, 97, 103, 135, 123), new GenericBox(1, 111, 123, 162, 149), new GenericBox(1, 114, 148, 139, 194) }),
            new FrameData(15358, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 82, 126, 95), new GenericBox(1, 103, 93, 140, 115), new GenericBox(1, 110, 115, 141, 138), new GenericBox(1, 97, 137, 150, 170) }),
            new FrameData(15359, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 82, 131, 98), new GenericBox(1, 102, 93, 140, 112), new GenericBox(1, 109, 112, 140, 136), new GenericBox(1, 109, 136, 136, 158) }),

            new FrameData(15456, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 79, 133, 96), new GenericBox(1, 100, 90, 137, 109), new GenericBox(1, 102, 109, 134, 136), new GenericBox(1, 99, 136, 120, 174) }, hasHit: false),
            new FrameData(15457, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 80, 136, 98), new GenericBox(1, 100, 90, 139, 115), new GenericBox(1, 109, 115, 176, 134), new GenericBox(1, 99, 134, 130, 169), new GenericBox(0, 143, 113, 197, 137) }),
            new FrameData(15458, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 81, 128, 95), new GenericBox(1, 93, 91, 142, 111), new GenericBox(1, 102, 111, 168, 136), new GenericBox(1, 101, 136, 132, 168), new GenericBox(0, 154, 116, 197, 137) }),
            new FrameData(15459, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 104, 80, 127, 96), new GenericBox(1, 92, 93, 139, 111), new GenericBox(1, 102, 111, 146, 135), new GenericBox(1, 111, 135, 136, 167) }),
            new FrameData(15460, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 81, 127, 96), new GenericBox(1, 100, 94, 137, 118), new GenericBox(1, 67, 117, 134, 138), new GenericBox(1, 107, 138, 136, 168), new GenericBox(0, 43, 113, 86, 138) }),
            new FrameData(15461, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 111, 79, 133, 97), new GenericBox(1, 100, 89, 143, 112), new GenericBox(1, 78, 113, 134, 137), new GenericBox(1, 102, 136, 129, 165), new GenericBox(0, 55, 112, 100, 138) }),

            new FrameData(15456, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 79, 133, 96), new GenericBox(1, 100, 90, 137, 109), new GenericBox(1, 102, 109, 134, 136), new GenericBox(1, 99, 136, 120, 174) }, hasHit: false),
            new FrameData(15457, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 80, 136, 98), new GenericBox(1, 100, 90, 139, 115), new GenericBox(1, 109, 115, 176, 134), new GenericBox(1, 99, 134, 130, 169), new GenericBox(0, 143, 113, 197, 137) }),
            new FrameData(15458, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 81, 128, 95), new GenericBox(1, 93, 91, 142, 111), new GenericBox(1, 102, 111, 168, 136), new GenericBox(1, 101, 136, 132, 168), new GenericBox(0, 154, 116, 197, 137) }),
            new FrameData(15459, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 104, 80, 127, 96), new GenericBox(1, 92, 93, 139, 111), new GenericBox(1, 102, 111, 146, 135), new GenericBox(1, 111, 135, 136, 167) }),
            new FrameData(15460, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 81, 127, 96), new GenericBox(1, 100, 94, 137, 118), new GenericBox(1, 67, 117, 134, 138), new GenericBox(1, 107, 138, 136, 168), new GenericBox(0, 43, 113, 86, 138) }),
            new FrameData(15461, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 111, 79, 133, 97), new GenericBox(1, 100, 89, 143, 112), new GenericBox(1, 78, 113, 134, 137), new GenericBox(1, 102, 136, 129, 165), new GenericBox(0, 55, 112, 100, 138) }),

            new FrameData(15366, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 82, 132, 99), new GenericBox(1, 103, 92, 147, 110), new GenericBox(1, 107, 110, 137, 134), new GenericBox(1, 95, 133, 154, 157) }),
            new FrameData(15367, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 112, 82, 131, 98), new GenericBox(1, 100, 93, 140, 114), new GenericBox(1, 106, 114, 139, 136), new GenericBox(1, 100, 135, 140, 164) }),
            new FrameData(15368, this.tatso_speed, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 86, 132, 101), new GenericBox(1, 100, 95, 143, 116), new GenericBox(1, 105, 116, 140, 139), new GenericBox(1, 97, 138, 152, 178) }),
        };

        var EXTatsoFrames = new List<Frame> {
            new FrameData(15356, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 122, 88, 138, 102), new GenericBox(1, 108, 99, 143, 122), new GenericBox(1, 105, 121, 143, 143), new GenericBox(1, 81, 145, 148, 195) }, sound: "tatso", hasHit: false),
            new FrameData(15357, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 110, 89, 127, 107), new GenericBox(1, 97, 103, 135, 123), new GenericBox(1, 111, 123, 162, 149), new GenericBox(1, 114, 148, 139, 194) }, sound:"EX"),
            new FrameData(15358, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 82, 126, 95), new GenericBox(1, 103, 93, 140, 115), new GenericBox(1, 110, 115, 141, 138), new GenericBox(1, 97, 137, 150, 170) }),
            new FrameData(15359, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 82, 131, 98), new GenericBox(1, 102, 93, 140, 112), new GenericBox(1, 109, 112, 140, 136), new GenericBox(1, 109, 136, 136, 158) }),

            new FrameData(15456, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 79, 133, 96), new GenericBox(1, 100, 90, 137, 109), new GenericBox(1, 102, 109, 134, 136), new GenericBox(1, 99, 136, 120, 174) }, hasHit: false),
            new FrameData(15457, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 80, 136, 98), new GenericBox(1, 100, 90, 139, 115), new GenericBox(1, 109, 115, 176, 134), new GenericBox(1, 99, 134, 130, 169), new GenericBox(0, 143, 113, 197, 137) }),
            new FrameData(15458, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 81, 128, 95), new GenericBox(1, 93, 91, 142, 111), new GenericBox(1, 102, 111, 168, 136), new GenericBox(1, 101, 136, 132, 168), new GenericBox(0, 154, 116, 197, 137) }),
            new FrameData(15459, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 104, 80, 127, 96), new GenericBox(1, 92, 93, 139, 111), new GenericBox(1, 102, 111, 146, 135), new GenericBox(1, 111, 135, 136, 167) }),
            new FrameData(15460, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 81, 127, 96), new GenericBox(1, 100, 94, 137, 118), new GenericBox(1, 67, 117, 134, 138), new GenericBox(1, 107, 138, 136, 168), new GenericBox(0, 43, 113, 86, 138) }),
            new FrameData(15461, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 111, 79, 133, 97), new GenericBox(1, 100, 89, 143, 112), new GenericBox(1, 78, 113, 134, 137), new GenericBox(1, 102, 136, 129, 165), new GenericBox(0, 55, 112, 100, 138) }),

            new FrameData(15456, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 79, 133, 96), new GenericBox(1, 100, 90, 137, 109), new GenericBox(1, 102, 109, 134, 136), new GenericBox(1, 99, 136, 120, 174) }, hasHit: false),
            new FrameData(15457, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 80, 136, 98), new GenericBox(1, 100, 90, 139, 115), new GenericBox(1, 109, 115, 176, 134), new GenericBox(1, 99, 134, 130, 169), new GenericBox(0, 143, 113, 197, 137) }),
            new FrameData(15458, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 81, 128, 95), new GenericBox(1, 93, 91, 142, 111), new GenericBox(1, 102, 111, 168, 136), new GenericBox(1, 101, 136, 132, 168), new GenericBox(0, 154, 116, 197, 137) }),
            new FrameData(15459, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 104, 80, 127, 96), new GenericBox(1, 92, 93, 139, 111), new GenericBox(1, 102, 111, 146, 135), new GenericBox(1, 111, 135, 136, 167) }),
            new FrameData(15460, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 81, 127, 96), new GenericBox(1, 100, 94, 137, 118), new GenericBox(1, 67, 117, 134, 138), new GenericBox(1, 107, 138, 136, 168), new GenericBox(0, 43, 113, 86, 138) }),
            new FrameData(15461, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 111, 79, 133, 97), new GenericBox(1, 100, 89, 143, 112), new GenericBox(1, 78, 113, 134, 137), new GenericBox(1, 102, 136, 129, 165), new GenericBox(0, 55, 112, 100, 138) }),

            new FrameData(15456, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 79, 133, 96), new GenericBox(1, 100, 90, 137, 109), new GenericBox(1, 102, 109, 134, 136), new GenericBox(1, 99, 136, 120, 174) }, hasHit: false),
            new FrameData(15457, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 80, 136, 98), new GenericBox(1, 100, 90, 139, 115), new GenericBox(1, 109, 115, 176, 134), new GenericBox(1, 99, 134, 130, 169), new GenericBox(0, 143, 113, 197, 137) }),
            new FrameData(15458, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 81, 128, 95), new GenericBox(1, 93, 91, 142, 111), new GenericBox(1, 102, 111, 168, 136), new GenericBox(1, 101, 136, 132, 168), new GenericBox(0, 154, 116, 197, 137) }),
            new FrameData(15459, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 104, 80, 127, 96), new GenericBox(1, 92, 93, 139, 111), new GenericBox(1, 102, 111, 146, 135), new GenericBox(1, 111, 135, 136, 167) }),
            new FrameData(15460, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 81, 127, 96), new GenericBox(1, 100, 94, 137, 118), new GenericBox(1, 67, 117, 134, 138), new GenericBox(1, 107, 138, 136, 168), new GenericBox(0, 43, 113, 86, 138) }),
            new FrameData(15461, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 111, 79, 133, 97), new GenericBox(1, 100, 89, 143, 112), new GenericBox(1, 78, 113, 134, 137), new GenericBox(1, 102, 136, 129, 165), new GenericBox(0, 55, 112, 100, 138) }),

            new FrameData(15456, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 79, 133, 96), new GenericBox(1, 100, 90, 137, 109), new GenericBox(1, 102, 109, 134, 136), new GenericBox(1, 99, 136, 120, 174) }, hasHit: false),
            new FrameData(15457, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 115, 80, 136, 98), new GenericBox(1, 100, 90, 139, 115), new GenericBox(1, 109, 115, 176, 134), new GenericBox(1, 99, 134, 130, 169), new GenericBox(0, 143, 113, 197, 137) }),
            new FrameData(15458, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 108, 81, 128, 95), new GenericBox(1, 93, 91, 142, 111), new GenericBox(1, 102, 111, 168, 136), new GenericBox(1, 101, 136, 132, 168), new GenericBox(0, 154, 116, 197, 137) }),
            new FrameData(15459, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 104, 80, 127, 96), new GenericBox(1, 92, 93, 139, 111), new GenericBox(1, 102, 111, 146, 135), new GenericBox(1, 111, 135, 136, 167) }),
            new FrameData(15460, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 107, 81, 127, 96), new GenericBox(1, 100, 94, 137, 118), new GenericBox(1, 67, 117, 134, 138), new GenericBox(1, 107, 138, 136, 168), new GenericBox(0, 43, 113, 86, 138) }),
            new FrameData(15461, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 111, 79, 133, 97), new GenericBox(1, 100, 89, 143, 112), new GenericBox(1, 78, 113, 134, 137), new GenericBox(1, 102, 136, 129, 165), new GenericBox(0, 55, 112, 100, 138) }),

            new FrameData(15366, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 82, 132, 99), new GenericBox(1, 103, 92, 147, 110), new GenericBox(1, 107, 110, 137, 134), new GenericBox(1, 95, 133, 154, 157) }),
            new FrameData(15367, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 112, 82, 131, 98), new GenericBox(1, 100, 93, 140, 114), new GenericBox(1, 106, 114, 139, 136), new GenericBox(1, 100, 135, 140, 164) }),
            new FrameData(15368, this.tatso_speed, 0.0f, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 114, 86, 132, 101), new GenericBox(1, 100, 95, 143, 116), new GenericBox(1, 105, 116, 140, 139), new GenericBox(1, 97, 138, 152, 178) }),
        };

        var airTatsoFrames = new List<Frame> {
            new FrameData(15456, 3.0f, 0.0f, len: 1, boxes:new List<GenericBox> { airPuxbox, new GenericBox(1, 114, 79, 130, 92), new GenericBox(1, 98, 92, 138, 109), new GenericBox(1, 97, 110, 135, 136) }),
            new FrameData(15457, 3.0f, 0.0f, len: 1, boxes:new List<GenericBox> { airPuxbox, new GenericBox(1, 115, 80, 132, 94), new GenericBox(1, 103, 90, 138, 106), new GenericBox(1, 104, 107, 141, 135), new GenericBox(0, 157, 111, 196, 137) }),
            new FrameData(15458, 3.0f, 0.0f, len: 1, boxes:new List<GenericBox> { airPuxbox, new GenericBox(1, 108, 80, 127, 94), new GenericBox(1, 95, 90, 139, 113), new GenericBox(1, 102, 113, 139, 138), new GenericBox(0, 157, 115, 194, 136) }),
            new FrameData(15459, 3.0f, 0.0f, len: 1, boxes:new List<GenericBox> { airPuxbox, new GenericBox(1, 105, 80, 127, 94), new GenericBox(1, 94, 91, 136, 112), new GenericBox(1, 102, 113, 142, 135) }),
            new FrameData(15460, 3.0f, 0.0f, len: 1, boxes:new List<GenericBox> { airPuxbox, new GenericBox(1, 106, 80, 127, 94), new GenericBox(1, 98, 95, 138, 116), new GenericBox(1, 98, 116, 133, 137), new GenericBox(0, 42, 114, 82, 136) }),
            new FrameData(15461, 3.0f, 0.0f, len: 1, boxes:new List<GenericBox> { airPuxbox, new GenericBox(0, 58, 117, 88, 137), new GenericBox(1, 112, 78, 131, 94), new GenericBox(1, 101, 88, 142, 111), new GenericBox(1, 99, 111, 135, 139) }),      
        };

        // Fall and wakeup
        var AirbonedFrames = new List<Frame> {
            new FrameData("14880", 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 69, 86, 90, 107), new GenericBox(1, 91, 94, 155, 153) }),
            new FrameData("14881", 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 65, 99, 87, 120), new GenericBox(1, 87, 98, 155, 155) }),
            new FrameData("14882", 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 65, 108, 87, 131), new GenericBox(1, 85, 104, 154, 145) }),
            new FrameData("14883", 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 65, 108, 90, 138), new GenericBox(1, 85, 104, 154, 144) }),
            new FrameData("14884", 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 68, 114, 89, 139), new GenericBox(1, 85, 105, 155, 145) }),
            new FrameData("14885", 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 70, 121, 96, 145), new GenericBox(1, 84, 106, 155, 146) }),
            new FrameData("14886", 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 71, 139, 93, 163), new GenericBox(1, 85, 104, 154, 157) }),
            new FrameData("14887", 0.0f, 0.0f, len: 3, boxes: new List<GenericBox> { airPuxbox, new GenericBox(1, 96, 114, 140, 177), new GenericBox(1, 90, 164, 114, 190) })
        };

        var fallingFrames = new List<Frame> {
            new FrameData(14888, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14889, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14890, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14891, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14892, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14893, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14894, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14895, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14896, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14897, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14898, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14899, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14900, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14901, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14902, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14903, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox }),
        };

        var sweepedFrames = new List<Frame> {
            // new FrameData("14916", 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 155, 195, 95, 86) }),
            new FrameData("14917", 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 86, 104, 175, 164) }),
            new FrameData("14918", 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 85, 106, 175, 166) }),
            new FrameData("14919", 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 85, 115, 165, 174) }),
            new FrameData("14920", 0.0f, 0.0f, len: 2, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 85, 125, 154, 185) }),
        };

        var OnGroundFrames = new List<Frame> {
            new FrameData(14904, 0, 0, len: 20, boxes: new List<GenericBox> { pushbox }),
        };

        var wakeupFrames = new List<Frame> {
            new FrameData(14976, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14977, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14978, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14979, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14980, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14981, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14982, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14983, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14984, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox }),
            new FrameData(14985, 0, 0, len: 4, boxes: new List<GenericBox> { pushbox }),
        };

        // Super
        var  SA1 = new List<Frame>{
            new FrameData(15136, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 117, 92, 137, 109), new GenericBox(1, 103, 102, 141, 123), new GenericBox(1, 106, 123, 142, 152), new GenericBox(1, 89, 151, 153, 195) }),
            new FrameData(15137, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 118, 93, 137, 106), new GenericBox(1, 104, 103, 142, 118), new GenericBox(1, 108, 117, 140, 144), new GenericBox(1, 83, 145, 157, 195) }),
            new FrameData(15138, 0, 0, len: 3, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 116, 92, 140, 108), new GenericBox(1, 103, 100, 141, 123), new GenericBox(1, 107, 123, 143, 151), new GenericBox(1, 98, 151, 154, 194) }),
            new FrameData(15139, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 102, 143, 150), new GenericBox(1, 82, 107, 100, 132), new GenericBox(1, 138, 124, 170, 150), new GenericBox(1, 114, 143, 148, 193), new GenericBox(1, 106, 92, 131, 106)}),
            new FrameData(15073, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 176, 89, 210, 117), new GenericBox(0, 158, 105, 184, 126), new GenericBox(0, 139, 118, 166, 142), new GenericBox(1, 90, 93, 141, 146), new GenericBox(1, 110, 137, 137, 194), new GenericBox(1, 69, 114, 90, 136)}, sound: "shinpu"),
            new FrameData(15074, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 186, 95, 219, 120), new GenericBox(0, 162, 106, 189, 129), new GenericBox(0, 145, 117, 171, 140), new GenericBox(1, 91, 91, 148, 140), new GenericBox(1, 112, 140, 139, 194), new GenericBox(1, 70, 120, 92, 140) }),
            new FrameData(15075, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 174, 118, 204, 144), new GenericBox(0, 145, 123, 174, 146), new GenericBox(1, 93, 94, 148, 139), new GenericBox(1, 112, 137, 144, 193), new GenericBox(1, 79, 128, 101, 148) }),
            new FrameData(15076, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 91, 131, 105), new GenericBox(1, 94, 96, 154, 140), new GenericBox(1, 135, 138, 176, 163), new GenericBox(1, 107, 141, 140, 193)}),
            new FrameData(15077, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 92, 149, 145), new GenericBox(1, 106, 139, 144, 194) }),
            new FrameData(15078, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 102, 88, 124, 106), new GenericBox(1, 90, 100, 150, 132), new GenericBox(1, 100, 133, 141, 194)}),
            new FrameData(15079, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 89, 131, 107), new GenericBox(1, 92, 101, 144, 148), new GenericBox(1, 96, 148, 145, 195) }),
            new FrameData(15080, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 117, 88, 137, 105), new GenericBox(1, 108, 100, 142, 151), new GenericBox(1, 95, 100, 153, 130), new GenericBox(1, 98, 143, 156, 195)}),
            new FrameData(15104, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 124, 91, 145, 109), new GenericBox(1, 104, 96, 141, 155), new GenericBox(1, 139, 114, 156, 131), new GenericBox(1, 93, 144, 155, 195)}, hasHit: false),
            new FrameData(15105, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 127, 92, 149, 107), new GenericBox(1, 105, 100, 153, 153), new GenericBox(1, 124, 152, 161, 193), new GenericBox(1, 149, 129, 171, 155) }),
            new FrameData(15106, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 125, 91, 149, 105), new GenericBox(1, 107, 100, 153, 147), new GenericBox(1, 121, 147, 157, 194), new GenericBox(1, 153, 129, 172, 157), new GenericBox(0, 186, 154, 207, 181), new GenericBox(0, 171, 139, 187, 169) }),
            new FrameData(15107, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 126, 91, 149, 105), new GenericBox(1, 104, 97, 155, 146), new GenericBox(1, 123, 140, 157, 193), new GenericBox(1, 151, 129, 174, 160) }),
            new FrameData(15108, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 128, 91, 148, 109), new GenericBox(1, 104, 99, 154, 150), new GenericBox(1, 127, 130, 163, 194)}),
            new FrameData(15109, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 128, 90, 148, 106), new GenericBox(1, 107, 99, 156, 154), new GenericBox(1, 108, 153, 159, 193) }),
            new FrameData(15110, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 122, 90, 141, 107), new GenericBox(1, 100, 97, 139, 153), new GenericBox(1, 137, 105, 156, 132), new GenericBox(1, 97, 143, 155, 195)}),
            new FrameData(15072, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 102, 143, 150), new GenericBox(1, 82, 107, 100, 132), new GenericBox(1, 138, 124, 170, 150), new GenericBox(1, 114, 143, 148, 193), new GenericBox(1, 106, 92, 131, 106)}, hasHit: false),
            new FrameData(15073, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 176, 89, 210, 117), new GenericBox(0, 158, 105, 184, 126), new GenericBox(0, 139, 118, 166, 142), new GenericBox(1, 90, 93, 141, 146), new GenericBox(1, 110, 137, 137, 194), new GenericBox(1, 69, 114, 90, 136)}),
            new FrameData(15074, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 186, 95, 219, 120), new GenericBox(0, 162, 106, 189, 129), new GenericBox(0, 145, 117, 171, 140), new GenericBox(1, 91, 91, 148, 140), new GenericBox(1, 112, 140, 139, 194), new GenericBox(1, 70, 120, 92, 140) }),
            new FrameData(15075, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 174, 118, 204, 144), new GenericBox(0, 145, 123, 174, 146), new GenericBox(1, 93, 94, 148, 139), new GenericBox(1, 112, 137, 144, 193), new GenericBox(1, 79, 128, 101, 148) }),
            new FrameData(15076, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 91, 131, 105), new GenericBox(1, 94, 96, 154, 140), new GenericBox(1, 135, 138, 176, 163), new GenericBox(1, 107, 141, 140, 193)}),
            new FrameData(15077, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 92, 149, 145), new GenericBox(1, 106, 139, 144, 194) }),
            new FrameData(15078, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 102, 88, 124, 106), new GenericBox(1, 90, 100, 150, 132), new GenericBox(1, 100, 133, 141, 194)}),
            new FrameData(15079, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 89, 131, 107), new GenericBox(1, 92, 101, 144, 148), new GenericBox(1, 96, 148, 145, 195) }),
            new FrameData(15080, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 117, 88, 137, 105), new GenericBox(1, 108, 100, 142, 151), new GenericBox(1, 95, 100, 153, 130), new GenericBox(1, 98, 143, 156, 195)}),
            new FrameData(15104, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 124, 91, 145, 109), new GenericBox(1, 104, 96, 141, 155), new GenericBox(1, 139, 114, 156, 131), new GenericBox(1, 93, 144, 155, 195)}, hasHit: false),
            new FrameData(15105, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 127, 92, 149, 107), new GenericBox(1, 105, 100, 153, 153), new GenericBox(1, 124, 152, 161, 193), new GenericBox(1, 149, 129, 171, 155) }),
            new FrameData(15106, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 125, 91, 149, 105), new GenericBox(1, 107, 100, 153, 147), new GenericBox(1, 121, 147, 157, 194), new GenericBox(1, 153, 129, 172, 157), new GenericBox(0, 186, 154, 207, 181), new GenericBox(0, 171, 139, 187, 169) }),
            new FrameData(15107, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 126, 91, 149, 105), new GenericBox(1, 104, 97, 155, 146), new GenericBox(1, 123, 140, 157, 193), new GenericBox(1, 151, 129, 174, 160) }),
            new FrameData(15108, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 128, 91, 148, 109), new GenericBox(1, 104, 99, 154, 150), new GenericBox(1, 127, 130, 163, 194)}),
            new FrameData(15109, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 128, 90, 148, 106), new GenericBox(1, 107, 99, 156, 154), new GenericBox(1, 108, 153, 159, 193) }),
            new FrameData(15110, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 122, 90, 141, 107), new GenericBox(1, 100, 97, 139, 153), new GenericBox(1, 137, 105, 156, 132), new GenericBox(1, 97, 143, 155, 195)}),
            new FrameData(15072, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 102, 143, 150), new GenericBox(1, 82, 107, 100, 132), new GenericBox(1, 138, 124, 170, 150), new GenericBox(1, 114, 143, 148, 193), new GenericBox(1, 106, 92, 131, 106)}, hasHit: false),
            new FrameData(15073, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 176, 89, 210, 117), new GenericBox(0, 158, 105, 184, 126), new GenericBox(0, 139, 118, 166, 142), new GenericBox(1, 90, 93, 141, 146), new GenericBox(1, 110, 137, 137, 194), new GenericBox(1, 69, 114, 90, 136)}),
            new FrameData(15074, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 186, 95, 219, 120), new GenericBox(0, 162, 106, 189, 129), new GenericBox(0, 145, 117, 171, 140), new GenericBox(1, 91, 91, 148, 140), new GenericBox(1, 112, 140, 139, 194), new GenericBox(1, 70, 120, 92, 140) }),
            new FrameData(15075, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(0, 174, 118, 204, 144), new GenericBox(0, 145, 123, 174, 146), new GenericBox(1, 93, 94, 148, 139), new GenericBox(1, 112, 137, 144, 193), new GenericBox(1, 79, 128, 101, 148) }),
            new FrameData(15076, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 91, 131, 105), new GenericBox(1, 94, 96, 154, 140), new GenericBox(1, 135, 138, 176, 163), new GenericBox(1, 107, 141, 140, 193)}),
            new FrameData(15077, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 96, 92, 149, 145), new GenericBox(1, 106, 139, 144, 194) }),
            new FrameData(15078, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 102, 88, 124, 106), new GenericBox(1, 90, 100, 150, 132), new GenericBox(1, 100, 133, 141, 194)}),
            new FrameData(15079, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 109, 89, 131, 107), new GenericBox(1, 92, 101, 144, 148), new GenericBox(1, 96, 148, 145, 195) }),
            new FrameData(15080, 6.25f, 0, len: 1, boxes: new List<GenericBox> { pushbox, new GenericBox(1, 117, 88, 137, 105), new GenericBox(1, 108, 100, 142, 151), new GenericBox(1, 95, 100, 153, 130), new GenericBox(1, 98, 143, 156, 195)}),
        };

        var SA1_tail = new List<Frame> {
            new FrameData(15283, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(2, 103, 91, 144, 163), new GenericBox(1, 118, 80, 135, 94), new GenericBox(1, 103, 92, 148, 153) }, sound: "Jinraikyaku", hasHit: false),
            new FrameData(15284, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(2, 105, 90, 149, 151), new GenericBox(1, 114, 81, 132, 95), new GenericBox(1, 104, 89, 150, 151) }),
            new FrameData(15285, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(1, 96, 91, 139, 140), new GenericBox(1, 106, 77, 127, 94), new GenericBox(1, 140, 99, 181, 128), new GenericBox(2, 95, 90, 141, 142), new GenericBox(0, 180, 86, 209, 113), new GenericBox(0, 164, 99, 183, 118) }),
            new FrameData(15286, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(0, 188, 93, 206, 110), new GenericBox(0, 164, 98, 188, 118), new GenericBox(1, 110, 81, 129, 95), new GenericBox(1, 97, 92, 143, 139), new GenericBox(1, 143, 102, 174, 126), new GenericBox(2, 97, 91, 143, 140) }),
            new FrameData(15287, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(2, 99, 91, 144, 142), new GenericBox(1, 108, 81, 126, 94), new GenericBox(1, 99, 90, 145, 142), new GenericBox(1, 145, 101, 169, 132) }),
            new FrameData(15288, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(1, 102, 92, 147, 146), new GenericBox(1, 114, 80, 133, 96), new GenericBox(2, 102, 91, 147, 146) }),
            new FrameData(15283, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(2, 103, 91, 144, 163), new GenericBox(1, 118, 80, 135, 94), new GenericBox(1, 103, 92, 148, 153) }, hasHit: false),
            new FrameData(15284, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(2, 105, 90, 149, 151), new GenericBox(1, 114, 81, 132, 95), new GenericBox(1, 104, 89, 150, 151) }),
            new FrameData(15285, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(1, 96, 91, 139, 140), new GenericBox(1, 106, 77, 127, 94), new GenericBox(1, 140, 99, 181, 128), new GenericBox(2, 95, 90, 141, 142), new GenericBox(0, 180, 86, 209, 113), new GenericBox(0, 164, 99, 183, 118) }),
            new FrameData(15286, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(0, 188, 93, 206, 110), new GenericBox(0, 164, 98, 188, 118), new GenericBox(1, 110, 81, 129, 95), new GenericBox(1, 97, 92, 143, 139), new GenericBox(1, 143, 102, 174, 126), new GenericBox(2, 97, 91, 143, 140) }),
            new FrameData(15287, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(2, 99, 91, 144, 142), new GenericBox(1, 108, 81, 126, 94), new GenericBox(1, 99, 90, 145, 142), new GenericBox(1, 145, 101, 169, 132) }),
            new FrameData(15288, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(1, 102, 92, 147, 146), new GenericBox(1, 114, 80, 133, 96), new GenericBox(2, 102, 91, 147, 146) }),
            new FrameData(15283, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(2, 103, 91, 144, 163), new GenericBox(1, 118, 80, 135, 94), new GenericBox(1, 103, 92, 148, 153) }, hasHit: false),
            new FrameData(15284, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(2, 105, 90, 149, 151), new GenericBox(1, 114, 81, 132, 95), new GenericBox(1, 104, 89, 150, 151) }),
            new FrameData(15285, 0, 0, len: 2, boxes: new List<GenericBox> { new GenericBox(1, 96, 91, 139, 140), new GenericBox(1, 106, 77, 127, 94), new GenericBox(1, 140, 99, 181, 128), new GenericBox(2, 95, 90, 141, 142), new GenericBox(0, 180, 86, 209, 113), new GenericBox(0, 164, 99, 183, 118) }),
        };

        var SA1_exit = new List<Frame> {
            new FrameData(15286, 0, 0, len: 3, boxes: new List<GenericBox> { new GenericBox(0, 188, 93, 206, 110), new GenericBox(0, 164, 98, 188, 118), new GenericBox(1, 110, 81, 129, 95), new GenericBox(1, 97, 92, 143, 139), new GenericBox(1, 143, 102, 174, 126), new GenericBox(2, 97, 91, 143, 140) }),
            new FrameData(15287, 0, 0, len: 3, boxes: new List<GenericBox> { new GenericBox(2, 99, 91, 144, 142), new GenericBox(1, 108, 81, 126, 94), new GenericBox(1, 99, 90, 145, 142), new GenericBox(1, 145, 101, 169, 132) }),
            new FrameData(15288, 0, 0, len: 3, boxes: new List<GenericBox> { new GenericBox(1, 102, 92, 147, 146), new GenericBox(1, 114, 80, 133, 96), new GenericBox(2, 102, 91, 147, 146) }),
        };

        var Shungoku = new List<Frame> {
            new FrameData(14768, 0, 0, len: 10, boxes: new List<GenericBox> { new GenericBox(0, 166, 129, 200, 173) }),         
        };

        // States
        this.states = new Dictionary<string, State> {
            // Basic
            { "Idle", new State(idleFrames, "Idle", not_busy: true)},
            { "OnBlock", new State(OnBlockFrames, "Idle", on_block: true)}, 
            { "OnBlockLow", new State(OnBlockLowFrames, "Crouching", low: true, on_block: true)},
            { "OnHit", new State(OnHitFrames, "Idle", on_hit: true)},
            { "OnHitLow", new State(OnHitLowFrames, "Crouching", low: true, on_hit: true)},
            { "Airboned", new State(AirbonedFrames, "Falling", change_on_ground: true, change_on_end: false, loop: false, air: true, on_hit: true)},
            { "Parry", new State(parryFrames, "Idle", 6, glow: true, is_parry: true)},
            { "AirParry", new State(airParryFrames, "JumpFalling", 6, glow: true, air: true, is_parry: true)},
            // Normals
            { "LightP", new State(LPFrames, "Idle", 0, can_harm: true)},
            { "LowLightP", new State(lowLPFrames, "Crouching", 0, low: true, can_harm: true)},
            { "AirLightP", new State(airLPFrames, "Idle", 0, change_on_end: false, change_on_ground: true, loop: false, air: true, can_harm: true)},
            { "LightK", new State(LKFrames, "Idle", 0, can_harm: true)},
            { "LowLightK", new State(lowLKFrames, "Crouching", 1, hitstop: "Medium", low: true, can_harm: true)},
            { "AirLightK", new State(airLKFrames, "Idle", 0, change_on_end: false, change_on_ground: true, loop: false, air: true, can_harm: true)},
            { "MediumP", new State(MPFrames, "Idle", 1, hitstop: "Medium", can_harm: true)},
            { "LowMediumP", new State(lowMPFrames, "Crouching", 1, hitstop: "Medium", low: true, can_harm: true)},
            { "AirMediumP", new State(airMPFrames, "Idle", 1, hitstop: "Medium", change_on_end: false, change_on_ground: true, loop: false, air: true, can_harm: true)},
            { "BackMediumP", new State(backMPframes, "Idle", 2, hitstop: "Heavy", can_harm: true)},
            { "MediumK", new State(MKFrames, "Idle", 1, hitstop: "Medium", can_harm: true)},
            { "LowMediumK", new State(lowMKFrames, "Crouching", 2, hitstop: "Heavy", low: true, can_harm: true)},
            { "AirMediumK", new State(airMKFrames, "Idle", 1, hitstop: "Medium", change_on_end: false, change_on_ground: true, loop: false, air: true, can_harm: true)},
            { "BackMediumK", new State(BackMKFrames, "Idle", 1, can_harm: true)},
            { "CloseMP", new State(cl_HPFrames, "Idle", 1, hitstop: "Medium", can_harm: true)},
            // Movement
            { "WalkingForward", new State(walkingForwardFrames, "WalkingForward", not_busy: true)},
            { "WalkingBackward", new State(walkingBackwardFrames, "WalkingBackward", not_busy: true)},
            { "DashForward", new State(dashForwardFrames, "Idle", 20)},
            { "DashBackward", new State(dashBackwardFrames, "Idle", 20)},
            { "Jump", new State(jumpFrames, "JumpFalling", not_busy: true, air: true)},
            { "JumpForward", new State(jumpForward, "JumpFalling", not_busy: true, air: true)}, 
            { "JumpBackward", new State(JumpBackward, "JumpFalling", not_busy: true, air: true)},
            { "JumpFalling", new State(jumpFallingFrames, "Landing", not_busy: true, change_on_end: false, change_on_ground: true, loop: false, air: true)},
            { "Landing", new State(landingFrames, "Idle", not_busy: true)},
            { "CrouchingIn", new State(crouchingInFrames, "Crouching", not_busy: true, low: true)},
            { "Crouching", new State(crouchingFrames, "Crouching", not_busy: true, low: true)},
            // Super
            { "SA1", new State(SA1, "MediumK", 4, trace: true, drama_wait: true, can_harm: true)},
            { "SA1_tail", new State(SA1_tail, "SA1_exit", 4, trace: true, air: true, drama_wait: true, can_harm: true)},
            { "SA1_exit", new State(SA1_exit, "JumpFalling", 4)},
            // Specials
            { "LightShory", new State(lightShoryFrames, "ShoryFalling", 3, hitstop: "Heavy", air: true, can_harm: true)},
            { "HeavyShory", new State(heavyShoryFrames, "ShoryFalling", 3, hitstop: "Heavy", air: true, can_harm: true)},
            { "ShoryEX", new State(EXShoryFrames, "ShoryFalling", 3, hitstop: "Heavy", glow: true, trace: true, air: true, can_be_hit: false, can_harm: true)},
            { "ShoryFalling", new State(shoryFallingFrames, "Landing", change_on_end: false, change_on_ground: true, loop: false, air: true, can_harm: true)},
            { "LightHaduken", new State(hadukenFrames, "Idle", 3, hitstop: "Medium", air: true, can_harm: true)},
            { "HeavyHaduken", new State(hadukenFrames, "Idle", 3, hitstop: "Medium", can_harm: true)},
            { "HadukenEX", new State(hadukenFrames, "Idle", 3, hitstop: "Heavy", glow: true, trace: true, can_harm: true)},
            { "LightTatso", new State(lightTatsoFrames, "Landing", 3, hitstop: "Light", can_harm: true)},
            { "HeavyTatso", new State(heavyTatsoFrames, "Landing", 3, hitstop: "Light", can_harm: true)},
            { "TatsoEX", new State(EXTatsoFrames, "Landing", 3, hitstop: "Light", glow: true, trace: true, can_harm: true)},
            { "AirTatso", new State(airTatsoFrames, "Landing", 3, change_on_ground: true, change_on_end: false, air: true, can_harm: true)},
            { "AirTatsoEX", new State(airTatsoFrames, "Landing", 3, change_on_ground: true, change_on_end: false, glow: true, trace: true, air: true, can_harm: true)},
            // Other
            { "Falling", new State(fallingFrames, "OnGround", can_be_hit: false)},
            { "Sweeped", new State(sweepedFrames, "Falling", low: true, can_be_hit: false)},
            { "OnGround", new State(OnGroundFrames, "Wakeup", low: true, can_be_hit: false)},
            { "Wakeup", new State(wakeupFrames, "Idle", can_be_hit: false)},
            // Bonus
            { "Intro", new State(introFrames, "Idle", can_be_hit: false)},
        };
    }
    public override void DoBehave() {
        if (this.behave == false) return;

        if ((this.current_state == "WalkingForward" || this.current_state == "WalkingBackward") & !InputManager.Key_hold("Left", player: this.player_index, facing: this.facing) & !InputManager.Key_hold("Right", player: this.player_index, facing: this.facing)) {
            this.ChangeState("Idle");
        }

        // Crouching
        if (InputManager.Key_hold("Down", player: this.player_index, facing: this.facing) && !InputManager.Key_hold("Up", player: this.player_index, facing: this.facing) && this.not_acting) {
            this.ChangeState("CrouchingIn");
        }
        if (this.current_state == "Crouching" && !InputManager.Key_hold("Down", player: this.player_index, facing: this.facing)) {
            this.ChangeState("Idle");
        }

        // Dashing
        if (InputManager.Key_sequence_press("Right Right", 10, flexEntry: false, flexTransition: false, player: this.player_index, facing: this.facing) && this.can_dash) {
            this.ChangeState("DashForward");
        } 
        else if (InputManager.Key_sequence_press("Left Left", 10, flexEntry: false, flexTransition: false, player: this.player_index, facing: this.facing) && this.can_dash) {
            this.ChangeState("DashBackward");
        }

        // Walking
        if (InputManager.Key_hold("Left", player: this.player_index, facing: this.facing) && !InputManager.Key_hold("Right", player: this.player_index, facing: this.facing) && (this.current_state == "Idle" || this.current_state == "WalkingForward" || this.current_state == "WalkingBackward")) {
            this.ChangeState("WalkingBackward");
        } else if (InputManager.Key_hold("Right", player: this.player_index, facing: this.facing) && !InputManager.Key_hold("Left", player: this.player_index, facing: this.facing) && (this.current_state == "Idle" || this.current_state == "WalkingBackward" || this.current_state == "WalkingForward")) {
            this.ChangeState("WalkingForward");
        }

        // Jumps
        if (this.not_acting && this.current_anim_frame_index > 0 && InputManager.Key_hold("Up", player: this.player_index, facing: this.facing) && !InputManager.Key_hold("Left", player: this.player_index, facing: this.facing) && InputManager.Key_hold("Right", player: this.player_index, facing: this.facing)) {
            this.ChangeState("JumpForward");
        } else if (this.current_state == "JumpForward" && this.current_anim_frame_index == 1) {
            this.SetVelocity(
                X: this.move_speed + 1, 
                Y: this.jump_hight);
        } 
        else if (this.not_acting && this.current_anim_frame_index > 0 && InputManager.Key_hold("Up", player: this.player_index, facing: this.facing) && InputManager.Key_hold("Left", player: this.player_index, facing: this.facing) && !InputManager.Key_hold("Right", player: this.player_index, facing: this.facing)) {
            this.ChangeState("JumpBackward");
        } else if (this.current_state == "JumpBackward" && this.current_anim_frame_index == 1) {
            this.SetVelocity(
                X: -(this.move_speed + 1), 
                Y: this.jump_hight);
        }
        else if (this.not_acting && this.current_anim_frame_index > 0 && InputManager.Key_hold("Up", player: this.player_index, facing: this.facing)) {
            this.ChangeState("Jump");
            this.SetVelocity(
                X: 0, 
                Y: this.jump_hight);
        }

        // Super
        if (InputManager.Key_sequence_press("Down Down RB", 10, player: this.player_index, facing: this.facing) && !this.on_air && (this.not_acting || this.not_acting_low || (this.has_hit && (this.current_state == "CloseMP" || this.current_state.Contains("Shory") || this.current_state == "LowLightK"))) && Character.CheckAuraPoints(this, 100)) {
            Character.UseSuperPoints(this, 100);
            this.ChangeState("SA1");
            this.stage.StopFor(16);
            this.hitstop_counter = 0;
            this.SA_flag = false;

        } else if (this.current_state == "SA1" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.stage.spawnParticle("SALighting", this.body.Position.X, this.body.Position.Y, X_offset: 50, Y_offset: -120, facing: this.facing);
            this.stage.StopFor(50);

        } else if (this.current_state == "SA1" && this.current_animation.on_last_frame && this.SA_flag) {
            this.ChangeState("SA1_tail");

        } else if (this.current_state == "SA1_tail" && this.current_anim_frame_index == 3 && this.has_frame_change ) {
            this.AddVelocity(
                X: 1, 
                Y: 150);
        }

        // Shorys
        if (InputManager.Key_sequence_press("Right Down Right C", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || this.has_hit && (this.current_state == "MediumP" || this.current_state == "LightP" || this.current_state == "CloseMP" || this.current_state == "LowLightK" || this.current_state == "LowMediumP"))) {
            this.ChangeState("LightShory");
        } else if (this.current_state == "LightShory" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.AddVelocity(
                X: 1.6f, 
                Y: 50);
        } 
        if (InputManager.Key_sequence_press("Right Down Right D", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || this.has_hit && this.current_state == "LowMediumP" || this.current_state == "LowLightK" )) {
            this.ChangeState("HeavyShory");
        } else if (this.current_state == "HeavyShory" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.AddVelocity(
                X: 5f, 
                Y: 80);
        } 
        if (InputManager.Key_sequence_press("Right Down Right RB", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || this.has_hit && (this.current_state == "LowMediumP" || this.current_state == "LowLightK")) && Character.CheckAuraPoints(this, 50)) {
            Character.UseSuperPoints(this, 50);
            this.ChangeState("ShoryEX");
        } else if (this.current_state == "ShoryEX" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.AddVelocity(
                X: 6f, 
                Y: 100);
        } 
        if (this.current_state.Contains("Shory") && this.state.animation.on_last_frame) this.body.Velocity.X = 0;

        // Haduken
        if (this.current_fireball != null && this.current_fireball.remove) this.current_fireball = null;
        if (this.current_fireball == null && InputManager.Key_sequence_press("Down Right C", 10, player: this.player_index, facing: this.facing) && ((this.not_acting || this.not_acting_low) || (this.has_hit && (this.current_state == "MediumP" || this.current_state == "LightP" || this.current_state == "LowLightK")))) {
            this.ChangeState("LightHaduken");
        } else if (this.current_state == "LightHaduken" && this.current_anim_frame_index == 3 && this.current_fireball == null) {
            this.current_fireball = stage.spawnFireball("Ken1", this.body.Position.X, this.body.Position.Y - 5, this.facing, this.player_index, X_offset: 25);
        } 
        if (this.current_fireball == null && InputManager.Key_sequence_press("Down Right D", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low)) {
            this.ChangeState("HeavyHaduken");
        } else if (this.current_state == "HeavyHaduken" && this.current_anim_frame_index == 4 && this.current_fireball == null) {
            this.current_fireball = stage.spawnFireball("Ken2", this.body.Position.X, this.body.Position.Y - 5, this.facing, this.player_index, X_offset: 25);
        }
        if (this.current_fireball == null && InputManager.Key_sequence_press("Down Right RB", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low) && Character.CheckAuraPoints(this, 50)) {
            Character.UseSuperPoints(this, 50);
            this.ChangeState("HadukenEX");
        } else if (this.current_state == "HadukenEX" && this.current_anim_frame_index == 4 && this.current_fireball == null) {
            this.current_fireball = stage.spawnFireball("Ken3", this.body.Position.X, this.body.Position.Y - 5, this.facing, this.player_index, life_points: 2, X_offset: 25);
            this.PlaySound("EX");
        }

        // Tatso
        if (InputManager.Key_sequence_press("Down Left A", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || (this.has_hit && this.current_state == "LowLightK"))) {
            this.ChangeState("LightTatso");
            this.SetVelocity(Y: 5);
        } else if (this.current_state == "LightTatso") {
            this.AddVelocity(Y: 0.5f, raw_set: true);

        } else if (InputManager.Key_sequence_press("Down Left B", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || (this.has_hit && this.current_state == "LowLightK"))) {
            this.ChangeState("HeavyTatso");
            this.SetVelocity(Y: 5);
        } else if (this.current_state == "HeavyTatso") {
            this.AddVelocity(Y: 0.55f, raw_set: true);

        } else if (InputManager.Key_sequence_press("Down Left RB", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || (this.has_hit && this.current_state == "LowLightK")) && Character.CheckAuraPoints(this, 50)) {
            Character.UseSuperPoints(this, 50);
            this.ChangeState("TatsoEX");
            this.SetVelocity(Y: 5);
        } else if (this.current_state == "TatsoEX") {
            this.AddVelocity(Y: 0.5f, raw_set: true);
        }
        
        // Air tatso
        if ((InputManager.Key_sequence_press("Down Left B", 10, player: this.player_index, facing: this.facing) || InputManager.Key_sequence_press("Down Left A", 10, player: this.player_index, facing: this.facing)) && this.not_acting_air) {
            this.ChangeState("AirTatso");
            this.PlaySound("tatso");
        } else if (InputManager.Key_sequence_press("Down Left RB", 10, player: this.player_index, facing: this.facing) && this.not_acting_air && Character.CheckAuraPoints(this, 50)) {
            Character.UseSuperPoints(this, 50);
            this.ChangeState("AirTatsoEX");
            this.PlaySound("EX");
            this.PlaySound("tatso");            
        } 

        // Normals plus
        if (InputManager.Key_sequence_press("D", Config.hit_stop_time, player: this.player_index, facing: this.facing) && this.has_hit && this.current_state == "LightP") {
            this.SetVelocity();
            this.ChangeState("CloseMP");
        } else if (InputManager.Key_press("B", player: this.player_index, facing: this.facing) && InputManager.Key_hold("Left", player: this.player_index, facing: this.facing) && this.not_acting && !this.crounching) {
            this.ChangeState("BackMediumK");
        } else if (InputManager.Key_press("D", player: this.player_index, facing: this.facing) && InputManager.Key_hold("Left", player: this.player_index, facing: this.facing) && (this.not_acting || (this.has_hit && this.current_state == "CloseMP"))) {
            this.ChangeState("BackMediumP");
        } 

        // Normals
        if (this.not_acting_low) {
            if (InputManager.Key_press("C", player: this.player_index, facing: this.facing)) this.ChangeState("LowLightP");
            else if (InputManager.Key_press("A", player: this.player_index, facing: this.facing)) this.ChangeState("LowLightK");
            else if (InputManager.Key_press("D", player: this.player_index, facing: this.facing)) this.ChangeState("LowMediumP");
            else if (InputManager.Key_press("B", player: this.player_index, facing: this.facing)) this.ChangeState("LowMediumK");
        } else if (this.not_acting) {
            if (InputManager.Key_press("C", player: this.player_index, facing: this.facing)) this.ChangeState("LightP");     
            else if (InputManager.Key_press("A", player: this.player_index, facing: this.facing)) this.ChangeState("LightK");     
            else if (InputManager.Key_press("D", player: this.player_index, facing: this.facing)) this.ChangeState("MediumP");     
            else if (InputManager.Key_press("B", player: this.player_index, facing: this.facing) ) this.ChangeState("MediumK");
        } else if (this.not_acting_air) {
            if (InputManager.Key_press("C", player: this.player_index, facing: this.facing)) this.ChangeState("AirLightP");
            else if (InputManager.Key_press("A", player: this.player_index, facing: this.facing)) this.ChangeState("AirLightK");
            else if (InputManager.Key_press("D", player: this.player_index, facing: this.facing)) this.ChangeState("AirMediumP");
            else if (InputManager.Key_press("B", player: this.player_index, facing: this.facing)) this.ChangeState("AirMediumK");
        }
    }
    public override int ImposeBehavior(Character target) {
        int hit = -1;

        switch (this.current_state) {
            case "LightP":
                Character.Push(target: target, self: this, Config.light_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 3);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 13, 50);
                    target.Stun(this, 6);
                }
                Character.AddAuraPoints(target, this, hit);
                break;
                
            case "LowLightP":
                Character.Push(target: target, self: this, Config.light_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 3);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 13, 50);
                    target.Stun(this, 6);
                }
                Character.AddAuraPoints(target, this, hit);
                break;

            case "AirLightP":
                Character.Push(target: target, self: this, Config.light_pushback, force_push: true);
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 7);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 50, 100);
                    target.Stun(this, 7);
                }
                Character.AddAuraPoints(target, this, hit);
                break;

            case "LightK":
                Character.Push(target: target, self: this, Config.light_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 2);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 44, 50);
                    target.Stun(this, 2);
                }
                Character.AddAuraPoints(target, this, hit);
                break;
                
            case "LowLightK":
                Character.Push(target: target, self: this, Config.light_pushback);
                if (target.isBlockingLow()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -4);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 85, 50);
                    target.Stun(this, -3);
                }
                Character.AddAuraPoints(target, this, hit);
                break;
            
            case "AirLightK":
                Character.Push(target: target, self: this, Config.light_pushback, force_push: true);
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 10);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 90, 150);
                    target.Stun(this, 10);
                }
                Character.AddAuraPoints(target, this, hit);
                break;

            case "MediumP":
                Character.Push(target: target, self: this, Config.medium_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -2);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 100, 172);
                    target.Stun(this, 0);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "LowMediumP":
                Character.Push(target: target, self: this, Config.medium_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 3);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 90, 100);
                    target.Stun(this, 4);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "AirMediumP":
                Character.Push(target: target, self: this, Config.medium_pushback, force_push: true);
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 10);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 100, 180);
                    target.Stun(this, 10);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "MediumK":
                Character.Push(target: target, self: this, Config.medium_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -6);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 120, 235);
                    target.Stun(this, 0);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "LowMediumK":
                Character.Push(target: target, self: this, Config.medium_pushback);
                if (target.isBlockingLow()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -14);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 110, 200);
                    target.Stun(target, 0, sweep: true);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "AirMediumK":
                Character.Push(target: target, self: this, Config.medium_pushback, force_push: true);
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 13);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 125, 235);
                    target.Stun(this, 13);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "BackMediumK":
                Character.Push(target: target, self: this, Config.heavy_pushback);
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 1);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 56, 94);
                    target.Stun(this, 10);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 6);
                break;

            case "BackMediumP":
                Character.Push(target: target, self: this, Config.heavy_pushback);
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 1);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 56, 94);
                    target.Stun(this, 10);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "CloseMP":
                Character.Push(target: target, self: this, Config.medium_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -2);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 80, 100);
                    target.Stun(this, 0);
                }
                Character.AddAuraPoints(target, this, hit);
                break;
            
            case "LightShory":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -10);
                    Character.Push(target: target, self: this, Config.light_pushback);

                } else {
                    hit = Character.HIT;
                    target.Stun(this, 0, airbone: true);
                    Character.Push(target: target, self: this, Config.light_pushback, Y_amount: 110f, airbone: true);
                    Character.Damage(target: target, self: this, 80, 160);
                }
                Character.AddAuraPoints(target, this, hit);
                break;

            case "HeavyShory":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -17);
                    Character.Push(target: target, self: this, Config.medium_pushback);

                } else {
                    hit = Character.HIT;
                    if (this.current_anim_frame_index >= 3) {
                        target.Stun(this, 0, airbone: true);
                        Character.Push(target: target, self: this, Config.medium_pushback, Y_amount: 100f, airbone: true);

                    } else target.Stun(this, 0);

                    Character.Damage(target: target, self: this, 100, 85);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "ShoryEX":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -10);
                    Character.Push(target: target, self: this, Config.medium_pushback);
                    Character.Damage(target: target, self: this, 20, 20);

                } else {
                    hit = Character.HIT;
                    if (this.current_anim_frame_index >= 3) {
                        target.Stun(this, 0, airbone: true);
                        Character.Push(target: target, self: this, Config.medium_pushback, Y_amount: 110f, airbone: true);

                    } else target.Stun(this, 0);
                    
                    Character.Damage(target: target, self: this, 120, 85);

                }
                Character.AddAuraPoints(target, this, hit, self_amount: 2);
                break;
            
            case "LightTatso":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -6);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 66, 203);
                    target.Stun(this, -3);
                }
                Character.Push(target: target, self: this, Config.light_pushback);
                Character.AddAuraPoints(target, this, hit);
                break;
            
            case "HeavyTatso":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -5);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 54, 234);
                    target.Stun(this, -3);
                }
                Character.Push(target: target, self: this, Config.light_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 8);
                break;

            case "TatsoEX":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 10, force: true);
                    Character.Damage(target: target, self: this, 10, 30);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 54, 234);
                    target.Stun(this, -3);
                }
                Character.Push(target: target, self: this, Config.light_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 2);
                break;
            
            case "AirTatso":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 15, force: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 80, 140);
                    target.Stun(this, 15, force: true);
                }
                Character.Push(target: target, self: this, Config.light_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 14);
                break;

            case "AirTatsoEX":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 1);

                } else {
                    hit = Character.HIT;
                    target.Stun(this, -6, airbone: true);
                    Character.Push(target: target, self: this, 2f, Y_amount: 50, airbone: true);
                    Character.Damage(target: target, self: this, 80, 140);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 4);
                break;

            case "SA1":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 30, force: true);
                    Character.Push(target: target, self: this, Config.heavy_pushback);
                    Character.Damage(target: target, self: this, 5, 0);
                } else {
                    hit = Character.HIT;
                    target.Stun(this, 10, force_stand: true);
                    Character.Push(target: target, self: this, 1, Y_amount: 20);
                    Character.Damage(target: target, self: this, 45, 35);
                    this.SA_flag = true;
                }
                break;
                
            case "SA1_tail":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 5, 0);
                    target.BlockStun(this, 30, force: true);

                } else {
                    hit = Character.HIT;
                    target.Stun(this, 5, airbone: true);

                    if (this.current_anim_frame_index < 6) {
                        Character.Push(target: target, self: this, 1, Y_amount: 150, airbone: true);
                    } else if (this.current_anim_frame_index >= 13) {
                        Character.Push(target: target, self: this, Config.heavy_pushback, Y_amount: 80, airbone: true);
                    }

                    Character.Damage(target: target, self: this, 45, 35);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 2);
                break;
                
            case "Shungoku":
                this.stage.spawnParticle("Shungoku", target.body.Position.X, this.body.Position.Y, Y_offset: -125, facing: this.facing);
                this.stage.spawnParticle("Shungoku_text", Camera.X, Camera.Y);
                this.stage.StopFor(40 * 4); // 40 frames a 15 fps

                this.ChangeState("Shungoku_End");
                this.SetVelocity();
                this.body.Position.X = target.body.Position.X - 100 * this.facing;

                target.SetVelocity();
                target.ChangeState("OnGround");

                Character.Damage(target: target, self: this, 400, 0);
                Character.AddAuraPoints(target, this, hit, self_amount: 0, target_amount: 30);
                break;

            default:
                break;
        }

        return hit;
    }

    // AI
    public override void SelectMovement(FightState state) {
        if (state.enemyDistance < 0.3f) {
            var choise = AI.rand.Next(0, 20);
            if (choise == 0 && state.onCorner) { 
                this.BOT.EnqueueMove("Up Right", 5);
            } else if (choise <= 2) { 
                this.BOT.EnqueueMove("Left", 10);
            } else if (choise <= 5) {
                this.BOT.EnqueueMove("Left", 20);
                this.BOT.EnqueueMove("Right", 10);
            } else if (choise == 10) {
                this.BOT.EnqueueMove("Right *", 2);
                this.BOT.EnqueueMove("*", 2);
                this.BOT.EnqueueMove("Right *", 2);
            } else
                this.BOT.EnqueueMove("", 60);

        } else if (state.enemyDistance < 0.7f) {
            var choise = AI.rand.Next(0, 20);
            if (choise == 0 || choise == 20) {
                this.BOT.EnqueueMove("Up Right", 5);

            } else if (choise <= 2) { 
                this.BOT.EnqueueMove("Right", 20);

            } else if (choise <= 4) { 
                this.BOT.EnqueueMove("Right", 20);
                this.BOT.EnqueueMove("Down", 10);

            } else if (choise <= 6) { 
                this.BOT.EnqueueMove("Left", 10);

            } else if (choise <= 8) { 
                this.BOT.EnqueueMove("Left", 10);
                this.BOT.EnqueueMove("Down", 10);
                this.BOT.EnqueueMove("Right", 20);  

            } else if (choise <= 10) {
                this.BOT.EnqueueMove("Left", 10);
                this.BOT.EnqueueMove("Right", 30);

            } else 
                this.BOT.EnqueueMove("", 30);

        } else {
            var choise = AI.rand.Next(0, 10);
            if (choise == 1) { 
                this.BOT.EnqueueMove("Down", 20);
            } else if (choise < 6) {
                this.BOT.EnqueueMove("Right", 20);
            } else 
                this.BOT.EnqueueMove("", 30);
        } 

    }
    public override void SelectAction(FightState f_state) {
        if (f_state.enemyIsDead) return;
        if (this.state.can_harm && !this.on_hit && !this.state.on_block && !this.state.on_parry) return;

        if (this.on_hit || this.state.on_block || (f_state.enemyIsAttacking && f_state.enemyDistance <= 0.5f)) { // Block
            if (f_state.enemyIsCrouching) this.BOT.EnqueueAction("Left Down *", 5);
            else this.BOT.EnqueueAction("Left *", 5);

            return;
        }
        
        if (f_state.enemyIsAirborne && f_state.enemyDistance <= 0.3f) { // Anti air
            var choise = AI.rand.Next(0, 2);
            if (choise == 0 && this.state.not_busy) { // Shoryuken
                this.BOT.EnqueueAction("Right *", 2);
                this.BOT.EnqueueAction("Down *", 2);
                this.BOT.EnqueueAction("Right *", 2);
                this.BOT.EnqueueAction("C", 5);
            } 

        } else if (this.state.air && f_state.enemyDistance < 0.5f) { // Air
            var choise = AI.rand.Next(0, 4);

            if (choise == 0) {
                this.BOT.EnqueueAction("A", 3);
            } else if (choise == 1) {
                this.BOT.EnqueueAction("B", 3);
            } else if (choise == 2) {
                this.BOT.EnqueueAction("D", 3);
            } else {
                this.BOT.EnqueueAction("Down *", 5);
                this.BOT.EnqueueAction("Left *", 5);

                if (Character.CheckAuraPoints(this, 50))
                    this.BOT.EnqueueAction("RB", 1);
                else
                    this.BOT.EnqueueAction("A", 1);
            }
            
        } else if (f_state.enemyDistance < 0.3f) { // Close range
            var choise = AI.rand.Next(0, 10);
            if (f_state.enemyIsCrouching) { // LOW
                if (choise < 3) { // Normais
                    choise = AI.rand.Next(0, 2);
                    if (choise < 1) this.BOT.EnqueueAction("A", 3);
                    else if (choise < 2) this.BOT.EnqueueAction("B", 3);

                } else if (choise < 5) { // Low LK
                    this.BOT.EnqueueAction("Down *", 3);
                    this.BOT.EnqueueAction("Down A *", 3);
                    this.BOT.EnqueueAction("", 3);

                    choise = AI.rand.Next(0, 10);
                    if (choise < 1) { // Path: Light Tatso
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("Left *", 3);
                        this.BOT.EnqueueAction("A *", 3);

                    } else if (choise < 3 && Character.CheckAuraPoints(this, 50)) { // Path: Tatso
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("Left *", 3);

                        if (choise == 1) this.BOT.EnqueueAction("RB *", 3);
                        else this.BOT.EnqueueAction("A *", 3);

                    } else if (choise < 5) { // Path: Light Shoryuken
                        this.BOT.EnqueueAction("Right *", 3);
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("Right *", 3);
                        this.BOT.EnqueueAction("C *", 3);

                    }
                } else if (choise < 6) { // Sweep
                    this.BOT.EnqueueAction("Down *", 5);
                    this.BOT.EnqueueAction("Down B *", 5);
                } else if (choise < 7) { // Overhead
                    this.BOT.EnqueueAction("Left B *", 5);
                }

            } else { // STAND
                if (choise < 2) { // Normais
                    choise = AI.rand.Next(0, 4);
                    if (choise < 1) this.BOT.EnqueueAction("D", 3);
                    else if (choise < 2) this.BOT.EnqueueAction("B", 3);
                    else if (choise < 3) this.BOT.EnqueueAction("C", 3);
                    else this.BOT.EnqueueAction("A", 3);

                } else if (choise < 4) { // Low LK
                    this.BOT.EnqueueAction("Down *", 3);
                    this.BOT.EnqueueAction("Down A *", 3);
                    this.BOT.EnqueueAction("", 3);

                    choise = AI.rand.Next(0, 10);
                    if (choise == 0) { // Path: Light Tatso
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("Left *", 3);
                        this.BOT.EnqueueAction("A *", 3);

                    } else if (choise == 1 && Character.CheckAuraPoints(this, 50)) { // Path: EX Tatso
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("Left *", 3);
                        this.BOT.EnqueueAction("RB *", 3);

                    } else if (choise == 3) { // Path: Light Shoryuken
                        this.BOT.EnqueueAction("Right *", 3);
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("Right *", 3);
                        this.BOT.EnqueueAction("C *", 3);

                    } else if (Character.CheckAuraPoints(this, 100)) { // Path: Super Art 1
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction(" *", 3);
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("RB *", 3);

                    }

                } else if (choise < 5) { // Sweep
                    this.BOT.EnqueueAction("Down *", 5);
                    this.BOT.EnqueueAction("Down B *", 5);

                } else if (choise < 6) { // Light Shory
                    this.BOT.EnqueueAction("Right *", 5);
                    this.BOT.EnqueueAction("Down *", 5);
                    this.BOT.EnqueueAction("Right *", 5);
                    this.BOT.EnqueueAction("C", 1);

                } else if (choise < 7) { // Tatso                
                    this.BOT.EnqueueAction("Down *", 5);
                    this.BOT.EnqueueAction("Left *", 5);

                    choise = AI.rand.Next(0, 10);
                    if (choise == 0 && Character.CheckAuraPoints(this, 50)) this.BOT.EnqueueAction("RB", 1);
                    else if (choise < 4) this.BOT.EnqueueAction("B", 1);
                    else this.BOT.EnqueueAction("A", 1);

                } else if (choise < 10) { // Target combo
                    this.BOT.EnqueueAction("C", 3);
                    this.BOT.EnqueueAction("D", 3);
                    this.BOT.EnqueueAction("*", 3);

                    choise = AI.rand.Next(0, 4);
                    if (choise == 0) { // Path: BackMP
                        this.BOT.EnqueueAction("Left *", 3);
                        this.BOT.EnqueueAction("Left D *", 5);

                    } else if (choise == 1) { // Path: Light Shoryuken
                        this.BOT.EnqueueAction("Right *", 5);
                        this.BOT.EnqueueAction("Down *", 5);
                        this.BOT.EnqueueAction("Right *", 5);
                        this.BOT.EnqueueAction("C", 1);

                    } else if (Character.CheckAuraPoints(this, 100)) { // Path: Super Art 1
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("*", 3);
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("RB", 3);
                    }
                }
            }

            return;

        } else if (f_state.enemyDistance < 0.4f) { // Mid range
            var choise = AI.rand.Next(0, 10);
            if (choise < 1) { // Medium kick
                this.BOT.EnqueueAction("B", 1);

            } else if (choise < 2) { // Low LK
                this.BOT.EnqueueAction("Right *", 20);
                this.BOT.EnqueueAction("Down A *", 2);

                if (AI.rand.Next(0, 10) < 3) { // Follow up with Tatso
                    choise = AI.rand.Next(0, 10);
                    if (choise == 0 && Character.CheckAuraPoints(this, 50)) {  // EX Tatso
                        this.BOT.EnqueueAction("Down *", 5);
                        this.BOT.EnqueueAction("Left *", 5);
                        this.BOT.EnqueueAction("RB", 1);

                    } else if (choise < 4) { // Heavy Tatso
                        this.BOT.EnqueueAction("Down *", 5);
                        this.BOT.EnqueueAction("Left *", 5);
                        this.BOT.EnqueueAction("B", 1);

                    } else { // Light Tatso
                        this.BOT.EnqueueAction("Down *", 5);
                        this.BOT.EnqueueAction("Left *", 5);
                        this.BOT.EnqueueAction("A", 1);
                    }
                }
            } else if (choise < 3) { // Tatso
                choise = AI.rand.Next(0, 10);
                this.BOT.EnqueueAction("Down *", 15);
                this.BOT.EnqueueAction("Left *", 15);
                
                if (choise == 0 && Character.CheckAuraPoints(this, 50)) {  // EX Tatso
                    this.BOT.EnqueueAction("RB", 1);
                } else if (choise < 4) { // Heavy Tatso
                    this.BOT.EnqueueAction("B", 1);
                } else { // Light Tatso
                    this.BOT.EnqueueAction("A", 1);
                }
            } 

        } else { // Long range
            if (AI.rand.Next(0, 5) == 0 && this.current_fireball == null) {  // Hadouken
                this.BOT.EnqueueAction("Down *", 20);
                this.BOT.EnqueueAction("Right *", 5);

                var choise = AI.rand.Next(0, 3);
                if (choise == 0 && Character.CheckAuraPoints(this, 50)) 
                    this.BOT.EnqueueAction("RB", 5);
                else if (choise == 1) 
                    this.BOT.EnqueueAction("D", 5);
                else 
                    this.BOT.EnqueueAction("C", 5);

                return;
            } else 
                this.BOT.EnqueueAction("", AI.rand.Next(10, 30)); // Nothing
        }
    
        this.BOT.EnqueueAction("", AI.rand.Next(5, 10));
    }
}