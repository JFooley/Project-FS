using Data_space;
using SFML.Graphics;
using SFML.System;
using Stage_Space;
using System.Drawing;

namespace UI_space {
    public class UI {
        private static UI instance;
        private int elapsed = 0;
        public int counter = 0;

        // Clocks
        public bool blink30Hz = true;
        public bool blink10Hz = true;
        public bool blink4Hz = true;
        public bool blink2Hz = true;
        public bool blink1Hz = true;

        private int graylife_A = 150;
        private int graylife_B = 150;

        public SFML.Graphics.Color light_background = new SFML.Graphics.Color(150, 150, 150);
        public SFML.Graphics.Color background = new SFML.Graphics.Color(35, 31, 34);
        public SFML.Graphics.Color outline = new SFML.Graphics.Color(255, 255, 255);
        public SFML.Graphics.Color bar_graylife = new SFML.Graphics.Color(200, 0, 0);
        public SFML.Graphics.Color bar_fulllife = new SFML.Graphics.Color(50, 190, 60);
        public SFML.Graphics.Color bar_life = new SFML.Graphics.Color(240, 220, 20);
        public SFML.Graphics.Color bar_super = new SFML.Graphics.Color(5, 110, 150);
        public SFML.Graphics.Color bar_super_full = new SFML.Graphics.Color(0, 185, 255);

        // visuals
        Sprite hud;
        private Dictionary<string, Dictionary<char, Sprite>> font_textures;
        private string superBarMsg = "max aura";

        private UI()
        {
            this.font_textures = new Dictionary<string, Dictionary<char, Sprite>>();
            this.hud = new Sprite(Program.visuals["hud"]);
        }

