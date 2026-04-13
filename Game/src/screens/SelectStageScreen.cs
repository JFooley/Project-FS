using UI_space;
using SFML.Graphics;
using Language_space;

public class WGSelectStage : Widget {
    public Sprite stage_thumb = new Sprite(Program.stage?.thumb);
    private Sprite frame = new Sprite(Data.textures["screens:frame"]);
    int pointer = 0;

    public override void Render() {
        stage_thumb.Texture = Data.stages[pointer].thumb;
        Program.window.Draw(stage_thumb);
        Program.window.Draw(frame);
        for (int i = 0; i < Data.stages.Count; i++) 
            UI.DrawText(i == pointer ? ")" : "(", (i * 10) - ((Data.stages.Count - 1) * 5), -80, textureName: "icons");
        
        // draw texts
        UI.DrawText(Program.playerA_wins.ToString(), -Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "left");
        UI.DrawText(Program.playerB_wins.ToString(), Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "right");

        UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
        if (UI.DrawButton(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: Input.Key_hold("LB"), action: Input.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.MainMenu);
            pointer = 0;
        }

        UI.DrawText("F", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(Language.GetText("Controls"), 182, 67, spacing: Config.spacing_small, alignment: "right", click: Input.Key_hold("RB"), action: Input.Key_up("RB"), click_font: "default small click", hover_font: "default small")) 
            Program.ChangeState(Program.Controls);

        if (Input.Key_down("Left") || (Input.Key_hold_for("Left", Config.hold_time) && UI.Clock(Config.hold_clock)))
            pointer = pointer <= 0 ? Data.stages.Count - 1 : pointer - 1;
        else if (Input.Key_down("Right") || (Input.Key_hold_for("Right", Config.hold_time) && UI.Clock(Config.hold_clock)))
            pointer = pointer >= Data.stages.Count - 1 ? 0 : pointer + 1;

        if (UI.DrawButton(Language.GetText(Data.stages[pointer].name), 0, -95, spacing: Config.spacing_medium, click: Input.faceButtonHold, action: Input.faceButtonUp, click_font: "default medium click", hover_font: "default medium white")) {
            if (Data.stages[pointer].name == "Random")
                pointer = AI.rand.Next(1, Data.stages.Count() - 2);
            Program.stage = Data.stages[pointer];
            Program.ChangeState(Program.SelectChar);
            pointer = 0;
        }
    }
}