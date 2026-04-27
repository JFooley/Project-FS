using SFML.Audio;
using SFML.Graphics;

public static class Data {
    public static List<Stage> stages = new List<Stage>();
    public static List<Character> characters = new List<Character>();
    public static List<Character> particles = new List<Character>();
    public static Dictionary<string, SoundBuffer> sounds = new Dictionary<string, SoundBuffer>();
    public static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

    public static Dictionary<string, Texture> LoadTexturesDat(string fileName, Dictionary<string, Texture> existingTextures = null) {
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
    public static Dictionary<string, SoundBuffer> LoadSoundsDat(string fileName, Dictionary<string, SoundBuffer> existingSounds = null) {
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
    public static Dictionary<string, List<Frame>> LoadAnimationDat(string path, bool have_data = true) {
        var result = new Dictionary<string, List<Frame>>();

        using var fs = new FileStream(path, FileMode.Open);
        using var br = new BinaryReader(fs);

        int animCount = br.ReadInt32();

        for (int i = 0; i < animCount; i++) {
            string animName = br.ReadString();
            int frameCount = br.ReadInt32();

            var frames = new List<Frame>(frameCount);

            for (int j = 0; j < frameCount; j++) {
                string sprite = br.ReadString();
                float dx = br.ReadSingle();
                float dy = br.ReadSingle();

                int len = br.ReadInt32();
                string sound = br.ReadString();
                bool hasHit = br.ReadBoolean();

                int boxCount = br.ReadInt32();
                var boxes = new List<GenericBox>(boxCount);

                for (int k = 0; k < boxCount; k++) {
                    int type = br.ReadInt32();
                    int ax = (int) br.ReadSingle();
                    int ay = (int) br.ReadSingle();
                    int bx = (int) br.ReadSingle();
                    int by = (int) br.ReadSingle();

                    boxes.Add(new GenericBox(type, ax, ay, bx, by));
                }

                if (have_data) frames.Add(new FrameData(sprite, dx, dy, boxes, len, sound, hasHit));
                else frames.Add(new Frame(sprite, len, sound));
            }

            result[animName] = frames;
        }

        return result;
    }

    public static string GetPath(string relativePath) {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath));
    }
}


