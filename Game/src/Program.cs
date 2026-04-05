using SFML.Graphics;
using SFML.Window;
using SFML.System;
using UI_space;
using System.Diagnostics;
using Language_space;

public static class Program {
    // Winner index
    public const int Drawn = 0;
    public const int PlayerA = 1;
    public const int PlayerB = 2;

    // Game States
    public const int Intro = 0;
    public const int StartScreen = 1;
    public const int MainMenu = 2;
    public const int SelectStage = 3;
    public const int SelectChar = 4;
    public const int LoadScreen = 5;
    public const int Battle = 6;
    public const int PostBattle = 7;
    public const int Settings = 8;
    public const int Controls = 9;
    public const int AccessibilityMenu = 10;
    public const int Credits = 11;

    // Battle States
    public const int RoundStart = 1;
    public const int Battling = 2;
    public const int RoundEnd = 3;
    public const int MatchEnd = 4;

    // State holders
    public static int game_state;
    public static int last_game_state;
    public static int sub_state;

    // Common objects
    public static Stage? stage;
    public static RenderWindow window;
    private static Stopwatch frametimer = new Stopwatch();

    // Session infos
    public static int playerA_wins = 0;
    public static int playerB_wins = 0;
    public static int winner = 0;

    // View
    public static View view = new View(new FloatRect(new Vector2f(0, 0), new Vector2f(Config.RenderWidth, Config.RenderHeight)));

    // Aux
    public static bool loading = false;
    public static double last_frame_time = 0;

    public static bool AI_playerA = true;
    public static bool AI_playerB = true;

    // Shaders
    public static Shader colorTinterShader = new Shader(null, null, Data.GetPath("Assets/shaders/color_tinter.frag"));
    public static Shader colorFillShader = new Shader(null, null, Data.GetPath("Assets/shaders/color_fill.frag"));
    public static Shader hueChange = new Shader(null, null, Data.GetPath("Assets/shaders/hue_change.frag"));
    public static Shader paletteSwaper = new Shader(null, null, Data.GetPath("Assets/shaders/palette_swaper.frag"));

    public static void Main() {  
        Config.LoadFromFile();

        // Set initial states
        game_state = Intro;
        sub_state = Intro;
        
        // Carregamento de texturas genéricas
        try {
            Data.LoadTexturesFromFile(Data.GetPath("Assets/data/visuals.dat"), Data.textures);
        } catch (Exception) {
            var tex_data = Data.LoadTexturesFromPath(Data.GetPath("Assets/visuals"));
            Data.SaveTexturesToFile(Data.GetPath("Assets/data/visuals.dat"), tex_data);
            Data.LoadTexturesFromFile(Data.GetPath("Assets/data/visuals.dat"), Data.textures);
        }

        new UI();

        // Inicializações
        Language.Initialize();
        new InputManager(autoDetectDevice: true);
        new Camera();

        // Crie uma janela
        if (Config.Fullscreen == true) window = new RenderWindow(VideoMode.DesktopMode, Config.GameTitle, Styles.Default, SFML.Window.State.Fullscreen);
        else window = new RenderWindow(new VideoMode( new Vector2u(Config.RenderWidth * 3, Config.RenderHeight * 3)), Config.GameTitle, Styles.Default, SFML.Window.State.Windowed);
        window.Closed += (sender, e) => window.Close();
        window.SetFramerateLimit(Config.Framerate);
        window.SetVerticalSyncEnabled(Config.Vsync);
        window.SetMouseCursorVisible(false);
        
        // Cria uma view
        window.SetView(view);

        // Carregamento dos personagens
        Data.characters = new List<Character> {
            new Ken(),
        };

        // Carregamento dos Data.stages
        Data.stages = new List<Stage> {
            new Stage("Random", Data.textures["screens:random"]),
            new BurningDojo(),
            new MidnightDuel(),
            new NightAlley(),
            new NYCSubway(),
            new RindoKanDojo(),
            new TheSavana(),
            new JapanFields(),
            new TrainingStage(),
        };
        stage = Data.stages[0];

        // Widgets
        WGStart start_screen = new WGStart();
        WGIntro intro_screen = new WGIntro();
        WGMainMenu main_menu_screen = new WGMainMenu();
        WGSelectStage select_stage_screen = new WGSelectStage();
        WGSelectCharacter select_char_screen = new WGSelectCharacter();
        WGBattle battle_screen = new WGBattle();
        WGPostBattle post_battle_screen = new WGPostBattle();
        WGSettings settings_screen = new WGSettings();
        WGControls controls_screen = new WGControls();
        WGAcessibilityMenu accessibility_screen = new WGAcessibilityMenu();
        WGCredits credits_screen = new WGCredits();

        while (window.IsOpen) {
            window.DispatchEvents();
            InputManager.Update();
            UI.Update();
            Camera.Update();
            frametimer.Restart();

            switch (game_state) {
                case Intro:
                    intro_screen.Render();
                    break;

                case StartScreen:
                    start_screen.Render();
                    break;

                case MainMenu:
                    main_menu_screen.Render();
                    break;

                case SelectStage:
                    select_stage_screen.Render();
                    break;

                case SelectChar:
                    select_char_screen.Render();
                    break;

                case LoadScreen:
                    stage.LoadStage();
                    stage.LoadCharacters(select_char_screen.charA_selected, select_char_screen.charB_selected);
                    Camera.SetChars(stage.character_A, stage.character_B);
                    Camera.SetLimits(stage.length, stage.height);

                    loading = false;
                    select_char_screen.charA_selected = null;
                    select_char_screen.charB_selected = null;
                    select_char_screen.ready_charA = false;
                    select_char_screen.ready_charB = false;
                    select_char_screen.pointer_charA = 0;
                    select_char_screen.pointer_charB = 0;

                    ChangeState(Battle);
                    break;

                case Battle:
                    battle_screen.Render();
                    break;

                case PostBattle:
                    post_battle_screen.Render();
                    break;

                case Settings:
                    settings_screen.Render();
                    break;

                case Controls:
                    controls_screen.Render();
                    break;

                case AccessibilityMenu:
                    accessibility_screen.Render();
                    break;

                case Credits:
                    credits_screen.Render();
                    break;
            }

            // Finally
            last_frame_time = frametimer.Elapsed.TotalMilliseconds/1000;
            window.Display();
            window.Clear();            
        }
    }

    public static void ChangeState(int new_state) {
        last_game_state = game_state;
        game_state = new_state;
    }

    public static void MainLoader() {
        foreach (var character in Data.characters) {
            if (character.GetType() == typeof(Character)) continue;
            character.LoadPalette();
            character.LoadTextures();
            character.LoadSounds();
            character.SetThumb();
        }

        foreach (var stage in Data.stages) {
            if (stage.GetType() == typeof(Stage)) continue;
            stage.LoadTextures();
            stage.LoadSounds();
            stage.SetThumb();
        }

        Hitspark hs = new Hitspark();
        Particle pt = new Particle();

        hs.LoadTextures();
        hs.LoadSounds();

        pt.LoadTextures();
        pt.LoadSounds();

        loading = false;
        ChangeState(StartScreen);
    }
}

