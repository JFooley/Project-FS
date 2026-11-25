using UI_space;
using SFML.Graphics;
using SFML.System;

public class WGSelectCharacter : Widget {
    private const int default_difficulty = 2;
    public static int difficulty = 2;

    private static WGSelector selectorA;
    private static WGSelector selectorB;

    public static Character? charA_selected => selectorA.selected;
    public static Character? charB_selected => selectorB.selected;

    private static bool A_flag = false;
    private static bool B_flag = false; 

    private static int offset = 0;

    private static Sprite char_bg;
    private static Sprite fade90;

    public WGSelectCharacter() {
        selectorA = new WGSelector("selectorA");
        selectorB = new WGSelector("selectorB");
        fade90 = new Sprite(Data.textures["screens:90fade"]) {Color = Color.Transparent};
        char_bg = new Sprite(Data.textures["screens:bgchar"]);
    }

    public override void Render() {
        MainRender.Draw(char_bg, MainRender.BackgroundLayer);
        // Program.colorFillShader.SetUniform("fillColor", new Vector3f(UI.darkA.R, UI.darkA.G, UI.darkA.B));

        Accessibility.Speak("SC", TTSRequisition.TEXT, true, S("select char"));

        DrawSessionInfo();

        if (Input.Key_up("B", 1) && selectorA.state == WGSelector.State.SelectingChar || Input.Key_up("B", 2) && selectorB.state == WGSelector.State.SelectingChar) {
            Program.ChangeState(Program.previous_state);
        }

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
    
        if (selectorA.state == WGSelector.State.Ready && selectorB.state == WGSelector.State.Ready) {
            MainRender.Draw(fade90, MainRender.OverlayLayer);
            offset += 5;
            fade90.Color = new Color(255, 255, 255, (byte) Math.Min(255, fade90.Color.A + 15));
            if (offset >= Config.RenderWidth / 2 && fade90.Color.A == 255) {
                WGSelectCharacter.charA_selected.BOT.difficulty = WGSelectCharacter.difficulty;
                WGSelectCharacter.charB_selected.BOT.difficulty = WGSelectCharacter.difficulty;
                A_flag = false;
                B_flag = false;
                offset = 0;
                difficulty = default_difficulty;
                fade90.Color = Color.Transparent;
                Program.ChangeState(Program.State.LoadScreen);
            }
        }
    }

    private void Versus() {
        selectorA.Render(-77 - offset, -30, 1, x_scale: 1);
        selectorB.Render(77 + offset, -30, 2, x_scale: -1);
    }
    private void VersusBOT() {
        if (Stage.AI_playerA && Stage.AI_playerB && Input.anyKeyA) Stage.AI_playerA = false;
        if (Stage.AI_playerB && Stage.AI_playerA && Input.anyKeyB) Stage.AI_playerB = false;

        selectorA.Render(-77 - offset, -30, B_flag ? 0 : 1, x_scale: 1, is_bot: Stage.AI_playerA);
        selectorB.Render(77 + offset, -30, A_flag ? 0 : 2, x_scale: -1, is_bot: Stage.AI_playerB);

        A_flag = selectorA.state == WGSelector.State.Ready;
        B_flag = selectorB.state == WGSelector.State.Ready;
    }
    private void Training() {
        selectorA.Render(-77 - offset, -30, B_flag ? 0 : 1, x_scale: 1);
        selectorB.Render(77 + offset, -30, A_flag ? 0 : 2, x_scale: -1);

        A_flag = selectorA.state == WGSelector.State.Ready;
        B_flag = selectorB.state == WGSelector.State.Ready;
    }

    private void DrawSessionInfo() {
        UI.DrawTextAtlas(S(Program.playerA_wins.ToString()), 0, 63, alignment: UI.Alignment.Right);
        UI.DrawTextAtlas(S(Program.playerB_wins.ToString()), 0, 63, alignment: UI.Alignment.Left );
        UI.DrawTextAtlas(S(Stage.AI_playerA ? "COM" : "P1"), -15, 63, textureName: "default small", alignment: UI.Alignment.Right, spacing: Config.spacing_small);
        UI.DrawTextAtlas(S(Stage.AI_playerB ? "COM" : "P2"),  15, 63, textureName: "default small", alignment: UI.Alignment.Left, spacing: Config.spacing_small);
    }
    public void Reset() {
        selectorA.Reset();
        selectorB.Reset();
    }
}

public class WGSelector : Widget {
    public enum State {
        SelectingChar,
        SelectingPalette,
        SelectingDifficulty,
        Ready
    }

    private string selector_id;
    public int pointer = 0;
    public State state = State.SelectingChar;
    public Character? selected = null;

    Sprite sprite;

    Vector2f position;

