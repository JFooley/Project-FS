using Data_space;
using Language_space;
using SFML.Graphics;
using SFML.System;
using Stage_Space;
using System.Drawing;

namespace UI_space {
    public class UI {
        private static int elapsed = 0;
        public static int counter = 0;

        // Clocks
        public static bool blink30Hz = true;
        public static bool blink10Hz = true;
        public static bool blink4Hz = true;
        public static bool blink2Hz = true;
        public static bool blink1Hz = true;

        private static int graylife_A = 150;
        private static int graylife_B = 150;

        public static SFML.Graphics.Color bar_graylife = new SFML.Graphics.Color(195, 248, 233);
        public static SFML.Graphics.Color bar_fulllife = new SFML.Graphics.Color(44, 191, 54);
        public static SFML.Graphics.Color bar_life = new SFML.Graphics.Color(60, 166, 136);
        public static SFML.Graphics.Color bar_super = new SFML.Graphics.Color(5, 110, 150);
        public static SFML.Graphics.Color bar_super_full = new SFML.Graphics.Color(0, 185, 255);
        public static SFML.Graphics.Color bar_stun = new SFML.Graphics.Color(242, 65, 12);

        // visuals
        private static Sprite hud;
        private static Dictionary<string, Dictionary<char, Sprite>> font_textures;
        private static string superBarMsg = "max aura";

        // Positions
        private static int life_bar_Y = -93;
        private static int life_bar_X =  -180;
        
        private static int super_bar_Y = 96;
        private static int super_bar_X = -179;

        private static int stun_bar_Y = -84;
        private static int stun_bar_X = -178;

        public UI() {
            font_textures = new Dictionary<string, Dictionary<char, Sprite>>();
            UI.hud = new Sprite(Program.visuals["hud"]);
            UI.Load();
        }

        public static void Update() {
            UI.counter = UI.counter <= 60 ? UI.counter + 1 : 1;
            UI.blink30Hz = UI.counter % (60/30) == 0 ? UI.blink30Hz = !UI.blink30Hz : UI.blink30Hz;
            UI.blink10Hz = UI.counter % (60/10) == 0 ? UI.blink10Hz = !UI.blink10Hz : UI.blink10Hz;
            UI.blink4Hz = UI.counter % (60/4) == 0 ? UI.blink4Hz = !UI.blink4Hz : UI.blink4Hz;
            UI.blink2Hz = UI.counter % (60/2) == 0 ? UI.blink2Hz = !UI.blink2Hz : UI.blink2Hz;
            UI.blink1Hz = UI.counter % 60 == 0 ? UI.blink1Hz = !UI.blink1Hz : UI.blink1Hz;;
        }

        // Loads
        public static void LoadFontSprites(float size, string textureName) {
            Dictionary<char, Sprite> characterSprites = new Dictionary<char, Sprite>(); // Cria grupo de sprites
            foreach (char c in BitmapFont.characters) {
                Sprite sprite = BitmapFont.GetCharacterSprite(c, size, textureName);
                if (sprite != null) characterSprites[c] = sprite;
            }
            UI.font_textures[textureName] = characterSprites;
        }

        // Draw Callers
        public static void ShowFramerate(string textureName) {
            UI.elapsed = UI.counter % (60/2) == 0 ? (int) (1 / Program.last_frame_time) : UI.elapsed;
            UI.DrawText(UI.elapsed.ToString() + " - " + Program.last_frame_time.ToString("F5"), 0, 82, spacing: Config.spacing_small, textureName: textureName);
        }

        public static void DrawText(string text, float X, float Y, float spacing = 0, string alignment = "center", bool absolutePosition = false, string textureName = "default medium") {
            float totalWidth = 0;
            float pos_X;
            float pos_Y;
            float offset_X = X;
            List<Sprite> text_sprites = new List<Sprite> {};

            // Calcular a largura total do texto

            foreach (char c in text) {
                if (UI.font_textures[textureName].TryGetValue(c, out Sprite letter)) {
                    var sprite = new Sprite(letter);
                    totalWidth += sprite.GetGlobalBounds().Width + spacing;
                    text_sprites.Add(sprite);
                }
            }
            
            // Compensa o primeiro caractere
            totalWidth -= spacing;

            // Ajustar posição se centralizado
            if (alignment == "center") {
                offset_X -= totalWidth / 2; 
            } else if (alignment == "right") {
                offset_X -= totalWidth; 
            }

            if (absolutePosition) {
                pos_X = X;
                pos_Y = Y;
                offset_X = 0;
            } else {
                pos_X = Camera.X;
                pos_Y = Camera.Y;
            }
            foreach (Sprite sprite in text_sprites) {   
                sprite.Position = new Vector2f(pos_X + offset_X, pos_Y + Y);
                Program.window.Draw(sprite);
                offset_X += sprite.GetGlobalBounds().Width + spacing;
            }
        }

