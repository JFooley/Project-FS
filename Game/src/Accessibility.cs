using SFML.Audio;
using UI_space;

public class Accessibility{
    public static bool accessibility => TTS || high_contrast || audio_cue || atack_haptic_feedback;
    public static bool TTS = false;
    public static bool high_contrast = false;
    public static bool atack_haptic_feedback = true;
    public static bool spacialized_audio = true;
    public static bool audio_cue => fall_get_up_cue || distance_cue || atack_hight_cue;

    public static bool fall_get_up_cue = false;
    public static bool distance_cue = false;
    public static bool atack_hight_cue = false;
    public static bool navigation_cue = true;

    public static float TTS_speed = 1f;
    public const float atack_feedback_intensity = 0.8f;
    public const float defend_feedback_intensity = 0.1f;

    // Radar Data
    private static Sound radar_beep_BA;
    private static Sound radar_beep_AB;
    private static Sound falling_sound;
    private static Sound wake_up_sound;
    private static Sound air_hit_sound;
    private static Sound stand_hit_sound;
    private static Sound crouch_hit_sound;

    // TTS data
    private static Sound current_sound;
    private static bool must_finish;
    private static Queue<object[]> sound_queue = new Queue<object[]>{};

    // Colors
    public static SFML.Graphics.Color bar_graylife => new SFML.Graphics.Color(255, 255, 255);

    public Accessibility() {
        Accessibility.radar_beep_AB = new Sound(Data.sounds["accessibility:beepAB"]) {Volume = Config.Effect_Volume};
        Accessibility.radar_beep_BA = new Sound(Data.sounds["accessibility:beepBA"]) {Volume = Config.Effect_Volume};

        Accessibility.falling_sound = new Sound(Data.sounds["accessibility:falling"]) {Volume = Config.Effect_Volume};
        Accessibility.wake_up_sound = new Sound(Data.sounds["accessibility:wake_up"]) {Volume = Config.Effect_Volume};

        Accessibility.stand_hit_sound = new Sound(Data.sounds["accessibility:toneA"]) {Volume = Config.Effect_Volume};
        Accessibility.crouch_hit_sound = new Sound(Data.sounds["accessibility:toneB"]) {Volume = Config.Effect_Volume};
        Accessibility.air_hit_sound = new Sound(Data.sounds["accessibility:toneC"]) {Volume = Config.Effect_Volume};
    }

    public static void AtackHapticFeedback(Character on_hit_char, Character hitting_char, uint frames) {
        if (!Accessibility.atack_haptic_feedback) return;
        Input.SetVibration(on_hit_char.player_index, Accessibility.defend_feedback_intensity, 0, frames);
        Input.SetVibration(hitting_char.player_index, 0, Accessibility.atack_feedback_intensity, frames);
    }
    public static void DistanceAudioCue(Character char_A, Character char_B) {
        if (!Accessibility.distance_cue) return;

        float pos_diff = char_B.body.Position.X - char_A.body.Position.X;
        float distance = Math.Abs(pos_diff) / Config.RenderWidth;

        float frequency = 20;
        if (distance < 0.3f) frequency /= 2;
        else if (distance > 0.6f) frequency *= 2;

        if (UI.ForEach(frequency)) {
            if (pos_diff > 0) {
                Accessibility.radar_beep_AB.Pitch = 1.5f - 1f * distance;
                Accessibility.radar_beep_AB.Play();
            } else {
                Accessibility.radar_beep_BA.Pitch = 1.5f - 1f * distance;
                Accessibility.radar_beep_BA.Play();
            }
        }
    }
    public static void FallWakeUpAudioCue(Character char_A, Character char_B) {
        if (!Accessibility.fall_get_up_cue) return;

        if ((char_A.current_state == "Falling" && char_A.current_animation.on_first_frame) || (char_B.current_state == "Falling" && char_B.current_animation.on_first_frame)) {
            Accessibility.falling_sound.Play();
        } else if ((char_A.current_state == "WakeUp" && char_A.current_animation.on_first_frame) || (char_B.current_state == "WakeUp" && char_B.current_animation.on_first_frame)) {
            Accessibility.wake_up_sound.Play();
        }
    }
    public static void AtackHeightAudioCue(Character char_A, Character char_B) {
        if (!Accessibility.atack_hight_cue) return;

        if (char_A.state.can_harm && char_A.has_hit) {
            if (char_A.state.low) Accessibility.crouch_hit_sound.Play();
            else if (char_A.state.air) Accessibility.air_hit_sound.Play();
            else Accessibility.stand_hit_sound.Play();
        }
        if (char_B.state.can_harm && char_B.has_hit) {
            if (char_B.state.low) Accessibility.crouch_hit_sound.Play();
            else if (char_B.state.air) Accessibility.air_hit_sound.Play();
            else Accessibility.stand_hit_sound.Play();
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