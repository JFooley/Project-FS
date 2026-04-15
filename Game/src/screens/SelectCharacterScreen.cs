using UI_space;
using SFML.Graphics;
using Language_space;
using SFML.System;

public class WGSelectCharacter : Widget {
    private WGSelector selectorA = new WGSelector();
    private WGSelector selectorB = new WGSelector();

    public Character? charA_selected => selectorA.selected;
    public Character? charB_selected => selectorB.selected;

    private bool A_flag = false;
    private bool B_flag = false; 

    private Sprite char_bg = new Sprite(Data.textures["screens:bgchar"]);

    public override void Render() {
        Program.window.Draw(char_bg);
        Program.colorFillShader.SetUniform("fillColor", Color.Black);

        switch (WGBattle.battle_mode) {
            case WGBattle.Versus:
                Versus();
                break;
            case WGBattle.VersusBot:
                VersusBOT();
                break;
            case WGBattle.Training:
                Training();
                break;
        }

        this.DrawSessionInfo();

        // Ends when chars are selected and ready
        if (selectorA.state == WGSelector.READY && selectorB.state == WGSelector.READY) {
            if (UI.blink2Hz) UI.DrawText(Language.GetText("press start"), 0, -90, spacing: Config.spacing_medium);
            if (Input.Key_down("Start") ) {
                this.A_flag = false;
                this.B_flag = false;
                Program.ChangeState(Program.LoadScreen);
            }
        }
    }

    private void Versus() {
        selectorA.Render(-77, -30, 1, x_scale: 1);
        selectorB.Render(77, -30, 2, x_scale: -1);

        // Shoulder buttons
        UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
        if (UI.DrawButton(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: Input.Key_hold("LB"), action: Input.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.SelectStage);
            selectorA.selected = null;
            selectorB.selected = null;
            selectorA.state = WGSelector.SELECTING_CHAR;
            selectorB.state = WGSelector.SELECTING_CHAR;
        }

        UI.DrawText("F", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(Language.GetText("Controls"), 182, 67, spacing: Config.spacing_small, alignment: "right", click: Input.Key_hold("RB"), action: Input.Key_up("RB"), click_font: "default small click", hover_font: "default small")) 
            Program.ChangeState(Program.Controls);
    }
    private void VersusBOT() {
        if (Stage.AI_playerA && Stage.AI_playerB && Input.anyKeyA) Stage.AI_playerA = false;
        if (Stage.AI_playerB && Stage.AI_playerA && Input.anyKeyB) Stage.AI_playerB = false;

        var player = Stage.AI_playerB ? 1 : 2;
        selectorA.Render(0, -30, player, x_scale: 1);

        if (selectorA.state == WGSelector.READY) {
            if (player == 1) {
                selectorB.selected = Data.characters[AI.rand.Next(0, Data.characters.Count)].Copy();
                if (selectorA.selected?.name == selectorB.selected?.name && selectorA.selected?.palette_index == 0) selectorB.selected?.SetPalette(AI.rand.Next(0, (int) (selectorB.selected.palette_quantity - 2)));
                selectorB.state = WGSelector.READY;
            } else {
                selectorA.selected = Data.characters[AI.rand.Next(0, Data.characters.Count)].Copy();
                if (selectorB.selected?.name == selectorA.selected?.name && selectorB.selected?.palette_index == 0) selectorA.selected?.SetPalette(AI.rand.Next(0, (int) (selectorA.selected.palette_quantity - 2)));
                selectorA.state = WGSelector.READY;
            }
        }

        // Shoulder buttons
        UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
        if (UI.DrawButton(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: Input.Key_hold("LB"), action: Input.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.SelectStage);
            selectorA.selected = null;
            selectorB.selected = null;
            selectorA.state = WGSelector.SELECTING_CHAR;
            selectorB.state = WGSelector.SELECTING_CHAR;
            Stage.AI_playerA = true;
            Stage.AI_playerB = true;
        }

        UI.DrawText("F", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(Language.GetText("Controls"), 182, 67, spacing: Config.spacing_small, alignment: "right", click: Input.Key_hold("RB"), action: Input.Key_up("RB"), click_font: "default small click", hover_font: "default small")) 
            Program.ChangeState(Program.Controls);
    }
    private void Training() {
        // CONSERTAR ISSO
        selectorA.Render(-77, -30, this.B_flag ? 0 : 1, x_scale: 1);
        selectorB.Render(77, -30, this.A_flag ? 0 : 2, x_scale: -1);

        if (selectorA.state == WGSelector.READY) this.A_flag = true;
        if (selectorB.state == WGSelector.READY) this.B_flag = true;

        // Shoulder buttons
        UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
        if (UI.DrawButton(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: Input.Key_hold("LB"), action: Input.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.SelectStage);
            selectorA.selected = null;
            selectorB.selected = null;
            selectorA.state = WGSelector.SELECTING_CHAR;
            selectorB.state = WGSelector.SELECTING_CHAR;
            this.A_flag = false;
            this.B_flag = false;
        }

        UI.DrawText("F", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(Language.GetText("Controls"), 182, 67, spacing: Config.spacing_small, alignment: "right", click: Input.Key_hold("RB"), action: Input.Key_up("RB"), click_font: "default small click", hover_font: "default small")) 
            Program.ChangeState(Program.Controls);
    }

