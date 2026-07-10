using UI_space;
using SFML.Graphics;
using SFML.System;

public class WGIntro : Widget {
    private const int LOADING = 0;
    private const int SELECTING_MODE = 1;
    private const int TYPING_IP = 2;
    
    private int pointer = 0;
    private int state = 1;
    private Selector selector = new Selector(new List<int> {2, 1});
    private int online_type = 0;

    Sprite fslogo;

    public WGIntro() {
        fslogo = new Sprite(Data.textures["typography:fs"]);
    }

    public override void Render() {
        if (state == SELECTING_MODE) {
            selector.Update();

            if (UI.DrawButton(S("RECEIVER"), 0, 0, action: Input.Key_up("A"), click: Input.Key_down("A"), hover: selector.is_on(0, 0), alignment: "right")) {
                state = TYPING_IP;
                online_type = OnlineInput.RECEIVER;
            }
            if (UI.DrawButton(S("SENDER"), 0, 0, action: Input.Key_up("A"), click: Input.Key_down("A"), hover: selector.is_on(1, 0), alignment: "left")) {
                state = TYPING_IP;
                online_type = OnlineInput.SENDER;
            }
            if (UI.DrawButton(S("OFFLINE"), 0, 15, action: Input.Key_up("A"), click: Input.Key_down("A"), hover: selector.is_on(0, 1), alignment: "center")) {
                state = LOADING;
                online_type = OnlineInput.NONE;
            }

        } else if (state == TYPING_IP) {
            UI.DrawText(S("IP ADDRESS:"), 0, -55, alignment: "center", spacing: Config.spacing_medium);
            UI.DrawText(S(VirtualKeyboard.text), 0, -40, alignment: "center", spacing: Config.spacing_medium);

            UI.virtual_keyboard?.Render(0, 30);

            if (VirtualKeyboard.ended) {
                state = LOADING;
                UI.virtual_keyboard?.Clear();

                OnlineInput.Connect(VirtualKeyboard.text, online_type);
            }

        } else {
            fslogo.Position = new Vector2f(10, 139);
            Program.window.Draw(fslogo);

            if (UI.frame_counter % 20 == 0) pointer = pointer < 3 ? pointer + 1 : 0;
            UI.DrawText(S(string.Concat(Enumerable.Repeat(".", pointer))), -122, 68, alignment: "left", spacing: -24);

            if (!Program.loading) {
                Thread main_loader = new Thread(Program.MainLoader);
                main_loader.Start();
                Program.loading = true;
            }
        }
    }
}