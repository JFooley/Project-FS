using SFML.Graphics;
using SFML.System;
using SFML.Audio;
using UI_space;
using System.Diagnostics;
using Language_space;

public class Stage {
    // Basic Infos
    public string name;
    public string folder_path;
    public Texture thumb;

    // Stage configs
    public static bool training_mode => WGBattle.battle_mode == WGBattle.Training;
    public static bool pause = false;
    public static bool show_boxs = false;
    public static int block = 0; // 0 - never, 1 - after hit, 2 - always
    public static int parry = 0; // 0 - never, 1 - always
    public static bool refil_life = true;
    public static bool refil_super = true;
    public static int reset_frames = 0;

    // Widgets
    private WGPause wg_pause;

    // Battle Info
    public List<Character> OnSceneCharacters = new List<Character> {};
    public List<Character> OnSceneCharactersSorted => this.OnSceneCharacters
            .OrderByDescending(x => x.state.priority)
            .ThenBy(x => AI.rand.Next())
            .ToList();
    public List<Character> OnSceneCharactersRender => this.OnSceneCharacters
            .OrderBy(x => x.state.priority)
            .ToList();
    public List<Character> OnSceneParticles = new List<Character> {};
    public List<Character> newCharacters = new List<Character> {};
    public List<Character> newParticles = new List<Character> {};

    public static bool AI_playerA = true;
    public static bool AI_playerB = true;

    public Character character_A;
    public Character character_B;
    public Vector2f last_pos_A => character_A.body.LastPosition;
    public Vector2f last_pos_B => character_B.body.LastPosition;
    public int rounds_A;
    public int rounds_B;
    public int round => rounds_A + rounds_B + 1;
    public int elapsed_time => this.matchTimer.Elapsed.Seconds;
    public int round_time => elapse_time ? Config.round_length - (int) matchTimer.Elapsed.TotalSeconds : Config.round_length;
    public double raw_round_time => elapse_time ? Config.round_length - this.matchTimer.Elapsed.TotalMilliseconds/1000 : Config.round_length;
    public bool elapse_time = true;

    // Technical infos
    public int floor_line;
    public int length;
    public int height;
    public int start_point_A;
    public int start_point_B;
    public Vector2f center_point => new Vector2f(length / 2, height / 2);

    // Timers
    private Stopwatch timer;
    private Stopwatch matchTimer;

    // Animation infos
    public string CurrentState { get; set; }
    public string LastState { get; set; }
    public Dictionary<string, State> states;
    private Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
    private Dictionary<string, SoundBuffer> sounds = new Dictionary<string, SoundBuffer>();
    public Frame CurrentSprite => CurrentAnimation.GetCurrentFrameSimple();
    public Sound music;
    public State state => states[CurrentState];
    public Animation CurrentAnimation => states[CurrentState].animation;

    // Visual info
    public Color AmbientLight = new Color(255, 255, 255, 255);
    private Sprite fade90 = new Sprite(Data.textures["screens:90fade"]);
    private Sprite shadow;

    public Stage(string name, int floorLine, int length, int height, string folder_path) {
        this.name = name;
        this.floor_line = floorLine;
        this.length = length;
        this.height = height;
        this.start_point_A = (int) ((length / 2) - 100);
        this.start_point_B = (int) ((length / 2) + 100);
        this.folder_path = folder_path;
        this.rounds_A = 0;
        this.rounds_B = 0;
        this.CurrentState = "Default";

        this.wg_pause = new WGPause(this);

        this.timer = new Stopwatch();
        this.matchTimer = new Stopwatch();
    }
    public Stage(string name, Texture thumb) {
        this.name = name;
        this.thumb = thumb;
    }

