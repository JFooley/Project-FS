using UI_space;

class WGCredits : Widget {
    public override void Render() {
        UI.DrawText(S("Q"), 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(S("Return"), 182, 67, tts: false, spacing: Config.spacing_small, alignment: "right", click: Input.Key_hold("B"), action: Input.Key_up("B"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.previous_state);
        }
    }
}