using UI_space;
using SFML.Graphics;

public class WGMainMenu : Widget {
    private static List<KeyValuePair<string, Texture>> main_menu;
    private static Selector selector;
    private static Sprite frame;

    public WGMainMenu() {
        main_menu = new List<KeyValuePair<string, Texture>>() {
            { new KeyValuePair<string, Texture>("versus player", Data.textures["screens:versus_player"]) },
            { new KeyValuePair<string, Texture>("versus bot", Data.textures["screens:versus_bot"]) },
            { new KeyValuePair<string, Texture>("training mode", Data.textures["screens:training_mode"]) },
            { new KeyValuePair<string, Texture>("controls", Data.textures["screens:controls"]) },
            { new KeyValuePair<string, Texture>("accessibility", Data.textures["screens:accessibility"]) },
            { new KeyValuePair<string, Texture>("settings", Data.textures["screens:settings"]) },
            { new KeyValuePair<string, Texture>("exit game", Data.textures["screens:exit"]) }
        };
        selector = new Selector(new List<int>() { main_menu.Count });
        frame = new Sprite(Data.textures["screens:frame"]);
    }

    public override void Render() {
        MainRender.Draw(new Sprite(main_menu.ElementAt(selector.pointer.X).Value), MainRender.BackgroundLayer);
        MainRender.Draw(frame, MainRender.InterfaceLayer);
        for (int i = 0; i < main_menu.Count; i++) 
            UI.DrawTextAtlas(S(i == selector.pointer.X ? ")" : "("), (i * 10) - ((main_menu.Count - 1) * 5), -80, textureName: "icons");
        
        // Buttons
        UI.DrawTextAtlas(S("Q"), 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: UI.Alignment.Right);
        if (UI.DrawButton(S("Return"), 182, 67, tts: false, spacing: Config.spacing_small, alignment: UI.Alignment.Right, click: Input.Key_hold("B"), action: Input.Key_up("B"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.State.StartScreen);
            selector.pointer.X = 0;
        }
        
        // Update selector
        selector.Update();

        // Do option
        if (UI.DrawButton(S(main_menu.ElementAt(selector.pointer.X).Key), 0, -95, spacing: Config.spacing_medium, click: Input.Key_hold("A"), action: Input.Key_up("A"), click_font: "default medium click", hover_font: "default medium white")) {
            Stage.AI_playerA = false;
            Stage.AI_playerB = false;

            switch (main_menu.ElementAt(selector.pointer.X).Key) {
                case "versus player":
                    Stage.AI_playerA = false;
                    Stage.AI_playerB = false;
                    WGBattle.battle_mode = WGBattle.Versus;
                    Program.ChangeState(Program.State.SelectStage);
                    break;

                case "versus bot":
                    Stage.AI_playerA = true;
                    Stage.AI_playerB = true;
                    WGBattle.battle_mode = WGBattle.VersusBot;
                    Program.ChangeState(Program.State.SelectStage);

                    break;
                case "training mode":
                    Stage.AI_playerA = false;
                    Stage.AI_playerB = false;
                    WGBattle.battle_mode = WGBattle.Training;
                    Program.ChangeState(Program.State.SelectStage);

                    break;
                case "accessibility":
                    Program.ChangeState(Program.State.AccessibilityMenu);

                    break;
                case "controls":
                    Program.ChangeState(Program.State.Controls);

                    break;
                case "settings":
                    Program.ChangeState(Program.State.Settings);

                    break;
                case "exit game":
                    Program.window.Close();
                    break;
            }
        }
    }
}