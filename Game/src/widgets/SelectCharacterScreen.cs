using UI_space;
using SFML.Graphics;
using Language_space;
using SFML.System;

public class WGSelectCharacter : Widget {
    private WGSelector selectorA = new WGSelector();
    private WGSelector selectorB = new WGSelector();

    public Character? charA_selected => selectorA.selected;
    public Character? charB_selected => selectorB.selected;

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

        // Shoulder buttons
        UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
        if (UI.DrawButton(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: InputManager.Key_hold("LB"), action: InputManager.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.SelectStage);
            selectorA.selected = null;
            selectorB.selected = null;
            selectorA.state = WGSelector.SELECTING_CHAR;
            selectorB.state = WGSelector.SELECTING_CHAR;
        }

        UI.DrawText("F", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(Language.GetText("Controls"), 182, 67, spacing: Config.spacing_small, alignment: "right", click: InputManager.Key_hold("RB"), action: InputManager.Key_up("RB"), click_font: "default small click", hover_font: "default small")) 
            Program.ChangeState(Program.Controls);

        // Ends when chars are selected and ready
        if (selectorA.state == WGSelector.READY && selectorB.state == WGSelector.READY) {
            if (UI.blink2Hz) UI.DrawText(Language.GetText("press start"), 0, -90, spacing: Config.spacing_medium);
            if (InputManager.Key_down("Start") ) {
                Program.ChangeState(Program.LoadScreen);
            }
        }
    }

    private void Versus() {
        selectorA.Render(-77, -30, 1, x_scale: 1);
        selectorB.Render(77, -30, 2, x_scale: -1);
    }
    private void VersusBOT() {
        if (Stage.AI_playerA && Stage.AI_playerB && InputManager.anyKeyA) Stage.AI_playerA = false;
        if (Stage.AI_playerB && Stage.AI_playerA && InputManager.anyKeyB) Stage.AI_playerB = false;
        var player = Stage.AI_playerB ? 1 : 2;
        selectorA.Render(0, -30, player, x_scale: 1);

        if (selectorA.state == WGSelector.READY) {
            if (player == 1) {
                selectorB.selected = Data.characters[AI.rand.Next(0, Data.characters.Count)].Copy();
                if (selectorA.selected?.name == selectorB.selected?.name) selectorB.selected?.ChangePalette(AI.rand.Next(0, (int) (selectorB.selected.palette_quantity - 2)));
                selectorB.state = WGSelector.READY;
            } else {
                selectorA.selected = Data.characters[AI.rand.Next(0, Data.characters.Count)].Copy();
                if (selectorA.selected?.name == selectorA.selected?.name) selectorA.selected?.ChangePalette(AI.rand.Next(0, (int) (selectorA.selected.palette_quantity - 2)));
                selectorA.state = WGSelector.READY;
            }
        }

    }
    private void Training() {
        selectorA.Render(-77, -30, selectorA.state == WGSelector.READY ? 2 : 1, x_scale: 1);
        selectorB.Render(77, -30, selectorA.state == WGSelector.READY ? 1 : 2, x_scale: -1);
    }

    private void DrawSessionInfo() {
        UI.DrawText(Program.playerA_wins.ToString(), 0, 63, alignment: "right");
        UI.DrawText(Program.playerB_wins.ToString(), 0, 63, alignment: "left" );
        UI.DrawText(Language.GetText(Stage.AI_playerA ? "COM" : "P1"), -15, 63, textureName: "default small", alignment: "right", spacing: Config.spacing_small);
        UI.DrawText(Language.GetText(Stage.AI_playerB ? "COM" : "P2"),  15, 63, textureName: "default small", alignment:  "left", spacing: Config.spacing_small);
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

        Vector2f position = new Vector2f(Camera.X + x - (sprite.GetLocalBounds().Width / 2)*x_scale, Camera.Y + y - sprite.GetLocalBounds().Height / 2);

        // Shadow
        if (state == SELECTING_PALETTE || state == READY) {
            sprite.Position = position + new Vector2f(shadow_x_offset, 0);
            Program.window.Draw(sprite, new RenderStates(Program.colorFillShader));
        }

        // Sprite
        sprite.Position = position;
        Program.window.Draw(sprite, Data.characters[pointer].SetSwaperShader(
            palette_index: (int) (state == SELECTING_CHAR ? 0 : selected.palette_index), 
            light: state == READY ? new Color(128, 128, 128) : Color.White
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
        if (state != READY && UI.DrawButton("<   ", x, y, hover: true, click: InputManager.Key_hold("Left", player: player), action: InputManager.Key_down("Left", player: player), hover_font: "default small", font: "", spacing: 0)) {
            if (state == SELECTING_CHAR) pointer = pointer > 0 ? pointer - 1 : Data.characters.Count - 1;
            else if (state == SELECTING_PALETTE) selected.ChangePalette(-1);
        } 
        
        if (state != READY && UI.DrawButton("   >", x, y, hover: true, click: InputManager.Key_hold("Right", player: player), action: InputManager.Key_down("Right", player: player), hover_font: "default small", font: "", spacing: 0)) {
            if (state == SELECTING_CHAR) pointer = pointer < Data.characters.Count - 1 ? pointer + 1 : 0;
            else if (state == SELECTING_PALETTE) selected.ChangePalette(1);
        }

        if (UI.DrawButton(Data.characters[pointer].name, x, y + info_y_offset - 10, spacing: Config.spacing_small, action: InputManager.Key_down("A", player: player), click: InputManager.Key_hold("A", player: player), hover_font: "default small")) {
            if (state == SELECTING_CHAR) {
                selected = Data.characters[pointer].Copy();
                state = SELECTING_PALETTE;

            } else if (state == SELECTING_PALETTE) {
                state = READY;

            } else if (state == READY) {
                state = SELECTING_CHAR;
                selected = null;
            }
        }
    }

    public void Reset() {
        pointer = 0;
        state = SELECTING_CHAR;
        selected = null;
    }
}