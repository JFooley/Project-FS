using SFML.Audio;
using UI_space;

public class Accessibility{
    public static bool accessibility = false;
    public static bool TTS = false;
    public static bool high_contrast = false;
    public static bool distance_radar = true;
    public static bool atack_feedback = false;

    // Options values
    public static float TTS_speed = 1f;
    public const float atack_feedback_intensity = 0.8f;
    public const float defend_feedback_intensity = 0.1f;

    // Radar Data
    private static Sound radar_beep_BA;
    private static Sound radar_beep_AB;

    // TTS data
    private static Sound? current_sound;
    private static bool must_finish;
    private static Queue<object[]> sound_queue = new Queue<object[]>{};

    // Colors
    public static SFML.Graphics.Color bar_graylife => new SFML.Graphics.Color(255, 255, 255);

    public Accessibility() {
        radar_beep_AB = new Sound(Data.sounds["beepAB"]);
        radar_beep_BA = new Sound(Data.sounds["beepBA"]);
    }

    public static void Radar(Character char_A, Character char_B) {
        // ADICIONAR SOM DE QUANDO TROCA DE LADO

        // ADICIONAR NO FEEDBACK SONORO SONS DIFERENTES PRA GOLPES HIGH E LOW

        float pos_diff = char_B.body.Position.X - char_A.body.Position.X;
        float distance = Math.Abs(pos_diff) / Config.RenderWidth;
        if (UI.ForEach(distance < 0.3f ? 10 : 20)) {
            if (pos_diff > 0) {
                Accessibility.radar_beep_AB.Pitch = 1.9f - 0.9f * distance;
                Accessibility.radar_beep_AB.Play();
            } else {
                Accessibility.radar_beep_BA.Pitch = 1.9f - 0.9f * distance;
                Accessibility.radar_beep_BA.Play();
            }
        }
    }
    public static void TTSUpdate() {
        if (!Accessibility.TTS) return;

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
    
}