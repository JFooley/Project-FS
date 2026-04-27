using UI_space;
using Language_space;

class WGCredits : Widget {
    public override void Render() {
        UI.DrawText("Q", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(Language.GetText("Return"), 182, 67, spacing: Config.spacing_small, alignment: "right", click: Input.Key_hold("B"), action: Input.Key_up("B"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.last_game_state);
        }
    }
}