    // Behaviour
    public void Update() {
        if (!this.character_A.on_hit) this.character_B.combo_counter = 0;
        if (!this.character_B.on_hit) this.character_A.combo_counter = 0;

        // Pause
        if (Input.Key_down("Start") && WGBattle.battle_state == WGBattle.Battling) this.Pause();

        // Render stage sprite
        if (this.textures.ContainsKey(this.CurrentSprite.Sprite_index)) {
            Sprite temp_sprite = new Sprite(this.textures[this.CurrentSprite.Sprite_index]);
            temp_sprite.Position = new Vector2f(0, 0);
            Program.window.Draw(temp_sprite);
        }

        // Advance to the next frame
        CurrentAnimation.AdvanceFrame();
        if (this.CurrentAnimation.ended && state.change_on_end) {
            if (states.ContainsKey(this.state.post_state)) {
                this.LastState = this.CurrentState;
                this.CurrentState = this.state.post_state;
                if (CurrentState != LastState) this.states[LastState].animation.Reset();
            }
        }

        // Keep music playing
        this.PlayMusic();

        // Render chars
        foreach (Character char_object in this.OnSceneCharactersRender) {
            char_object.Bot();
            this.DrawShadow(char_object);
            char_object.Render(Stage.show_boxs);
        }
        UI.DrawBattleUI(this);
        foreach (Character part_object in this.OnSceneParticles) part_object.Render(Stage.show_boxs);

        // Update chars
        foreach (Character char_object in this.OnSceneCharactersSorted) char_object.Update();
        this.OnSceneCharacters.RemoveAll(obj => obj.remove);
        this.OnSceneCharacters.AddRange(this.newCharacters);
        this.newCharacters.Clear();
        this.DoBehavior();

        // Update particles
        foreach (Character part_object in this.OnSceneParticles) part_object.Update();
        this.OnSceneParticles.RemoveAll(obj => obj.remove);
        this.OnSceneParticles.AddRange(this.newParticles);
        this.newParticles.Clear();

        // Render Pause menu and Traning assets
        if (Stage.training_mode) this.TrainingMode();
        if (Stage.pause) this.PauseScreen();
    }
    private void DoBehavior() {
        // Move characters away from border
        character_A.body.Position.X = Math.Max(character_A.push_box_width, Math.Min(character_A.body.Position.X, this.length - character_A.push_box_width));
        character_B.body.Position.X = Math.Max(character_B.push_box_width, Math.Min(character_B.body.Position.X, this.length - character_B.push_box_width));

        // Keep characters close 
        float deltaS = Math.Abs(character_A.body.Position.X - character_B.body.Position.X);
        if (deltaS >= Config.max_distance) {
            if ((character_A.facing == 1 && character_A.body.Position.X < last_pos_A.X) || (character_A.facing == -1 && character_A.body.Position.X > last_pos_A.X)) {
                character_A.body.Position.X = this.last_pos_A.X;
            }
            if ((character_B.facing == 1 && character_B.body.Position.X < last_pos_B.X) || (character_B.facing == -1 && character_B.body.Position.X > last_pos_B.X)) {
                character_B.body.Position.X = this.last_pos_B.X;
            }
        }
        
        // Keep characters facing each other
        if (this.character_A.body.Position.X < this.character_B.body.Position.X) {
            if (this.character_A.not_acting) this.character_A.facing = 1;
            if (this.character_B.not_acting) this.character_B.facing = -1;
        } else {
            if (this.character_A.not_acting) this.character_A.facing = -1;
            if (this.character_B.not_acting) this.character_B.facing = 1;
        }
        
        this.DoSpecialBehaviour();
    }
    public virtual void DoSpecialBehaviour() {}
    public void TrainingMode() {
        this.ResetRoundTime();
        if (Config.debug) UI.ShowFramerate("default small white");

        // Show life points
        UI.DrawText(this.character_A.life_points.X.ToString(), -18, -Config.RenderHeight/2, spacing: Config.spacing_small, alignment: "right", textureName: "default small white");
        UI.DrawText(this.character_B.life_points.X.ToString(), 18, -Config.RenderHeight/2, spacing: Config.spacing_small, alignment: "left", textureName: "default small white");
        UI.DrawText((this.character_A.life_points.Y - this.character_A.life_points.X).ToString(), -Config.RenderWidth/2, -Config.RenderHeight/2, spacing: Config.spacing_small, alignment: "left", textureName: "default small red");
        UI.DrawText((this.character_B.life_points.Y - this.character_B.life_points.X).ToString(), Config.RenderWidth/2, -Config.RenderHeight/2, spacing: Config.spacing_small, alignment: "right", textureName: "default small red");

        // Reset frames
        if (this.character_A.hitstop_counter == 0 && this.character_B.hitstop_counter == 0) {
            Stage.reset_frames += 1;
        }

        // Parry & Block: After hit (NOT WORKING)
        if (this.character_B.on_hit) {
            if (Stage.block == 1) this.character_B.blocking = true;
            if (Stage.parry == 1) this.character_B.parring = true;
            Stage.reset_frames = 0;
        } 
        if (this.character_A.on_hit) {
            if (Stage.block == 1) this.character_A.blocking = true;
            if (Stage.parry == 1) this.character_A.parring = true;
            Stage.reset_frames = 0;
        }

        // Block: Allways
        if (Stage.block == 2) {
            this.character_A.blocking = true;
            this.character_B.blocking = true;
        }

        // Parry: Allways
        if (Stage.parry == 2) {
            this.character_A.parring = true;
            this.character_B.parring = true;
        }

        // Reset chars life, stun and super bar
        if (Stage.reset_frames >= Config.reset_frames) {
            if (this.character_B.not_acting_all) {
                if (Stage.block != 2) this.character_B.blocking = false;
                if (Stage.parry != 2) this.character_B.parring = false;

                if (Stage.refil_life) {
                    this.character_B.life_points.X = this.character_B.life_points.Y;
                    this.character_B.dizzy_points.X = this.character_B.dizzy_points.Y;
                }
                if (Stage.refil_super) this.character_B.aura_points.X = this.character_B.aura_points.Y;
            }

            if (this.character_A.not_acting_all) {
                if (Stage.block != 2) this.character_A.blocking = false;
                if (Stage.parry != 2) this.character_A.parring = false;

                if (Stage.refil_life) {
                    this.character_A.life_points.X = this.character_A.life_points.Y;
                    this.character_A.dizzy_points.X = this.character_A.dizzy_points.Y;
                }
                if (Stage.refil_super) this.character_A.aura_points.X = this.character_A.aura_points.Y;
            }
            
            Stage.reset_frames = Config.reset_frames;
        }
    }
    public void PauseScreen() {
        fade90.Position = new Vector2f(Camera.X - Config.RenderWidth/2, Camera.Y - Config.RenderHeight/2);
        Program.window.Draw(fade90);
        Program.window.Draw(fade90);

        this.wg_pause.Render();
    }

