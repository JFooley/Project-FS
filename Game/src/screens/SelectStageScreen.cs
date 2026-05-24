using UI_space;
using SFML.Graphics;

public class WGSelectStage : Widget {
    public static Sprite stage_thumb = new Sprite(Program.stage?.thumb);
    private static Sprite frame = new Sprite(Data.textures["screens:frame"]);
    private static Selector selector = new Selector(new List<int>(){ Data.stages.Count });

    public override void Render() {
        selector.Update();
        stage_thumb.Texture = Data.stages[selector.pointer.X].thumb;
        Program.window.Draw(stage_thumb);
        Program.window.Draw(frame);
        
        Accessibility.Speak("SS", TTSRequisition.TEXT, true, S("select stage"));
        for (int i = 0; i < Data.stages.Count; i++) UI.DrawText(S(i == selector.pointer.X ? ")" : "("), (i * 10) - ((Data.stages.Count - 1) * 5), -80, textureName: "icons");
        
        // draw texts
        UI.DrawText(S(Program.playerA_wins.ToString()), -Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "left");
        UI.DrawText(S(Program.playerB_wins.ToString()), Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "right");

        UI.DrawText(S("Q"), 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(S("Return"), 182, 67, tts: false, spacing: Config.spacing_small, alignment: "right", click: Input.Key_hold("B"), action: Input.Key_up("B"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.MainMenu);
            selector.pointer.X = 0;
        }

        if (UI.DrawButton(S(Data.stages[selector.pointer.X].name), 0, -95, spacing: Config.spacing_medium, click: Input.Key_hold("A"), action: Input.Key_up("A"), click_font: "default medium click", hover_font: "default medium white")) {
            if (Data.stages[selector.pointer.X].name == "Random")
                selector.pointer.X = AI.rand.Next(1, Data.stages.Count() - 2);
            Program.stage = Data.stages[selector.pointer.X];
            Program.ChangeState(Program.SelectChar);
            selector.pointer.X = 0;
        }
    }
}