        public static void DrawRectangle(float X, float Y, float width, float height, SFML.Graphics.Color color) {
            RectangleShape rectangle = new RectangleShape(new Vector2f(width, height))
            {
                Position = new Vector2f(Camera.X + X, Camera.Y + Y),
                FillColor = color
            };

            Program.window.Draw(rectangle);
        }
    
        public static void DrawBar(float X, float Y, float currentValue, float maxValue, string textureName, string alignment = "center", bool mirrored = false, SFML.Graphics.Color? color = null, bool grow_inverted = true) {
            if (!Program.visuals.ContainsKey(textureName)) return;
            
            Sprite barSprite = new Sprite(Program.visuals[textureName]);
            IntRect originalRect = barSprite.TextureRect;

            // Pinta a sprite se uma cor for fornecida
            RenderStates renderStates = RenderStates.Default;
            if (color.HasValue)  {
                Program.colorFillShader.SetUniform("fillColor", new SFML.Graphics.Glsl.Vec3(color.Value.R, color.Value.G, color.Value.B));
                renderStates = new RenderStates(Program.colorFillShader);
            }
            
            // Calcula a porcentagem da barra
            float percentage = Math.Max(Math.Min(currentValue / maxValue, 1f), 0f);
            
            // Define a região da textura que será mostrada
            IntRect textureRect = originalRect;
            textureRect.Width = (int)(originalRect.Width * percentage);
            
            // Calcula quanto foi diminuído
            int reducedWidth = originalRect.Width - textureRect.Width;
            if (grow_inverted) textureRect.Left = originalRect.Left + reducedWidth;
            
            float scaleX = mirrored ? -1f : 1f;
            barSprite.Scale = new Vector2f(scaleX, 1f);
            
            // Ajusta a posição baseado no alinhamento
            float posX = Camera.X + X;
            float posY = Camera.Y + Y;
            
            // 2. Move a posição original do rect + X para a direita
            if (mirrored) {
                if (grow_inverted) posX -= reducedWidth;

                if (alignment == "center")
                    posX += originalRect.Width / 2f;
                else if (alignment == "left")
                    posX += originalRect.Width;

            } else {
                if (grow_inverted) posX += reducedWidth;

                if (alignment == "center") 
                    posX -= originalRect.Width / 2f;
                else if (alignment == "right")
                    posX -= originalRect.Width;
            }
            
            barSprite.TextureRect = textureRect;
            barSprite.Position = new Vector2f(posX, posY);
            
            Program.window.Draw(barSprite, renderStates);
        }
        
        public static bool DrawButton(string text, float pos_X, float pos_Y, bool hover = false, bool click = false, float spacing = Config.spacing_small, string alignment = "center", bool absolutePosition = false, string font = "default small", string hover_font = "default small hover", string click_font = "default small click") {
            if (click && hover) {
                UI.DrawText(text, pos_X, pos_Y, spacing: spacing, alignment: alignment, absolutePosition: absolutePosition, textureName: click_font);
                return true;
            } else if (hover) {
                UI.DrawText(text, pos_X, pos_Y, spacing: spacing, alignment: alignment, absolutePosition: absolutePosition, textureName: hover_font);
                return false;
            } else {
                UI.DrawText(text, pos_X, pos_Y, spacing: spacing, alignment: alignment, absolutePosition: absolutePosition, textureName: font);
                return false;
            }
        }

