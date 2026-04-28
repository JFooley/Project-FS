using UI_space;
using SFML.Graphics;
using Language_space;

public class WGMainMenu : Widget {
    private static List<KeyValuePair<string, Texture>> main_menu = new List<KeyValuePair<string, Texture>>() {
        { new KeyValuePair<string, Texture>("versus player", Data.textures["screens:versus_player"]) },
        { new KeyValuePair<string, Texture>("versus bot", Data.textures["screens:versus_bot"]) },
        { new KeyValuePair<string, Texture>("training mode", Data.textures["screens:training_mode"]) },
        { new KeyValuePair<string, Texture>("controls", Data.textures["screens:controls"]) },
        { new KeyValuePair<string, Texture>("accessibility", Data.textures["screens:accessibility"]) },
        { new KeyValuePair<string, Texture>("settings", Data.textures["screens:settings"]) },
        { new KeyValuePair<string, Texture>("exit game", Data.textures["screens:exit"]) }
    };
    private static Selector selector = new Selector(new List<int>() { main_menu.Count });
    private static Sprite frame = new Sprite(Data.textures["screens:frame"]);

    public override void Render() {
        Program.window.Draw(new Sprite(main_menu.ElementAt(selector.pointer.X).Value));
        Program.window.Draw(frame);
        for (int i = 0; i < main_menu.Count; i++) 
            UI.DrawText(i == selector.pointer.X ? ")" : "(", (i * 10) - ((main_menu.Count - 1) * 5), -80, textureName: "icons");
        
        // Buttons
        UI.DrawText("Q", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(Language.GetText("Return"), 182, 67, spacing: Config.spacing_small, alignment: "right", click: Input.Key_hold("B"), action: Input.Key_up("B"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.StartScreen);
            selector.pointer.X = 0;
        }
        
        // Update selector
        selector.Update();

        // Do option
        if (UI.DrawButton(Language.GetText(main_menu.ElementAt(selector.pointer.X).Key), 0, -95, spacing: Config.spacing_medium, click: Input.Key_hold("A"), action: Input.Key_up("A"), click_font: "default medium click", hover_font: "default medium white")) {
            Stage.AI_playerA = false;
            Stage.AI_playerB = false;

            switch (main_menu.ElementAt(selector.pointer.X).Key) {
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
                case "accessibility":
                    Program.ChangeState(Program.AccessibilityMenu);

                    break;
                case "controls":
                    Program.ChangeState(Program.Controls);

                    break;
                case "settings":
                    Program.ChangeState(Program.Settings);

                    break;
                case "exit game":
                    Program.window.Close();
                    break;
            }
        }
    }
}