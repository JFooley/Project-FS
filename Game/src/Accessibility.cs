using SFML.Audio;

public class Accessibility{
    public static bool accessibility => TTS || high_contrast || distance_radar || atack_feedback;
    public static bool TTS;
    public static bool high_contrast;
    public static bool distance_radar;
    public static bool atack_feedback = true;
    public static float atack_feedback_intensity = 0.8f;
    public static bool defend_feedback = true;
    public static float defend_feedback_intensity = 0.3f;

    // Options values
    public static float TTS_speed = 1f;

    // Data
    private static Sound? current_sound;
    private static bool must_finish;
    private static Queue<object[]> sound_queue = new Queue<object[]>{};

    public Accessibility() {}

    public static void Update() {
        if (!accessibility) return;

        // Play the next sound leting it finish if it's prioritary
        if (Accessibility.sound_queue.Count > 0 && (!Accessibility.must_finish || Accessibility.current_sound?.Status == SoundStatus.Stopped)) {
            object[] sound_data = Accessibility.sound_queue.Dequeue();
            Accessibility.must_finish = (bool) sound_data[3];
            Accessibility.current_sound = new Sound((SoundBuffer) sound_data[0]) {
                Volume = (float) sound_data[1],
                Pan = (float) sound_data[2]
            };
            Accessibility.current_sound.Play();
        }
    }

    public static void EnqueueSound(SoundBuffer sound, float volume = 100, float pan = 0, bool must_finish = false) {
        object[] sound_obj = new object[] {sound, volume, pan, must_finish};
        Accessibility.sound_queue.Enqueue(sound_obj);
    }
    public static void Context() {}

    public static void SaveToFile() {}
    public static void LoadToFile() {}   
}