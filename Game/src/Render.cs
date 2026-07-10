using System.Collections.Generic;
using SFML.Graphics;

public static class RenderBuffer {
    private static NetworkFramePacket _currentFrame = new NetworkFramePacket();
    private static readonly Dictionary<Shader, string> _shaderNames = new Dictionary<Shader, string>();
    private static readonly Dictionary<Texture, string> _textureNames = new Dictionary<Texture, string>();
    private static readonly Dictionary<string, Texture> _texturesById = new Dictionary<string, Texture>();
    private static readonly Dictionary<string, Shader> _shadersById = new Dictionary<string, Shader>();

    public static void RegisterShader(string name, Shader shader) {
        _shaderNames[shader] = name;
        _shadersById[name] = shader;
    }

    public static void RegisterTexture(string name, Texture texture) {
        if (texture == null) return;
        _textureNames[texture] = name;
        _texturesById[name] = texture;
    }

    public static Texture GetTexture(string id) {
        Texture texture;
        if (_texturesById.TryGetValue(id, out texture)) {
            return texture;
        }
        return null;
    }

    public static Shader GetShader(string id) {
        Shader shader;
        if (_shadersById.TryGetValue(id, out shader)) {
            return shader;
        }
        return null;
    }

    private static string GetOrCreateTextureId(Texture texture) {
        if (texture == null) return "none";
        
        string id;
        if (_textureNames.TryGetValue(texture, out id)) {
            return id;
        }

        string fallbackId = "fallback/" + texture.CPointer.ToString();
        RegisterTexture(fallbackId, texture);
        return fallbackId;
    }

    public static void Draw(Sprite sprite) => Draw(sprite, RenderStates.Default);

    public static void Draw(Sprite sprite, RenderStates states, Dictionary<string, float> shaderParams = null) {
        Program.window.Draw(sprite, states);
        string shaderId = GetShaderId(states.Shader);
        string textureId = GetOrCreateTextureId(sprite.Texture);

        _currentFrame.Commands.Add(new DrawCommand {
            Type = DrawType.Sprite,
            AssetId = textureId,
            X = sprite.Position.X,
            Y = sprite.Position.Y,
            ScaleX = sprite.Scale.X,
            ScaleY = sprite.Scale.Y,
            ShaderId = shaderId,
            ShaderParams = shaderParams
        });
    }

    public static void Draw(RectangleShape rect) => Draw(rect, RenderStates.Default);

    public static void Draw(RectangleShape rect, RenderStates states, Dictionary<string, float> shaderParams = null) {
        Program.window.Draw(rect, states);
        string shaderId = GetShaderId(states.Shader);

        _currentFrame.Commands.Add(new DrawCommand {
            Type = DrawType.Rectangle,
            X = rect.Position.X,
            Y = rect.Position.Y,
            ScaleX = rect.Scale.X,
            ScaleY = rect.Scale.Y,
            Width = rect.Size.X,
            Height = rect.Size.Y,
            FillR = rect.FillColor.R,
            FillG = rect.FillColor.G,
            FillB = rect.FillColor.B,
            FillA = rect.FillColor.A,
            ShaderId = shaderId,
            ShaderParams = shaderParams
        });
    }

    public static void Draw(VertexArray vertexArray) => Draw(vertexArray, RenderStates.Default);

    public static void Draw(VertexArray vertexArray, RenderStates states, Dictionary<string, float> shaderParams = null) {
        Program.window.Draw(vertexArray, states);
        string shaderId = GetShaderId(states.Shader);
        string textureId = GetOrCreateTextureId(states.Texture);

        uint vertexCount = vertexArray.VertexCount;
        var netVertices = new List<NetworkVertex>((int)vertexCount);
        
        for (uint i = 0; i < vertexCount; i++) {
            Vertex v = vertexArray[i];
            netVertices.Add(new NetworkVertex {
                X = v.Position.X,
                Y = v.Position.Y,
                Tx = v.TexCoords.X,
                Ty = v.TexCoords.Y,
                R = v.Color.R,
                G = v.Color.G,
                B = v.Color.B,
                A = v.Color.A
            });
        }

        _currentFrame.Commands.Add(new DrawCommand {
            Type = DrawType.VertexArray,
            AssetId = textureId,
            Vertices = netVertices,
            PrimType = vertexArray.PrimitiveType,
            ShaderId = shaderId,
            ShaderParams = shaderParams
        });
    }

    private static string GetShaderId(Shader shader) {
        string name;
        if (shader != null && _shaderNames.TryGetValue(shader, out name)) {
            return name;
        }
        return "none";
    }

    public static NetworkFramePacket GetAndClear() {
        var packet = _currentFrame;
        _currentFrame = new NetworkFramePacket();
        return packet;
    }
}

[Serializable]
public enum DrawType : byte {
    Sprite,
    Rectangle,
    VertexArray
}

[Serializable]
public struct NetworkVertex {
    public float X, Y;
    public float Tx, Ty;
    public byte R, G, B, A;
}

[Serializable]
public struct DrawCommand {
    public DrawType Type;
    public string AssetId;
    public float X;
    public float Y;
    public float ScaleX;
    public float ScaleY;
    public float Width;
    public float Height;
    public byte FillR, FillG, FillB, FillA;
    public List<NetworkVertex> Vertices;
    public PrimitiveType PrimType;
    public string ShaderId;
    public Dictionary<string, float> ShaderParams;
}

[Serializable]
public class NetworkFramePacket {
    public List<DrawCommand> Commands { get; set; } = new List<DrawCommand>();
}