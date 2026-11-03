using SFML.Graphics;
using SFML.System;

public static class MainRender {
    public const int BackgroundLayer = 0;
    public const int ShadowLayer = 10;
    public const int CharacterLayer = 20;
    public const int EffectLayer = 30;
    public const int DebugLayer = 40;
    public const int InterfaceLayer = 100;
    public const int OverlayLayer = 200;

    private static readonly SortedDictionary<int, RenderTexture> layers = new SortedDictionary<int, RenderTexture>();
    private static Vector2u outputSize;
    private static RenderTexture? cachedFrame;
    private static bool hasPreviousFrame;
    private static View outputView = new View(new FloatRect(
        new Vector2f(0, 0),
        new Vector2f(Config.RenderWidth, Config.RenderHeight)));
    private static readonly View frameView = new View(new FloatRect(
        new Vector2f(0, 0),
        new Vector2f(Config.RenderWidth, Config.RenderHeight)));
    private static readonly BlendMode premultipliedAlpha = new BlendMode(
        BlendMode.Factor.One,
        BlendMode.Factor.OneMinusSrcAlpha,
        BlendMode.Equation.Add,
        BlendMode.Factor.One,
        BlendMode.Factor.OneMinusSrcAlpha,
        BlendMode.Equation.Add);

    public static void BeginFrame() {
        UpdateOutputView();

        foreach (RenderTexture layer in layers.Values) {
            layer.SetView(Program.view);
            layer.Clear(Color.Transparent);
        }
    }

    public static void Draw(IDrawable drawable, int layer_index = BackgroundLayer) {
        Draw(drawable, RenderStates.Default, layer_index);
    }

    public static void Draw(IDrawable drawable, RenderStates states, int layer_index = BackgroundLayer) {
        if (!layers.TryGetValue(layer_index, out RenderTexture? layer)) {
            layer = new RenderTexture(new Vector2u(Config.RenderWidth, Config.RenderHeight));
            layer.Smooth = false;
            layer.SetView(Program.view);
            layer.Clear(Color.Transparent);
            layers.Add(layer_index, layer);
        }

        layer.Draw(drawable, states);
    }

    public static bool HasPreviousFrame => hasPreviousFrame;

    public static Vector2f AlignToVirtualPixel(Vector2f position) {
        return new Vector2f(
            AlignCoordinate(position.X, Program.view.Center.X, Program.view.Size.X, Config.RenderWidth),
            AlignCoordinate(position.Y, Program.view.Center.Y, Program.view.Size.Y, Config.RenderHeight));
    }

    public static void EndFrame() {
        UpdateOutputView();

        foreach (RenderTexture layer in layers.Values) {
            layer.Display();
        }

        if (cachedFrame == null) {
            cachedFrame = new RenderTexture(new Vector2u(Config.RenderWidth, Config.RenderHeight));
            cachedFrame.Smooth = false;
        }

        cachedFrame.SetView(frameView);
        cachedFrame.Clear(Color.Transparent);
        foreach (RenderTexture layer in layers.Values) {
            cachedFrame.Draw(new Sprite(layer.Texture), new RenderStates(premultipliedAlpha));
        }
        cachedFrame.Display();
        hasPreviousFrame = true;

        PresentFrame();
    }

    public static void PresentPreviousFrame() {
        if (!hasPreviousFrame) {
            throw new InvalidOperationException("There is no rendered frame to present.");
        }

        PresentFrame();
    }

    private static void PresentFrame() {
        if (cachedFrame == null) {
            throw new InvalidOperationException("There is no rendered frame to present.");
        }

        UpdateOutputView();
        Program.window.SetView(outputView);
        Program.window.Clear(Color.Black);

        float scale = (float) outputSize.Y / Config.RenderHeight;

        float scaledWidth = Config.RenderWidth * scale;
        Sprite frameSprite = new Sprite(cachedFrame.Texture) {
            Position = new Vector2f((outputSize.X - scaledWidth) / 2f, 0),
            Scale = new Vector2f(scale, scale)
        };
        Program.window.Draw(frameSprite, new RenderStates(premultipliedAlpha));

        Program.window.Display();
        Program.window.Clear();
        Program.window.SetView(Program.view);
    }

    private static float AlignCoordinate(float position, float viewCenter, float viewSize, int targetSize) {
        float pixelPosition = (position - viewCenter) * targetSize / viewSize;
        return viewCenter + (float)Math.Round(pixelPosition) * viewSize / targetSize;
    }

    private static void UpdateOutputView() {
        Vector2u currentSize = Program.window.Size;
        if (outputSize.X == currentSize.X && outputSize.Y == currentSize.Y) return;

        outputSize = currentSize;
        outputView.Size = new Vector2f(outputSize.X, outputSize.Y);
        outputView.Center = new Vector2f(outputSize.X / 2f, outputSize.Y / 2f);
    }
}
