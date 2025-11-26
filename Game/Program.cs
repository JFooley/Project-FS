using Input_Space;
using SFML.Graphics;
using SFML.Window;
using SFML.System;
using Stage_Space;
using UI_space;
using System.Diagnostics;
using Character_Space;
using Data_space;
using Language_space;
using System.Windows.Forms;

public static class Program {
    // Winner index
    public const int Drawn = 0;
    public const int Player1 = 1;
    public const int Player2 = 2;

    // Game States
    public const int Intro = 0;
    public const int MainMenu = 1;
    public const int SelectStage = 2;
    public const int SelectChar = 3;
    public const int LoadScreen = 4;
    public const int Battle = 5;
    public const int PostBattle = 6;
    public const int Settings = 7;
    public const int Controls = 8;
    public const int AccessibilityMenu = 9;
    public const int Credits = 10;

    // Battle States
    public const int RoundStart = 1;
    public const int Battling = 2;
    public const int RoundEnd = 3;
    public const int MatchEnd = 4;

    // State holders
    public static int game_state;
    public static int return_state;
    public static int sub_state;

    // Common objects
    public static Stage stage;
    public static RenderWindow window;
    private static Stopwatch frametimer;

    // Session infos
    public static int player1_wins = 0;
    public static int player2_wins = 0;
    public static int winner = 0;

    // View
    public static SFML.Graphics.View view = new SFML.Graphics.View(new FloatRect(0, 0, Config.RenderWidth, Config.RenderHeight));

    // Aux
    private static int pointer = 0;
    private static int controls_pointer = 0;
    private static string charA_selected = null;
    private static string charB_selected = null;
    private static int pointer_charA = 0;
    private static int pointer_charB = 0;
    public static bool loading = false;
    public static double last_frame_time = 0;

    // Shaders
    public static Shader colorTinterShader = new Shader(null, null, "Assets/shaders/color_tinter.frag");
    public static Shader colorFillShader = new Shader(null, null, "Assets/shaders/color_fill.frag");
    public static Shader hueChange = new Shader(null, null, "Assets/shaders/hue_change.frag");

    // Data
    private static List<Stage> stages;
    private static List<Character> characters;
    public static Dictionary<string, Texture> visuals = new Dictionary<string, Texture>();
    public static Dictionary<string, Texture> thumbs = new Dictionary<string, Texture>();
    private static Fireball fb = new Fireball();
    private static Hitspark hs = new Hitspark();
    private static Particle pt = new Particle();

