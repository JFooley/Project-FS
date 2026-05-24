using SFML.Audio;
using SFML.Graphics;
using UI_space;

public class Accessibility{
    // Accessibility options
    public static bool TTS = false;
    public static bool high_contrast = false;
    public static bool atack_haptic_feedback = true;
    public static bool spacialized_audio = true;
    public static bool distance_cue = false;
    public static bool fall_get_up_cue = false;
    public static bool atack_hight_cue = false;
    public static bool navigation_cue = true;

    public static bool cover_screen = false; 
    
    // Accessibility options data
    public static float TTS_speed = 1f;
    public const float atack_feedback_intensity = 1f;
    public const float defend_feedback_intensity = 0.2f;

    // Audio cue data
    private static Sound? radar_beep_BA;
    private static Sound? radar_beep_AB;
    private static Sound? falling_sound;
    private static Sound? wake_up_sound;
    private static Sound? air_hit_sound;
    private static Sound? stand_hit_sound;
    private static Sound? crouch_hit_sound;

    // TTS data
    private static HashSet<string> current_tts_ids = new();
    private static HashSet<string> last_tts_ids = new();
    private static Queue<TTSRequisition> tts_requisitions = new Queue<TTSRequisition>{};
    private static TTSRequisition? current_tts_requisition => tts_requisitions.Count > 0 ? tts_requisitions.Peek() : null;
    private static Sound? tts_sound;
    private static int current_audio_index = 0;

    // Colorblind data
    public static SFML.Graphics.Color bar_graylife => new SFML.Graphics.Color(255, 255, 255);

    public Accessibility() {
        Accessibility.radar_beep_AB = new Sound(Data.sounds["accessibility:beepSAB"]) {Volume = Config.Effect_Volume};
        Accessibility.radar_beep_BA = new Sound(Data.sounds["accessibility:beepSBA"]) {Volume = Config.Effect_Volume};

        Accessibility.falling_sound = new Sound(Data.sounds["accessibility:falling"]) {Volume = Config.Effect_Volume};
        Accessibility.wake_up_sound = new Sound(Data.sounds["accessibility:wake_up"]) {Volume = Config.Effect_Volume};

        Accessibility.stand_hit_sound = new Sound(Data.sounds["accessibility:toneA"]) {Volume = Config.Effect_Volume};
        Accessibility.crouch_hit_sound = new Sound(Data.sounds["accessibility:toneB"]) {Volume = Config.Effect_Volume};
        Accessibility.air_hit_sound = new Sound(Data.sounds["accessibility:toneC"]) {Volume = Config.Effect_Volume};
    }

    public static void Speak(string id, int type, bool priority, params string[] strings) {
        if (!TTS) return;

        current_tts_ids.Add(id);
        if (!last_tts_ids.Contains(id)) {
            tts_requisitions.Enqueue(new TTSRequisition(type, priority, strings));
        } 
    }
    public static void UpdateTTS() {
        if (!TTS) return;
        
        last_tts_ids = new HashSet<string>(current_tts_ids);
        current_tts_ids.Clear();

        if (current_tts_requisition == null) return;

        if (tts_requisitions.Count > 1 && !current_tts_requisition.pririty) { // Finaliza caso não seja prioritária e há fila
            tts_sound?.Stop();
            current_audio_index = 0;
            tts_requisitions.Dequeue();
            return;
        }

        if (tts_sound == null || tts_sound.Status == SoundStatus.Stopped) { 
            if (current_audio_index >= current_tts_requisition.strings.Length) { // Finaliza caso tenha sido satisfeita
                current_audio_index = 0;
                tts_requisitions.Dequeue();
                tts_sound = null;
                return;
            }

            // Toca o próximo áudio
            if (current_tts_requisition.type == TTSRequisition.TEXT) {
                tts_sound = Language.Vocalize(current_tts_requisition.strings[current_audio_index]);
            } else {
                tts_sound = new Sound(Data.sounds[current_tts_requisition.strings[current_audio_index]]);
            }

            current_audio_index++;
            tts_sound?.Play();
        }
    }
    public static void AtackHapticFeedback(Character on_hit_char, Character hitting_char, uint frames) {
        if (!Accessibility.atack_haptic_feedback) return;
        Input.SetVibration(on_hit_char.player_index, Accessibility.defend_feedback_intensity, 0, frames);
        Input.SetVibration(hitting_char.player_index, 0, Accessibility.atack_feedback_intensity, frames);
    }
    public static void DistanceAudioCue(Character char_A, Character char_B) {
        if (!Accessibility.distance_cue) return;

        float pos_diff = char_B.body.position.X - char_A.body.position.X;
        float distance = Math.Abs(pos_diff) / Config.RenderWidth;

        float frequency = 20;
        if (distance < 0.3f) frequency /= 2;
        else if (distance > 0.6f) frequency *= 2;

        if (UI.ForEach(frequency)) {
            if (pos_diff > 0) {
                Accessibility.radar_beep_AB.Pitch = 1.5f - 1f * distance;
                Accessibility.radar_beep_AB?.Play();
            } else {
                Accessibility.radar_beep_BA.Pitch = 1.5f - 1f * distance;
                Accessibility.radar_beep_BA?.Play();
            }
        }
    }
    public static void FallWakeUpAudioCue(Character char_A, Character char_B) {
        if (!Accessibility.fall_get_up_cue) return;

        if ((char_A.current_state == "Falling" && char_A.current_animation.on_first_frame) || (char_B.current_state == "Falling" && char_B.current_animation.on_first_frame)) {
            Accessibility.falling_sound?.Play();
        } else if ((char_A.current_state == "WakeUp" && char_A.current_animation.on_first_frame) || (char_B.current_state == "WakeUp" && char_B.current_animation.on_first_frame)) {
            Accessibility.wake_up_sound?.Play();
        }
    }
    public static void AtackHeightAudioCue(Character char_A, Character char_B) {
        if (!Accessibility.atack_hight_cue) return;

        if (char_A.state.will_hit) {
            if (char_A.state.low) Accessibility.crouch_hit_sound?.Play();
            else if (char_A.state.air) Accessibility.air_hit_sound?.Play();
            else Accessibility.stand_hit_sound?.Play();
        }
        if (char_B.state.will_hit) {
            if (char_B.state.low) Accessibility.crouch_hit_sound?.Play();
            else if (char_B.state.air) Accessibility.air_hit_sound?.Play();
            else Accessibility.stand_hit_sound?.Play();
        }
    }
    
}

public class TTSRequisition {
    public const int TEXT = 0;
    public const int AUDIO = 1;

    public bool pririty;
    public string[] strings;
    public int type;

    public TTSRequisition(int type, bool priority, params string[] strings) {
        this.type = type;
        this.pririty = priority;
        this.strings = strings;
    }
}