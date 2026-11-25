using SFML.System;
using SFML.Graphics;
using UI_space;

public class WGPause : Widget {
    private Selector s_main = new Selector(new List<int>{1, 1, 1, 1, 1});
    private Selector s_tra = new Selector(new List<int>{1, 1, 2, 2, 2, 1});
    private Selector s_acc = new Selector(new List<int>{1, 1, 1});
    private int sub_menu = 0;
    private Stage stage;

    private Sprite fade90;

    public WGPause(Stage stage) {
        this.stage = stage;
        fade90 = new Sprite(Data.textures["screens:90fade"]) {Color = new Color(255, 255, 255, 230)};
    }

    public override void Render() {
        fade90.Position = new Vector2f(Camera.X - Config.RenderWidth/2, Camera.Y - Config.RenderHeight/2);
        MainRender.Draw(fade90, MainRender.InterfaceLayer);

        var face_release = Input.Key_up("A");
        var face_hold = Input.Key_hold("A");

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
        s_main.options = Stage.training_mode ? new List<int>{1, 1, 1, 1, 1} : new List<int>{1, 1, 1, 1};

        s_main.Update();
        UI.DrawTextAtlas(S("Pause"), 0, -75, TTS: true, TTS_id: "pau", priority: true, spacing: Config.spacing_medium, textureName: "default medium");

        // 0 
        if (UI.DrawButton(S("Settings"), 0, -45, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_main.is_on(0, 0), click_font: "default medium click", hover_font: "default medium", font: "default medium hover"))
            Program.ChangeState(Program.State.Settings);
        // 1
        if (UI.DrawButton(S("Controls"), 0, -30, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_main.is_on(0, 1), click_font: "default medium click", hover_font: "default medium", font: "default medium hover"))
            Program.ChangeState(Program.State.Controls);
        // 2
        if (UI.DrawButton(S("Accessibility"), 0, -15, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_main.is_on(0, 2), click_font: "default medium click", hover_font: "default medium", font: "default medium hover")) {
            Program.ChangeState(Program.State.AccessibilityMenu);
        }
        // 3
        if (Stage.training_mode && UI.DrawButton(S("Training settings"), 0, 0, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: Stage.training_mode && s_main.is_on(0, 3), click_font: "default medium click", hover_font: "default medium", font: "default medium hover")) {
            this.sub_menu = 1;
            s_main.pointer = new Vector2i(0, 0);
        }
        // 4
        if (UI.DrawButton(S("End match"), 0, 70, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_main.is_on(0, Stage.training_mode ? 4 : 3), click_font: "default medium click", hover_font: "default medium red", font: "default medium hover")) {
            this.stage.Pause();
            WGBattle.match_winner = WGBattle.Drawn;
            WGBattle.battle_state = WGBattle.State.MatchEnd;
            Stage.show_boxs = false;
            Stage.block = Stage.BlockMode.Never;
            Stage.refil_life = true;
            Stage.refil_super = true;
            s_main.pointer = new Vector2i(0, 0);
        }
    }
    private void Training(bool face_hold, bool face_release) {
        s_tra.Update();
        UI.DrawTextAtlas(S("Training settings"), 0, -75,  TTS: true, TTS_id: "traS", priority: true, spacing: Config.spacing_medium, textureName: "default medium");
        int spacing = 10;
        int anchor = -45; 

        // 0
        if (UI.DrawButton(S("Reset Characters"), 0, anchor + spacing*0, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(0, 0), click_font: "default small click", hover_font: "default small", font: "default small hover"))
            this.stage.ResetPlayers();
        // 1
        if (UI.DrawButton(S("Hitboxes",  ": ", Stage.show_boxs ? "Show" : "Hide"), 0, anchor + spacing*1, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(0, 1), click_font: "default small click", hover_font: "default small", font: "default small hover"))
            Stage.show_boxs = !Stage.show_boxs;
        // 2.0
        if (UI.DrawButton(Stage.block switch {
                Stage.BlockMode.Never => S("Block", ": ", "Never"),
                Stage.BlockMode.AfterHit => S("Block", ": ", "After hit"),
                Stage.BlockMode.Always => S("Block", ": ", "Always"),
                _ => S("Block", ": ", "Error")
            }, 0, anchor + spacing*2, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(0, 2), click_font: "default small click", hover_font: "default small", font: "default small hover", alignment: UI.Alignment.Right))
            Stage.block = Stage.block switch {
                Stage.BlockMode.Never => Stage.BlockMode.AfterHit,
                Stage.BlockMode.AfterHit => Stage.BlockMode.Always,
                _ => Stage.BlockMode.Never
            };
        // 2.1
        if (UI.DrawButton(Stage.parry switch {
                Stage.ParryMode.Never => S("Parry", ": ", "Never"),
                Stage.ParryMode.AfterHit => S("Parry", ": ", "After hit"),
                Stage.ParryMode.Always => S("Parry", ": ", "Always"),
                _ => S("Parry", ": ", "Error")
            }, 0, anchor + spacing*2, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(1, 2), click_font: "default small click", hover_font: "default small", font: "default small hover", alignment: UI.Alignment.Left))
            Stage.parry = Stage.parry switch {
                Stage.ParryMode.Never => Stage.ParryMode.AfterHit,
                Stage.ParryMode.AfterHit => Stage.ParryMode.Always,
                _ => Stage.ParryMode.Never
            };
        // 3.0
        if (UI.DrawButton(Stage.refil_life ? S("Life", ": ", "Refil") : S("Life", ": ", "Keep"), 0, anchor + spacing*3, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(0, 3), click_font: "default small click", hover_font: "default small", font: "default small hover", alignment: UI.Alignment.Right))
            Stage.refil_life = !Stage.refil_life;
        // 3.1
        if (UI.DrawButton(Stage.refil_super ? S("Super", ": ", "Refil") : S("Super", ": ", "Keep"), 0, anchor + spacing*3, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(1, 3), click_font: "default small click", hover_font: "default small", font: "default small hover", alignment: UI.Alignment.Left))
            Stage.refil_super = !Stage.refil_super;
        // 4.0
        if (UI.DrawButton(Stage.AI_playerA ? S("player 1", " ", "BOT", ": ", "on") : S("player 1", " ", "BOT", ": ", "off"), -0, anchor + spacing*4, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(0 ,4), click_font: "default small click", hover_font: "default small", font: "default small hover", alignment: UI.Alignment.Right)) {
            Stage.AI_playerA = !Stage.AI_playerA;
            Program.stage.character_A.BotEnabled = Stage.AI_playerA;
            Program.stage.character_A.AIEnabled = Stage.AI_playerA;
        }
        // 4.1
        if (UI.DrawButton(Stage.AI_playerB ? S("player 2", " ", "BOT", ": ", "on") : S("player 2", " ", "BOT", ": ", "off"), +0, anchor + spacing*4, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(1 ,4), click_font: "default small click", hover_font: "default small", font: "default small hover", alignment: UI.Alignment.Left)) {
            Stage.AI_playerB = !Stage.AI_playerB;
            Program.stage.character_B.BotEnabled = Stage.AI_playerB;
            Program.stage.character_B.AIEnabled = Stage.AI_playerB;
        }
            
        // 5
        if (UI.DrawButton(S("return"), 0, anchor + spacing*6, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: s_tra.is_on(0, 5), click_font: "default small click", hover_font: "default small", font: "default small hover") || Input.Key_down("B")) {
            s_tra.pointer = new Vector2i(0, 0);
            this.sub_menu = 0;
        }

    }
    // Unused
    private void Accessibility(bool face_hold, bool face_release) {
        s_acc.Update();
        UI.DrawTextAtlas(S("accessibility"), 0, -75, TTS: true, TTS_id: "acc", priority: true, spacing: Config.spacing_medium, textureName: "default medium");
        
        // 0 
        if (UI.DrawButton(S("accessibility settings"), 0, -45, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_acc.is_on(0, 0), click_font: "default medium click", hover_font: "default medium", font: "default medium hover"))
            Program.ChangeState(Program.State.AccessibilityMenu);
        // 1
        if (UI.DrawButton(S("sound dictionary"), 0, -30, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_acc.is_on(0, 1), click_font: "default medium click", hover_font: "default medium", font: "default medium hover"))
            Program.ChangeState(Program.State.AccessibilitySounds);
        // 2
        if (UI.DrawButton(S("return"), 0, 70, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: s_acc.is_on(0, 2), click_font: "default medium click", hover_font: "default medium", font: "default medium hover") || Input.Key_down("B")) {
            s_acc.pointer = new Vector2i(0, 0);
            sub_menu = 0;
        }
    }
}
