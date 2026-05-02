using System.Drawing;
using SFML.Audio;

public class Accessibility{
    public static bool accessibility = false;
    public static bool TTS = false;
    public static bool high_contrast = false;
    public static bool distance_radar = false;
    public static bool atack_feedback = false;

    // Options values
    public static float TTS_speed = 1f;
    public const float atack_feedback_intensity = 0.8f;
    public const float defend_feedback_intensity = 0.3f;

    // Data
    private static Sound? current_sound;
    private static bool must_finish;
    private static Queue<object[]> sound_queue = new Queue<object[]>{};

    // Colors
    public static SFML.Graphics.Color bar_graylife => new SFML.Graphics.Color(255, 255, 255);
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
}