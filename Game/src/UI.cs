
using SFML.Audio;
using SFML.Graphics;
using SFML.System;

namespace UI_space {
    public class UI {
        private static int elapsed = 0;
        public static uint frame_counter = 0;

        // Clocks
        public static bool blink30Hz = true;
        public static bool blink10Hz = true;
        public static bool blink4Hz = true;
        public static bool blink2Hz = true;
        public static bool blink1Hz = true;

        private static int graylife_A = 150;
        private static int graylife_B = 150;

        public static SFML.Graphics.Color bar_graylife => Accessibility.high_contrast ? Accessibility.bar_graylife : new SFML.Graphics.Color(195, 248, 233);
        public static SFML.Graphics.Color bar_fulllife = new SFML.Graphics.Color(44, 191, 54);
        public static SFML.Graphics.Color bar_life = new SFML.Graphics.Color(60, 166, 136);
        public static SFML.Graphics.Color bar_super =  new SFML.Graphics.Color(5, 110, 150);
        public static SFML.Graphics.Color bar_super_full = new SFML.Graphics.Color(0, 185, 255);
        public static SFML.Graphics.Color bar_stun = new SFML.Graphics.Color(242, 65, 12);

        // visuals
        private static Sprite? hud;
        private static string superBarMsg = "max aura";
        private static int last_frame_round_time = 0;

        // Sounds
        private static Sound[]? button_sounds;
        private static Sound? time_tick_sound;
        private static Sound[]? aura_sounds;

        // Control
        private static int last_aura_A = 0;
        private static int last_aura_B = 0;

        // Positions
        private static int life_bar_Y = -93;
        private static int life_bar_X =  -180;
        
        private static int aura_bar_Y = 96;
        private static int aura_bar_X = -179;

        private static int stun_bar_Y = -84;
        private static int stun_bar_X = -178;

        public UI() {
            UI.hud = new Sprite(Data.textures["ui:hud"]);
            UI.button_sounds = new Sound[] {
                new Sound(Data.sounds["ui:change"]) {Volume = Config.Effect_Volume},
                new Sound(Data.sounds["ui:forward"]) {Volume = Config.Effect_Volume},
                new Sound(Data.sounds["ui:back"]) {Volume = Config.Effect_Volume}
            };
            UI.aura_sounds = new Sound[] {
                new Sound(Data.sounds["ui:aura_full"]) {Volume = Config.Effect_Volume},
                new Sound(Data.sounds["ui:aura_half"]) {Volume = Config.Effect_Volume}
            };
            UI.time_tick_sound = new Sound(Data.sounds["ui:time_tick"]) {Volume = Config.Effect_Volume};
            BitmapFont.Load();
        }
        public static void Update() {
            UI.frame_counter++;
            UI.blink30Hz = UI.frame_counter % (60/30) == 0 ? UI.blink30Hz = !UI.blink30Hz : UI.blink30Hz;
            UI.blink10Hz = UI.frame_counter % (60/10) == 0 ? UI.blink10Hz = !UI.blink10Hz : UI.blink10Hz;
            UI.blink4Hz = UI.frame_counter % (60/4) == 0 ? UI.blink4Hz = !UI.blink4Hz : UI.blink4Hz;
            UI.blink2Hz = UI.frame_counter % (60/2) == 0 ? UI.blink2Hz = !UI.blink2Hz : UI.blink2Hz;
            UI.blink1Hz = UI.frame_counter % 60 == 0 ? UI.blink1Hz = !UI.blink1Hz : UI.blink1Hz;;
        }
        
        // Aux
        public static bool ForEach(float frames) {
            return frames > 0 ? UI.frame_counter % frames == 0 : true;
        }
        
