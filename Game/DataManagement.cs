using System.Runtime.Remoting;
using SFML.Audio;
using SFML.Graphics;

namespace Data_space {
    public static class DataManagement {
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
                    Image image = new Image(width, height);

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
                            image.SetPixel(x, y, new Color(r, g, b, a));
                        }
                    }

                    textures[key] = new Texture(image);
                }
            }

            return textures;
        }
        public static Dictionary<string, Texture> LoadTexturesFromPath(string directoryPath, Dictionary<string, Texture> existingTextures = null) {
            Dictionary<string, Texture> textures = existingTextures ?? new Dictionary<string, Texture>();
            string[] files = Directory.GetFiles(directoryPath);

            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp")
                {
                    string key = Path.GetFileNameWithoutExtension(file);
                    textures[key] = new Texture(file);
                }
            }

            return textures;
        }

        public static void SaveSoundsToFile(string fileName, Dictionary<string, SoundBuffer> sounds) {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                writer.Write(sounds.Count); // Escreve quantos sons serão salvos

                foreach (var pair in sounds)
                {
                    // Salva o nome do som
                    writer.Write(pair.Key);

                    // Obtém os dados brutos do SoundBuffer
                    SoundBuffer buffer = pair.Value;
                    short[] samples = buffer.Samples;
                    byte[] sampleBytes = new byte[samples.Length * 2];
                    Buffer.BlockCopy(samples, 0, sampleBytes, 0, sampleBytes.Length);

                    // Escreve os metadados e dados do áudio
                    writer.Write(buffer.SampleRate);
                    writer.Write((byte)buffer.ChannelCount);
                    writer.Write(sampleBytes.Length);
                    writer.Write(sampleBytes);
                }
            }
        }
        public static Dictionary<string, SoundBuffer> LoadSoundsFromFile(string fileName, Dictionary<string, SoundBuffer> existingSounds = null)
        {
            Dictionary<string, SoundBuffer> loadedSounds = existingSounds ?? new Dictionary<string, SoundBuffer>();

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                int soundCount = reader.ReadInt32();

                for (int i = 0; i < soundCount; i++)
                {
                    string soundName = reader.ReadString();
                    uint sampleRate = reader.ReadUInt32();
                    byte channels = reader.ReadByte();
                    int dataLength = reader.ReadInt32();
                    byte[] audioData = reader.ReadBytes(dataLength);

                    // Reconstrói o SoundBuffer
                    short[] samples = new short[dataLength / 2];
                    Buffer.BlockCopy(audioData, 0, samples, 0, dataLength);
                    SoundBuffer buffer = new SoundBuffer(samples, channels, sampleRate);

                    // Cria o Sound e adiciona ao dicionário
                    loadedSounds[soundName] = buffer;
                }
            }

            return loadedSounds;
        }
        public static Dictionary<string, SoundBuffer> LoadSoundsFromPath(string directoryPath, Dictionary<string, SoundBuffer> existingSounds = null)
        {
            Dictionary<string, SoundBuffer> sounds = existingSounds ?? new Dictionary<string, SoundBuffer>();
            string[] files = Directory.GetFiles(directoryPath);

            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (extension == ".wav" || extension == ".ogg" || extension == ".flac" || extension == ".mp3")
                {
                    string key = Path.GetFileNameWithoutExtension(file);
                    sounds[key] = new SoundBuffer(file);
                }
            }

            return sounds;
        }
    
        public static void IndexTextureColors(Dictionary<string, Texture> textures, Texture palette) {
            var palette_img = palette.CopyToImage();

            Dictionary<Color, byte> colorToIndexMap = new Dictionary<Color, byte>();
            for (uint x = 0; x < palette_img.Size.X; x++) {
                colorToIndexMap[palette_img.GetPixel(x, 0)] = (byte) x;
            }

            foreach (Texture textureEntry in textures.Values) {
                var temp_img = textureEntry.CopyToImage();

                for (uint y = 0; y < temp_img.Size.Y; y++) {
                    for (uint x = 0; x < temp_img.Size.X; x++) {
                        
                        if (temp_img.GetPixel(x, y).A == 0) continue;

                        if (colorToIndexMap.TryGetValue(temp_img.GetPixel(x, y), out byte index)) {
                            temp_img.SetPixel(x, y, new Color(index, index, index, temp_img.GetPixel(x,y).A));
                        }
                    }
                }

                textureEntry.Update(temp_img);
            }
        }

    }
}