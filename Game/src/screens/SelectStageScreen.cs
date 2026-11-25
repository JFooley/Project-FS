using UI_space;
using SFML.Graphics;

public class WGSelectStage : Widget {
    public static Sprite stage_thumb;
    private static Sprite frame;
    private static Selector selector = new Selector(new List<int>(){ Data.stages.Count });

    public WGSelectStage() {
        frame = new Sprite(Data.textures["screens:frame"]);
        stage_thumb = new Sprite(Program.stage?.thumb);
    }

    public override void Render() {
        selector.Update();
        stage_thumb.Texture = Data.stages[selector.pointer.X].thumb;
        MainRender.Draw(stage_thumb, MainRender.BackgroundLayer);
        MainRender.Draw(frame, MainRender.InterfaceLayer);
        
        Accessibility.Speak("SS", TTSRequisition.TEXT, true, S("select stage"));
        for (int i = 0; i < Data.stages.Count; i++) UI.DrawTextAtlas(S(i == selector.pointer.X ? ")" : "("), (i * 10) - ((Data.stages.Count - 1) * 5), -80, textureName: "icons");
        
        // draw texts
        UI.DrawTextAtlas(S(Program.playerA_wins.ToString()), -Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: UI.Alignment.Left);
        UI.DrawTextAtlas(S(Program.playerB_wins.ToString()), Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: UI.Alignment.Right);

        UI.DrawTextAtlas(S("Q"), 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: UI.Alignment.Right);
        if (UI.DrawButton(S("Return"), 182, 67, tts: false, spacing: Config.spacing_small, alignment: UI.Alignment.Right, click: Input.Key_hold("B"), action: Input.Key_up("B"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.State.MainMenu);
            selector.pointer.X = 0;
        }

        if (UI.DrawButton(S(Data.stages[selector.pointer.X].name), 0, -95, spacing: Config.spacing_medium, click: Input.Key_hold("A"), action: Input.Key_up("A"), click_font: "default medium click", hover_font: "default medium white")) {
            if (Data.stages[selector.pointer.X].name == "Random")
                selector.pointer.X = AI.rand.Next(1, Data.stages.Count() - 2);
            Program.stage = Data.stages[selector.pointer.X];
            Program.ChangeState(Program.State.SelectChar);
            selector.pointer.X = 0;
        }
    }
}