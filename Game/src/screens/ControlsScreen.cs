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
        Program.window.Draw(bg);
        Program.window.Draw(frame);

        UI.DrawText(S("Q"), 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(S("Return"), 182, 67, tts: false, alignment: "right", action: Input.Key_up("B"), click: Input.Key_hold("B"), hover_font: "default small")) {
            if (Program.previous_state == Program.Battle) Camera.LockCamera();
            Program.ChangeState(Program.previous_state);
        }
    }
}