        // Battle UI
        public static void DrawBattleUI(Stage stage) {
            // Draw hud
            hud.Position = new Vector2f(Camera.X - 192, Camera.Y - 108);
            Program.window.Draw(hud);

            // Lifebar A
            var lifeA_scale = stage.character_A.life_points.X * 150 / stage.character_A.life_points.Y;
            var lifeA = Math.Max(Math.Min(lifeA_scale, 150), 0);
            if (stage.character_B.combo_counter == 0) UI.graylife_A = lifeA > UI.graylife_A ? UI.graylife_A = lifeA : (int) (UI.graylife_A + (lifeA - UI.graylife_A) * 0.01);            
            UI.DrawBar(life_bar_X, life_bar_Y, UI.graylife_A, 150, "lifebar", alignment: "left", mirrored: false, color: UI.bar_graylife);
            var lifeColorA = stage.character_A.life_points.X == stage.character_A.life_points.Y ? UI.bar_fulllife : UI.bar_life;
            UI.DrawBar(life_bar_X, life_bar_Y, lifeA, 150, "lifebar", alignment: "left", mirrored: false, color: lifeColorA);

            // Lifebar B
            var lifeB_scale = stage.character_B.life_points.X * 150 / stage.character_B.life_points.Y;
            var lifeB = Math.Max(Math.Min(lifeB_scale, 150), 0);
            if (stage.character_A.combo_counter == 0) UI.graylife_B = lifeB > UI.graylife_B ? UI.graylife_B = lifeB : (int) (UI.graylife_B + (lifeB - UI.graylife_B) * 0.01);            
            UI.DrawBar(-life_bar_X, life_bar_Y, UI.graylife_B, 150, "lifebar", alignment: "right", mirrored: true, color: UI.bar_graylife);
            var lifeColorB = stage.character_B.life_points.X == stage.character_B.life_points.Y ? UI.bar_fulllife : UI.bar_life;
            UI.DrawBar(-life_bar_X, life_bar_Y, lifeB, 150, "lifebar", alignment: "right", mirrored: true, color: lifeColorB);
            
            // Super bar A
            var superA_scale = stage.character_A.aura_points.X * 117 / stage.character_A.aura_points.Y;
            var superA = Math.Max(Math.Min(superA_scale, 117), 0);
            UI.DrawBar(super_bar_X, super_bar_Y, superA, 117, "aurabar", alignment: "left", mirrored: false, color: UI.bar_super, grow_inverted: false);
            if (stage.character_A.aura_points.X >= stage.character_A.aura_points.Y/2) {
                var control = stage.character_A.aura_points.X == stage.character_A.aura_points.Y ? UI.blink10Hz : true;
                if (control) UI.DrawBar(super_bar_X, super_bar_Y, superA, 117, "aurabar", alignment: "left", mirrored: false, color: UI.bar_super_full, grow_inverted: false);
            }
            if (stage.character_A.aura_points.X == stage.character_A.aura_points.Y && UI.blink2Hz) UI.DrawText(UI.superBarMsg, -193, 73, spacing: Config.spacing_medium, alignment: "left", textureName: "default small white");
                        
            // Super bar B
            var superB_scale = stage.character_B.aura_points.X * 117 / stage.character_B.aura_points.Y;
            var superB = Math.Max(Math.Min(superB_scale, 117), 0);
            UI.DrawBar(-super_bar_X, super_bar_Y, superB, 117, "aurabar", alignment: "right", mirrored: true, color: UI.bar_super, grow_inverted: false);
            if (stage.character_B.aura_points.X >= stage.character_B.aura_points.Y/2) {
                var control = stage.character_B.aura_points.X == stage.character_B.aura_points.Y ? UI.blink10Hz : true;
                if (control) UI.DrawBar(-super_bar_X, super_bar_Y, superB, 117, "aurabar", alignment: "right", mirrored: true, color: UI.bar_super_full, grow_inverted: false);
            }
            if (stage.character_B.aura_points.X == stage.character_B.aura_points.Y && UI.blink2Hz) UI.DrawText(UI.superBarMsg, 193, 73, spacing: Config.spacing_medium, alignment: "right", textureName: "default small white");

            // Stun bar A
            var stunA_scale = ( stage.character_A.dizzy_points.Y - stage.character_A.dizzy_points.X) * 150 / stage.character_A.dizzy_points.Y;
            var stunA = Math.Max(Math.Min(stunA_scale, 150), 0);
            UI.DrawBar(stun_bar_X, stun_bar_Y, stunA, 150, "stunbar", alignment: "left", mirrored: false, color: bar_stun, grow_inverted: true);

            // Stun bar B
            var stunB_scale = ( stage.character_B.dizzy_points.Y - stage.character_B.dizzy_points.X) * 150 / stage.character_B.dizzy_points.Y;
            var stunB = Math.Max(Math.Min(stunB_scale, 150), 0);
            UI.DrawBar(-stun_bar_X, stun_bar_Y, stunB, 150, "stunbar", alignment: "right", mirrored: true, color: bar_stun, grow_inverted: true);
            
            // Names
            UI.DrawText(stage.character_A.name, -191, -94, spacing: Config.spacing_small, alignment: "left", textureName: "default small white");
            UI.DrawText(stage.character_B.name, 191, -94, spacing: Config.spacing_small, alignment: "right", textureName: "default small white");

            if (stage.character_A.AIEnabled && stage.character_A.BotEnabled && UI.blink1Hz) 
                UI.DrawText(Language.GetText("P1 - press select"), -Config.RenderWidth/2, -Config.RenderHeight/2 - 10, spacing: Config.spacing_small, alignment: "left", textureName: "default small white");
            if (stage.character_B.AIEnabled && stage.character_B.BotEnabled && UI.blink1Hz) 
                UI.DrawText(Language.GetText("press select - P2"), Config.RenderWidth/2, -Config.RenderHeight/2 - 10, spacing: Config.spacing_small, alignment: "right", textureName: "default small white");

            // Combo text
            if (stage.character_A.combo_counter > 1) UI.DrawText(stage.character_A.combo_counter + " hits", -190, -80, spacing: -23, alignment: "left", textureName: "default medium white");
            if (stage.character_B.combo_counter > 1) UI.DrawText(stage.character_B.combo_counter + " hits", 190, -80, spacing: -23, alignment: "right", textureName: "default medium white");

            // Time
            UI.DrawText("" + Math.Max(stage.round_time, 0), 0, -106, alignment: "center", spacing: -8, textureName: "1");

            // Round indicators
            UI.DrawText(string.Concat(Enumerable.Repeat("-", Math.Max(Config.max_rounds - stage.rounds_A, 0))) + string.Concat(Enumerable.Repeat("*", stage.rounds_A)), -20, -91, spacing: -19, alignment: "right", textureName: "icons");
            UI.DrawText(string.Concat(Enumerable.Repeat("*", stage.rounds_B)) + string.Concat(Enumerable.Repeat("-", Math.Max(Config.max_rounds - stage.rounds_B, 0))),  20, -91, spacing: -19, alignment: "left", textureName: "icons");
        }
        
