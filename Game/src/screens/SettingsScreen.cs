using UI_space;
using SFML.Graphics;
using Language_space;
using SFML.Window;
using SFML.System;

public class WGSettings : Widget {
    Sprite settings_bg = new Sprite(Data.textures["screens:settings_bg"]);
    int pointer = 0;

    public override void Render() {
        Camera.UnlockCamera();
        Program.window.Draw(settings_bg);

        UI.DrawText(Language.GetText("Settings"), -80, -107, spacing: Config.spacing_medium);
        //0
        UI.DrawText(Language.GetText("Main volume"), -170, -74, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 0 ? "default small hover" : "default small");
        UI.DrawText("< " + Config.Main_Volume.ToString() + "% >", 0, -74, spacing: Config.spacing_small, textureName: pointer == 0 ? "default small red" : "default small");
        //1
        UI.DrawText(Language.GetText("Music volume"), -170, -64, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 1 ? "default small hover" : "default small");
        UI.DrawText("< " + Config._music_volume.ToString() + "% >", 0, -64, spacing: Config.spacing_small, textureName: pointer == 1 ? "default small red" : "default small");
        //2
        UI.DrawText(Language.GetText("V-sync"), -170, -54, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 2 ? "default small hover" : "default small");
        UI.DrawText("< " + (Config.Vsync ? Language.GetText("on") : Language.GetText("off")) + " >", 0, -54, spacing: Config.spacing_small, textureName: pointer == 2 ? "default small red" : "default small");
        //3
        UI.DrawText(Language.GetText("Window mode"), -170, -44, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 3 ? "default small hover" : "default small");
        UI.DrawText("< " + (Config.Fullscreen ? Language.GetText("Fullscreen") : Language.GetText("Windowed")) + " >", 0, -44, spacing: Config.spacing_small, textureName: pointer == 3 ? "default small red" : "default small");
        //4
        UI.DrawText(Language.GetText("Round Length"), -170, -34, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 4 ? "default small hover" : "default small");
        UI.DrawText("< " + (Config.round_length == Config.default_round_length ? Language.GetText("Default") + " (" + Config.default_round_length + "s)" : Config.round_length + "s") + " >", 0, -34, spacing: Config.spacing_small, textureName: pointer == 4 ? "default small red" : "default small");
        //5
        UI.DrawText(Language.GetText("Match"), -170, -24, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 5 ? "default small hover" : "default small");
        UI.DrawText("< " + (Config.max_rounds == Config.default_max_rounds ? Language.GetText("Default") + " (FT" + Config.default_max_rounds + ")" : Language.GetText("First to") + " " + Config.max_rounds.ToString()) + " >", 0, -24, spacing: Config.spacing_small, textureName: pointer == 5 ? "default small red" : "default small");
        //6
        UI.DrawText(Language.GetText("Hitstop"), -170, -14, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 6 ? "default small hover" : "default small");
        UI.DrawText("< " + (Config.hit_stop_time == Config.default_hit_stop_time ? Language.GetText("Default") : Config.hit_stop_time + " " + Language.GetText("frames")) + " >", 0, -14, spacing: Config.spacing_small, textureName: pointer == 6 ? "default small red" : "default small");
        //7
        UI.DrawText(Language.GetText("Language"), -170, -4, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 7 ? "default small hover" : "default small");
        UI.DrawText("< " + Language.Supported[Config.Language] + " >", 0, -4, spacing: Config.spacing_small, textureName: pointer == 7 ? "default small red" : "default small");
        //8
        UI.DrawText(Language.GetText("Save and Exit"), -170, 16, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 8 ? "default small hover" : "default small");

        // Change option 
        if (Input.Key_down("Up") || (Input.Key_hold_for("Up", Config.hold_time) && UI.Clock(Config.hold_clock)))
            pointer = pointer <= 0 ? 8 : pointer - 1;
        else if (Input.Key_down("Down") || (Input.Key_hold_for("Down", Config.hold_time) && UI.Clock(Config.hold_clock)))
            pointer = pointer >= 8 ? 0 : pointer + 1;

        // Do option
        switch (pointer) {
            case 0:
                if ((Input.Key_down("Left") || (Input.Key_hold_for("Left", Config.hold_time) && UI.Clock(Config.hold_clock))) && Config.Main_Volume > 0) 
                    Config.Main_Volume -= 1;
                else if ((Input.Key_down("Right") || (Input.Key_hold_for("Right", Config.hold_time) && UI.Clock(Config.hold_clock))) && Config.Main_Volume < 100) 
                    Config.Main_Volume += 1;
                break;
            case 1:
                if ((Input.Key_down("Left") || (Input.Key_hold_for("Left", Config.hold_time) && UI.Clock(Config.hold_clock))) && Config._music_volume > 0) 
                    Config._music_volume -= 1;
                else if ((Input.Key_down("Right") || (Input.Key_hold_for("Right", Config.hold_time) && UI.Clock(Config.hold_clock))) && Config._music_volume < 100) 
                    Config._music_volume += 1;
                break;
            case 2:
                if (Input.Key_down("Left") || Input.Key_down("Right")) {
                    Config.Vsync = !Config.Vsync;
                    Program.window.SetVerticalSyncEnabled(Config.Vsync);   
                }
                break;
            case 3:
                if (Input.Key_down("Left") || Input.Key_down("Right")) {
                    Config.Fullscreen = !Config.Fullscreen;
                    Program.window.Close();
                    if (Config.Fullscreen == true) Program.window = new RenderWindow(VideoMode.DesktopMode, Config.GameTitle, Styles.Default, SFML.Window.State.Fullscreen);
                    else Program.window = new RenderWindow(new VideoMode( new Vector2u(Config.RenderWidth * 3, Config.RenderHeight * 3)), Config.GameTitle, Styles.Default, SFML.Window.State.Windowed);
                    Program.window.Closed += (sender, e) => Program.window.Close();
                    Program.window.SetFramerateLimit(Config.Framerate);
                    Program.window.SetVerticalSyncEnabled(Config.Vsync);
                    Program.window.SetMouseCursorVisible(false);
                    Program.window.SetView(Program.view);   
                }
                break;
            case 4:
                if (Input.Key_down("Left") && Config.round_length > 1) 
                    Config.round_length -= 1;
                else if (Input.Key_down("Right") && Config.round_length < 99) 
                    Config.round_length += 1;
                break;
            case 5:
                if (Input.Key_down("Left") && Config.max_rounds > 1) 
                    Config.max_rounds -= 1;
                else if (Input.Key_down("Right") && Config.max_rounds < 5) 
                    Config.max_rounds += 1;
                break;
            case 6:
                if (Input.Key_down("Left") && Config.hit_stop_time > 0) 
                    Config.hit_stop_time -= 1;
                else if (Input.Key_down("Right")) 
                    Config.hit_stop_time += 1;
                break;
            case 7:
                if (Input.Key_down("Left") && Config.Language > 0) 
                    Config.Language -= 1;
                else if (Input.Key_down("Right") && Config.Language < Language.Supported.Length - 1) 
                    Config.Language += 1;
                break;
            case 8:
                if (Input.faceButtonUp) {
                    Config.SaveToFile();
                    Camera.LockCamera();
                    Program.ChangeState(Program.last_game_state);
                    pointer = 0;
                }
                break; 
        }
    }
}