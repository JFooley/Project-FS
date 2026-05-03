using SFML.Audio;
using UI_space;

public class Accessibility{
    public static bool accessibility => TTS || high_contrast || audio_cue || haptic_feedback;
    public static bool TTS = false;
    public static bool high_contrast = false;
    public static bool audio_cue = false;
    public static bool haptic_feedback = false;

    // Options values
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
        Accessibility.radar_beep_AB = new Sound(Data.sounds["accessibility:beepAB"]);
        Accessibility.radar_beep_BA = new Sound(Data.sounds["accessibility:beepBA"]);

        Accessibility.falling_sound = new Sound(Data.sounds["accessibility:falling"]);
        Accessibility.wake_up_sound = new Sound(Data.sounds["accessibility:wake_up"]);

        Accessibility.stand_hit_sound = new Sound(Data.sounds["accessibility:toneA"]) {Volume = 80};
        Accessibility.crouch_hit_sound = new Sound(Data.sounds["accessibility:toneB"]) {Volume = 80};
        Accessibility.air_hit_sound = new Sound(Data.sounds["accessibility:toneC"]) {Volume = 80};
    }

    public static void HapticFeedback(Character on_hit_char, Character hitting_char, uint frames) {
        Input.SetVibration(on_hit_char.player_index, Accessibility.defend_feedback_intensity, 0, frames);
        Input.SetVibration(hitting_char.player_index, 0, Accessibility.atack_feedback_intensity, frames);
        
    }
    public static void AudioCue(Character char_A, Character char_B) {
        // Falling and wake up
        if ((char_A.current_state == "Falling" && char_A.current_animation.on_first_frame) || (char_B.current_state == "Falling" && char_B.current_animation.on_first_frame)) {
            Accessibility.falling_sound.Play();
        } else if ((char_A.current_state == "WakeUp" && char_A.current_animation.on_last_frame) || (char_B.current_state == "WakeUp" && char_B.current_animation.on_last_frame)) {
            Accessibility.wake_up_sound.Play();
        }

        // Hit high feedback
        if (char_A.state.can_harm && char_A.has_hit && (char_B.on_hit || char_B.on_block || char_B.on_parry) && char_B.current_animation.on_first_frame && char_B.hitstop_counter == 0) {
            if (char_A.state.low) Accessibility.crouch_hit_sound.Play();
            else if (char_A.state.air) Accessibility.air_hit_sound.Play();
            else Accessibility.stand_hit_sound.Play();
        }
        if (char_B.state.can_harm && char_B.has_hit && (char_A.on_hit || char_A.on_block || char_A.on_parry) && char_A.current_animation.on_first_frame && char_A.hitstop_counter == 0) {
            if (char_B.state.low) Accessibility.crouch_hit_sound.Play();
            else if (char_B.state.air) Accessibility.air_hit_sound.Play();
            else Accessibility.stand_hit_sound.Play();
        }

        // Relative position
        float pos_diff = char_B.body.Position.X - char_A.body.Position.X;
        float distance = Math.Abs(pos_diff) / Config.RenderWidth;

        float frequency = 40;
        if (distance < 0.3f) frequency = 10;
        else if (distance < 0.6f) frequency = 20;

        if (UI.ForEach(frequency)) {
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