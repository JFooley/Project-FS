using SFML.System;
using Character_Space;

public class Camera {
    public static bool isLocked => lock_on_players;
    private static bool lock_on_players = false;

    public static Character CharA { get; private set; }
    public static Character CharB { get; private set; }

    private static int X_stage_limits = 0;
    private static int Y_stage_limits = 0;
    public static Vector2f target => (Camera.CharA.body.Position + Camera.CharB.body.Position) / 2;
    public static int camera_follow_threshold = 50;

    // Camera Position
    public static float X { get; private set; }
    public static float Y { get; private set; }

    public Camera(int X = Config.RenderWidth/2, int Y = Config.RenderHeight/2) {
        Camera.X = X;
        Camera.Y = Y;
    }

    public static void Update() {
        if (Camera.lock_on_players && CharA != null && CharB != null) {
            // Update camera pos based on target
            Camera.X = Camera.Lerp(Camera.X, Camera.target.X, 0.04f);
            Camera.Y = Camera.Lerp(Camera.Y, Camera.target.Y - Config.camera_height, 0.1f);

            // Old camera
            // Camera.X = Camera.target.X;
            // Camera.Y = Camera.target.Y - Config.camera_height;
            
            // Limit camera pos
            Camera.X = Math.Max(Program.view.Size.X / 2, Math.Min(Camera.X, Camera.X_stage_limits - Program.view.Size.X / 2));
            Camera.Y = Math.Max(Program.view.Size.Y / 2, Math.Min(Camera.Y, Camera.Y_stage_limits - Program.view.Size.Y / 2));
        } else {
            Camera.X = Config.RenderWidth / 2;
            Camera.Y = Config.RenderHeight / 2;
        }

        // View pos to camera pos
        Program.view.Center = new Vector2f(Camera.X, Camera.Y);
        Program.window.SetView(Program.view);
    }
    public static void Center() {
        if (Camera.lock_on_players && CharA != null && CharB != null) {
            Camera.X = Camera.target.X;
            Camera.Y = Camera.target.Y - Config.camera_height;
        } else {
            Camera.X = Config.RenderWidth / 2;
            Camera.Y = Config.RenderHeight / 2;
        }
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
    public static void LockCamera() {
        Camera.lock_on_players = true;
        Camera.Center();
    }
    public static void UnlockCamera() {
        Camera.lock_on_players = false;
        Camera.Center();
    }
    public static void SetChars(Character charA, Character charB) {
        Camera.CharA = charA;
        Camera.CharB = charB;
        Camera.lock_on_players = true;
        Camera.Center();
    }
    public static void SetLimits(int length, int height) {
        Camera.X_stage_limits = length;
        Camera.Y_stage_limits = height;
    }
    public static float Lerp(float value1, float value2, float amount) {
        return Math.Max((value1 * (1f - amount)) + (value2 * amount), 1);
    }
}