    private void DrawSessionInfo() {
        UI.DrawText(Program.playerA_wins.ToString(), 0, 63, alignment: "right");
        UI.DrawText(Program.playerB_wins.ToString(), 0, 63, alignment: "left" );
        UI.DrawText(Stage.AI_playerA ? "COM" : "P1", -15, 63, textureName: "default small", alignment: "right", spacing: Config.spacing_small);
        UI.DrawText(Stage.AI_playerB ? "COM" : "P2",  15, 63, textureName: "default small", alignment:  "left", spacing: Config.spacing_small);
    }
    public void Reset() {
        selectorA.Reset();
        selectorB.Reset();
    }
}

public class WGSelector : Widget {
    public const int SELECTING_CHAR = 0;
    public const int SELECTING_PALETTE = 1;
    public const int READY = 2;

    public int pointer = 0;
    public int state = SELECTING_CHAR;
    public Character? selected = null;

    Sprite sprite = new Sprite(Data.textures["other:placeholder"]);

    public void Render(float x, float y, int player, int x_scale = 1, int shadow_x_offset = -10, int info_y_offset = 75) {
        sprite.Texture = Data.characters[pointer].thumb;
        sprite.Scale = new Vector2f(x_scale, 1f);

        Vector2f position = new Vector2f(Camera.X + x - (sprite.GetLocalBounds().Width / 2) * x_scale, Camera.Y + y - sprite.GetLocalBounds().Height / 2);

        // Shadow
        if (state == SELECTING_PALETTE || state == READY) {
            sprite.Position = position + new Vector2f(shadow_x_offset, 0);
            Program.window.Draw(sprite, new RenderStates(Program.colorFillShader));
        }

        // Sprite
        sprite.Position = position;
        Program.window.Draw(sprite, Data.characters[pointer].SetSwaperShader(
            palette_index: (int) (state == SELECTING_CHAR ? 0 : selected.palette_index), 
            light: state == READY || Input.anyKey[player] ? new Color(128, 128, 128) : Color.White
        ));

        // Info
        if (state == READY) { 
            UI.DrawText(Language.GetText("ready"), x, y, textureName: "default small", spacing: Config.spacing_small);
        } else if (state == SELECTING_CHAR) {
            for (int i = 0; i < Data.characters.Count; i++) UI.DrawText(i == pointer ? ")" : "(", (i * 10) - ((Data.characters.Count - 1) * 5) + x, y + info_y_offset, textureName: "icons");
        } else if (state == SELECTING_PALETTE) {
            for (int i = 0; i < selected.palette_quantity; i++) UI.DrawText(i == selected.palette_index ? ")" : "(", (i * 10) - ((selected.palette_quantity - 1) * 5) + x, y + info_y_offset, textureName: "icons");
        }

        // Buttons
        if (state != READY && UI.DrawButton("<   ", x, y, hover: true, click: Input.Key_hold("Left", player), action: Input.Key_up("Left", player) || (Input.Key_hold_for("Left", Config.hold_time, player) && UI.Clock(Config.hold_clock)), hover_font: "default small", font: "", spacing: 0)) {
            if (state == SELECTING_CHAR) pointer = pointer > 0 ? pointer - 1 : Data.characters.Count - 1;
            else if (state == SELECTING_PALETTE) selected.SetPalette(-1);
        } 
        
        if (state != READY && UI.DrawButton("   >", x, y, hover: true, click: Input.Key_hold("Right", player), action: Input.Key_up("Right", player) || (Input.Key_hold_for("Right", Config.hold_time, player) && UI.Clock(Config.hold_clock)), hover_font: "default small", font: "", spacing: 0)) {
            if (state == SELECTING_CHAR) pointer = pointer < Data.characters.Count - 1 ? pointer + 1 : 0;
            else if (state == SELECTING_PALETTE) selected.SetPalette(1);
        }

        if (UI.DrawButton(Data.characters[pointer].name, x, y + info_y_offset - 10, spacing: Config.spacing_small, action: Input.Key_up("A", player), click: Input.Key_hold("A", player: player) && this.state != READY, hover_font: "default small")) {
            if (state == SELECTING_CHAR) {
                selected = Data.characters[pointer].Copy();
                state = SELECTING_PALETTE;

            } else if (state == SELECTING_PALETTE) {
                state = READY;
            }
        }
    }

    public void Reset() {
        pointer = 0;
        state = SELECTING_CHAR;
        selected = null;
    }
}