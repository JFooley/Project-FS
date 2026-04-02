using SFML.Audio;
using SFML.Graphics;
using Stage_Space;
using Character_Space;
using SFML.System;

public static class Data {
    public static List<Stage>? stages;
    public static List<Character>? characters;
    public static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
    public static Dictionary<string, Texture> thumbs = new Dictionary<string, Texture>();

    public static void SaveTexturesToFile(string fileName, Dictionary<string, Texture> textures) {
        using (var stream = new FileStream(fileName, FileMode.Create))
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(textures.Count);

            foreach (var kvp in textures)
            {
                writer.Write(kvp.Key);
                Image image = kvp.Value.CopyToImage();

                // Escreve dimensões
                writer.Write(image.Size.X);
                writer.Write(image.Size.Y);

                // Obtém pixels como byte array (sempre RGBA no SFML)
                byte[] pixels = image.Pixels;
                writer.Write(pixels.Length);
                writer.Write(pixels);
            }
        }
    }
    public static Dictionary<string, Texture> LoadTexturesFromFile(string fileName, Dictionary<string, Texture> existingTextures = null) {
        var textures = existingTextures ?? new Dictionary<string, Texture>();

        using (var stream = new FileStream(fileName, FileMode.Open))
        using (var reader = new BinaryReader(stream))
        {
            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                uint width = reader.ReadUInt32();
                uint height = reader.ReadUInt32();
                int pixelDataLength = reader.ReadInt32();
                byte[] pixelData = reader.ReadBytes(pixelDataLength);

                // Cria imagem - SFML sempre usa RGBA para Image
                Image image = new Image( new Vector2u(width, height));

                // Copia os dados pixel a pixel
                for (uint y = 0; y < height; y++)
                {
                    for (uint x = 0; x < width; x++)
                    {
                        int index = (int)((y * width + x) * 4);
                        byte r = pixelData[index];
                        byte g = pixelData[index + 1];
                        byte b = pixelData[index + 2];
                        byte a = pixelData[index + 3];
                        image.SetPixel(new Vector2u(x, y), new Color(r, g, b, a));
                    }
                }

                textures[key] = new Texture(image);
            }
        }

        return textures;
    }
    public static Dictionary<string, Texture> LoadTexturesFromPath(string directoryPath, Dictionary<string, Texture> existingTextures = null, string parentFolder = "") {
        Dictionary<string, Texture> textures = existingTextures ?? new Dictionary<string, Texture>();
        string[] files = Directory.GetFiles(directoryPath);

        foreach (string file in files) {
            string extension = Path.GetExtension(file).ToLower();
            if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp") {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string key = string.IsNullOrEmpty(parentFolder) ? fileName : $"{parentFolder}:{fileName}";
                textures[key] = new Texture(file);
            }
        }

        string[] subDirectories = Directory.GetDirectories(directoryPath);
        foreach (string subDir in subDirectories) {
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
    public static Dictionary<string, SoundBuffer> LoadSoundsFromFile(string fileName) {
        var result = new Dictionary<string, SoundBuffer>();

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

    public static void IndexTextureColors(Dictionary<string, Texture> textures, Texture palette) {
        var palette_img = palette.CopyToImage();

        Dictionary<Color, byte> colorToIndexMap = new Dictionary<Color, byte>();
        for (uint x = 0; x < palette_img.Size.X; x++) {
            colorToIndexMap[palette_img.GetPixel(new Vector2u(x, 0))] = (byte) x;
        }

        foreach (Texture textureEntry in textures.Values) {
            var temp_img = textureEntry.CopyToImage();

            for (uint y = 0; y < temp_img.Size.Y; y++) {
                for (uint x = 0; x < temp_img.Size.X; x++) {
                    
                    if (temp_img.GetPixel(new Vector2u(x, y)).A == 0) continue;

                    if (colorToIndexMap.TryGetValue(temp_img.GetPixel(new Vector2u(x, y)), out byte index)) {
                        temp_img.SetPixel(new Vector2u(x, y), new Color(index, index, index, temp_img.GetPixel(new Vector2u(x, y)).A));
                    }
                }
            }

            textureEntry.Update(temp_img);
        }
    }

}
