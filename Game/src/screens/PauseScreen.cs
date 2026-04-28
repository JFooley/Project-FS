using Language_space;
using SFML.System;
using SFML.Graphics;
using UI_space;

public class WGPause : Widget {
    private Selector s_main = new Selector(new List<int>{1, 1, 1, 1, 1});
    private Selector s_tra = new Selector(new List<int>{1, 1, 2, 2, 2, 1});
    private Selector s_acc = new Selector(new List<int>{1, 1, 1});
    private int sub_menu = 0;
    private Stage stage;

    private Sprite fade90 = new Sprite(Data.textures["screens:90fade"]) {Color = new Color(255, 255, 255, 230)};

    public WGPause(Stage stage) {
        this.stage = stage;
    }

    public override void Render() {
        fade90.Position = new Vector2f(Camera.X - Config.RenderWidth/2, Camera.Y - Config.RenderHeight/2);
        Program.window.Draw(fade90);

        var face_release = Input.Key_up("A");
        var face_hold = Input.Key_hold("A");

        switch (sub_menu) {
            case 0:
                Main(face_hold, face_release);
                break;
            case 1:
                Training(face_hold, face_release);
                break;
            case 2:
                Accessibility(face_hold, face_release);
                break;
        }
    }

    private void Main(bool face_hold, bool face_release) {
        s_main.options = Stage.training_mode ? new List<int>{1, 1, 1, 1, 1} : new List<int>{1, 1, 1, 1};

        s_main.Update();
        UI.DrawText(Language.GetText("Pause"), 0, -75, spacing: Config.spacing_medium, textureName: "default medium");

        // 0 
        if (UI.DrawButton(Language.GetText("Settings"), 0, -45, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_main.is_on(0, 0), click_font: "default medium click", hover_font: "default medium hover", font: "default medium"))
            Program.ChangeState(Program.Settings);
        // 1
        if (UI.DrawButton(Language.GetText("Controls"), 0, -30, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_main.is_on(0, 1), click_font: "default medium click", hover_font: "default medium hover", font: "default medium"))
            Program.ChangeState(Program.Controls);
        // 2
        if (UI.DrawButton(Language.GetText("Accessibility"), 0, -15, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_main.is_on(0, 2), click_font: "default medium click", hover_font: "default medium hover", font: "default medium")) {
            this.sub_menu = 2;
            s_main.pointer = new Vector2i(0, 0);
        }
        // 3
        if (Stage.training_mode && UI.DrawButton(Language.GetText("Training settings"), 0, 0, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: Stage.training_mode && s_main.is_on(0, 3), click_font: "default medium click", hover_font: "default medium hover", font: "default medium")) {
            this.sub_menu = 1;
            s_main.pointer = new Vector2i(0, 0);
        }
        // 4
        if (UI.DrawButton(Language.GetText("End match"), 0, 70, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_main.is_on(0, Stage.training_mode ? 4 : 3), click_font: "default medium click", hover_font: "default medium red", font: "default medium")) {
            this.stage.Pause();
            Program.winner = Program.Drawn;
            WGBattle.battle_state = WGBattle.MatchEnd;
            Stage.show_boxs = false;
            Stage.block = 0;
            Stage.refil_life = true;
            Stage.refil_super = true;
            s_main.pointer = new Vector2i(0, 0);
        }

        if (Input.Key_down("B")) s_main.pointer = new Vector2i(0, Stage.training_mode ? 4 : 3);
    }
    private void Training(bool face_hold, bool face_release) {
        s_tra.Update();
        UI.DrawText(Language.GetText("Training settings"), 0, -75, spacing: Config.spacing_medium, textureName: "default medium");
        int spacing = 10;
        int anchor = -45; 

        // 0
        if (UI.DrawButton(Language.GetText("Reset Characters"), 0, anchor + spacing*0, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(0, 0), click_font: "default small click", hover_font: "default small hover", font: "default small"))
            this.stage.ResetPlayers();
        // 1
        if (UI.DrawButton(Language.GetText("Hitboxes") + ": " + (Stage.show_boxs ? Language.GetText("Show") : Language.GetText("Hide")), 0, anchor + spacing*1, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(0, 1), click_font: "default small click", hover_font: "default small hover", font: "default small"))
            Stage.show_boxs = !Stage.show_boxs;
        // 2.0
        if (UI.DrawButton(Stage.block switch { 0 => Language.GetText("Block") + ": " + Language.GetText("Never"), 1 => Language.GetText("Block") + ": " + Language.GetText("After hit"), 2 => Language.GetText("Block") + ": " + Language.GetText("Always"), _ => Language.GetText("Block") + ": " + Language.GetText("Error")}, 0, anchor + spacing*2, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(0, 2), click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "right"   ))
            Stage.block = Stage.block >= 2 ? 0 : Stage.block + 1;
        // 2.1
        if (UI.DrawButton(Stage.parry switch { 0 => Language.GetText("Parry") + ": " + Language.GetText("Never"), 1 => Language.GetText("Parry") + ": " + Language.GetText("After hit"), 2 => Language.GetText("Parry") + ": " + Language.GetText("Always"), _ => Language.GetText("Parry") + ": " + Language.GetText("Error")}, 0, anchor + spacing*2, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(1, 2), click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "left"   ))
            Stage.parry = Stage.parry >= 2 ? 0 : Stage.parry + 1;
        // 3.0
        if (UI.DrawButton(Stage.refil_life ? Language.GetText("Life") + ": " + Language.GetText("Refil") : Language.GetText("Life") + ": " + Language.GetText("Keep"), 0, anchor + spacing*3, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(0, 3), click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "right"))
            Stage.refil_life = !Stage.refil_life;
        // 3.1
        if (UI.DrawButton(Stage.refil_super ? Language.GetText("Super") + ": " + Language.GetText("Refil") : Language.GetText("Super") + ": " + Language.GetText("Keep"), 0, anchor + spacing*3, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(1, 3), click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "left"))
            Stage.refil_super = !Stage.refil_super;
        // 4.0
        if (UI.DrawButton(Stage.AI_playerA ? Language.GetText("player") + " 1 BOT : " + Language.GetText("on") : Language.GetText("player") + " 1 BOT: " + Language.GetText("off"), -0, anchor + spacing*4, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(0 ,4), click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "right")) {
            Stage.AI_playerA = !Stage.AI_playerA;
            Program.stage.character_A.BotEnabled = Stage.AI_playerA;
            Program.stage.character_A.AIEnabled = Stage.AI_playerA;
        }
        // 4.1
        if (UI.DrawButton(Stage.AI_playerB ? Language.GetText("player") + " 2 BOT : " + Language.GetText("on") : Language.GetText("player") + " 2 BOT: " + Language.GetText("off"), +0, anchor + spacing*4, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(1 ,4), click_font: "default small click", hover_font: "default small hover", font: "default small", alignment: "left")) {
            Stage.AI_playerB = !Stage.AI_playerB;
            Program.stage.character_B.BotEnabled = Stage.AI_playerB;
            Program.stage.character_B.AIEnabled = Stage.AI_playerB;
        }
            
        // 5
        if (UI.DrawButton(Language.GetText("return"), 0, anchor + spacing*6, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(0, 5), click_font: "default small click", hover_font: "default small hover", font: "default small") || Input.Key_down("B")) {
            s_tra.pointer = new Vector2i(0, 0);
            this.sub_menu = 0;
        }

    }
    private void Accessibility(bool face_hold, bool face_release) {
        s_acc.Update();
        UI.DrawText(Language.GetText("Accessibility"), 0, -75, spacing: Config.spacing_medium, textureName: "default medium");
        
        // 0 
        if (UI.DrawButton(Language.GetText("Accessibility menu"), 0, -45, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_acc.is_on(0, 0), click_font: "default medium click", hover_font: "default medium hover", font: "default medium"))
            Program.ChangeState(Program.AccessibilityMenu);
        // 1
        if (UI.DrawButton(Language.GetText("Sounds list"), 0, -30, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_acc.is_on(0, 1), click_font: "default medium click", hover_font: "default medium hover", font: "default medium"))
            Program.ChangeState(Program.AccessibilitySounds);
        // 2
        if (UI.DrawButton(Language.GetText("Return"), 0, 70, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_acc.is_on(0, 2), click_font: "default medium click", hover_font: "default medium hover", font: "default medium") || Input.Key_down("B")) {
            s_acc.pointer = new Vector2i(0, 0);
            sub_menu = 0;
        }
    }
}