    // Spawns
    public void spawnParticle(String state, float X, float Y, int facing = 1, int X_offset = 0, int Y_offset = 0) {
        var par = new Particle(state, X + X_offset * facing, Y + Y_offset, facing);
        par.ChangeState(state, reset: true);
        par.Load();
        this.newParticles.Add(par);
    }
    public void spawnHitspark(int hit, float X, float Y, int facing, int X_offset = 0, int Y_offset = 0) {
        string state;
        if (hit == Character.PARRY) {
            state = "Parry";
        } else if (hit == Character.HIT) {
            state = "Hit" + AI.rand.Next(1, 4);
        } else if (hit == Character.BLOCK){
            state = "Block";
        } else return;

        var hs = new Hitspark(state, X + X_offset * facing, Y + Y_offset, facing);
        hs.ChangeState(state, reset: true);
        hs.Load();
        this.newParticles.Add(hs);
    }

    // Visuals
    public void DrawShadow(Character char_obj) {
        if (char_obj.shadow_size != -1) {
            int shadow_index = (int) (char_obj.shadow_size * (Config.contact_shadow - Math.Min(this.floor_line - char_obj.body.Position.Y, Config.contact_shadow)) / Config.contact_shadow);
            this.shadow.Texture = Data.textures["ui:shadow" + shadow_index];
            this.shadow.Position = new Vector2f(char_obj.body.Position.X - this.shadow.GetLocalBounds().Width/2, this.floor_line - this.shadow.GetLocalBounds().Height/2 - 55 );
            this.shadow.Color = this.AmbientLight;
            Program.window.Draw(this.shadow);
        }
    }

