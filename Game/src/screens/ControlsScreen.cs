using Language_space;
using UI_space;
using SFML.Graphics;

public class WGControls : Widget {
    Sprite frame = new Sprite(Data.textures["screens:frame"]);
    Sprite bg = new Sprite(Data.textures["screens:controls_0"]);
    private static Selector selector = new Selector(new List<int> {2});

    public override void Render() {
        if (Camera.isLocked) Camera.UnlockCamera();
        selector.Update();

        bg.Texture = new Sprite(Data.textures["screens:controls_" + selector.pointer.X]).Texture;
        Program.window.Draw(bg);
        Program.window.Draw(frame);

        UI.DrawText("Q", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(Language.GetText("Return"), 182, 67, alignment: "right", action: Input.Key_up("B"), click: Input.Key_hold("B"), hover_font: "default small")) {
            if (Program.last_game_state == Program.Battle) Camera.LockCamera();
            Program.ChangeState(Program.last_game_state);
        }
    }
}