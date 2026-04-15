using UI_space;
using Language_space;
using SFML.Graphics;
using SFML.System;

public class WGAccessibilityMenu : Widget {
    private Sprite bg = new Sprite(Data.textures["screens:accessibility_bg"]);

    private int pointer = 0;
    private int size = 5;
    private Vector2i anchor = new Vector2i(-170, -74);
    private int n = 10;

    public override void Render() {
        Camera.UnlockCamera();
        Program.window.Draw(bg);

        UI.DrawText(Language.GetText("accessibility"), -80, -107, spacing: Config.spacing_medium);
        
        // 0
        if (UI.DrawButton(Language.GetText("text-to-speech") + ": " + Language.GetText(Accessibility.TTS ? "enabled" : "disabled"), anchor.X, anchor.Y, hover: this.pointer == 0, click: Input.faceButtonHold, action: Input.faceButtonUp, spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left"))
            Accessibility.TTS = !Accessibility.TTS;

        // 1
        UI.DrawButton(Language.GetText("tts speed") + ": < " + Accessibility.TTS_speed + "x >", anchor.X, anchor.Y+n, hover: this.pointer == 1, click: Input.Key_hold("Left") || Input.Key_hold("Right"), action: Input.Key_hold("Left") || Input.Key_hold("Right"), spacing: Config.spacing_small, click_font: Accessibility.TTS ? "default small click" : "default small grad", hover_font: Accessibility.TTS ? "default small hover" : "default small grad", font: "default small", alignment: "Left");
        if (this.pointer == 1 && Accessibility.TTS) {
            if (Input.Key_up("Left") || (Input.Key_hold_for("Left", Config.hold_time) && UI.Clock(Config.hold_clock))) 
                Accessibility.TTS_speed -= 0.1f;
            else if (Input.Key_up("Right") || (Input.Key_hold_for("Right", Config.hold_time) && UI.Clock(Config.hold_clock))) 
                Accessibility.TTS_speed += 0.1f;
        }

        // 2
        if (UI.DrawButton(Language.GetText("high contrast") + ": " + Language.GetText(Accessibility.high_contrast ? "enabled" : "disabled"), anchor.X, anchor.Y+2*n, hover: this.pointer == 2, click: Input.faceButtonHold, action: Input.faceButtonUp, spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left"))
            Accessibility.high_contrast = !Accessibility.high_contrast;

        // 3
        if (UI.DrawButton(Language.GetText("atack feedback") + ": " + Language.GetText(Accessibility.atack_feedback ? "enabled" : "disabled"), anchor.X, anchor.Y+3*n, hover: this.pointer == 3, click: Input.faceButtonHold, action: Input.faceButtonUp, spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left"))
            Accessibility.atack_feedback = !Accessibility.atack_feedback;
        
        // 4
        if (UI.DrawButton(Language.GetText("defense feedback") + ": " + Language.GetText(Accessibility.defend_feedback ? "enabled" : "disabled"), anchor.X, anchor.Y+4*n, hover: this.pointer == 4, click: Input.faceButtonHold, action: Input.faceButtonUp, spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left"))
            Accessibility.defend_feedback = !Accessibility.defend_feedback;

        // 5
        if (UI.DrawButton(Language.GetText("save and exit"), anchor.X, anchor.Y+6*n, hover: this.pointer == 5, click: Input.faceButtonHold, action: Input.faceButtonUp, spacing: Config.spacing_small, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "Left")) {
            Accessibility.SaveToFile();
            Camera.LockCamera();
            Program.ChangeState(Program.last_game_state);
            pointer = 0;
        }

        // Change option 
        if (Input.Key_down("Up") || (Input.Key_hold_for("Up", Config.hold_time) && UI.Clock(Config.hold_clock))) {
            this.pointer = this.pointer <= 0 ? size : pointer - 1;
        } else if (Input.Key_down("Down") || (Input.Key_hold_for("Down", Config.hold_time) && UI.Clock(Config.hold_clock))) {
            this.pointer = this.pointer >= size ? 0 : pointer + 1;
        }
    }
}

public class WGAccessibilitySounds : Widget {
    private Sprite bg = new Sprite(Data.textures["screens:accessibility_bg"]);
    private int pointer = 0;
    private int size = 0;

    public override void Render() {
        Camera.UnlockCamera();
        Program.window.Draw(bg);

        UI.DrawText(Language.GetText("Sound's list"), -80, -107, spacing: Config.spacing_medium);

        // Change option 
        if (Input.Key_down("Up") || (Input.Key_hold_for("Up", Config.hold_time) && UI.Clock(Config.hold_clock))) {
            this.pointer = this.pointer <= 0 ? size : pointer - 1;
        } else if (Input.Key_down("Down") || (Input.Key_hold_for("Down", Config.hold_time) && UI.Clock(Config.hold_clock))) {
            this.pointer = this.pointer >= size ? 0 : pointer + 1;
        }

        UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
        if (UI.DrawButton(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: Input.Key_hold("LB"), action: Input.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.last_game_state);
            Camera.LockCamera();
            this.pointer = 0;
        }
    }
}