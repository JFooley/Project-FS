using SFML.Audio;
using SFML.Graphics;
using SFML.System;


public static class Data {
    public static List<Stage>? stages;
    public static List<Character>? characters;
    public static Dictionary<string, SoundBuffer> sounds = new Dictionary<string, SoundBuffer>();
    public static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
    public static Dictionary<string, Texture> thumbs = new Dictionary<string, Texture>();

    public static void SaveTexturesToFile(string fileName, Dictionary<string, byte[]> textures) {
        using var fs = new FileStream(fileName, FileMode.Create);
        using var writer = new BinaryWriter(fs);

        writer.Write(textures.Count);

        foreach (var pair in textures) {
            writer.Write(pair.Key);
            writer.Write(pair.Value.Length);
            writer.Write(pair.Value);
        }
    }
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
    public static Dictionary<string, byte[]> LoadTexturesFromPath(string directoryPath, Dictionary<string, byte[]> existingTextures = null, string parentFolder = "") {
        var textures = existingTextures ?? new Dictionary<string, byte[]>();

        foreach (string file in Directory.GetFiles(directoryPath)) {
            string extension = Path.GetExtension(file).ToLower();

            if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp") {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string key = string.IsNullOrEmpty(parentFolder) ? fileName : $"{parentFolder}:{fileName}";
                textures[key] = File.ReadAllBytes(file);
            }
        }

        foreach (string subDir in Directory.GetDirectories(directoryPath)) {
            string folderName = Path.GetFileName(subDir);
            string newParent = string.IsNullOrEmpty(parentFolder) ? folderName : $"{parentFolder}:{folderName}";

            LoadTexturesFromPath(subDir, textures, newParent);
        }

        return textures;
    }

    public static void SaveSoundsToFile(string fileName, Dictionary<string, byte[]> sounds) {
        using var fs = new FileStream(fileName, FileMode.Create);
        using var writer = new BinaryWriter(fs);

        writer.Write(sounds.Count);

        foreach (var pair in sounds) {
            writer.Write(pair.Key);
            writer.Write(pair.Value.Length);
            writer.Write(pair.Value);
        }
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
    public static Dictionary<string, byte[]> LoadSoundsFromPath(string directoryPath) {
        var sounds = new Dictionary<string, byte[]>();
        string[] files = Directory.GetFiles(directoryPath);

        foreach (string file in files) {
            string extension = Path.GetExtension(file).ToLower();

            if (extension == ".wav" || extension == ".ogg" || extension == ".flac" || extension == ".mp3") {
                string key = Path.GetFileNameWithoutExtension(file);
                sounds[key] = File.ReadAllBytes(file);
            }
        }

        return sounds;
    }
}
