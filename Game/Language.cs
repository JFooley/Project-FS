using Newtonsoft.Json;

namespace Language_space {
    public static class Language {
        public static string[] Supported { get; private set; } = Array.Empty<string>();
        public static string[] LanguageCodes { get; private set; } = Array.Empty<string>();
        
        private static List<Dictionary<string, string>> _languageData = new();

        public static void Initialize() {
            LoadAllLanguages();
        }

        private static void LoadAllLanguages() {
            var languagesDir = "Assets/languages";
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
        }

        public static string GetText(string text) {
            try {
                return _languageData[Config.Language][text.ToLower()];
            } catch (KeyNotFoundException) {
                return text;
            }
        }
    }
}