    // Auxiliary
    public bool CheckRoundEnd() {
        if (Stage.training_mode || Stage.pause ) return false;
        
        bool doEnd = false;

        if (this.round_time == 0) {
            if (character_A.life_points.X <= character_B.life_points.X) {
                this.rounds_B += 1;
            } 
            if (character_A.life_points.X >= character_B.life_points.X) {
                this.rounds_A += 1;
            } 

            return true;
        }

        if (character_A.life_points.X <= 0 && !character_B.state.drama_wait) {
            this.rounds_B += 1;
            doEnd = true;
            // Spawn efeito de hit do KO
        }
        if (character_B.life_points.X <= 0 && !character_A.state.drama_wait) {
            this.rounds_A += 1;
            doEnd = true;
            // Spawn efeito de hit do KO
        }
        
        if (doEnd) this.StopFor(Config.last_hit_stop_time);

        return doEnd;
    }
    public bool CheckMatchEnd() {       
        if (this.rounds_A == this.rounds_B && this.rounds_A >= Config.max_rounds) {
            Program.winner = Program.Drawn;
            return true;
        }
        else if (this.rounds_A >= Config.max_rounds) {
            Program.winner = Program.PlayerA;
            Program.playerA_wins += 1;
            return true;
        }
        else if (this.rounds_B >= Config.max_rounds) {
            Program.winner = Program.PlayerB;
            Program.playerB_wins += 1;
            return true;
        }
        
        return false;
    }
    public void ResetMatch() {
        this.rounds_A = 0;
        this.rounds_B = 0;
    }
    public void Pause() {
        Stage.pause = !Stage.pause;
        this.TogglePlayers();
        this.PauseRoundTime();
        this.PauseTimer();
        this.ToggleMusicVolume(Stage.pause, volume_A: 20f);
        foreach (Character char_object in this.OnSceneCharacters) char_object.animate = !char_object.animate;
        foreach (Character part_object in this.OnSceneParticles) part_object.animate = ! part_object.animate;
    }
    public void Hitstop(string amount, int hit_type, Character on_hit_char) {
        uint frames;

        if (hit_type == Character.PARRY) {
            frames = (uint) Config.hit_stop_time * 1/2;
            switch (amount) {
                case "Light":
                        this.StopFor(lenght: (Config.hit_stop_time * 1/3) + on_hit_char.current_animation.lenght + Config.parry_advantage * 2, target: on_hit_char, target_lenght: Config.hit_stop_time * 1/3);
                    break;

                case "Medium":
                        this.StopFor(lenght: (Config.hit_stop_time * 1/3) + on_hit_char.current_animation.lenght + Config.parry_advantage * 3/2, target: on_hit_char, target_lenght: Config.hit_stop_time * 1/3);
                    break;

                case "Heavy":
                        this.StopFor(lenght: (Config.hit_stop_time * 1/3) + on_hit_char.current_animation.lenght + Config.parry_advantage, target: on_hit_char, target_lenght: Config.hit_stop_time * 1/3);
                    break;

                default:
                    this.StopFor(lenght: (Config.hit_stop_time * 1/3) + on_hit_char.current_animation.lenght + Config.parry_advantage * 2, target: on_hit_char, target_lenght: Config.hit_stop_time * 1/3);
                    break;
            }

        } else {
            switch (amount) {
                case "Light":
                    this.StopFor(Config.hit_stop_time * 1/2);
                    frames = (uint) Config.hit_stop_time * 1/2;
                    break;

                case "Medium":
                    this.StopFor(Config.hit_stop_time * 2/3);
                    frames = (uint) Config.hit_stop_time * 2/3;
                    break;

                case "Heavy":
                    this.StopFor(Config.hit_stop_time);
                    frames = (uint) Config.hit_stop_time;
                    break;

                default:
                    this.StopFor(Config.hit_stop_time * 1/2);
                    frames = (uint) Config.hit_stop_time * 1/2;
                    break;
            }
        }


        if (on_hit_char.player_index == Input.PLAYER_A) {
            if (Accessibility.atack_feedback) Input.SetVibration(Input.PLAYER_A, Accessibility.defend_feedback_intensity, 0, frames);
            if (Accessibility.atack_feedback) Input.SetVibration(Input.PLAYER_B, 0, Accessibility.atack_feedback_intensity, frames);
        } else if (on_hit_char.player_index == Input.PLAYER_B) {
            if (Accessibility.atack_feedback) Input.SetVibration(Input.PLAYER_B, Accessibility.defend_feedback_intensity, 0, frames);
            if (Accessibility.atack_feedback) Input.SetVibration(Input.PLAYER_A, 0, Accessibility.atack_feedback_intensity, frames);
        }

    }
    public void StopFor(int lenght, int target_lenght = 0, Character target = null) {
        foreach (var entity in this.OnSceneCharacters) entity.hitstop_counter = lenght;
        if (target != null) target.hitstop_counter = target_lenght;
    }
    public bool MustWait() {
        return this.character_A.state.drama_wait || this.character_B.state.drama_wait;
    }

