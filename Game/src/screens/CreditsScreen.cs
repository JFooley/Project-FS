using UI_space;
using Language_space;

class WGCredits : Widget {
    public override void Render() {
        UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
        if (UI.DrawButton(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: Input.Key_hold("LB"), action: Input.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.last_game_state);
        }
    }
}