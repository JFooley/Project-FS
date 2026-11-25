using UI_space;
using SFML.Graphics;
using SFML.System;

public class WGIntro : Widget {
    private int pointer = 0;
    Sprite fslogo;

    public WGIntro() {
        fslogo = new Sprite(Data.textures["typography:fs"]);
    }

    public override void Render() {
        fslogo.Position = new Vector2f(10, 139);
        MainRender.Draw(fslogo, MainRender.BackgroundLayer);

        if (UI.frame_counter % 20 == 0) pointer = pointer < 3 ? pointer + 1 : 0;
        UI.DrawTextAtlas(S(string.Concat(Enumerable.Repeat(".", pointer))), -122, 68, alignment: UI.Alignment.Left, spacing: -24);

        if (!Program.loading) {
            Thread main_loader = new Thread(Program.MainLoader);
            main_loader.Start();
            Program.loading = true;
        }
    }
}