    public static void Main() {  
        Config.LoadFromFile();

        // Set initial states
        game_state = Intro;
        sub_state = Intro;
        
        // Carregamento de texturas gerais
        try {
            DataManagement.LoadTexturesFromFile("Assets/data/visuals.dat", visuals);
        } catch (Exception e) {
            DataManagement.LoadTexturesFromPath("Assets/ui", visuals);
            DataManagement.SaveTexturesToFile("Assets/data/visuals.dat", visuals);
        }
        Stage.LoadThumbs();
        Character.LoadThumbs();
        new UI();

        // Inicializações
        Language.Initialize();
        new InputManager(autoDetectDevice: true);
        new Camera();
        frametimer = new Stopwatch();

        // Crie uma janela
        if (Config.Fullscreen == true) window = new RenderWindow(VideoMode.DesktopMode, Config.GameTitle, Styles.Fullscreen);
        else window = new RenderWindow(new VideoMode(Config.RenderWidth * 3, Config.RenderHeight * 3), Config.GameTitle, Styles.Default);
        window.Closed += (sender, e) => window.Close();
        window.SetFramerateLimit(Config.Framerate);
        window.SetVerticalSyncEnabled(Config.Vsync);
        window.SetMouseCursorVisible(false);
        
        // Cria uma view
        window.SetView(view);

        // Carregamento dos personagens
        characters = new List<Character> {
            new Ken(),
        };

        // Carregamento dos stages
        stages = new List<Stage> {
            new Stage("Random", visuals["random"]),
            new BurningDojo(),
            new MidnightDuel(),
            new NightAlley(),
            new NYCSubway(),
            new RindoKanDojo(),
            new TheSavana(),
            new JapanFields(),
            new TrainingStage(),
            new Stage("Settings", visuals["settings"]),
            new Stage("Exit game", visuals["exit"]),
        };
        stage = stages[0];

        // Sprites
        Sprite main_bg = new Sprite(visuals["title"]);
        Sprite settings_bg = new Sprite(visuals["settings_bg"]);
        Sprite char_bg = new Sprite(visuals["bgchar"]);
        Sprite stage_bg = new Sprite();

        Sprite frame = new Sprite(visuals["frame"]);
        Sprite fade90 = new Sprite(visuals["90fade"]);

        Sprite fight_logo = new Sprite(visuals["fight"]);
        Sprite timesup_logo = new Sprite(visuals["timesup"]);
        Sprite KO_logo = new Sprite(visuals["ko"]);
        Sprite fslogo = new Sprite(visuals["fs"]);

        Sprite sprite_A = new Sprite(characters[pointer_charA].thumb);
        Sprite sprite_B = new Sprite(characters[pointer_charB].thumb);

        while (window.IsOpen) {
            window.DispatchEvents();
            InputManager.Update();
            UI.Update();
            Camera.Update();
            frametimer.Restart();

            switch (game_state) {
                case Intro:
                    if (UI.counter % 20 == 0) pointer = pointer < 3 ? pointer + 1 : 0;
                    fslogo.Position = new Vector2f(10, 139);

                    window.Draw(fslogo);

                    UI.DrawText(string.Concat(Enumerable.Repeat(".", pointer)), -122, 68, alignment: "left", spacing: -24);

                    if (!loading)
                    {
                        Thread main_loader = new Thread(MainLoader);
                        main_loader.Start();
                        loading = true;
                    }
                    break;

                case MainMenu:
                    window.Draw(main_bg);
                    UI.DrawText("by JFooley", 0, 76, spacing: Config.spacing_small - 1, textureName: "default small");
                    if (UI.blink2Hz || InputManager.Key_hold("Start")) UI.DrawText(Language.GetText("press start"), 0, 50, spacing: Config.spacing_medium, textureName: InputManager.Key_hold("Start") ? "default medium click" : "default medium white");

                    if (InputManager.Key_up("Start"))
                    {
                        ChangeState(SelectStage);
                        pointer = 0;
                    }
                    break;

                case SelectStage:
                    window.Draw(stages[pointer].thumb);
                    window.Draw(frame);

                    // draw texts
                    UI.DrawText(Language.GetText(stages[pointer].name), 0, -80, spacing: Config.spacing_medium, textureName: InputManager.Key_hold("A") || InputManager.Key_hold("B") || InputManager.Key_hold("C") || InputManager.Key_hold("D") ? "default medium click" : "default medium white");
                    UI.DrawText(Program.player1_wins.ToString(), -Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "left");
                    UI.DrawText(Program.player2_wins.ToString(), Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "right");

                    UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
                    UI.DrawText(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", textureName: InputManager.Key_hold("LB") ? "default small click" : "default small");

                    UI.DrawText("F", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
                    UI.DrawText(Language.GetText("Controls"), 182, 67, spacing: Config.spacing_small, alignment: "right", textureName: InputManager.Key_hold("RB") ? "default small click" : "default small");

                    if (InputManager.Key_down("Left"))
                        pointer = pointer <= 0 ? stages.Count - 1 : pointer - 1;
                    else if (InputManager.Key_down("Right"))
                        pointer = pointer >= stages.Count - 1 ? 0 : pointer + 1;

                    if (InputManager.Key_up("A") || InputManager.Key_up("B") || InputManager.Key_up("C") || InputManager.Key_up("D")) {
                        if (stages[pointer].name == "Settings") // TODO: traduzir
                            ChangeState(Settings);
                        else if (stages[pointer].name == "Exit game") // TODO: traduzir
                            window.Close();
                        else {
                            if (stages[pointer].name == "Random") // TODO: traduzir
                                pointer = AI.rand.Next(1, stages.Count() - 2);
                            Program.stage = stages[pointer];
                            ChangeState(SelectChar);
                        }
                        pointer = 0;
                    }
                    else if (InputManager.Key_up("LB")) ChangeState(MainMenu);
                    else if (InputManager.Key_up("RB")) ChangeState(Controls);
                    break;

                case SelectChar:
                    window.Draw(char_bg);

                    // Setup sprites texture
                    sprite_A.Texture = characters[pointer_charA].thumb;
                    sprite_B.Texture = characters[pointer_charB].thumb;

                    // Draw Shadows
                    colorFillShader.SetUniform("fillColor", new SFML.Graphics.Glsl.Vec3(0, 0, 0));

                    sprite_A.Scale = new Vector2f(1f, 1f);
                    sprite_A.Position = new Vector2f(Camera.X - 81 - sprite_A.GetLocalBounds().Width / 2, Camera.Y - 20 - sprite_A.GetLocalBounds().Height / 2);
                    window.Draw(sprite_A, new RenderStates(colorFillShader));

                    sprite_B.Scale = new Vector2f(-1f, 1f);
                    sprite_B.Position = new Vector2f(Camera.X + 73 + sprite_B.GetLocalBounds().Width / 2, Camera.Y - 20 - sprite_B.GetLocalBounds().Height / 2);
                    window.Draw(sprite_B, new RenderStates(colorFillShader));

                    // Draw main sprite
                    colorTinterShader.SetUniform("tintColor", new SFML.Graphics.Glsl.Vec3(0, 0, 0));
                    colorTinterShader.SetUniform("intensity", 0.75f);

                    sprite_A.Position = new Vector2f(Camera.X - 77 - sprite_B.GetLocalBounds().Width / 2, Camera.Y - 20 - sprite_B.GetLocalBounds().Height / 2);
                    if (charA_selected != null) window.Draw(sprite_A, new RenderStates(colorTinterShader));
                    else window.Draw(sprite_A);

                    sprite_B.Position = new Vector2f(Camera.X + 77 + sprite_B.GetLocalBounds().Width / 2, Camera.Y - 20 - sprite_B.GetLocalBounds().Height / 2);
                    sprite_B.Scale = new Vector2f(-1f, 1f);
                    if (charB_selected != null) window.Draw(sprite_B, new RenderStates(colorTinterShader));
                    else window.Draw(sprite_B);

                    // Draw texts
                    if (charA_selected == null) UI.DrawText("<  >", -77, -16);
                    if (charB_selected == null) UI.DrawText("<  >", +77, -16);

                    UI.DrawText(player1_wins.ToString(), 0, 63, alignment: "right");
                    UI.DrawText(player2_wins.ToString(), 0, 63, alignment: "left");

                    UI.DrawText(characters[pointer_charA].name, -77, 45, spacing: Config.spacing_small, textureName: "default small");
                    UI.DrawText(characters[pointer_charB].name, +77, 45, spacing: Config.spacing_small, textureName: "default small");

                    UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
                    UI.DrawText(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", textureName: InputManager.Key_hold("LB") ? "default small click" : "default small");

                    UI.DrawText("F", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
                    UI.DrawText(Language.GetText("Controls"), 182, 67, spacing: Config.spacing_small, alignment: "right", textureName: InputManager.Key_hold("RB") ? "default small click" : "default small");

                    // Chose option A
                    if (InputManager.Key_down("Left", player: 1) && charA_selected == null)
                        pointer_charA = pointer_charA <= 0 ? characters.Count - 1 : pointer_charA - 1;
                    else if (InputManager.Key_down("Right", player: 1) && charA_selected == null)
                        pointer_charA = pointer_charA >= characters.Count - 1 ? 0 : pointer_charA + 1;

                    if (InputManager.Key_down("A", player: 1) || InputManager.Key_down("B", player: 1) || InputManager.Key_down("C", player: 1) || InputManager.Key_down("D", player: 1))
                        charA_selected = characters[pointer_charA].name;

                    // Chose option B
                    if (InputManager.Key_down("Left", player: 2) && charB_selected == null)
                        pointer_charB = pointer_charB <= 0 ? characters.Count - 1 : pointer_charB - 1;
                    else if (InputManager.Key_down("Right", player: 2) && charB_selected == null)
                        pointer_charB = pointer_charB >= characters.Count - 1 ? 0 : pointer_charB + 1;

                    if (InputManager.Key_down("A", player: 2) || InputManager.Key_down("B", player: 2) || InputManager.Key_down("C", player: 2) || InputManager.Key_down("D", player: 2))
                        charB_selected = characters[pointer_charB].name;

                    // Return option
                    if (InputManager.Key_up("LB")) {
                        charB_selected = null;
                        charA_selected = null;
                        ChangeState(SelectStage);
                    }
                    else if (InputManager.Key_up("RB")) ChangeState(Controls);

                    // Ends when chars are selected
                    if (charA_selected != null && charB_selected != null && (InputManager.Key_up("A") || InputManager.Key_up("B") || InputManager.Key_up("C") || InputManager.Key_up("D")))
                        ChangeState(LoadScreen);

                    break;

                case LoadScreen:
                    stage.LoadStage();
                    stage.LoadCharacters(charA_selected, charB_selected);
                    Camera.SetChars(stage.character_A, stage.character_B);
                    Camera.SetLimits(stage.length, stage.height);

                    loading = false;
                    pointer = 0;
                    charA_selected = null;
                    charB_selected = null;
                    pointer_charA = 0;
                    pointer_charB = 0;

                    ChangeState(Battle);
                    break;

                case Battle:
                    stage.Update();

                    switch (sub_state) {
                        case Intro:
                            stage.SetMusicVolume();
                            stage.StopRoundTime();
                            stage.ResetTimer();
                            if (stage.character_A.current_state == "Idle" && stage.character_B.current_state == "Idle") 
                                sub_state = RoundStart;
                            break;

                        case RoundStart: // Inicia a round
                            if (stage.CheckTimer(3))
                            {
                                stage.ResetRoundTime();
                                stage.StartRoundTime();
                                stage.ReleasePlayers();
                                sub_state = Battling;
                            }
                            else if (stage.CheckTimer(2))
                            {
                                fight_logo.Position = new Vector2f(Camera.X - 89, Camera.Y - 54);
                                window.Draw(fight_logo);
                            }
                            else if (stage.CheckTimer(1)) UI.DrawText(Language.GetText("Ready")+"?", 0, -30, spacing: Config.spacing_medium, textureName: "default medium white");
                            else UI.DrawText(Language.GetText("Round") + " " + stage.round, 0, -30, spacing: Config.spacing_medium, textureName: "default medium white");

                            break;

                        case Battling: // Durante a batalha
                            if (stage.CheckRoundEnd())
                            {
                                sub_state = RoundEnd;
                                stage.StopRoundTime();
                                stage.ResetTimer();
                            }
                            break;

                        case RoundEnd: // Fim de round
                            if (stage.GetTimerValue() < 3)
                            {
                                if (stage.character_A.life_points.X <= 0 || stage.character_B.life_points.X <= 0)
                                {
                                    KO_logo.Position = new Vector2f(Camera.X - 75, Camera.Y - 54);
                                    window.Draw(KO_logo);
                                }
                                else
                                {
                                    timesup_logo.Position = new Vector2f(Camera.X - 131, Camera.Y - 55);
                                    window.Draw(timesup_logo);
                                }
                            }
                            if (stage.CheckTimer(4))
                            {
                                stage.LockPlayers();
                                stage.ResetTimer();
                                if (stage.CheckMatchEnd())
                                {
                                    sub_state = MatchEnd;
                                }
                                else
                                {
                                    sub_state = RoundStart;
                                    stage.ResetPlayers();
                                }
                            }
                            break;

                        case MatchEnd: // Fim da partida
                            Camera.Reset();
                            stage.ResetMatch();
                            stage.ResetPlayers(force: true, total_reset: true);

                            sub_state = Intro;
                            ChangeState(PostBattle);
                            break;

                    } break;

                case PostBattle:
                    window.Draw(stage.thumb);
                    window.Draw(fade90);

                    stage.music.Volume = Math.Max(0, stage.music.Volume - 0.5f);

                    string winner_text;
                    if (winner == Program.Drawn) winner_text = Language.GetText("Drawn");
                    else winner_text = Language.GetText("Player") + " " + winner + " " + Language.GetText("Wins");

                    UI.DrawText(Program.player1_wins.ToString(), -Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "left");
                    UI.DrawText(Program.player2_wins.ToString(), Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "right");
                    UI.DrawText(winner_text, 0, -100, spacing: Config.spacing_medium, textureName: "default medium");
                    
                    UI.DrawButton(Language.GetText("Rematch"), 0, 0, spacing: Config.spacing_medium, hover: pointer == 0, font: "default medium", hover_font: "default medium white", click_font: "default medium click", click: InputManager.Key_hold("A") || InputManager.Key_hold("B") || InputManager.Key_hold("C") || InputManager.Key_hold("D"));
                    UI.DrawButton(Language.GetText("Change stage"), 0, 20, spacing: Config.spacing_medium, hover: pointer == 1, font: "default medium", hover_font: "default medium white", click_font: "default medium click", click: InputManager.Key_hold("A") || InputManager.Key_hold("B") || InputManager.Key_hold("C") || InputManager.Key_hold("D"));
                    UI.DrawButton(Language.GetText("Exit game"), 0, 40, spacing: Config.spacing_medium, hover: pointer == 2, font: "default medium", hover_font: "default medium red", click_font: "default medium click", click: InputManager.Key_hold("A") || InputManager.Key_hold("B") || InputManager.Key_hold("C") || InputManager.Key_hold("D"));

                    // Change option
                    if (InputManager.Key_down("Up") && pointer > 0)
                        pointer -= 1;
                    else if (InputManager.Key_down("Down") && pointer < 2)
                        pointer += 1;

                    // Do option
                    if (pointer == 0 && (InputManager.Key_up("A") || InputManager.Key_up("B") || InputManager.Key_up("C") || InputManager.Key_up("D")))
                    { // rematch
                        Camera.SetChars(stage.character_A, stage.character_B);
                        Camera.SetLimits(stage.length, stage.height);
                        stage.LockPlayers();
                        ChangeState(Battle);

                    }
                    else if (pointer == 1 && (InputManager.Key_up("A") || InputManager.Key_up("B") || InputManager.Key_up("C") || InputManager.Key_up("D")))
                    { // MENU 
                        stage.StopMusic();
                        stage.UnloadStage();
                        ChangeState(SelectStage);
                    }
                    else if (pointer == 2 && (InputManager.Key_up("A") || InputManager.Key_up("B") || InputManager.Key_up("C") || InputManager.Key_up("D")))
                        window.Close();

                    break;

                case Settings:
                    Camera.UnlockCamera();
                    window.Draw(settings_bg);

                    UI.DrawText(Language.GetText("Settings"), -80, -107, spacing: Config.spacing_medium);
                    //0
                    UI.DrawText(Language.GetText("Main volume"), -170, -74, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 0 ? "default small hover" : "default small");
                    UI.DrawText("< " + Config.Main_Volume.ToString() + "% >", 0, -74, spacing: Config.spacing_small, textureName: pointer == 0 ? "default small red" : "default small");
                    //1
                    UI.DrawText(Language.GetText("Music volume"), -170, -64, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 1 ? "default small hover" : "default small");
                    UI.DrawText("< " + Config._music_volume.ToString() + "% >", 0, -64, spacing: Config.spacing_small, textureName: pointer == 1 ? "default small red" : "default small");
                    //2
                    UI.DrawText(Language.GetText("V-sync"), -170, -54, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 2 ? "default small hover" : "default small");
                    UI.DrawText("< " + (Config.Vsync ? Language.GetText("on") : Language.GetText("off")) + " >", 0, -54, spacing: Config.spacing_small, textureName: pointer == 2 ? "default small red" : "default small");
                    //3
                    UI.DrawText(Language.GetText("Window mode"), -170, -44, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 3 ? "default small hover" : "default small");
                    UI.DrawText("< " + (Config.Fullscreen ? Language.GetText("Fullscreen") : Language.GetText("Windowed")) + " >", 0, -44, spacing: Config.spacing_small, textureName: pointer == 3 ? "default small red" : "default small");
                    //4
                    UI.DrawText(Language.GetText("Round Length"), -170, -34, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 4 ? "default small hover" : "default small");
                    UI.DrawText("< " + (Config.round_length == Config.default_round_length ? Language.GetText("Default") + " (" + Config.default_round_length + "s)" : Config.round_length + "s") + " >", 0, -34, spacing: Config.spacing_small, textureName: pointer == 4 ? "default small red" : "default small");
                    //5
                    UI.DrawText(Language.GetText("Match"), -170, -24, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 5 ? "default small hover" : "default small");
                    UI.DrawText("< " + (Config.max_rounds == Config.default_max_rounds ? Language.GetText("Default") + " (FT" + Config.default_max_rounds + ")" : Language.GetText("First to") + " " + Config.max_rounds.ToString()) + " >", 0, -24, spacing: Config.spacing_small, textureName: pointer == 5 ? "default small red" : "default small");
                    //6
                    UI.DrawText(Language.GetText("Hitstop"), -170, -14, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 6 ? "default small hover" : "default small");
                    UI.DrawText("< " + (Config.hit_stop_time == Config.default_hit_stop_time ? Language.GetText("Default") : Config.hit_stop_time + " " + Language.GetText("frames")) + " >", 0, -14, spacing: Config.spacing_small, textureName: pointer == 6 ? "default small red" : "default small");
                    //7
                    UI.DrawText(Language.GetText("Input window"), -170, -04, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 7 ? "default small hover" : "default small");
                    UI.DrawText("< " + (Config.input_window_time == Config.default_input_window_time ? Language.GetText("Default") : Config.input_window_time + " " + Language.GetText("frames")) + " >", 0, -04, spacing: Config.spacing_small, textureName: pointer == 7 ? "default small red" : "default small");
                    //8
                    UI.DrawText(Language.GetText("Language"), -170, 6, alignment: "left", spacing: Config.spacing_small, textureName: pointer == 8 ? "default small hover" : "default small");
                    UI.DrawText("< " + Language.GetText(Language.Supported[Config.Language]) + " >", 0, 6, spacing: Config.spacing_small, textureName: pointer == 8 ? "default small red" : "default small");


                    // Change option 
                    if (InputManager.Key_down("Up"))
                        pointer = pointer <= 0 ? 8 : pointer - 1;
                    else if (InputManager.Key_down("Down"))
                        pointer = pointer >= 8 ? 0 : pointer + 1;

                    // Do option
                    if (pointer == 0)
                    {
                        if (InputManager.Key_down("Left") && Config.Main_Volume > 0) Config.Main_Volume -= 1;
                        else if (InputManager.Key_down("Right") && Config.Main_Volume < 100) Config.Main_Volume += 1;
                    }
                    else if (pointer == 1)
                    {
                        if (InputManager.Key_down("Left") && Config._music_volume > 0) Config._music_volume -= 1;
                        else if (InputManager.Key_down("Right") && Config._music_volume < 100) Config._music_volume += 1;
                    }
                    else if (pointer == 2 && (InputManager.Key_down("Left") || InputManager.Key_down("Right")))
                    {
                        Config.Vsync = !Config.Vsync;
                        window.SetVerticalSyncEnabled(Config.Vsync);
                    }
                    else if (pointer == 3 && (InputManager.Key_down("Left") || InputManager.Key_down("Right")))
                    {
                        Config.Fullscreen = !Config.Fullscreen;
                        window.Close();
                        if (Config.Fullscreen == true) window = new RenderWindow(VideoMode.DesktopMode, Config.GameTitle, Styles.Fullscreen);
                        else window = new RenderWindow(new VideoMode(Config.RenderWidth * 3, Config.RenderHeight * 3), Config.GameTitle, Styles.Default);
                        window.Closed += (sender, e) => window.Close();
                        window.SetFramerateLimit(Config.Framerate);
                        window.SetVerticalSyncEnabled(Config.Vsync);
                        window.SetMouseCursorVisible(false);
                        window.SetView(view);
                    }
                    else if (pointer == 4)
                    {
                        if (InputManager.Key_down("Left") && Config.round_length > 1) Config.round_length -= 1;
                        else if (InputManager.Key_down("Right") && Config.round_length < 99) Config.round_length += 1;
                    }
                    else if (pointer == 5)
                    {
                        if (InputManager.Key_down("Left") && Config.max_rounds > 1) Config.max_rounds -= 1;
                        else if (InputManager.Key_down("Right") && Config.max_rounds < 5) Config.max_rounds += 1;
                    }
                    else if (pointer == 6)
                    {
                        if (InputManager.Key_down("Left") && Config.hit_stop_time > 0) Config.hit_stop_time -= 1;
                        else if (InputManager.Key_down("Right")) Config.hit_stop_time += 1;
                    }
                    else if (pointer == 7)
                    {
                        if (InputManager.Key_down("Left") && Config.input_window_time > 1) Config.input_window_time -= 1;
                        else if (InputManager.Key_down("Right")) Config.input_window_time += 1;
                    }
                    else if (pointer == 8)
                    {
                        if (InputManager.Key_down("Left") && Config.Language > 0) Config.Language -= 1;
                        else if (InputManager.Key_down("Right") && Config.Language < Language.Supported.Length - 1) Config.Language += 1;
                    }

                    // Return option
                    UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
                    UI.DrawText(Language.GetText("save and exit"), -182, 67, spacing: Config.spacing_small, alignment: "left", textureName: InputManager.Key_hold("LB") ? "default small click" : "default small");

                    if (InputManager.Key_up("LB")) {
                        Config.SaveToFile();
                        Camera.LockCamera();
                        ChangeState(return_state);
                        pointer = 0;
                    }
                    break;

                case Controls:
                    if (Camera.isLocked) Camera.UnlockCamera();
                    window.Draw(new Sprite(visuals["frame"]));
                    window.Draw(new Sprite(visuals["controls_" + controls_pointer]));

                    if (InputManager.Key_up("Left")) controls_pointer = controls_pointer < 1 ? controls_pointer + 1 : 0;
                    else if (InputManager.Key_up("Right")) controls_pointer = controls_pointer > 0 ? controls_pointer - 1 : 1;

                    UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
                    UI.DrawButton(Language.GetText("Return"), -182, 67, alignment: "left", hover: true, click: InputManager.Key_hold("LB"), hover_font: "default small");
                    if (InputManager.Key_up("LB")) {
                        if (return_state == Battle) Camera.LockCamera();
                        ChangeState(return_state);
                    }
                    break;

                case AccessibilityMenu:
                    break;

                case Credits:
                    break;
            }

            // Finally
            last_frame_time = frametimer.Elapsed.TotalMilliseconds/1000;
            window.Display();
            window.Clear();
        }
    }

    public static void ChangeState(int new_state) {
        return_state = game_state;
        game_state = new_state;
    }

    public static void MainLoader()
    {
        foreach (var character in characters)
        {
            if (character.GetType() == typeof(Character)) continue;
            character.LoadTextures();
            character.LoadSounds();
        }

        foreach (var stage in stages)
        {
            if (stage.GetType() == typeof(Stage)) continue;
            stage.LoadTextures();
            stage.LoadSounds();
        }

        fb.LoadTextures();
        fb.LoadSounds();

        hs.LoadTextures();
        hs.LoadSounds();

        pt.LoadTextures();
        pt.LoadSounds();

        loading = false;
        ChangeState(MainMenu);
    }
}

