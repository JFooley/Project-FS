using Newtonsoft.Json;

public static class Config {
    // Window
    public const string GameTitle = "Project FS";
    public const int RenderWidth = 384;
    public const int RenderHeight = 216;
    public const int Framerate = 60;

    public static bool Fullscreen = true;
    public static bool Vsync = false;
    public static bool debug = false;

    public static int Language = 0;

    // Battle
    public static int round_length = 90;
    public const int default_round_length = 90;
    public static int hit_stop_time = 15;
    public const int default_hit_stop_time = 15;
    public static int max_rounds = 2;
    public const int default_max_rounds = 2;
    public static int input_window_time = 4;
    public const int default_input_window_time = 4;
    public static int parry_advantage = 2;
    public static int parry_window = 10;

    // Audio
    public static float _main_volume = 100f;
    public static float _character_volume = 80f;
    public static float _music_volume = 80f;
    public static float _effect_volume = 100f;

    public static float Main_Volume
    {
        get { return _main_volume; }
        set { _main_volume = value; }
    }
    public static float Character_Volume
    {
        get { return _character_volume * (_main_volume / 100); }
        set { _character_volume = value; }
    }
    public static float Music_Volume
    {
        get { return _music_volume  * (_main_volume / 100); }
        set { _music_volume = value; }
    }
    public static float Effect_Volume
    {
        get { return _effect_volume * (_main_volume / 100); }
        set { _effect_volume = value; }
    }

    // Battle constants
    public const float light_pushback = 3f;
    public const float medium_pushback = 4f;
    public const float heavy_pushback = 5.5f;

    // Game constants
    public const int max_distance = 350;
    public const int reset_frames = 20;
    public const float gravity = 2450f / (Framerate*Framerate);
    public const int corner_limit = 125;
    public const int camera_height = 140;
    public const int last_hit_stop_time = 20;

    // Text
    public const int spacing_small = -26; 
    public const int spacing_medium = -23;

    public static void SaveToFile(string filePath = "Assets/config.json") {
        var configData = new
        {
            Fullscreen,
            Vsync,
            debug,
            input_window_time,
            parry_advantage,
            parry_window,
            _main_volume,
            _character_volume,
            _music_volume,
            _effect_volume,
            round_length,
            hit_stop_time,
            max_rounds,
            Language
        };

        string jsonString = JsonConvert.SerializeObject(configData, Formatting.Indented);
        File.WriteAllText(filePath, jsonString);
    }
    public static void LoadFromFile(string filePath = "Assets/config.json") {
        if (File.Exists(filePath)) {
            string jsonString = File.ReadAllText(filePath);
            var configData = JsonConvert.DeserializeObject<ConfigData>(jsonString);

            Fullscreen = configData.Fullscreen;
            Vsync = configData.Vsync;
            debug = configData.Debug;
            input_window_time = configData.input_window_time;
            parry_advantage = configData.parry_advantage;
            parry_window = configData.parry_window;
            _main_volume = configData._main_volume;
            _character_volume = configData._character_volume;
            _music_volume = configData._music_volume;
            _effect_volume = configData._effect_volume;
            round_length = configData.round_length;
            hit_stop_time = configData.hit_stop_time;
            max_rounds = configData.max_rounds;
            Language = configData.Language;
        } else {
            Console.WriteLine("Config file not found. Using default settings.");
            Config.SaveToFile(filePath);
        }
    }
    private class ConfigData {
        public bool Fullscreen { get; set; }
        public bool Vsync { get; set; }
        public bool Debug { get; set; }
        public int input_window_time { get; set; }
        public int parry_advantage { get; set; }
        public int parry_window { get; set; }
        public float _main_volume { get; set; }
        public float _character_volume { get; set; }
        public float _music_volume { get; set; }
        public float _effect_volume { get; set; }
        public int round_length { get; set; }
        public int hit_stop_time { get; set; }
        public int max_rounds { get; set; }
        public int Language { get; set; }
    }
}