        // Draws
        public static void ShowFramerate(string textureName) {
            UI.elapsed = UI.frame_counter % 60 == 0 ? (int) (1 / Program.last_frame_time) : UI.elapsed;
            UI.DrawText(new[] {UI.elapsed.ToString() + " - " + Program.last_frame_time.ToString("F5")}, 0, 82, spacing: Config.spacing_small, textureName: textureName);
        }
        public static void ShowDebugInfo(Character c) {
            float px = c.player_index == 1 ? -Config.RenderWidth/2 : Config.RenderWidth/2;
            float py = -Config.RenderHeight/4;
            string ali = c.player_index == 1 ? "left" : "right";
            int n = 10;

            DrawText(new[] { "Has hit: " + c.has_hit }, px, py, spacing: Config.spacing_small, alignment: ali, textureName: "default small white");
            DrawText(new[] { "Facing: " + c.facing }, px, py + n, spacing: Config.spacing_small, alignment: ali, textureName: "default small white");
            DrawText(new[] { c.current_logic_frame_index + "/" + (c.current_animation.lenght - 1)}, px, py + 2*n, spacing: Config.spacing_small, alignment: ali, textureName: c.current_animation.on_last_frame ? "default small red" : "default small white");
            DrawText(new[] { c.current_anim_frame_index.ToString() }, px, py + 3*n, spacing: Config.spacing_small, alignment: ali, textureName: "default small white");
            DrawText(new[] { c.current_state }, px, py + 4*n, spacing: Config.spacing_small, alignment: ali, textureName: "default small white");
            DrawText(new[] { c.state.idle ? "waiting" : "busy" }, px, py + 5*n, spacing: Config.spacing_small, alignment: ali, textureName: "default small white");
            DrawText(new[] { "Vel: " + c.body.velocity.X.ToString("F1") + "/" + c.body.velocity.Y.ToString("F1") }, px, py + 6*n, spacing: Config.spacing_small, alignment: ali, textureName: "default small white");
            DrawText(new[] { "Acc: " + c.body.acceleration.X.ToString("F1") + "/" + c.body.acceleration.Y.ToString("F1") }, px, py + 7*n, spacing: Config.spacing_small, alignment: ali, textureName: "default small white");

            RectangleShape anchor = new RectangleShape(new Vector2f(2, 2)) {
                Position = new Vector2f(c.body.position.X, c.body.position.Y - 60),
                FillColor = Color.Black,
            };
            Vector2f v = new Vector2f(c.body.velocity.X, -c.body.velocity.Y) * 5f;
            var vel = new RectangleShape(new Vector2f((float)Math.Sqrt(v.X * v.X + v.Y * v.Y), 1f)) {
                Position = new Vector2f(c.body.position.X, c.body.position.Y - 125),
                FillColor = Color.Yellow,
                Rotation = (float)(Math.Atan2(v.Y, v.X) * 180f / Math.PI),
                Origin = new Vector2f(0, 0)
            };
            Vector2f a = new Vector2f(c.body.acceleration.X, -c.body.acceleration.Y) * 5f;
            var acc = new RectangleShape(new Vector2f((float)Math.Sqrt(a.X * a.X + a.Y * a.Y), 1f)) {
                Position = new Vector2f(c.body.position.X, c.body.position.Y - 125),
                FillColor = Color.Blue,
                Rotation = (float)(Math.Atan2(a.Y, a.X) * 180f / Math.PI),
                Origin = new Vector2f(0, 0)
            };

            Program.window.Draw(anchor);
            Program.window.Draw(vel);
            Program.window.Draw(acc);
        }
        public static void DrawText(string[] raw_text, float X, float Y, float spacing = 0, string alignment = "center", bool absolutePosition = false, string textureName = "default medium", string TTS_id = "", bool TTS = false, bool priority = false) {           
            if (!BitmapFont.textures.TryGetValue(textureName, out var texture)) return;
            if (raw_text.Length == 0) return;

            string text = Language.Translate(raw_text);
            if (TTS) Accessibility.Speak(TTS_id + text, TTSRequisition.TEXT, priority, raw_text);

            float totalWidth = text.Length > 0 ? text.Length * (BitmapFont.CellSize + spacing) - spacing : 0;
            float offset_X = X;
            
            if (alignment == "center") offset_X -= totalWidth / 2f;
            else if (alignment == "right") offset_X -= totalWidth;

            VertexArray vertices = new VertexArray(PrimitiveType.Triangles);

            float currentX = absolutePosition ? X + offset_X : Camera.X + offset_X;
            float currentY = absolutePosition ? Y : Camera.Y + Y;

            foreach (char c in text) {
                IntRect rect = BitmapFont.GetCharacter(c);

                float w = rect.Width;
                float h = rect.Height;

                float tx = rect.Left;
                float ty = rect.Top;

                // 6 vértices (2 triângulos para cada caractere)
                vertices.Append(new Vertex(new Vector2f(currentX, currentY), new Vector2f(tx, ty))); // v0
                vertices.Append(new Vertex(new Vector2f(currentX + w, currentY), new Vector2f(tx + w, ty))); // v1
                vertices.Append(new Vertex(new Vector2f(currentX, currentY + h), new Vector2f(tx, ty + h))); // v2

                vertices.Append(new Vertex(new Vector2f(currentX, currentY + h), new Vector2f(tx, ty + h))); // v2
                vertices.Append(new Vertex(new Vector2f(currentX + w, currentY), new Vector2f(tx + w, ty))); // v1
                vertices.Append(new Vertex(new Vector2f(currentX + w, currentY + h), new Vector2f(tx + w, ty + h))); // v3

                currentX += w + spacing;
            }

            RenderStates states = new RenderStates(texture);
            Program.window.Draw(vertices, states);
        }
        public static void DrawRectangle(float X, float Y, float width, float height, SFML.Graphics.Color? outline_color = null, SFML.Graphics.Color? fill_color = null, string alignment = "center", bool absolutePosition = false) {
            RectangleShape rectangle;
            float pos_X = absolutePosition ? X : Camera.X + X;
            float pos_Y = absolutePosition ? Y : Camera.Y + Y;

            if (alignment == "left") {
                rectangle = new RectangleShape(new Vector2f(width, height)) {
                    Position = new Vector2f(pos_X, pos_Y),
                    OutlineThickness = 1,
                    OutlineColor = outline_color.HasValue ? outline_color.Value : SFML.Graphics.Color.Transparent,
                    FillColor = fill_color.HasValue ? fill_color.Value : SFML.Graphics.Color.Transparent};
            } else if (alignment == "right") {
                rectangle = new RectangleShape(new Vector2f(width, height)) {
                    Position = new Vector2f(pos_X - width, pos_Y),
                    OutlineThickness = 1,
                    OutlineColor = outline_color.HasValue ? outline_color.Value : SFML.Graphics.Color.Transparent,
                    FillColor = fill_color.HasValue ? fill_color.Value : SFML.Graphics.Color.Transparent};            
            } else {
                rectangle = new RectangleShape(new Vector2f(width, height)) {
                    Position = new Vector2f(pos_X - ((int) width/2), pos_Y),
                    OutlineThickness = 1,
                    OutlineColor = outline_color.HasValue ? outline_color.Value : SFML.Graphics.Color.Transparent,
                    FillColor = fill_color.HasValue ? fill_color.Value : SFML.Graphics.Color.Transparent};  
            }

            Program.window.Draw(rectangle);
        }
        public static void DrawBar(float X, float Y, float currentValue, float maxValue, string textureName, string alignment = "center", bool mirrored = false, SFML.Graphics.Color? color = null, bool grow_inverted = true, bool absolutePosition = false) {
            if (!Data.textures.ContainsKey(textureName)) return;
            
            var barSprite = new Sprite(Data.textures[textureName]);
            IntRect originalRect = barSprite.TextureRect;

            // Pinta a sprite se uma cor for fornecida
            RenderStates renderStates = RenderStates.Default;
            if (color.HasValue)  {
                Program.colorTinterShader.SetUniform("color", new SFML.Graphics.Glsl.Vec3(color.Value.R, color.Value.G, color.Value.B));
                renderStates = new RenderStates(Program.colorTinterShader);
            }
            
            // Calcula a porcentagem da barra
            float percentage = Math.Max(Math.Min(currentValue / maxValue, 1f), 0f);
            
            // Define a região da textura que será mostrada
            IntRect textureRect = originalRect;
            textureRect.Size.X = (int)(originalRect.Width * percentage);
            
            // Calcula quanto foi diminuído
            int reducedWidth = originalRect.Width - textureRect.Width;
            if (grow_inverted) {
                textureRect = new IntRect( new Vector2i(originalRect.Left + reducedWidth, textureRect.Top), textureRect.Size);
            }
            
            float scaleX = mirrored ? -1f : 1f;
            barSprite.Scale = new Vector2f(scaleX, 1f);
            
            // Ajusta a posição baseado no alinhamento
            float posX = absolutePosition ? X : Camera.X + X;
            float posY = absolutePosition ? Y : Camera.Y + Y;
            
            // Move a posição original do rect + X para a direita
            if (mirrored) {
                if (grow_inverted) posX -= reducedWidth;

                if (alignment == "center")
                    posX += (int) originalRect.Width / 2;
                else if (alignment == "left")
                    posX += originalRect.Width;

            } else {
                if (grow_inverted) posX += reducedWidth;

                if (alignment == "center") 
                    posX -= (int) originalRect.Width / 2;
                else if (alignment == "right")
                    posX -= originalRect.Width;
            }
            
            barSprite.TextureRect = textureRect;
            barSprite.Position = new Vector2f(posX, posY);
            
            Program.window.Draw(barSprite, renderStates);
        }
        public static bool DrawButton(string[] text, float pos_X, float pos_Y, bool action = false, bool hover = true, bool click = false, float spacing = Config.spacing_small, string alignment = "center", bool absolutePosition = false, int button_sound = 1, string font = "default small white", string hover_font = "default small hover", string click_font = "default small click", bool tts = true, string id = "", bool priority = false) {
            if (click && hover) {
                UI.DrawText(text, pos_X, pos_Y, TTS: tts, TTS_id: id, priority: priority, spacing: spacing, alignment: alignment, absolutePosition: absolutePosition, textureName: click_font);
            } else if (hover) {
                UI.DrawText(text, pos_X, pos_Y, TTS: tts, TTS_id: id, priority: priority, spacing: spacing, alignment: alignment, absolutePosition: absolutePosition, textureName: hover_font);
            } else {
                UI.DrawText(text, pos_X, pos_Y, spacing: spacing, alignment: alignment, absolutePosition: absolutePosition, textureName: font);
            }

            if (action && hover) {
                if (Accessibility.navigation_cue) button_sounds?[button_sound].Play();
                return true;
            } else {
                return false;
            }
        }

