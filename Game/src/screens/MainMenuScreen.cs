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
        if (UI.DrawButton(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: Input.Key_hold("LB"), action: Input.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.StartScreen);
            pointer = 0;
        }

        // Change option
        if (Input.Key_down("Left"))
            pointer = pointer <= 0 ? main_menu.Count - 1 : pointer - 1;
        else if (Input.Key_down("Right"))
            pointer = pointer >= main_menu.Count - 1 ? 0 : pointer + 1;

        // Do option
        if (UI.DrawButton(Language.GetText(main_menu.ElementAt(pointer).Key), 0, -95, spacing: Config.spacing_medium, click: Input.faceButtonHold, action: Input.faceButtonUp, click_font: "default medium click", hover_font: "default medium white")) {
            Stage.AI_playerA = false;
            Stage.AI_playerB = false;

            switch (main_menu.ElementAt(pointer).Key) {
                case "versus player":
                    Stage.AI_playerA = false;
                    Stage.AI_playerB = false;
                    WGBattle.battle_mode = WGBattle.Versus;
                    Program.ChangeState(Program.SelectStage);
                    break;

                case "versus bot":
                    Stage.AI_playerA = true;
                    Stage.AI_playerB = true;
                    WGBattle.battle_mode = WGBattle.VersusBot;
                    Program.ChangeState(Program.SelectStage);

                    break;
                case "training mode":
                    Stage.AI_playerA = false;
                    Stage.AI_playerB = false;
                    WGBattle.battle_mode = WGBattle.Training;
                    Program.ChangeState(Program.SelectStage);

                    break;
                case "acessibility":
                    Program.ChangeState(Program.AccessibilityMenu);

                    break;
                case "controls":
                    Program.ChangeState(Program.Controls);

                    break;
                case "settings":
                    Program.ChangeState(Program.Settings);

                    break;
                case "Exit":
                    Program.window.Close();
                    break;
            }
        }
    }
}