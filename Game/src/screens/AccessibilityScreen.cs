using UI_space;
using SFML.Graphics;
using SFML.System;
using SFML.Audio;

public class WGAccessibilityMenu : Widget {
    private static Sprite bg;
    public static Sprite fade;

    private Vector2i anchor = new Vector2i(-185, -74);
    private int n = 10;

    private static Selector selector = new Selector(new List<int> {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1});

    public WGAccessibilityMenu() {
        bg = new Sprite(Data.textures["screens:accessibility_bg"]);
        fade = new Sprite(Data.textures["screens:90fade"]);
    }

    public override void Render() {
        Camera.UnlockCamera();
        Program.window.Draw(bg);
        selector.Update();

        UI.DrawText(S("accessibility"), -80, -107, spacing: Config.spacing_medium);

        // 0
        if (UI.DrawButton(S("text-to-speech", ": ", Accessibility.TTS ? "enabled" : "disabled"), anchor.X, anchor.Y, hover: selector.is_on(0, 0), click: Input.Key_hold("A"), action: Input.Key_up("A"), spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left")) {
            Accessibility.TTS = !Accessibility.TTS;
        }

        // 1
        UI.DrawButton(S("tts speed", ": < " + Accessibility.TTS_speed + "x >"), anchor.X, anchor.Y+n, hover: selector.is_on(0, 1), click: Input.Key_hold("Left") || Input.Key_hold("Right"), spacing: Config.spacing_small, click_font: Accessibility.TTS ? "default small click" : "default small grad", hover_font: Accessibility.TTS ? "default small hover" : "default small grad", font: "default small", alignment: "Left");
        if (selector.is_on(0, 1) && Accessibility.TTS) {
            if (Input.Key_up("Left") || (Input.Key_hold_for("Left", Config.hold_time) && UI.ForEach(Config.hold_clock))) 
                Accessibility.TTS_speed -= 0.1f;
            else if (Input.Key_up("Right") || (Input.Key_hold_for("Right", Config.hold_time) && UI.ForEach(Config.hold_clock))) 
                Accessibility.TTS_speed += 0.1f;
        }

        // 2 
        if (UI.DrawButton(S("fall/get up", ": ", Accessibility.fall_get_up_cue ? "enabled" : "disabled"), anchor.X, anchor.Y+2*n, hover: selector.is_on(0, 2), click: Input.Key_hold("A"), action: Input.Key_up("A"), spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left")) {
            Accessibility.fall_get_up_cue = !Accessibility.fall_get_up_cue;
        }

        // 3
        if (UI.DrawButton(S("players distance", ": ", Accessibility.distance_cue ? "enabled" : "disabled"), anchor.X, anchor.Y+3*n, hover: selector.is_on(0, 3), click: Input.Key_hold("A"), action: Input.Key_up("A"), spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left")) {
            Accessibility.distance_cue = !Accessibility.distance_cue;
        }

        // 4
        if (UI.DrawButton(S("atack height", ": ", Accessibility.atack_hight_cue ? "enabled" : "disabled"), anchor.X, anchor.Y+4*n, hover: selector.is_on(0, 4), click: Input.Key_hold("A"), action: Input.Key_up("A"), spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left")) {
            Accessibility.atack_hight_cue = !Accessibility.atack_hight_cue;
        }
        
        // 5
        if (UI.DrawButton(S("haptic feedback", ": ", Accessibility.atack_haptic_feedback ? "enabled" : "disabled"), anchor.X, anchor.Y+5*n, hover: selector.is_on(0, 5), click: Input.Key_hold("A"), action: Input.Key_up("A"), spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left")) {
            Accessibility.atack_haptic_feedback = !Accessibility.atack_haptic_feedback;
        }

        // 6
        if (UI.DrawButton(S("high contrast", ": ", Accessibility.high_contrast ? "enabled" : "disabled"), anchor.X, anchor.Y+6*n, hover: selector.is_on(0, 6), click: Input.Key_hold("A"), action: Input.Key_up("A"), spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left")) {
            Accessibility.high_contrast = !Accessibility.high_contrast;
        }

        // 7
        if (UI.DrawButton(S("navigation sounds", ": ", Accessibility.navigation_cue ? "enabled" : "disabled"), anchor.X, anchor.Y+7*n, hover: selector.is_on(0, 7), click: Input.Key_hold("A"), action: Input.Key_up("A"), spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left")) {
            Accessibility.navigation_cue = !Accessibility.navigation_cue;
        }

        // 8
        if (UI.DrawButton(S("spacialized audio", ": ", Accessibility.spacialized_audio ? "enabled" : "disabled"), anchor.X, anchor.Y+8*n, hover: selector.is_on(0, 8), click: Input.Key_hold("A"), action: Input.Key_up("A"), spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left")) {
            Accessibility.spacialized_audio = !Accessibility.spacialized_audio;
        }

        // 9
        if (UI.DrawButton(S("(TEST)", " ", "Cover screen", ": ", Accessibility.cover_screen ? "enabled" : "disabled"), anchor.X, anchor.Y+9*n, hover: selector.is_on(0, 9), click: Input.Key_hold("A"), action: Input.Key_up("A"), spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left")) {
            Accessibility.cover_screen = !Accessibility.cover_screen;
        }

        // 10
        if (UI.DrawButton(S("save and exit"), anchor.X, anchor.Y+12*n, hover: selector.is_on(0, 10), click: Input.Key_hold("A"), action: Input.Key_up("A"), spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left")) {
            Config.SaveToFile();
            Camera.LockCamera();
            Program.ChangeState(Program.previous_state);
            selector.pointer = new Vector2i(0, 0);
        }

        if (Input.Key_down("B")) selector.pointer = new Vector2i(0, selector.options.Count - 1);
    }
}

public class WGAccessibilitySounds : Widget {
    private static Sprite bg;
    private static Selector selector;
    public static Sound current_sound;
    
    private const int POSITION_x = -185;
    private const int POSITION_Y = -74;
    private const int ROWS = 12;
    private const int ROW_SPACING = 10;
    private const int ROLL_OFFSET = 6;

    private static List<String[]> keys; // ID, label, sound

    public WGAccessibilitySounds() {
        bg = new Sprite(Data.textures["screens:accessibility_bg"]);
        keys = new List<String[]> {
            new[] {"Players distance", "normal side", "accessibility:beepSAB"},
            new[] {"Players distance", "inverted side", "accessibility:beepSBA"},

            new[] {"atack height", "stand", "accessibility:toneA"},
            new[] {"atack height", "low", "accessibility:toneB"},
            new[] {"atack height", "air", "accessibility:toneC"},

            new[] {"fall/get up", "fall", "accessibility:falling"},
            new[] {"fall/get up", "get up", "accessibility:wake_up"},

            new[] {"interface", "aura bar full", "ui:aura_full"},
            new[] {"interface", "aura bar half", "ui:aura_half"},

            new[] {"blow", "hit", "hitspark:on_hit_light"},
            new[] {"blow", "block", "hitspark:on_block"},
            new[] {"blow", "parry", "hitspark:parry"},
            new[] {"blow", "grab", "hitspark:grab"},
            new[] {"blow", "tech throw", "hitspark:tech"},

            new[] {"generic", "step", "generic:step"},
            new[] {"generic", "jump", "generic:jump"},
            new[] {"generic", "landing", "generic:step1"},
            new[] {"generic", "dash", "generic:dash"},
            new[] {"generic", "knockdown", "generic:falling"},
            new[] {"generic", "aura use", "generic:ex"},
        };
        selector = new Selector(Enumerable.Repeat(1, keys.Count).ToList(), endless_roll: false);
    }

    public override void Render() {
        Camera.UnlockCamera();
        Program.window.Draw(bg);
        selector.Update();
        Program.stage?.StopMusic(); 

        UI.DrawText(S("sound dictionary"), -80, -107, spacing: Config.spacing_medium, TTS: true, TTS_id: "AStitle", priority: true);

        for (int i = 0; i < ROWS; i++) {
            int j = Math.Max(0, Math.Min(selector.pointer.Y + ROLL_OFFSET - ROWS, selector.options.Count - ROWS)) + i;
            RenderPlayer(keys[j][0], keys[j][1], "sound"+j , keys[j][2], selector.is_on(0, j), POSITION_x, POSITION_Y + i * ROW_SPACING);
        }

        UI.DrawText(S("Q"), 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(S("Return"), 182, 67, tts: false, spacing: Config.spacing_small, alignment: "right", click: Input.Key_hold("B"), action: Input.Key_up("B"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.previous_state);
            Camera.LockCamera();
            Program.stage?.PlayMusic();
            selector.pointer = new Vector2i(0, 0);
        }
    }

    public void RenderPlayer(string label, string value, string id, string sound, bool selected, float X, float Y) { 
        UI.DrawText(S(label, ": ", value), X, Y, spacing: Config.spacing_small, alignment: "Left", TTS: selected, TTS_id: id, priority: false, textureName: selected ? "default small" : "default small hover");

        if (selected && Input.Key_down("A")) {
            current_sound = new Sound(Data.sounds[sound]) {Volume = Config.Main_Volume};
            WGAccessibilitySounds.current_sound.Play();
        }
    }
}
