using UI_space;
using SFML.Graphics;
using Language_space;
using SFML.System;

public class WGSelectCharacter : Widget {
    private Sprite char_bg = new Sprite(Data.textures["screens:bgchar"]);
    Sprite sprite_A = new Sprite(Data.textures["other:placeholder"]);
    Sprite sprite_B = new Sprite(Data.textures["other:placeholder"]);
    public Character charA_selected;
    public Character charB_selected;
    public int pointer_charA = 0;
    public int pointer_charB = 0;
    public bool ready_charA = false;
    public bool ready_charB = false;

    public override void Render() {
        Program.window.Draw(char_bg);

        // Setup sprites texture
        sprite_A.Texture = Data.characters[pointer_charA].thumb;
        sprite_B.Texture = Data.characters[pointer_charB].thumb;
        UI.DrawText(Program.playerA_wins.ToString(), 0, 63, alignment: "right");
        UI.DrawText(Program.playerB_wins.ToString(), 0, 63, alignment: "left" );
        UI.DrawText(Language.GetText(Program.AI_playerA ? "COM" : "P1"), -15, 63, textureName: "default small", alignment: "right", spacing: Config.spacing_small);
        UI.DrawText(Language.GetText(Program.AI_playerB ? "COM" : "P2"),  15, 63, textureName: "default small", alignment:  "left", spacing: Config.spacing_small);

        // Draw Shadows
        Program.colorFillShader.SetUniform("fillColor", Color.Black);
        if (charA_selected != null) {
            sprite_A.Scale = new Vector2f(1f, 1f);
            sprite_A.Position = new Vector2f(Camera.X - 87 - sprite_A.GetLocalBounds().Width / 2, Camera.Y - 20 - sprite_A.GetLocalBounds().Height / 2);
            Program.window.Draw(sprite_A, new RenderStates(Program.colorFillShader));
        } if (charB_selected != null) {
            sprite_B.Scale = new Vector2f(-1f, 1f);
            sprite_B.Position = new Vector2f(Camera.X + 67 + sprite_B.GetLocalBounds().Width / 2, Camera.Y - 20 - sprite_B.GetLocalBounds().Height / 2);
            Program.window.Draw(sprite_B, new RenderStates(Program.colorFillShader));
        }

        // Draw main sprite A
        sprite_A.Scale = new Vector2f(1f, 1f);
        sprite_A.Position = new Vector2f(Camera.X - 77 - sprite_B.GetLocalBounds().Width / 2, Camera.Y - 20 - sprite_B.GetLocalBounds().Height / 2);
        Program.window.Draw(sprite_A, Data.characters[pointer_charA].SetSwaperShader(
            palette_index: (int) (charA_selected != null ? charA_selected.palette_index : 0), 
            light: ready_charA ? new Color(128, 128, 128) : Color.White
        ));
        if (ready_charA) UI.DrawText(Language.GetText("ready"), -77, -20, textureName: "default small", spacing: Config.spacing_small);
        if (charA_selected != null && !ready_charA) {
            UI.DrawRectangle(-77, 67, 6, 6, fill_color: Color.White);
            UI.DrawRectangle(-77, 68, 4, 4, fill_color: charA_selected.current_palette_color);
        } else {
            for (int i = 0; i < Data.characters.Count; i++) UI.DrawText(i == pointer_charA ? ")" : "(", (i * 10) - ((Data.characters.Count - 1) * 5) - 77, 55, textureName: "icons");
        }

        // Draw main sprite B
        sprite_B.Scale = new Vector2f(-1f, 1f);
        sprite_B.Position = new Vector2f(Camera.X + 77 + sprite_B.GetLocalBounds().Width / 2, Camera.Y - 20 - sprite_B.GetLocalBounds().Height / 2);
        Program.window.Draw(sprite_B, Data.characters[pointer_charB].SetSwaperShader( 
            palette_index: (int) (charB_selected != null ? charB_selected.palette_index : 0), 
            light: ready_charB ? new Color(128, 128, 128) : Color.White
        ));
        if (ready_charB) UI.DrawText(Language.GetText("ready"), +77, -20, textureName: "default small", spacing: Config.spacing_small);
        if (charB_selected != null && !ready_charB) {
            UI.DrawRectangle(77, 67, 6, 6, fill_color: Color.White);
            UI.DrawRectangle(77, 68, 4, 4, fill_color: charB_selected.current_palette_color);  
        } else {
            for (int i = 0; i < Data.characters.Count; i++) UI.DrawText(i == pointer_charB ? ")" : "(", (i * 10) - ((Data.characters.Count - 1) * 5) + 77, 55, textureName: "icons");
        }

        // Draw shoulder buttons
        UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
        if (UI.DrawButton(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: InputManager.Key_hold("LB"), action: InputManager.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
            Program.ChangeState(Program.SelectStage);
            charA_selected = null;
            charB_selected = null;
            ready_charA = false;
            ready_charB = false;
        }
        UI.DrawText("F", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
        if (UI.DrawButton(Language.GetText("Controls"), 182, 67, spacing: Config.spacing_small, alignment: "right", click: InputManager.Key_hold("RB"), action: InputManager.Key_up("RB"), click_font: "default small click", hover_font: "default small")) 
            Program.ChangeState(Program.Controls);

        // Autoselect
        if (charA_selected == null && Program.AI_playerA && ready_charB) {
            pointer_charA = AI.rand.Next(0, Data.characters.Count);
            charA_selected = Data.characters[pointer_charA].Copy();
            if (pointer_charA == pointer_charB && charB_selected.palette_index == 0) charA_selected.ChangePalette(AI.rand.Next());
            ready_charA = true;
        }
        if (charB_selected == null && Program.AI_playerB && ready_charA) {
            pointer_charB = AI.rand.Next(0, Data.characters.Count);
            charB_selected = Data.characters[pointer_charB].Copy();
            if (pointer_charB == pointer_charA && charA_selected.palette_index == 0) charB_selected.ChangePalette(AI.rand.Next());
            ready_charB = true;
        }

        // Buttons A
        if (!ready_charA && UI.DrawButton("<   ", -77, -16, hover: true, click: InputManager.Key_hold("Left", player: 1), action: InputManager.Key_down("Left", player: 1), hover_font: "default small", font: "", spacing: 0)) {
            if (charA_selected == null) pointer_charA = pointer_charA > 0 ? pointer_charA - 1 : Data.characters.Count - 1;
            else charA_selected.ChangePalette(-1);
        } if (!ready_charA && UI.DrawButton("   >", -77, -16, hover: true, click: InputManager.Key_hold("Right", player: 1), action: InputManager.Key_down("Right", player: 1), hover_font: "default small", font: "", spacing: 0)) {
            if (charA_selected == null) pointer_charA = pointer_charA < Data.characters.Count - 1 ? pointer_charA + 1 : 0;
            else charA_selected.ChangePalette(1);
        }

        if (UI.DrawButton(Data.characters[pointer_charA].name, -77, 45, spacing: Config.spacing_small, action: InputManager.Key_down("A", player: 1), click: InputManager.Key_hold("A", player: 1), hover_font: "default small"))
            if (charA_selected == null) charA_selected = Data.characters[pointer_charA].Copy();
            else ready_charA = !ready_charA;

        // Buttons B
        if (!ready_charB && UI.DrawButton("<   ", 77, -16, hover: true, click: InputManager.Key_hold("Left", player: 2), action: InputManager.Key_down("Left", player: 2), hover_font: "default small", font: "", spacing: 0)) {
            if (charB_selected == null) pointer_charB = pointer_charB > 0 ? pointer_charA - 1 : Data.characters.Count - 1;
            else charB_selected.ChangePalette(-1);
        } if (!ready_charB && UI.DrawButton("   >", 77, -16, hover: true, click: InputManager.Key_hold("Right", player: 2), action: InputManager.Key_down("Right", player: 2), hover_font: "default small", font: "", spacing: 0)) {
            if (charB_selected == null) pointer_charB = pointer_charB < Data.characters.Count - 1 ? pointer_charB + 1 : 0;
            else charB_selected.ChangePalette(1);
        }

        if (UI.DrawButton(Data.characters[pointer_charB].name, 77, 45, spacing: Config.spacing_small, action: InputManager.Key_down("A", player: 2), click: InputManager.Key_hold("A", player: 2), hover_font: "default small"))
            if (charB_selected == null) charB_selected = Data.characters[pointer_charB].Copy();
            else ready_charB = !ready_charB;

        // Ends when chars are selected and ready
        if (ready_charA && ready_charB) {
            if (UI.blink2Hz) UI.DrawText(Language.GetText("press start"), 0, -90, spacing: Config.spacing_medium);
            if (InputManager.Key_down("Start")) Program.ChangeState(Program.LoadScreen);
        }
    }
}