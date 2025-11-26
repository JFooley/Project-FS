using SFML.Graphics;
using SFML.System;
using Character_Space;
using SFML.Window;

public class Camera {
    private static readonly object _lock = new object();
    public static bool isLocked => lock_on_players;
    private static bool lock_on_players = false;

    public static Character CharA { get; private set; }
    public static Character CharB { get; private set; }

    public static int X_stage_limits = 0;
    public static int Y_stage_limits = 0;

    public static RenderWindow window => Program.window;

    // Camera Position
    public static float X { get; private set; }
    public static float Y { get; private set; }

    public Camera(int X = Config.RenderWidth/2, int Y = Config.RenderHeight/2) {
        Camera.X = X;
        Camera.Y = Y;
    }

    public static void LockCamera() {
        Camera.lock_on_players = true;
        Camera.Update();
    }
    public static void UnlockCamera() {
        Camera.lock_on_players = false;
        Camera.Update();
    }

    public static void SetChars(Character charA, Character charB) {
        Camera.CharA = charA;
        Camera.CharB = charB;
        Camera.lock_on_players = true;
    }
    public static void SetLimits(int length, int height) {
        Camera.X_stage_limits = length;
        Camera.Y_stage_limits = height;
    }
    public static void Update() {
        // Camera to center between players
        if (Camera.lock_on_players && CharA != null && CharB != null) {
            Camera.X = (Camera.CharA.body.Position.X + Camera.CharB.body.Position.X) / 2;
            Camera.Y = ((Camera.CharA.body.Position.Y + Camera.CharB.body.Position.Y) / 2) - Config.camera_height;

            // Limit camera pos
            float halfViewWidth = Program.view.Size.X / 2;
            float halfViewHeight = Program.view.Size.Y / 2;
            Camera.X = (int)Math.Max(halfViewWidth, Math.Min(Camera.X, Camera.X_stage_limits - halfViewWidth));
            Camera.Y = (int)Math.Max(halfViewHeight, Math.Min(Camera.Y, Camera.Y_stage_limits - halfViewHeight));
        } else {
            Camera.X = Config.RenderWidth / 2;
            Camera.Y = Config.RenderHeight / 2;
        }

        // View pos to camera pos
        Program.view.Center = new Vector2f(Camera.X, Camera.Y);
        Camera.window.SetView(Program.view);
    }
    public static void Reset() {
        Camera.CharA = null;
        Camera.CharB = null;
        Camera.lock_on_players = false;
        Camera.X = Config.RenderWidth/2;
        Camera.Y = Config.RenderHeight/2;
        Camera.X_stage_limits = 0;
        Camera.Y_stage_limits = 0;
    }

}
