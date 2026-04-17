using Language_space;
using UI_space;
using SFML.Graphics;

public class WGPostBattle : Widget {
    Sprite stage_thumb = new Sprite(Program.stage?.thumb);
    Sprite fade90 = new Sprite(Data.textures["screens:90fade"]) {Color = new Color(255, 255, 255, 230)};
    string winner_text = "";
    int pointer = 0;

    public override void Render() {
        stage_thumb.Texture = Program.stage?.thumb;
        Program.window.Draw(stage_thumb);
        Program.window.Draw(fade90);

        if (Program.stage?.music != null) Program.stage.music.Volume = Math.Max(0, Program.stage.music.Volume - 0.5f);

        if (Program.winner == Program.Drawn) winner_text = Language.GetText("Drawn");
        else winner_text = Language.GetText("Player") + " " + Program.winner + " " + Language.GetText("Wins");

        UI.DrawText(Program.playerA_wins.ToString(), -Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "left");
        UI.DrawText(Program.playerB_wins.ToString(), Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "right");
        UI.DrawText(winner_text, 0, -100, spacing: Config.spacing_medium, textureName: "default medium");

        // Change option
        if ((Input.Key_down("Up") || (Input.Key_hold_for("Up", Config.hold_time) && UI.Clock(Config.hold_clock))) && pointer > 0)
            pointer -= 1;
        else if ((Input.Key_down("Down") || (Input.Key_hold_for("Down", Config.hold_time) && UI.Clock(Config.hold_clock))) && pointer < 2)
            pointer += 1;

        // Do option
        if (UI.DrawButton(Language.GetText("rematch"), 0, 0, spacing: Config.spacing_medium, action: Input.faceButtonUp, click: Input.faceButtonHold, hover: pointer == 0, font: "default medium", hover_font: "default medium hover", click_font: "default medium click")) {
            Camera.SetChars(Program.stage?.character_A, Program.stage?.character_B);
            Camera.SetLimits(Program.stage.length, Program.stage.height);
            Program.stage?.LockPlayers();
            Program.ChangeState(Program.Battle);
            pointer = 0;

        } if (UI.DrawButton(Language.GetText("menu"), 0, 20, spacing: Config.spacing_medium, action: Input.faceButtonUp, click: Input.faceButtonHold, hover: pointer == 1, font: "default medium", hover_font: "default medium hover", click_font: "default medium click")) { 
            Program.stage?.StopMusic();
            Program.stage?.UnloadStage();
            Input.ResetAI();
            Program.ChangeState(Program.MainMenu);
            pointer = 0;

        } if (UI.DrawButton(Language.GetText("exit game"), 0, 40, spacing: Config.spacing_medium, action: Input.faceButtonUp, click: Input.faceButtonHold, hover: pointer == 2, font: "default medium", hover_font: "default medium hover", click_font: "default medium click"))
            Program.window.Close();
    }
}