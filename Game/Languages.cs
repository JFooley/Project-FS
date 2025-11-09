
public static class Languages {
    public static int current_language = 0;

    // Language indices
    public const int LANGUAGE_ENGLISH = 0;
    public const int LANGUAGE_PORTUGUESE = 1;

    // texts
    private static List<Dictionary<int, string>> texts = new List<Dictionary<int, string>>()
    {
        // English
        new Dictionary<int, string>()
        {
            { 0, "Press Start" },
            { 1, "Voltar" },
            { 2, "Controls" }
        },

        // Portuguese
        new Dictionary<int, string>()
        {
            { 0, "Pressione Start" },
            { 1, "Voltar" },
            { 2, "Controles" }
        }
    };
}