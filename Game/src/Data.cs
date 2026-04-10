using SFML.Audio;
using SFML.Graphics;

public static class Data {
    public static List<Stage> stages = new List<Stage>();
    public static List<Character> characters = new List<Character>();
    public static Dictionary<string, SoundBuffer> sounds = new Dictionary<string, SoundBuffer>();
    public static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

    public static Dictionary<string, Texture> LoadTexturesFromFile(string fileName, Dictionary<string, Texture> existingTextures = null) {
        var result = existingTextures ?? new Dictionary<string, Texture>();

        using var fs = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(fs);

        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++) {
            string name = reader.ReadString();
            int length = reader.ReadInt32();
            byte[] data = reader.ReadBytes(length);

            using var ms = new MemoryStream(data);

            if (result.ContainsKey(name)) {
                result[name].Dispose();
            }

            result[name] = new Texture(ms);
        }

        return result;
    }
    public static Dictionary<string, SoundBuffer> LoadSoundsFromFile(string fileName, Dictionary<string, SoundBuffer> existingSounds = null) {
        var result = existingSounds ?? new Dictionary<string, SoundBuffer>();

        using var fs = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(fs);

        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++) {
            string name = reader.ReadString();
            int length = reader.ReadInt32();
            byte[] data = reader.ReadBytes(length);

            using var ms = new MemoryStream(data);

            SoundBuffer buffer = new SoundBuffer(ms);
            result[name] = buffer;
        }

        return result;
    }

    public static string GetPath(string relativePath) {
        string baseDir = AppContext.BaseDirectory;
        string[] parts = relativePath.Split('/');
        return Path.GetFullPath(Path.Combine(baseDir, Path.Combine(parts))); 
    }
}
