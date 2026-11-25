using UI_space;
using SFML.Graphics;

public class WGControls : Widget {
    Sprite frame;
    Sprite bg;
    private static Selector selector = new Selector(new List<int> {2});

    public WGControls() {
        frame = new Sprite(Data.textures["screens:frame"]);
        bg = new Sprite(Data.textures["screens:controls_0"]);
    }

    public override void Render() {
        if (Camera.isLocked) Camera.UnlockCamera();
        selector.Update();

        bg.Texture = new Sprite(Data.textures["screens:controls_" + selector.pointer.X]).Texture;
        MainRender.Draw(bg, MainRender.BackgroundLayer);
        MainRender.Draw(frame, MainRender.InterfaceLayer);

        UI.DrawTextAtlas(S("Q"), 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: UI.Alignment.Right);
        if (UI.DrawButton(S("Return"), 182, 67, tts: false, alignment: UI.Alignment.Right, action: Input.Key_up("B"), click: Input.Key_hold("B"), hover_font: "default small")) {
            if (Program.previous_state == Program.State.Battle) Camera.LockCamera();
            Program.ChangeState(Program.previous_state);
        }
    }
}