using UI_space;
using SFML.Graphics;
using SFML.System;

public class WGIntro : Widget {
    private int pointer = 0;
    private bool loading = false;
    private Selector selector = new Selector(new List<int> {2, 1});
    Sprite fslogo;

    public WGIntro() {
        fslogo = new Sprite(Data.textures["typography:fs"]);
    }

    public override void Render() {
        if (!loading) {
            selector.Update();

            if (UI.DrawButton(S("RECIVER"), 0, 0, action: Input.Key_up("A"), click: Input.Key_down("A"), hover: selector.is_on(0, 0), alignment: "right")) {
                loading = true;
                Program.online_type = 2;
            }
            if (UI.DrawButton(S("SENDER"), 0, 0, action: Input.Key_up("A"), click: Input.Key_down("A"), hover: selector.is_on(1, 0), alignment: "left")) {
                loading = true;
                Program.online_type = 1;
            }
            if (UI.DrawButton(S("OFFLINE"), 0, 15, action: Input.Key_up("A"), click: Input.Key_down("A"), hover: selector.is_on(0, 1), alignment: "center")) {
                loading = true;
                Program.online_type = 0;
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