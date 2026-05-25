using UI_space;
using SFML.Graphics;

public class WGStart : Widget {
    Sprite main_bg;

    public WGStart() {
        main_bg = new Sprite(Data.textures["screens:title"]);
    }

    public override void Render() {
        Program.window.Draw(main_bg);
        UI.DrawText(S("2026 - by JFooley"), 0, 76, spacing: Config.spacing_small, textureName: "default small");

        Accessibility.Speak("PFS", TTSRequisition.TEXT, true, S("wc to project fs"));
        Accessibility.Speak("PS", TTSRequisition.TEXT, false, S("press start"));
        if (UI.DrawButton(S("press start"), 0, 50, tts: false, spacing: Config.spacing_medium, click: Input.Key_hold("Start"), action: Input.Key_up("Start"), button_sound: 2, hover_font: UI.blink2Hz ? "default medium white" : "", click_font: "default medium click")) {
            Program.ChangeState(Program.MainMenu);
        }
    }
}