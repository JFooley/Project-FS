using UI_space;
using SFML.Graphics;
using Language_space;

public class WGMainMenu : Widget {
    private int pointer = 0;
    Sprite frame = new Sprite(Data.textures["screens:frame"]);

    List<KeyValuePair<string, Texture>> main_menu = new List<KeyValuePair<string, Texture>>() {
        { new KeyValuePair<string, Texture>("versus player", Data.textures["screens:versus_player"]) },
        { new KeyValuePair<string, Texture>("versus bot", Data.textures["screens:versus_bot"]) },
        { new KeyValuePair<string, Texture>("training mode", Data.textures["screens:training_mode"]) },
        { new KeyValuePair<string, Texture>("controls", Data.textures["screens:controls"]) },
        { new KeyValuePair<string, Texture>("acessibility", Data.textures["screens:acessibility"]) },
        { new KeyValuePair<string, Texture>("settings", Data.textures["screens:settings"]) },
        { new KeyValuePair<string, Texture>("Exit", Data.textures["screens:exit"]) }
    };

    public override void Render() {
        Program.window.Draw(new Sprite(main_menu.ElementAt(pointer).Value));
        Program.window.Draw(frame);
        for (int i = 0; i < main_menu.Count; i++) 
            UI.DrawText(i == pointer ? ")" : "(", (i * 10) - ((main_menu.Count - 1) * 5), -80, textureName: "icons");
        
        // Shoulder buttons
        UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
        if (UI.DrawButton(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: InputManager.Key_hold("LB"), action: InputManager.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.StartScreen);
            pointer = 0;
        }

        // Change option
        if (InputManager.Key_down("Left"))
            pointer = pointer <= 0 ? main_menu.Count - 1 : pointer - 1;
        else if (InputManager.Key_down("Right"))
            pointer = pointer >= main_menu.Count - 1 ? 0 : pointer + 1;

        // Do option
        if (UI.DrawButton(Language.GetText(main_menu.ElementAt(pointer).Key), 0, -95, spacing: Config.spacing_medium, click: InputManager.faceButtonHold, action: InputManager.faceButtonUp, click_font: "default medium click", hover_font: "default medium white")) {
            if (main_menu.ElementAt(pointer).Key == "versus player") {
                Program.AI_playerA = false;
                Program.AI_playerB = false;
                Stage.training_mode = false;
                Program.ChangeState(Program.SelectStage);

            } else if (main_menu.ElementAt(pointer).Key == "versus bot") {
                Program.AI_playerA = false;
                Program.AI_playerB = true;
                Stage.training_mode = false;
                Program.ChangeState(Program.SelectStage);

            } else if (main_menu.ElementAt(pointer).Key == "training mode") {
                Program.AI_playerA = false;
                Program.AI_playerB = false;
                Stage.training_mode = true;
                Program.ChangeState(Program.SelectStage);

            } else if (main_menu.ElementAt(pointer).Key == "acessibility") {
                Program.ChangeState(Program.AccessibilityMenu);

            } else if (main_menu.ElementAt(pointer).Key == "controls") {
                Program.ChangeState(Program.Controls);

            } else if (main_menu.ElementAt(pointer).Key == "settings") {
                Program.ChangeState(Program.Settings);

            } else if (main_menu.ElementAt(pointer).Key == "Exit") {
                Program.window.Close();
            }
        }
    }
}