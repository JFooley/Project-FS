using System.Drawing;
using System.Windows.Forms;
using SFML.System;

public class WGButton : Widget {
    private const int STAND_BY = 0;
    private const int HOVER = 1;
    private const int CLICKED = 2;
    private const int DISABLED = 3;

    public int state;
    public Vector2f position;

    string bg_texture;
    string default_font_texture;
    string hover_font_texture;
    string clicked_font_texture;
    float spacing = Config.spacing_small;

    public WGButton(Vector2f position, string bg_texture = null, string default_font_texture = null, string hover_font_texture = null, string clicked_font_texture = null) {
        this.position = position;
        this.bg_texture = bg_texture;
        this.default_font_texture = default_font_texture;
        this.hover_font_texture = hover_font_texture;
        this.clicked_font_texture = clicked_font_texture;
        state = STAND_BY;
    }

    public override void Render() {}
}