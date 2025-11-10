namespace Language_space {
    public static class Language {
        // Language indices
        public static readonly string[] supported = { "English", "Portuguese" };
        public static int LANGUAGE_COUNT = supported.Length;
        public const int LANGUAGE_ENGLISH = 0;
        public const int LANGUAGE_PORTUGUESE = 1;

        // texts
        private static Dictionary<string, List<string>> texts = new Dictionary<string, List<string>>() { // English, Portuguese
            {"error", new List<string> {"Error", "Erro"}},
            {"press start", new List<string> {"Press start", "Pressione start"}},
            {"return", new List<string> {"Return", "Voltar"}},
            {"controls", new List<string> {"Controls", "Controles"}},
            {"settings", new List<string> {"Settings", "Configuracoes"}},
            {"random", new List<string> {"Random", "Aleatorio"}},
            {"player", new List<string> {"Player", "Jogador"}},
            {"wins", new List<string> {"Wins", "Vence"}},
            {"drawn", new List<string> {"Drawn", "Empate"}},
            {"rematch", new List<string> {"Rematch", "Revanche" }},
            {"change stage", new List<string> {"Change stage", "Mudar estagio"}},
            {"ready", new List<string> {"Ready", "Pronto"}},
            {"exit game", new List<string> {"Exit game", "Sair do jogo"}},
            {"pause", new List<string> {"Pause", "Pausa"}},
            {"training", new List<string> {"Training", "Treinamento"}},
            {"reset characters", new List<string> {"Reset Characters", "Resetar Personagens"}},
            {"show hitboxes", new List<string> {"Show hitboxes", "Mostrar hitboxes"}},
            {"block", new List<string> {"Block", "Bloquear"}},
            {"never", new List<string> {"Never", "Nunca"}},
            {"after hit", new List<string> {"After hit", "Apos acerto"}},
            {"always", new List<string> {"Always", "Sempre"}},
            {"super", new List<string> {"Super", "Super"}},
            {"refil", new List<string> {"Refil", "Recarregar"}},
            {"keep", new List<string> {"Keep", "Manter"}},
            {"end match", new List<string> {"End match", "Finalizar partida"}},
            {"life", new List<string> {"Life", "Vida"}},
            {"frames", new List<string> {"frames", "frames"}},
            {"default", new List<string> {"Default", "Padrao"}},
            {"hitstop", new List<string> {"Hitstop", "Hitstop"}},
            {"input window", new List<string> {"Input window", "Janela de input"}},
            {"window mode", new List<string> {"Window mode", "Modo de janela"}},
            {"fullscreen", new List<string> {"Fullscreen", "Tela cheia"}},
            {"borderless", new List<string> {"Borderless", "Sem bordas"}},
            {"windowed", new List<string> {"Windowed", "Janela"}},
            {"language", new List<string> {"Language", "Idioma"}},
            {"english", new List<string> {"English", "Ingles"}},
            {"portuguese", new List<string> {"Portuguese", "Portugues"}},
            {"main volume", new List<string> {"Main volume", "Volume principal"}},
            {"music volume", new List<string> {"Music volume", "Volume da musica"}},
            {"sfx volume", new List<string> {"SFX volume", "Volume dos efeitos"}},
            {"match", new List<string> {"Match", "Partida"}},
            {"round length", new List<string> {"Round length", "Duração da rodada"}},
            {"on", new List<string> {"On", "Ligado"}},
            {"off", new List<string> {"Off", "Desligado"}},
            {"save and exit", new List<string> {"Save and exit", "Salvar e sair"}},
            {"parry", new List<string> {"Parry", "Parry"}},
        };

        // Method to get text by id
        public static string GetText(string text) {
            try {
                return texts[text.ToLower()][Config.Language];
            } catch (KeyNotFoundException) {
                return text;
            }
        }
    }
}