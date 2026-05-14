using Newtonsoft.Json;
using SFML.Audio;
using System.Text.RegularExpressions;

public class Language {
    public static string[] Supported { get; private set; } = Array.Empty<string>();
    public static string[] LanguageCodes { get; private set; } = Array.Empty<string>();
    
    private static List<Dictionary<string, string>> _languageData;
    public static Dictionary<string, SoundBuffer> tts_audios;

    public Language() {
        _languageData = new List<Dictionary<string, string>>();
        tts_audios = new Dictionary<string, SoundBuffer>();
        LoadAllLanguages();
    }
    
    public static string Translate(params string[] strings) {
        string text = "";

        foreach (string s in strings) {
            try {
                text += _languageData[Config.Language][s.ToLower()];
            } catch (KeyNotFoundException) {
                text += s;
            }
        }

        return text;
    }
    public static Sound? Vocalize(string text) {
        try {
            text = NormalizeTTSKey(text);
            return new Sound(tts_audios[_languageData[Config.Language]["_code"] + ":" + text]) {Volume = Config.Effect_Volume, Pitch = Accessibility.TTS_speed};
        } catch (Exception) {
            return null;
        }
    }
    private static string NormalizeTTSKey(string text) {
        text = text.ToLower().Trim();
        text = Regex.Replace(
            text,
            @"[^\w\s-]",
            ""
        );
        text = Regex.Replace(
            text,
            @"[\s]+",
            "_"
        );
        return text;
    }
    
    // Loaders
    private static void LoadAllLanguages() {
        var languagesDir = Data.GetPath("assets/languages");

        if (!Directory.Exists(languagesDir)) {
            Console.WriteLine($"Diretório de idiomas não encontrado: {languagesDir}");
            return;
        }

        var languageFiles = Directory.GetFiles(languagesDir, "*.json");
        var supportedList = new List<string>();
        var codeList = new List<string>();
        var languageDataList = new List<Dictionary<string, string>>();

        foreach (var file in languageFiles) {
            try {
                var jsonContent = File.ReadAllText(file);
                var languageData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

                if (languageData == null || !languageData.ContainsKey("_language") || !languageData.ContainsKey("_code")) {
                    Console.WriteLine($"Arquivo de idioma inválido: {file}");
                    continue;
                }

                supportedList.Add(languageData["_language"]);
                codeList.Add(languageData["_code"]);
                languageDataList.Add(languageData);

            } catch (Exception ex) {
                Console.WriteLine($"Erro ao carregar idioma {file}: {ex.Message}");
            }
        }

        Supported = supportedList.ToArray();
        LanguageCodes = codeList.ToArray();
        _languageData = languageDataList;     

        try {
            Data.LoadSoundsDat(Data.GetPath("assets/languages/audio.dat"), Language.tts_audios);
        } catch (Exception e) {
            Console.WriteLine("Erro ao carregar dados de text-to-speak: " + e.Message);
        }
                
    }

}