    // Round Time
    public void ResetRoundTime() {
        this.elapse_time = true;
        this.matchTimer.Reset();
    }
    public void StartRoundTime() {
        this.elapse_time = true;
        this.matchTimer.Start();
    }
    public void StopRoundTime() {
        this.elapse_time = false;
        this.matchTimer.Stop();
    }
    public void PauseRoundTime() {
        if (matchTimer.IsRunning) matchTimer.Stop();
        else matchTimer.Start();
    }

    // Players
    public void SetChars(Character char_A, Character char_B) {
        this.character_A = char_A;
        this.character_A.facing = 1;
        this.character_A.player_index = 1;
        this.character_A.BotEnabled = Stage.training_mode || Stage.AI_playerA; ;
        this.character_A.AIEnabled = Stage.AI_playerA;

        this.character_B = char_B;
        this.character_B.facing = -1;
        this.character_B.player_index = 2;
        this.character_B.BotEnabled = Stage.training_mode || Stage.AI_playerB;
        this.character_B.AIEnabled = Stage.AI_playerB;

        this.character_A.floor_line = this.floor_line;
        this.character_B.floor_line = this.floor_line;
        this.character_A.body.Position.Y = this.floor_line;
        this.character_B.body.Position.Y = this.floor_line;
        this.character_A.body.Position.X = this.start_point_A;
        this.character_B.body.Position.X = this.start_point_B;

        this.OnSceneCharacters = new List<Character> {this.character_A, this.character_B};
        this.LockPlayers();
    }
    public void ResetPlayers(bool force = false, bool total_reset = false) {
        if (force) {
            this.character_A.Reset(this.start_point_A, facing: 1, state: "Intro", total_reset: total_reset);
            this.character_B.Reset(this.start_point_B, facing: -1, state: "Intro", total_reset: total_reset);
        } else {
            this.character_A.Reset(this.start_point_A, facing: 1, total_reset: total_reset);
            this.character_B.Reset(this.start_point_B, facing: -1, total_reset: total_reset);
        }
        
        this.OnSceneParticles.Clear();
        this.OnSceneCharacters = new List<Character> {this.character_A, this.character_B};
    }
    public void TogglePlayers() {
        this.character_A.behave = !this.character_A.behave;
        this.character_B.behave = !this.character_B.behave;
    }
    public void ReleasePlayers() {
        this.character_A.behave = true;
        this.character_B.behave = true;
    }
    public void LockPlayers() {
        this.character_A.behave = false;
        this.character_B.behave = false;
    }

