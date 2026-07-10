public class Random {
    private int seed = 0;
    public int Next(int min, int max) {
        seed = 1664525 * seed + (int) UI.frame_counter + 1013904223;
        return (int)(seed % (uint)(max - min + 1)) + min;
    }
}