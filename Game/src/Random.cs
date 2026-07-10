using UI_space;

public class Random {
    private int seed = 0;
    public int Next(int min, int max) {
        seed = ((seed + (int) UI.frame_counter) * 1103515245 + 12345) & 0x7fffffff;
        return min + (seed % (max - min));
    }
}