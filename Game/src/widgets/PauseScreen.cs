using Language_space;
using SFML.System;
using UI_space;

public class WGPause : Widget {
    private Vector2i pointer;
    private int sub_menu;
    private Stage stage;

    public WGPause(Stage stage) {
        this.stage = stage;
        this.pointer = new Vector2i(0, 0);
        this.sub_menu = 0;
    }

    public override void Render() {
        var face_release = InputManager.faceButtonUp;
        var face_hold = InputManager.faceButtonHold;

        switch (sub_menu) {
            case 0:
                Main(face_hold, face_release);
                break;
            case 1:
                Training(face_hold, face_release);
                break;
        }
    }

    private void Main(bool face_hold, bool face_release) {
        UI.DrawText(Language.GetText("Pause"), 0, -75, spacing: Config.spacing_medium, textureName: "default medium");

        if (UI.DrawButton(Language.GetText("Settings"), 0, -45, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: this.pointer.Y == 0, click_font: "default medium click", hover_font: "default medium hover", font: "default medium"))
            Program.ChangeState(Program.Settings);
        
        if (UI.DrawButton(Language.GetText("Controls"), 0, -30, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: this.pointer.Y == 1, click_font: "default medium click", hover_font: "default medium hover", font: "default medium"))
            Program.ChangeState(Program.Controls);
        
        if (Stage.training_mode && UI.DrawButton(Language.GetText("Training settings"), 0, -15, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: this.pointer.Y == 2, click_font: "default medium click", hover_font: "default medium hover", font: "default medium")) {
            this.sub_menu = 1;
            this.pointer = new Vector2i(0, 0);
        }

        if (UI.DrawButton(Language.GetText("End match"), 0, 70, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: this.pointer.Y == (Stage.training_mode ? 3 : 2), click_font: "default medium click", hover_font: "default medium red", font: "default medium")) {
            this.stage.Pause();
            Program.winner = Program.Drawn;
            Program.sub_state = Program.MatchEnd;
            Stage.show_boxs = false;
            Stage.block = 0;
            Stage.refil_life = true;
            Stage.refil_super = true;
            this.pointer = new Vector2i(0, 0);
        }       

        // Change option 
        if (InputManager.Key_down("Up") && this.pointer.Y > 0) {
            this.pointer.Y -= 1;
        } else if (InputManager.Key_down("Down") && this.pointer.Y < (Stage.training_mode ? 3 : 2)) {
            this.pointer.Y += 1;
        }
    }
    private void Training(bool face_hold, bool face_release) {
        UI.DrawText(Language.GetText("Training settings"), 0, -75, spacing: Config.spacing_medium, textureName: "default medium");
        int spacing = 10;
        int anchor = -45;

        // 0
        if (UI.DrawButton(Language.GetText("Reset Characters"), 0, anchor + spacing*0, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pointer.Y == 0, click_font: "default small click", hover_font: "default small hover", font: "default small"))
            this.stage.ResetPlayers();
        // 1
        if (UI.DrawButton(Language.GetText("Show hitboxes"), 0, anchor + spacing*1, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pointer.Y == 1, click_font: "default small click", hover_font: "default small hover", font: "default small"))
            Stage.show_boxs = !Stage.show_boxs;
        // 2.0
        if (UI.DrawButton(Stage.block switch { 0 => Language.GetText("Block") + ": " + Language.GetText("Never"), 1 => Language.GetText("Block") + ": " + Language.GetText("After hit"), 2 => Language.GetText("Block") + ": " + Language.GetText("Always"), _ => Language.GetText("Block") + ": " + Language.GetText("Error")}, 0, anchor + spacing*2, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pointer.X == 0 && this.pointer.Y == 2, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "right"   ))
            Stage.block = Stage.block >= 2 ? 0 : Stage.block + 1;
        // 2.1
        if (UI.DrawButton(Stage.parry switch { 0 => Language.GetText("Parry") + ": " + Language.GetText("Never"), 1 => Language.GetText("Parry") + ": " + Language.GetText("After hit"), 2 => Language.GetText("Parry") + ": " + Language.GetText("Always"), _ => Language.GetText("Parry") + ": " + Language.GetText("Error")}, 0, anchor + spacing*2, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pointer.X == 1 && this.pointer.Y == 2, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "left"   ))
            Stage.parry = Stage.parry >= 2 ? 0 : Stage.parry + 1;
        // 3.0
        if (UI.DrawButton(Stage.refil_life ? Language.GetText("Life") + ": " + Language.GetText("Refil") : Language.GetText("Life") + ": " + Language.GetText("Keep"), 0, anchor + spacing*3, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pointer.X == 0 && this.pointer.Y == 3, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "right"))
            Stage.refil_life = !Stage.refil_life;
        // 3.1
        if (UI.DrawButton(Stage.refil_super ? Language.GetText("Super") + ": " + Language.GetText("Refil") : Language.GetText("Super") + ": " + Language.GetText("Keep"), 0, anchor + spacing*3, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pointer.X == 1 && this.pointer.Y == 3, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "left"))
            Stage.refil_super = !Stage.refil_super;
        // 4.0
        if (UI.DrawButton(Program.AI_playerA ? Language.GetText("player") + " 1 BOT : " + Language.GetText("enabled") : Language.GetText("player") + " 1 BOT: " + Language.GetText("disabled"), -0, anchor + spacing*4, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pointer.X == 0 && this.pointer.Y == 4, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "right")) {
            Program.AI_playerA = !Program.AI_playerA;
            Program.stage.character_A.BotEnabled = Program.AI_playerA;
            Program.stage.character_A.AIEnabled = Program.AI_playerA;
        }
        // 4.1
        if (UI.DrawButton(Program.AI_playerB ? Language.GetText("player") + " 2 BOT : " + Language.GetText("enabled") : Language.GetText("player") + " 2 BOT: " + Language.GetText("disabled"), +0, anchor + spacing*4, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pointer.X == 1 && this.pointer.Y == 4, click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "left")) {
            Program.AI_playerB = !Program.AI_playerB;
            Program.stage.character_B.BotEnabled = Program.AI_playerB;
            Program.stage.character_B.AIEnabled = Program.AI_playerB;
        }
            
        // 5
        if (UI.DrawButton(Language.GetText("return"), 0, anchor + spacing*6, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pointer.Y == 5, click_font: "default small click", hover_font: "default small hover", font: "default small")) {
            this.pointer = new Vector2i(0, 0);
            this.sub_menu = 0;
        }
        // Change option 
        if (InputManager.Key_down("Up") && this.pointer.Y > 0) {
            this.pointer.Y -= 1;
        } else if (InputManager.Key_down("Down") && this.pointer.Y < 5) {
            this.pointer.Y += 1;
        }

        if (InputManager.Key_down("Left") && this.pointer.X > 0) {
            this.pointer.X -= 1;
        } else if (InputManager.Key_down("Right") && this.pointer.X < 1) {
            this.pointer.X += 1;
        }
    }
}