        // Battle UI
        public static void DrawBattleUI(Stage stage) {
            // Draw hud
            if (hud != null) hud.Position = new Vector2f(Camera.X - 192, Camera.Y - 108);
            Program.window.Draw(hud);

            // Lifebar A
            var lifeA_scale = stage.character_A.life_points.X * 150 / stage.character_A.life_points.Y;
            var lifeA = Math.Max(Math.Min(lifeA_scale, 150), 0);
            if (stage.character_B.combo_counter == 0) UI.graylife_A = lifeA > UI.graylife_A ? UI.graylife_A = lifeA : (int) (UI.graylife_A + (lifeA - UI.graylife_A) * 0.01);            
            UI.DrawBar(life_bar_X, life_bar_Y, UI.graylife_A, 150, "ui:lifebar", alignment: "left", mirrored: false, color: UI.bar_graylife);
            var lifeColorA = stage.character_A.life_points.X == stage.character_A.life_points.Y ? UI.bar_fulllife : UI.bar_life;
            UI.DrawBar(life_bar_X, life_bar_Y, lifeA, 150, "ui:lifebar", alignment: "left", mirrored: false, color: lifeColorA);

            // Lifebar B
            var lifeB_scale = stage.character_B.life_points.X * 150 / stage.character_B.life_points.Y;
            var lifeB = Math.Max(Math.Min(lifeB_scale, 150), 0);
            if (stage.character_A.combo_counter == 0) UI.graylife_B = lifeB > UI.graylife_B ? UI.graylife_B = lifeB : (int) (UI.graylife_B + (lifeB - UI.graylife_B) * 0.01);            
            UI.DrawBar(-life_bar_X, life_bar_Y, UI.graylife_B, 150, "ui:lifebar", alignment: "right", mirrored: true, color: UI.bar_graylife);
            var lifeColorB = stage.character_B.life_points.X == stage.character_B.life_points.Y ? UI.bar_fulllife : UI.bar_life;
            UI.DrawBar(-life_bar_X, life_bar_Y, lifeB, 150, "ui:lifebar", alignment: "right", mirrored: true, color: lifeColorB);
            
            // Super bar A
            var superA_scale = stage.character_A.aura_points.X * 117 / stage.character_A.aura_points.Y;
            var superA = Math.Max(Math.Min(superA_scale, 117), 0);
            var full_A = stage.character_A.aura_points.X == stage.character_A.aura_points.Y;
            var aura_color_A = stage.character_A.aura_points.X >= stage.character_A.aura_points.Y/2 && !(full_A && UI.blink10Hz) ? UI.bar_super_full : UI.bar_super;
            UI.DrawBar(aura_bar_X, aura_bar_Y, superA, 117, "ui:aurabar", alignment: "left", mirrored: false, color: aura_color_A, grow_inverted: false);
            
            if (stage.character_A.aura_points.X == stage.character_A.aura_points.Y && UI.blink2Hz) UI.DrawText(new[] {UI.superBarMsg}, -193, 73, spacing: Config.spacing_small, alignment: "left", textureName: "default small white");
            
            if (last_aura_A != stage.character_A.aura_points.X && stage.character_A.aura_points.X >= stage.character_A.aura_points.Y/2) {
                if (stage.character_A.aura_points.X == stage.character_A.aura_points.Y) UI.aura_sounds?[0].Play();
                else if (last_aura_A < stage.character_A.aura_points.Y/2) UI.aura_sounds?[1].Play();
            }
            last_aura_A = stage.character_A.aura_points.X;

            // Super bar B
            var superB_scale = stage.character_B.aura_points.X * 117 / stage.character_B.aura_points.Y;
            var superB = Math.Max(Math.Min(superB_scale, 117), 0);
            var full_B = stage.character_B.aura_points.X == stage.character_B.aura_points.Y;
            var aura_color_B = stage.character_B.aura_points.X >= stage.character_B.aura_points.Y/2 && !(full_B && UI.blink10Hz) ? UI.bar_super_full : UI.bar_super;
            UI.DrawBar(-aura_bar_X, aura_bar_Y, superB, 117, "ui:aurabar", alignment: "right", mirrored: true, color: aura_color_B, grow_inverted: false);
            
            if (stage.character_B.aura_points.X == stage.character_B.aura_points.Y && UI.blink2Hz) UI.DrawText(new[] {UI.superBarMsg}, 193, 73, spacing: Config.spacing_small, alignment: "right", textureName: "default small white");
            
            if (last_aura_B != stage.character_B.aura_points.X && stage.character_B.aura_points.X >= stage.character_B.aura_points.Y/2) {
                if (stage.character_B.aura_points.X == stage.character_B.aura_points.Y) UI.aura_sounds?[0].Play();
                else if (last_aura_B < stage.character_B.aura_points.Y/2) UI.aura_sounds?[1].Play();
            }
            last_aura_B = stage.character_B.aura_points.X;
            
            // Names
            UI.DrawText(new[] {stage.character_A.name}, -181, -93, spacing: Config.spacing_small, alignment: "left", textureName: "default small white");
            UI.DrawText(new[] {stage.character_B.name}, 181, -93, spacing: Config.spacing_small, alignment: "right", textureName: "default small white");

            UI.DrawBar(-177, -81, 1, 1, "ui:croma", color: stage.character_A.current_palette_color, alignment: "left", mirrored: false);
            UI.DrawBar(177, -81, 1, 1, "ui:croma", color: stage.character_B.current_palette_color, alignment: "right", mirrored: true);

            UI.DrawText(new[] {(stage.character_A.AIEnabled && stage.character_A.BotEnabled) ? "computer lv. " + (5 - stage.character_A.BOT.difficulty) : "player 1"}, -Config.RenderWidth/2, -Config.RenderHeight/2 - 10, spacing: Config.spacing_small, alignment: "left", textureName: "default small white");
            UI.DrawText(new[] {(stage.character_B.AIEnabled && stage.character_B.BotEnabled) ? "computer lv. " + (5 - stage.character_B.BOT.difficulty) : "player 2"}, Config.RenderWidth/2, -Config.RenderHeight/2 - 10, spacing: Config.spacing_small, alignment: "right", textureName: "default small white");
 
            // Combo text
            if (stage.character_A.combo_counter > 1) {
                UI.DrawText(new[] {"combo"}, -190, -80, spacing: Config.spacing_small, alignment: "left", textureName: "default small white");
                UI.DrawText(new[] {stage.character_A.combo_counter.ToString()}, -135, -70, spacing: Config.spacing_medium, alignment: "right", textureName: "default medium white");
            }
            if (stage.character_B.combo_counter > 1) {
                UI.DrawText(new[] {"combo"}, 190, -80, spacing: Config.spacing_small, alignment: "right", textureName: "default small white");
                UI.DrawText(new[] {stage.character_B.combo_counter.ToString()}, 135, -70, spacing: Config.spacing_medium, alignment: "left", textureName: "default medium white");
            }

            // Time
            UI.DrawText(new[] {Math.Max(stage.round_time, 0).ToString()}, 0, -106, alignment: "center", spacing: -8, textureName: "1");
            if ((stage.round_time == 30 || stage.round_time <= 10) && UI.last_frame_round_time != stage.round_time) UI.time_tick_sound?.Play();
            UI.last_frame_round_time = stage.round_time;

            // Round indicators
            UI.DrawText(new[] {string.Concat(Enumerable.Repeat("-", Math.Max(Config.max_rounds - stage.rounds_A, 0))) + string.Concat(Enumerable.Repeat("*", stage.rounds_A))}, -20, -91, spacing: -19, alignment: "right", textureName: "icons");
            UI.DrawText(new[] {string.Concat(Enumerable.Repeat("*", stage.rounds_B)) + string.Concat(Enumerable.Repeat("-", Math.Max(Config.max_rounds - stage.rounds_B, 0)))},  20, -91, spacing: -19, alignment: "left", textureName: "icons");
        }
    }

