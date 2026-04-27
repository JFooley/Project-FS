using Language_space;
using UI_space;
using SFML.Graphics;

public class WGControls : Widget {
    Sprite frame = new Sprite(Data.textures["screens:frame"]);
    Sprite bg = new Sprite(Data.textures["screens:controls_0"]);
    int pointer = 0;

    public override void Render() {
        if (Camera.isLocked) Camera.UnlockCamera();
        
        bg.Texture = new Sprite(Data.textures["screens:controls_" + pointer]).Texture;
        Program.window.Draw(bg);
        Program.window.Draw(frame);

        if (Input.Key_up("Left") || (Input.Key_hold_for("Left", Config.hold_time) && UI.Clock(Config.hold_clock))) 
            pointer = pointer < 1 ? pointer + 1 : 0;
        else if (Input.Key_up("Right") || (Input.Key_hold_for("Right", Config.hold_time) && UI.Clock(Config.hold_clock))) 
            pointer = pointer > 0 ? pointer - 1 : 1;

        UI.DrawText("Q", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(Language.GetText("Return"), 182, 67, alignment: "right", action: Input.Key_up("B"), click: Input.Key_hold("B"), hover_font: "default small")) {
            if (Program.last_game_state == Program.Battle) Camera.LockCamera();
            Program.ChangeState(Program.last_game_state);
        }
    }
}