using Newtonsoft.Json;

public static class Config {
    // Window
    public const string GameTitle = "Project FS";
    public const int RenderWidth = 384;
    public const int RenderHeight = 216;
    public const int Framerate = 60;

    public static bool Fullscreen = true;
    public static bool Vsync = true;
    public static bool debug = false;

    public static int Language = 0;

    // Battle
    public static int round_length = 90;
    public const int default_round_length = 90;
    public static int max_rounds = 2;
    public const int default_max_rounds = 2;
    public const int input_window_time = 4;
    public static int hit_stop_time = 15;
    public const int default_hit_stop_time = 15;
    public const int default_accessible_hit_stop_time = 22;

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

    public const int parry_advantage = 2;
    public const int parry_window = 10;

    // Game constants
    public const int input_buffer_size = 240;
    public const int max_distance = 350;
    public const int reset_frames = 20;
    public const float gravity = 2450f / (Framerate*Framerate);
    public const int corner_limit = 125;
    public const int camera_height = 140;
    public const int last_hit_stop_time = 20;
    public const int contact_shadow = 100;

    // Visual
    public const int hold_time = 20;
    public const int hold_clock = 10;

    // Text
    public const int spacing_small = -25;
    public const int spacing_medium = -24;

    public static void SaveToFile() {
        try {
            string folder_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Project FS");

            if (!Directory.Exists(folder_path)) {
                Directory.CreateDirectory(folder_path);
            }

            string file_path = Path.Combine(folder_path, "config.json");

            var configData = new {
                Fullscreen,
                Vsync,
                _main_volume,
                _character_volume,
                _music_volume,
                _effect_volume,
                round_length,
                hit_stop_time,
                max_rounds,
                Language,
                Accessibility.TTS,
                Accessibility.TTS_speed,
                Accessibility.high_contrast,
                Accessibility.distance_radar,
                Accessibility.atack_feedback
            };

            string jsonString = JsonConvert.SerializeObject(configData, Formatting.Indented);
            File.WriteAllText(file_path, jsonString);

        } catch (Exception ex) {
            Console.WriteLine($"Error saving config file: {ex.Message}");
        }
    }
    public static void LoadFromFile() {
            string folder_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Project FS");

            if (!Directory.Exists(folder_path)) {
                Directory.CreateDirectory(folder_path);
            }

            string file_path = Path.Combine(folder_path, "config.json");

        if (File.Exists(file_path)) {
            string jsonString = File.ReadAllText(file_path);
                var configData = JsonConvert.DeserializeObject<ConfigData>(jsonString);

                Fullscreen = configData.Fullscreen;
                Vsync = configData.Vsync;
                _main_volume = configData._main_volume;
                _character_volume = configData._character_volume;
                _music_volume = configData._music_volume;
                _effect_volume = configData._effect_volume;
                round_length = configData.round_length;
                hit_stop_time = configData.hit_stop_time;
                max_rounds = configData.max_rounds;
                Language = configData.Language;
                Accessibility.TTS = configData.TTS;
                Accessibility.TTS_speed = configData.TTS_speed;
                Accessibility.high_contrast = configData.high_contrast;
                Accessibility.distance_radar = configData.distance_radar;
                Accessibility.atack_feedback = configData.atack_feedback;
                
        } else {
            Config.SaveToFile();
        }
    }
    private class ConfigData {
        // Geral
        public bool Fullscreen { get; set; }
        public bool Vsync { get; set; }
        public float _main_volume { get; set; }
        public float _character_volume { get; set; }
        public float _music_volume { get; set; }
        public float _effect_volume { get; set; }
        public int round_length { get; set; }
        public int hit_stop_time { get; set; }
        public int max_rounds { get; set; }
        public int Language { get; set; }
        // Acessibilidade
        public bool TTS { get; set; }
        public float TTS_speed { get; set; }
        public bool high_contrast { get; set; }
        public bool distance_radar { get; set; }
        public bool atack_feedback { get; set; }
    }
}