    public static class BitmapFont {
        public static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
        public static Dictionary<char, int> charIndex = new Dictionary<char, int>();
        private static char[] characters = {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
            'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
            'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
            'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z', '!', '@', '#', '$', '%', '^', '&', '*',
            '(', ')', '_', '+', '-', '=', '[', ']', '{', '}',
            ';', ':', '\'', '"', '\\', '|', ',', '.', '<', '>',
            '/', '?', '¿', '\u0020', '«', '»', 'ç', '°', 'Ç', '~',
            'ã', 'Ã', 'á', 'Á', 'â', 'Â', 'à', 'À', 'é', 'É',
            'è', 'È', 'ê', 'Ê', 'ó', 'Ó', 'ô', 'Ô', 'õ', 'Õ',
            'ú', 'Ú', 'ù', 'Ù', 'û', 'Û', 'í', 'Í', 'ì', 'Ì'
        };

        public const int CellSize = 32; // Tamanho de cada célula
        public const int Columns = 10;  // Número de colunas
        public const int Rows = 13;     // Número de linhas

        public static void Load() {
            for (int i = 0; i < characters.Length; i++) charIndex[characters[i]] = i;

            string full_path = Data.GetPath("assets/fonts");
            if (!System.IO.Directory.Exists(full_path))
                throw new System.IO.DirectoryNotFoundException($"O diretório {full_path} não foi encontrado.");

            Data.LoadTexturesDat(Data.GetPath("assets/fonts/textures.dat"), BitmapFont.textures);
        }
        public static IntRect GetCharacter(char character) {
            if (!charIndex.TryGetValue(character, out int index)) index = -1;
            if (index == -1 || index >= Columns * Rows) {
                index = charIndex[' '];
            }

            int x = index % Columns * CellSize;
            int y = index / Columns * CellSize;

            return new IntRect(new Vector2i(x, y), new Vector2i(CellSize, CellSize));
        }
    }

}