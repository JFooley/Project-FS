using Input_Space;
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
    public static int return_state;
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
    private static int pointer = 0;
    private static int controls_pointer = 0;
    public static bool loading = false;
    public static double last_frame_time = 0;
    private static Character? charA_selected;
    private static Character? charB_selected;
    private static int pointer_charA = 0;
    private static int pointer_charB = 0;
    private static bool ready_charA = false;
    private static bool ready_charB = false;
    public static bool AI_playerA = true;
    public static bool AI_playerB = true;

    // Shaders
    public static Shader colorTinterShader = new Shader(null, null, "Assets/shaders/color_tinter.frag");
    public static Shader colorFillShader = new Shader(null, null, "Assets/shaders/color_fill.frag");
    public static Shader hueChange = new Shader(null, null, "Assets/shaders/hue_change.frag");
    public static Shader paletteSwaper = new Shader(null, null, "Assets/shaders/palette_swaper.frag");

    public static void Main() {  
        Config.LoadFromFile();

        // Set initial states
        game_state = Intro;
        sub_state = Intro;
        
        // Carregamento de texturas genéricas
        try {
            Data.LoadTexturesFromFile("Assets/data/visuals.dat", Data.textures);
        } catch (Exception) {
            var tex_data = Data.LoadTexturesFromPath("Assets/visuals");
            Data.SaveTexturesToFile("Assets/data/visuals.dat", tex_data);
            Data.LoadTexturesFromFile("Assets/data/visuals.dat", Data.textures);
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

        // Main menu
        List<KeyValuePair<string, Texture>> main_menu = new List<KeyValuePair<string, Texture>>() {
            { new KeyValuePair<string, Texture>("versus player", Data.textures["screens:versus_player"]) },
            { new KeyValuePair<string, Texture>("versus bot", Data.textures["screens:versus_bot"]) },
            { new KeyValuePair<string, Texture>("training mode", Data.textures["screens:training_mode"]) },
            { new KeyValuePair<string, Texture>("controls", Data.textures["screens:controls"]) },
            { new KeyValuePair<string, Texture>("acessibility", Data.textures["screens:acessibility"]) },
            { new KeyValuePair<string, Texture>("settings", Data.textures["screens:settings"]) },
            { new KeyValuePair<string, Texture>("Exit", Data.textures["screens:exit"]) }
        };

        // Sprites
        Sprite main_bg = new Sprite(Data.textures["screens:title"]);
        Sprite settings_bg = new Sprite(Data.textures["screens:settings_bg"]);
        Sprite char_bg = new Sprite(Data.textures["screens:bgchar"]);

        Sprite frame = new Sprite(Data.textures["screens:frame"]);
        Sprite fade90 = new Sprite(Data.textures["screens:90fade"]);

        Sprite fight_logo = new Sprite(Data.textures["typography:fight"]);
        Sprite timesup_logo = new Sprite(Data.textures["typography:timesup"]);
        Sprite KO_logo = new Sprite(Data.textures["typography:ko"]);
        Sprite fslogo = new Sprite(Data.textures["typography:fs"]);

        Sprite sprite_A = new Sprite(Data.textures["other:placeholder"]);
        Sprite sprite_B = new Sprite(Data.textures["other:placeholder"]);
        Sprite stage_thumb = new Sprite(stage.thumb);

        while (window.IsOpen) {
            window.DispatchEvents();
            InputManager.Update();
            UI.Update();
            Camera.Update();
            frametimer.Restart();

            switch (game_state) {
                case Intro:
                    if (UI.frame_counter % 20 == 0) pointer = pointer < 3 ? pointer + 1 : 0;
                    fslogo.Position = new Vector2f(10, 139);

                    window.Draw(fslogo);

                    UI.DrawText(string.Concat(Enumerable.Repeat(".", pointer)), -122, 68, alignment: "left", spacing: -24);

                    if (!loading) {
                        Thread main_loader = new Thread(MainLoader);
                        main_loader.Start();
                        loading = true;
                    }
                    break;

                case StartScreen:
                    window.Draw(main_bg);
                    UI.DrawText("by JFooley", 0, 76, spacing: Config.spacing_small - 1, textureName: "default small");

                    if (UI.DrawButton(Language.GetText("press start"), 0, 50, spacing: Config.spacing_medium, click: InputManager.Key_hold("Start"), action: InputManager.Key_up("Start"), hover_font: UI.blink2Hz ? "default medium white" : "", click_font: "default medium click")) {
                        ChangeState(MainMenu);
                        pointer = 0;
                    }
                    break;

                case MainMenu:
                    window.Draw(new Sprite(main_menu.ElementAt(pointer).Value));
                    window.Draw(frame);
                    for (int i = 0; i < main_menu.Count; i++) 
                        UI.DrawText(i == pointer ? ")" : "(", (i * 10) - ((main_menu.Count - 1) * 5), -80, textureName: "icons");
                    
                    // Shoulder buttons
                    UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
                    if (UI.DrawButton(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: InputManager.Key_hold("LB"), action: InputManager.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
                        ChangeState(StartScreen);
                        pointer = 0;
                    }

                    // Change option
                    if (InputManager.Key_down("Left"))
                        pointer = pointer <= 0 ? main_menu.Count - 1 : pointer - 1;
                    else if (InputManager.Key_down("Right"))
                        pointer = pointer >= main_menu.Count - 1 ? 0 : pointer + 1;

                    // Do option
                    if (UI.DrawButton(Language.GetText(main_menu.ElementAt(pointer).Key), 0, -95, spacing: Config.spacing_medium, click: InputManager.faceButtonHold, action: InputManager.faceButtonUp, click_font: "default medium click", hover_font: "default medium white")) {
                        if (main_menu.ElementAt(pointer).Key == "versus player") {
                            AI_playerA = false;
                            AI_playerB = false;
                            ChangeState(SelectStage);

                        } else if (main_menu.ElementAt(pointer).Key == "versus bot") {
                            AI_playerA = false;
                            AI_playerB = true;
                            ChangeState(SelectStage);

                        } else if (main_menu.ElementAt(pointer).Key == "training mode") {
                            AI_playerA = false;
                            AI_playerB = false;
                            Stage.training_mode = true;
                            ChangeState(SelectStage);

                        } else if (main_menu.ElementAt(pointer).Key == "acessibility") {
                            ChangeState(AccessibilityMenu);

                        } else if (main_menu.ElementAt(pointer).Key == "controls") {
                            ChangeState(Controls);

                        } else if (main_menu.ElementAt(pointer).Key == "settings") {
                            ChangeState(Settings);

                        } else if (main_menu.ElementAt(pointer).Key == "Exit") {
                            window.Close();
                        }
                        pointer = 0;
                    }
                    break;

                case SelectStage:
                    stage_thumb.Texture = Data.stages[pointer].thumb;
                    window.Draw(stage_thumb);
                    window.Draw(frame);
                    for (int i = 0; i < Data.stages.Count; i++) 
                        UI.DrawText(i == pointer ? ")" : "(", (i * 10) - ((Data.stages.Count - 1) * 5), -80, textureName: "icons");
                    
                    // draw texts
                    UI.DrawText(Program.playerA_wins.ToString(), -Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "left");
                    UI.DrawText(Program.playerB_wins.ToString(), Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "right");

                    UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
                    if (UI.DrawButton(Language.GetText("Return"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: InputManager.Key_hold("LB"), action: InputManager.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
                        ChangeState(MainMenu);
                        pointer = 0;
                    }

                    UI.DrawText("F", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
                    if (UI.DrawButton(Language.GetText("Controls"), 182, 67, spacing: Config.spacing_small, alignment: "right", click: InputManager.Key_hold("RB"), action: InputManager.Key_up("RB"), click_font: "default small click", hover_font: "default small")) 
                        ChangeState(Controls);

                    if (InputManager.Key_down("Left"))
                        pointer = pointer <= 0 ? Data.stages.Count - 1 : pointer - 1;
                    else if (InputManager.Key_down("Right"))
                        pointer = pointer >= Data.stages.Count - 1 ? 0 : pointer + 1;

                    if (UI.DrawButton(Language.GetText(Data.stages[pointer].name), 0, -95, spacing: Config.spacing_medium, click: InputManager.faceButtonHold, action: InputManager.faceButtonUp, click_font: "default medium click", hover_font: "default medium white")) {
                        if (Data.stages[pointer].name == "Random")
                            pointer = AI.rand.Next(1, Data.stages.Count() - 2);
                        Program.stage = Data.stages[pointer];
                        ChangeState(SelectChar);
                        pointer = 0;
                    }
                    break;

                case SelectChar:
                    window.Draw(char_bg);

                    // Setup sprites texture
                    sprite_A.Texture = Data.characters[pointer_charA].thumb;
                    sprite_B.Texture = Data.characters[pointer_charB].thumb;
                    UI.DrawText(playerA_wins.ToString(), 0, 63, alignment: "right");
                    UI.DrawText(playerB_wins.ToString(), 0, 63, alignment: "left" );
                    UI.DrawText(Language.GetText(AI_playerA ? "COM" : "P1"), -15, 63, textureName: "default small", alignment: "right", spacing: Config.spacing_small);
                    UI.DrawText(Language.GetText(AI_playerB ? "COM" : "P2"),  15, 63, textureName: "default small", alignment:  "left", spacing: Config.spacing_small);

                    // Draw Shadows
                    colorFillShader.SetUniform("fillColor", Color.Black);
                    if (charA_selected != null) {
                        sprite_A.Scale = new Vector2f(1f, 1f);
                        sprite_A.Position = new Vector2f(Camera.X - 87 - sprite_A.GetLocalBounds().Width / 2, Camera.Y - 20 - sprite_A.GetLocalBounds().Height / 2);
                        window.Draw(sprite_A, new RenderStates(colorFillShader));
                    } if (charB_selected != null) {
                        sprite_B.Scale = new Vector2f(-1f, 1f);
                        sprite_B.Position = new Vector2f(Camera.X + 67 + sprite_B.GetLocalBounds().Width / 2, Camera.Y - 20 - sprite_B.GetLocalBounds().Height / 2);
                        window.Draw(sprite_B, new RenderStates(colorFillShader));
                    }

                    // Draw main sprite A
                    sprite_A.Scale = new Vector2f(1f, 1f);
                    sprite_A.Position = new Vector2f(Camera.X - 77 - sprite_B.GetLocalBounds().Width / 2, Camera.Y - 20 - sprite_B.GetLocalBounds().Height / 2);
                    window.Draw(sprite_A, Data.characters[pointer_charA].SetSwaperShader(
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
                    window.Draw(sprite_B, Data.characters[pointer_charB].SetSwaperShader( 
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
                        ChangeState(SelectStage);
                        charA_selected = null;
                        charB_selected = null;
                        ready_charA = false;
                        ready_charB = false;
                    }
                    UI.DrawText("F", 194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "right");
                    if (UI.DrawButton(Language.GetText("Controls"), 182, 67, spacing: Config.spacing_small, alignment: "right", click: InputManager.Key_hold("RB"), action: InputManager.Key_up("RB"), click_font: "default small click", hover_font: "default small")) 
                        ChangeState(Controls);

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
                        if (InputManager.Key_down("Start")) ChangeState(LoadScreen);
                    }

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
                    ready_charA = false;
                    ready_charB = false;
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
                            if (stage.CheckRoundEnd()) {
                                sub_state = RoundEnd;
                                stage.StopRoundTime();
                                stage.ResetTimer();
                            }
                            break;

                        case RoundEnd: // Fim de round
                            if (!stage.CheckTimer(3)) {
                                if (stage.character_A.life_points.X <= 0 || stage.character_B.life_points.X <= 0) {
                                    KO_logo.Position = new Vector2f(Camera.X - 75, Camera.Y - 54);
                                    window.Draw(KO_logo);
                                } else {
                                    timesup_logo.Position = new Vector2f(Camera.X - 131, Camera.Y - 55);
                                    window.Draw(timesup_logo);
                                }
                            }
                            if (stage.CheckTimer(4)) {
                                stage.LockPlayers();
                                stage.ResetTimer();
                                if (stage.CheckMatchEnd()) {
                                    sub_state = MatchEnd;
                                } else {
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

                    } 
            
                    break;

                case PostBattle:
                    stage_thumb.Texture = stage.thumb;
                    window.Draw(stage_thumb);
                    window.Draw(fade90);

                    stage.SetMusicVolume((stage.music != null) ? Math.Max(0, stage.music.Volume - 0.5f) : 0);

                    string winner_text;
                    if (winner == Program.Drawn) winner_text = Language.GetText("Drawn");
                    else winner_text = Language.GetText("Player") + " " + winner + " " + Language.GetText("Wins");

                    UI.DrawText(Program.playerA_wins.ToString(), -Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "left");
                    UI.DrawText(Program.playerB_wins.ToString(), Config.RenderWidth / 2, -Config.RenderHeight / 2, spacing: Config.spacing_medium, textureName: "default medium", alignment: "right");
                    UI.DrawText(winner_text, 0, -100, spacing: Config.spacing_medium, textureName: "default medium");

                    // Change option
                    if (InputManager.Key_down("Up") && pointer > 0)
                        pointer -= 1;
                    else if (InputManager.Key_down("Down") && pointer < 2)
                        pointer += 1;

                    // Do option
                    if (UI.DrawButton(Language.GetText("rematch"), 0, 0, spacing: Config.spacing_medium, action: InputManager.faceButtonUp, click: InputManager.faceButtonHold, hover: pointer == 0, font: "default medium", hover_font: "default medium hover", click_font: "default medium click")) {
                        Camera.SetChars(stage.character_A, stage.character_B);
                        Camera.SetLimits(stage.length, stage.height);
                        stage.LockPlayers();
                        ChangeState(Battle);

                    } if (UI.DrawButton(Language.GetText("menu"), 0, 20, spacing: Config.spacing_medium, action: InputManager.faceButtonUp, click: InputManager.faceButtonHold, hover: pointer == 1, font: "default medium", hover_font: "default medium hover", click_font: "default medium click")) { 
                        stage.StopMusic();
                        stage.UnloadStage();
                        InputManager.ResetAI();
                        ChangeState(MainMenu);

                    } if (UI.DrawButton(Language.GetText("exit game"), 0, 40, spacing: Config.spacing_medium, action: InputManager.faceButtonUp, click: InputManager.faceButtonHold, hover: pointer == 2, font: "default medium", hover_font: "default medium red", click_font: "default medium click"))
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
                    if (pointer == 0) {
                        if (InputManager.Key_down("Left") && Config.Main_Volume > 0) Config.Main_Volume -= 1;
                        else if (InputManager.Key_down("Right") && Config.Main_Volume < 100) Config.Main_Volume += 1;
                    } else if (pointer == 1) {
                        if (InputManager.Key_down("Left") && Config._music_volume > 0) Config._music_volume -= 1;
                        else if (InputManager.Key_down("Right") && Config._music_volume < 100) Config._music_volume += 1;
                    } else if (pointer == 2 && (InputManager.Key_down("Left") || InputManager.Key_down("Right"))) {
                        Config.Vsync = !Config.Vsync;
                        window.SetVerticalSyncEnabled(Config.Vsync);
                    } else if (pointer == 3 && (InputManager.Key_down("Left") || InputManager.Key_down("Right"))) {
                        Config.Fullscreen = !Config.Fullscreen;
                        window.Close();
                        if (Config.Fullscreen == true) window = new RenderWindow(VideoMode.DesktopMode, Config.GameTitle, Styles.Default, SFML.Window.State.Fullscreen);
                        else window = new RenderWindow(new VideoMode( new Vector2u(Config.RenderWidth * 3, Config.RenderHeight * 3)), Config.GameTitle, Styles.Default, SFML.Window.State.Windowed);
                        window.Closed += (sender, e) => window.Close();
                        window.SetFramerateLimit(Config.Framerate);
                        window.SetVerticalSyncEnabled(Config.Vsync);
                        window.SetMouseCursorVisible(false);
                        window.SetView(view);
                    } else if (pointer == 4) {
                        if (InputManager.Key_down("Left") && Config.round_length > 1) Config.round_length -= 1;
                        else if (InputManager.Key_down("Right") && Config.round_length < 99) Config.round_length += 1;
                    } else if (pointer == 5) {
                        if (InputManager.Key_down("Left") && Config.max_rounds > 1) Config.max_rounds -= 1;
                        else if (InputManager.Key_down("Right") && Config.max_rounds < 5) Config.max_rounds += 1;
                    } else if (pointer == 6) {
                        if (InputManager.Key_down("Left") && Config.hit_stop_time > 0) Config.hit_stop_time -= 1;
                        else if (InputManager.Key_down("Right")) Config.hit_stop_time += 1;
                    } else if (pointer == 7) {
                        if (InputManager.Key_down("Left") && Config.input_window_time > 1) Config.input_window_time -= 1;
                        else if (InputManager.Key_down("Right")) Config.input_window_time += 1;
                    } else if (pointer == 8) {
                        if (InputManager.Key_down("Left") && Config.Language > 0) Config.Language -= 1;
                        else if (InputManager.Key_down("Right") && Config.Language < Language.Supported.Length - 1) Config.Language += 1;
                    }

                    // Return option
                    UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
                    if (UI.DrawButton(Language.GetText("save and exit"), -182, 67, spacing: Config.spacing_small, alignment: "left", click: InputManager.Key_hold("LB"), action: InputManager.Key_up("LB"), click_font: "default small click", hover_font: "default small")) {
                        Config.SaveToFile();
                        Camera.LockCamera();
                        ChangeState(return_state);
                        pointer = 0;
                    }
                    break;

                case Controls:
                    if (Camera.isLocked) Camera.UnlockCamera();
                    window.Draw(new Sprite(Data.textures["screens:frame"]));
                    window.Draw(new Sprite(Data.textures["screens:controls_" + controls_pointer]));

                    if (InputManager.Key_up("Left")) controls_pointer = controls_pointer < 1 ? controls_pointer + 1 : 0;
                    else if (InputManager.Key_up("Right")) controls_pointer = controls_pointer > 0 ? controls_pointer - 1 : 1;

                    UI.DrawText("E", -194, 67, spacing: Config.spacing_small, textureName: "icons", alignment: "left");
                    if (UI.DrawButton(Language.GetText("Return"), -182, 67, alignment: "left", action: InputManager.Key_up("LB"), click: InputManager.Key_hold("LB"), hover_font: "default small")) {
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

