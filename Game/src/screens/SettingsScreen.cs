using UI_space;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

public class WGSettings : Widget {
    Sprite settings_bg;
    private static Selector selector = new Selector(new List<int> {1, 1, 1, 1, 1, 1, 1, 1, 1});

    public WGSettings() {
        settings_bg = new Sprite(Data.textures["screens:settings_bg"]);
    }

    public override void Render() {
        selector.Update();
        Camera.UnlockCamera();
        Program.window.Draw(settings_bg);

        UI.DrawText(S("Settings"), -80, -107, TTS: true, TTS_id: "Settings", priority: true, spacing: Config.spacing_medium);
        //0
        UI.DrawButton(S("Main volume", ": < ", Config.Main_Volume.ToString(), "% >"), -185, -74, alignment: "left", spacing: Config.spacing_small, hover: selector.is_on(0, 0), hover_font: "default small hover", font: "default small");
        //1
        UI.DrawButton(S("Music volume", ": < ", Config._music_volume.ToString(), "% >"), -185, -64, alignment: "left", spacing: Config.spacing_small, hover: selector.is_on(0, 1), hover_font: "default small hover", font: "default small");
        //2
        UI.DrawButton(S("V-sync", ": < ", Config.Vsync ? "on": "off", " >"), -185, -54, alignment: "left", spacing: Config.spacing_small, hover: selector.is_on(0, 2), hover_font: "default small hover", font: "default small");
        //3
        UI.DrawButton(S("Window mode", ": < ", Config.Fullscreen ? "Fullscreen" : "Windowed", " >"), -185, -44, alignment: "left", spacing: Config.spacing_small, hover: selector.is_on(0, 3), hover_font: "default small hover", font: "default small");
        //4
        UI.DrawButton(S("Round Length", ": < ", Config.round_length == Config.default_round_length ? "Default" : "", " " + Config.round_length + "s", " >"), -185, -34, alignment: "left", spacing: Config.spacing_small, hover: selector.is_on(0, 4), hover_font: "default small hover", font: "default small");
        //5
        UI.DrawButton(S("Match rounds", ": < ", Config.max_rounds == Config.default_max_rounds ? "Default" : Config.max_rounds.ToString(), " >"), -185, -24, alignment: "left", spacing: Config.spacing_small, hover: selector.is_on(0, 5), hover_font: "default small hover", font: "default small");
        //6
        UI.DrawButton(S("Hitstop frames", ": < ", Config.hit_stop_time == Config.default_hit_stop_time ? "Default" : Config.hit_stop_time.ToString(), " >"), -185, -14, alignment: "left", spacing: Config.spacing_small, hover: selector.is_on(0, 6), hover_font: "default small hover", font: "default small");
        //7
        UI.DrawButton(S("Language", " < ", Language.Supported[Config.Language], " >"), -185, -4, alignment: "left", spacing: Config.spacing_small, hover: selector.is_on(0, 7), hover_font: "default small hover", font: "default small");
        //8
        UI.DrawButton(S("Save and Exit"), -185, 16, alignment: "left", spacing: Config.spacing_small, hover: selector.is_on(0, 8), hover_font: "default small hover", font: "default small");

        if (Input.Key_down("B")) selector.pointer.Y = 8;

        // Do option
        switch (selector.pointer.Y) {
            case 0:
                if ((Input.Key_down("Left") || (Input.Key_hold_for("Left", Config.hold_time) && UI.ForEach(Config.hold_clock - 5))) && Config.Main_Volume > 0) 
                    Config.Main_Volume -= 1;
                else if ((Input.Key_down("Right") || (Input.Key_hold_for("Right", Config.hold_time) && UI.ForEach(Config.hold_clock - 5))) && Config.Main_Volume < 100) 
                    Config.Main_Volume += 1;
                break;
            case 1:
                if ((Input.Key_down("Left") || (Input.Key_hold_for("Left", Config.hold_time) && UI.ForEach(Config.hold_clock - 5))) && Config._music_volume > 0) 
                    Config._music_volume -= 1;
                else if ((Input.Key_down("Right") || (Input.Key_hold_for("Right", Config.hold_time) && UI.ForEach(Config.hold_clock - 5))) && Config._music_volume < 100) 
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
                if ((Input.Key_down("Left") || (Input.Key_hold_for("Left", Config.hold_time) && UI.ForEach(Config.hold_clock - 5))) && Config.round_length > 1) 
                    Config.round_length -= 1;
                else if ((Input.Key_down("Right") || (Input.Key_hold_for("Right", Config.hold_time) && UI.ForEach(Config.hold_clock - 5))) && Config.round_length < 99) 
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
                if (Input.Key_up("A")) {
                    Config.SaveToFile();
                    Camera.LockCamera();
                    Program.ChangeState(Program.previous_state);
                    selector.pointer = new Vector2i(0, 0);
                }
                break;
        }
    }
}