    public WGSelector(string id) {
        this.selector_id = id;
        this.sprite = new Sprite(Data.textures["other:placeholder"]);
    }

    public void Render(float x, float y, int player, int x_scale = 1, int shadow_x_offset = -10, int info_y_offset = 75, bool is_bot = false) {
        sprite.Texture = Data.characters[pointer].thumb;
        sprite.Scale = new Vector2f(x_scale, 1f);
        position = new Vector2f(Camera.X + x - sprite.GetLocalBounds().Width / 2 * x_scale, Camera.Y + y - sprite.GetLocalBounds().Height / 2);

        // Shadow
        if (selected != null && (state == State.SelectingPalette || state == State.SelectingDifficulty || state == State.Ready)) {
            // sprite.Position = position + new Vector2f(shadow_x_offset, 0);
            // MainRender.Draw(sprite, new RenderStates(Program.colorFillShader), MainRender.ShadowLayer);
            UI.DrawText(S(selected.name), 0, 0, font: "style", color: UI.darkA, alignment: x_scale > 0 ? UI.Alignment.Right : UI.Alignment.Left, layer: MainRender.ShadowLayer);
        }

        // Sprite
        sprite.Position = position;
        MainRender.Draw(sprite, Data.characters[pointer].SetSwaperShader(
            palette_index: (int) (state == State.SelectingChar ? 0 : selected.palette_index),
            light: state == State.Ready || Input.anyKey[player] ? new Color(128, 128, 128) : Color.White
        ), MainRender.CharacterLayer);

        // Info
        if (state == State.Ready) {
            UI.DrawTextAtlas(S("ready"), x, y, textureName: "default small", spacing: Config.spacing_small);
        } else if (state == State.SelectingChar) {
            for (int i = 0; i < Data.characters.Count; i++) UI.DrawTextAtlas(S(i == pointer ? ")" : "("), (i * 10) - ((Data.characters.Count - 1) * 5) + x, y + info_y_offset, textureName: "icons");
        } else if (state == State.SelectingPalette) {
            for (int i = 0; i < selected?.palette_quantity; i++) UI.DrawTextAtlas(S(i == selected.palette_index ? ")" : "("), (i * 10) - ((selected.palette_quantity - 1) * 5) + x, y + info_y_offset, textureName: "icons");
        } else if (state == State.SelectingDifficulty) {
            UI.DrawTextAtlas(S("difficulty", ": ", (5 - WGSelectCharacter.difficulty).ToString()), x, y + info_y_offset, TTS: true, spacing: Config.spacing_small, textureName: "default small");
        } 

        // Buttons
        if (state != State.Ready && UI.DrawButton(S("<   "), x, y, hover: true, click: Input.Key_hold("Left", player), action: Input.Key_up("Left", player) || (Input.Key_hold_for("Left", Config.hold_time, player) && UI.ForEach(Config.hold_clock)), button_sound: 0, hover_font: "default small", font: "", spacing: 0)) {
            if (state == State.SelectingChar) pointer -= pointer > 0 ? 1 : 0;
            else if (state == State.SelectingPalette) selected.SetPalette(-1);
            else if (state == State.SelectingDifficulty) WGSelectCharacter.difficulty += WGSelectCharacter.difficulty < 5 ? 1 : 0;
        } 
        
        if (state != State.Ready && UI.DrawButton(S("   >"), x, y, hover: true, click: Input.Key_hold("Right", player), action: Input.Key_up("Right", player) || (Input.Key_hold_for("Right", Config.hold_time, player) && UI.ForEach(Config.hold_clock)), button_sound: 0, hover_font: "default small", font: "", spacing: 0)) {
            if (state == State.SelectingChar) pointer += pointer < Data.characters.Count - 1 ? 1 : 0;
            else if (state == State.SelectingPalette) selected.SetPalette(1);
            else if (state == State.SelectingDifficulty) WGSelectCharacter.difficulty -= WGSelectCharacter.difficulty > 0 ? 1 : 0;
        }

        if (UI.DrawButton(S(Data.characters[pointer].name), x, y + info_y_offset - 10, id: selector_id, priority: true, spacing: Config.spacing_medium, action: Input.Key_up("A", player), click: Input.Key_hold("A", player: player) && this.state != State.Ready, hover_font: "default small", button_sound: 1)) {
            if (state == State.SelectingChar) {
                selected = Data.characters[pointer].Copy();
                state = State.SelectingPalette;
            } else if (state == State.SelectingPalette) {
                state = is_bot ? State.SelectingDifficulty : State.Ready;
            } else if (state == State.SelectingDifficulty) {
                state = State.Ready;
            }
        }

        if (Input.Key_up("B", player)) {
            state = State.SelectingChar;
            selected = null;
        }
    }
    public void Reset() {
        pointer = 0;
        state = State.SelectingChar;
        selected = null;
    }
}