    // Timer
    public void ResetTimer() {
        this.timer.Restart();
    }
    public bool CheckTimer(double elapsed_time) {
        return elapsed_time <= this.timer.Elapsed.TotalMilliseconds/1000;
    }
    public void PauseTimer() {
        if (this.timer.IsRunning) this.timer.Stop();
        else this.timer.Start();
    }
    public double GetTimerValue() {
        return this.timer.Elapsed.TotalMilliseconds/1000;
    }
    
    // Music
    public void SetMusicVolume(float amount = 100) {
        if (this.music != null) this.music.Volume = amount * (Config.Music_Volume / 100);
    }
    public void StopMusic() {
        this.music?.Stop();
    }
    public void PauseMusic() {
        this.music?.Pause();
    }
    public void PlayMusic() {
        if (this.music?.Status == SoundStatus.Stopped || this.music?.Status == SoundStatus.Paused){
            this.music.Play();
        }
    }
    public void ToggleMusic() {
        if (this.music?.Status == SoundStatus.Playing) this.PauseMusic();
        else this.PlayMusic();
    }
    public void ToggleMusicVolume(bool control, float volume_A = 100, float volume_B = 100) {
        if (control) this.SetMusicVolume(volume_A);
        else this.SetMusicVolume(volume_B);
    }
    
    // Loads☺
    public void LoadCharacters(Character charA, Character charB) {        
        this.SetChars(charA, charB);
    }
    public void UnloadCharacters() {
        this.character_A = null;
        this.character_B = null;
    }
    public void LoadTextures() {
        string full_path = this.folder_path;

        // Verifica se o diretório existe
        if (!System.IO.Directory.Exists(full_path)) {
            throw new System.IO.DirectoryNotFoundException($"O diretório {full_path} não foi encontrado.");
        }

        // Set shadow            
        this.shadow = new Sprite(Data.textures["ui:shadow1"]);

        // Verifica se o arquivo binário existe, senão, carrega as texturas e cria ele
        string dat_path = Path.Combine(full_path, "textures.dat");
        Data.LoadTexturesDat(dat_path, this.textures);
    }
    public void UnloadTextures() {
        foreach (var image in this.textures.Values)
        {
            image.Dispose(); // Free the memory used by the image
        }
        this.textures.Clear(); // Clear the dictionary
    }
    public void LoadSounds() {
        string full_sound_path = this.folder_path;

        // Verifica se o diretório existe
        if (!System.IO.Directory.Exists(full_sound_path)) {
            throw new System.IO.DirectoryNotFoundException($"O diretório {full_sound_path} não foi encontrado.");
        }

        // Verifica se o arquivo binário existe, senão, carrega os sons e cria ele
        string dat_path = Path.Combine(full_sound_path, "audio.dat");
        this.sounds = Data.LoadSoundsDat(dat_path);

        // setta a musica
        if (this.sounds.ContainsKey("music")) this.music = new Sound(this.sounds["music"]);
        else this.music = null;
    }
    public void UnloadSounds() {
        this.StopMusic();
        foreach (var sound in this.sounds.Values) {
            sound.Dispose(); 
        }
        this.sounds.Clear(); 
    }
    public void SetThumb() {
        try {
            this.textures.TryGetValue("thumb", out Texture thumb);
            this.thumb = thumb ;
        } catch (Exception) {
            this.thumb = new Texture(new Vector2u(1,1));
        }
    }

    public virtual void LoadStage() { }
    public void UnloadStage() {
        this.ResetMatch();
        this.ResetRoundTime();
        this.ResetPlayers();
        this.ResetTimer();
        this.UnloadCharacters();
    }

}