        public static void Load() {
            BitmapFont.Load();
            BitmapFont.Rename("font1", "1");

            UI.LoadFontSprites(32, "default medium");
            UI.LoadFontSprites(32, "default medium grad");
            UI.LoadFontSprites(32, "default medium white");
            UI.LoadFontSprites(32, "default medium red");
            UI.LoadFontSprites(32, "default medium click");
            UI.LoadFontSprites(32, "default medium hover");

            UI.LoadFontSprites(32, "default small");
            UI.LoadFontSprites(32, "default small grad");
            UI.LoadFontSprites(32, "default small white");
            UI.LoadFontSprites(32, "default small red");
            UI.LoadFontSprites(32, "default small click");
            UI.LoadFontSprites(32, "default small hover");

            UI.LoadFontSprites(32, "1");
            UI.LoadFontSprites(32, "icons");
        }
    }

    public static class BitmapFont {
        private static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        public static char[] characters = {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
            'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
            'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
            'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z', '!', '@', '#', '$', '%', '^', '&', '*',
            '(', ')', '_', '+', '-', '=', '[', ']', '{', '}',
            ';', ':', '\'', '"', '\\', '|', ',', '.', '<', '>',
            '/', '?', '¿', '\u0020', '«', '»', '•', '°', 'µ', '~'
        };

        private const int CellSize = 32; // Tamanho de cada célula
        private const int Columns = 10;  // Número de colunas
        private const int Rows = 10;     // Número de linhas

        public static void Load() {
            string currentDirectory = Directory.GetCurrentDirectory();
            string full_path = Path.Combine(currentDirectory, "Assets/fonts");
            if (!System.IO.Directory.Exists(full_path))
                throw new System.IO.DirectoryNotFoundException($"O diretório {full_path} não foi encontrado.");

            try
            {
                DataManagement.LoadTexturesFromFile("Assets/fonts/fonts.dat", BitmapFont.textures);
            }
            catch (Exception e)
            {
                DataManagement.LoadTexturesFromPath("Assets/fonts", BitmapFont.textures);
                DataManagement.SaveTexturesToFile("Assets/fonts/fonts.dat", BitmapFont.textures);
            }
        }

        public static void Rename(string old_name, string new_name) {
            textures[new_name] = textures[old_name];
            textures.Remove(old_name);
        }

        public static Sprite GetCharacterSprite(char character, float size, string textureName)
        {
            // Verifica se a textura existe
            if (!textures.ContainsKey(textureName))
            {
                return null; // Retorna null se a textura não for encontrada
            }

            Texture texture = textures[textureName];

            // Encontra o índice do caractere no array
            int index = Array.IndexOf(characters, character);
            if (index == -1 || index >= Columns * Rows) // Ignora as últimas 8 células
            {
                return null; // Retorna null se o caractere não for encontrado
            }

            // Calcula a posição na textura
            int x = (index % Columns) * CellSize;
            int y = (index / Columns) * CellSize;

            // Cria um sprite do caractere
            Sprite sprite = new Sprite(texture, new IntRect(x, y, CellSize, CellSize));
            sprite.Scale = new Vector2f(size / CellSize, size / CellSize); // Ajusta o tamanho
            return sprite;
        }
    }

}