        public static UI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UI();
                }
                return instance;
            }
        }

        public void Update() {
            this.counter = this.counter <= 60 ? this.counter + 1 : 1;
            this.blink30Hz = this.counter % (60/30) == 0 ? this.blink30Hz = !this.blink30Hz : this.blink30Hz;
            this.blink10Hz = this.counter % (60/10) == 0 ? this.blink10Hz = !this.blink10Hz : this.blink10Hz;
            this.blink4Hz = this.counter % (60/4) == 0 ? this.blink4Hz = !this.blink4Hz : this.blink4Hz;
            this.blink2Hz = this.counter % (60/2) == 0 ? this.blink2Hz = !this.blink2Hz : this.blink2Hz;
            this.blink1Hz = this.counter % 60 == 0 ? this.blink1Hz = !this.blink1Hz : this.blink1Hz;;
        }

        // Loads
        public void LoadFontSprites(float size, string textureName) {
            Dictionary<char, Sprite> characterSprites = new Dictionary<char, Sprite>(); // Cria grupo de sprites
            foreach (char c in BitmapFont.characters) {
                Sprite sprite = BitmapFont.GetCharacterSprite(c, size, textureName);
                if (sprite != null)
                {
                    characterSprites[c] = sprite;
                }
            }
            this.font_textures[textureName] = characterSprites;
        }

        // Draw Callers
        public void ShowFramerate(string textureName) {
            this.elapsed = this.counter % (60/2) == 0 ? (int) (1 / Program.last_frame_time) : this.elapsed;
            this.DrawText(this.elapsed.ToString() + " - " + Program.last_frame_time.ToString("F5"), 0, 82, spacing: Config.spacing_small, size: 1f, textureName: textureName);
        }

        public void DrawText(string text, float X, float Y, float spacing = 0, float size = 1f, string alignment = "center", bool absolutePosition = false, string textureName = "default medium") {
            float totalWidth = 0;
            float pos_X;
            float pos_Y;
            float offset_X = X;
            List<Sprite> text_sprites = new List<Sprite> {};

            // Calcular a largura total do texto

            foreach (char c in text)
            {
                if (font_textures[textureName].TryGetValue(c, out Sprite letter))
                {
                    var sprite = new Sprite(letter);
                    if (size > 0) sprite.Scale = new Vector2f(size, size);
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
                pos_X = Camera.Instance.X;
                pos_Y = Camera.Instance.Y;
            }
            foreach (Sprite sprite in text_sprites) {   
                sprite.Position = new Vector2f(pos_X + offset_X, pos_Y + Y);
                Program.window.Draw(sprite);
                offset_X += sprite.GetGlobalBounds().Width + spacing;
            }
        }

        public void DrawRectangle(float X, float Y, float width, float height, SFML.Graphics.Color color) {
            RectangleShape rectangle = new RectangleShape(new Vector2f(width, height))
            {
                Position = new Vector2f(Camera.GetInstance().X + X, Camera.GetInstance().Y + Y),
                FillColor = color
            };

            Program.window.Draw(rectangle);
        }
    
        // Battle UI
        public void DrawBattleUI(Stage stage) {
            // Draw hud
            hud.Position = new Vector2f(Program.camera.X - 192, Program.camera.Y - 108);
            Program.window.Draw(hud);

            // Character A
            // Draw lifebar A
            var lifeA_scale = stage.character_A.life_points.X * 150 / stage.character_A.life_points.Y;
            var lifeA = Math.Max(Math.Min(lifeA_scale, 150), 0);
            if (stage.character_B.combo_counter == 0) this.graylife_A = lifeA > this.graylife_A ? this.graylife_A = lifeA : (int) (this.graylife_A + (lifeA - this.graylife_A) * 0.01);
            this.DrawRectangle(-180 + (150 - this.graylife_A), -95, this.graylife_A, 8, this.bar_graylife);
            this.DrawRectangle(-180 + (150 - lifeA), -95, lifeA, 8, stage.character_A.life_points.X == stage.character_A.life_points.Y ? this.bar_fulllife : this.bar_life);

            // Draw Super bar A
            var superA_scale = stage.character_A.aura_points.X * 119 / stage.character_A.aura_points.Y;
            var superA = Math.Max(Math.Min(superA_scale, 119), 0);
            this.DrawRectangle(-180 + (119 - superA), 97, superA, 4, this.bar_super);
            if (stage.character_A.aura_points.X >= stage.character_A.aura_points.Y/2) {
                var control = stage.character_A.aura_points.X == stage.character_A.aura_points.Y ? this.blink10Hz : true;
                if (control) this.DrawRectangle(-180 + (119 - superA), 97, superA, 4, this.bar_super_full);
            }
            if (stage.character_A.aura_points.X == stage.character_A.aura_points.Y && this.blink2Hz) this.DrawText(this.superBarMsg, -193, 75, spacing: Config.spacing_medium, alignment: "left", textureName: "default small");
            
            // Draw Stun bar A
            var stunA_scale = ( stage.character_A.dizzy_points.Y - stage.character_A.dizzy_points.X) * 150 / stage.character_A.dizzy_points.Y;
            var stunA = Math.Max(Math.Min(stunA_scale, 150), 0);
            this.DrawRectangle(-180 + (150 - stunA), -86, stunA, 1, this.bar_graylife);

            // Character B
            // Draw lifebar B
            var lifeB_scale = stage.character_B.life_points.X * 150 / stage.character_B.life_points.Y;
            var lifeB = Math.Max(Math.Min(lifeB_scale, 150), 0);
            if (stage.character_A.combo_counter == 0) this.graylife_B = lifeB > this.graylife_B ? this.graylife_B = lifeB : (int) (this.graylife_B + (lifeB - this.graylife_B) * 0.01);
            this.DrawRectangle(30, -95, this.graylife_B, 8, this.bar_graylife);
            this.DrawRectangle(30, -95, lifeB, 8, stage.character_B.life_points.X == stage.character_B.life_points.Y ? this.bar_fulllife : this.bar_life);
            
            // Draw Super bar B
            var superB_scale = stage.character_B.aura_points.X * 119 / stage.character_B.aura_points.Y;
            var superB = Math.Max(Math.Min(superB_scale, 119), 0);
            this.DrawRectangle(61, 97, superB, 4, this.bar_super);
            if (stage.character_B.aura_points.X >= stage.character_B.aura_points.Y/2) {
                var control = stage.character_B.aura_points.X == stage.character_B.aura_points.Y ? this.blink10Hz : true;
                if (control) this.DrawRectangle(61, 97, superB, 4, this.bar_super_full);
            }
            if (stage.character_B.aura_points.X == stage.character_B.aura_points.Y && this.blink2Hz) this.DrawText(this.superBarMsg, 193, 75, spacing: Config.spacing_medium, alignment: "right", textureName: "default small");

            // Draw Stun bar B
            var stunB_scale = ( stage.character_B.dizzy_points.Y - stage.character_B.dizzy_points.X) * 150 / stage.character_B.dizzy_points.Y;
            var stunB = Math.Max(Math.Min(stunB_scale, 150), 0);
            this.DrawRectangle(30, -86, stunB, 1, this.bar_graylife);
            
            
            // HUD elements
            // Draw names
            UI.Instance.DrawText(stage.character_A.name, -194, -95, spacing: Config.spacing_small, size: 1f, alignment: "left", textureName: "default small white");
            UI.Instance.DrawText(stage.character_B.name, 194, -95, spacing: Config.spacing_small, size: 1f, alignment: "right", textureName: "default small white");

            // Draw Combo text
            if (stage.character_A.combo_counter > 1) this.DrawText(stage.character_A.combo_counter + " hits", -190, -80, spacing: -23, alignment: "left", size: 1f, textureName: "default medium white");
            if (stage.character_B.combo_counter > 1) this.DrawText(stage.character_B.combo_counter + " hits", 190, -80, spacing: -23, alignment: "right", size: 1f, textureName: "default medium white");

            // Draw time
            this.DrawText("" + Math.Max(stage.round_time, 0), 0, -106, alignment: "center", spacing: -8, size: 1f, textureName: "1");

            // Draw round indicator ≈
            this.DrawText(string.Concat(Enumerable.Repeat("*", stage.rounds_A)), -20, -93, spacing: -19, alignment: "right", textureName: "icons");
            this.DrawText(string.Concat(Enumerable.Repeat("*", stage.rounds_B)),  20, -93, spacing: -19, alignment: "left", textureName: "icons");
        }
        
        public void LoadFonts() {
            BitmapFont.Load();
            BitmapFont.Rename("font1", "1");

            UI.Instance.LoadFontSprites(32, "default medium");
            UI.Instance.LoadFontSprites(32, "default medium grad");
            UI.Instance.LoadFontSprites(32, "default medium white");
            UI.Instance.LoadFontSprites(32, "default medium red");
            UI.Instance.LoadFontSprites(32, "default medium click");
            UI.Instance.LoadFontSprites(32, "default medium hover");

            UI.Instance.LoadFontSprites(32, "default small");
            UI.Instance.LoadFontSprites(32, "default small grad");
            UI.Instance.LoadFontSprites(32, "default small white");
            UI.Instance.LoadFontSprites(32, "default small red");
            UI.Instance.LoadFontSprites(32, "default small click");
            UI.Instance.LoadFontSprites(32, "default small hover");

            UI.Instance.LoadFontSprites(32, "1");
            UI.Instance.LoadFontSprites(32, "icons");
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
