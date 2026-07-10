public class Rand {
    public int Next(int min, int max) {
        UI.frame_counter = 1664525 * UI.frame_counter + 1013904223;

        return (int)(UI.frame_counter % (uint)(max - min + 1)) + min;
    }
}