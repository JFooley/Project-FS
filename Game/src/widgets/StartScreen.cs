using Language_space;
using UI_space;
using SFML.Graphics;

public class WGStart : Widget {
    Sprite main_bg = new Sprite(Data.textures["screens:title"]);

    public override void Render() {
        Program.window.Draw(main_bg);
        UI.DrawText("by JFooley", 0, 76, spacing: Config.spacing_small - 1, textureName: "default small");

        if (UI.DrawButton(Language.GetText("press start"), 0, 50, spacing: Config.spacing_medium, click: InputManager.Key_hold("Start"), action: InputManager.Key_up("Start"), hover_font: UI.blink2Hz ? "default medium white" : "", click_font: "default medium click")) {
            Program.ChangeState(Program.MainMenu);
        }
    }
}