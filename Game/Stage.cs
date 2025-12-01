using SFML.Graphics;
using SFML.System;
using SFML.Audio;
using Animation_Space;
using Character_Space;
using UI_space;
using Input_Space;
using System.Diagnostics;
using Data_space;
using Language_space;

namespace Stage_Space {
    public class Stage {
        // Basic Infos
        public string name = "";
        public string folder_path;
        public Sprite thumb;

        // Debug infos
        public bool training_mode = false;
        public bool pause = false;
        public bool show_boxs = false;
        public int block = 0; // 0 - never, 1 - after hit, 2 - always
        public int parry = 0; // 0 - never, 1 - always
        public bool refil_life = true;
        public bool refil_super = true;
        public int reset_frames = 0;

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
        public int floorLine;
        public int length;
        public int height;
        public int start_point_A;
        public int start_point_B;
        public Vector2f center_point => new Vector2f(length / 2, height / 2);

        // Timers
        private Stopwatch timer;
        private Stopwatch matchTimer;

        // Aux
        private Vector2i pause_pointer = new Vector2i(0,0);

        // Pre-renders
        private Hitspark spark; 
        private Fireball fireball;
        private Particle particle;

        // Animation infos
        public string CurrentState { get; set; }
        public string LastState { get; set; }
        public Dictionary<string, State> states;
        private Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
        private Dictionary<string, SoundBuffer> sounds = new Dictionary<string, SoundBuffer>();
        public string CurrentSprite => CurrentAnimation.GetCurrentSimpleFrame();
        public Sound music;
        public State state => states[CurrentState];
        public Animation CurrentAnimation => states[CurrentState].animation;
        public int CurrentFrameIndex => states[CurrentState].animation.anim_frame_index;

        // Visual info
        public Color AmbientLight = new Color(255, 255, 255, 255);
        private Sprite fade90 = new Sprite(Program.visuals["90fade"]);
        private Sprite shadow;

        public Stage(string name, int floorLine, int length, int height, string folder_path, Texture thumb) {
            this.name = name;
            this.floorLine = floorLine;
            this.length = length;
            this.height = height;
            this.start_point_A = (int) ((length / 2) - 100);
            this.start_point_B = (int) ((length / 2) + 100);
            this.folder_path = folder_path;
            this.rounds_A = 0;
            this.rounds_B = 0;
            this.CurrentState = "Default";

            this.spark = new Hitspark("Default", 0, 0, 1, this);
            this.fireball = new Fireball("Default", 1, 0, 0, 0, 1, this);
            this.particle = new Particle("Default", 0, 0, 1, this);

            this.thumb = new Sprite(thumb);

            this.timer = new Stopwatch();
            this.matchTimer = new Stopwatch();
        }
        public Stage(string name, Texture thumb) {
            this.name = name;
            this.thumb = new Sprite(thumb);
        }

        // Behaviour
        public void Update() {
            if (!this.character_A.on_hit) this.character_B.combo_counter = 0;
            if (!this.character_B.on_hit) this.character_A.combo_counter = 0;

            // Pause
            if (InputManager.Key_down("Start") && Program.sub_state == Program.Battling) this.Pause();

            // Bot
            if (InputManager.Key_down("Select", player: InputManager.PLAYER_A)) {
                this.character_A.BotEnabled = !this.character_A.BotEnabled;
                this.character_A.AIEnabled = !this.character_A.AIEnabled;
            }
            if (InputManager.Key_down("Select", player: InputManager.PLAYER_B)) {
                this.character_B.BotEnabled = !this.character_B.BotEnabled;
                this.character_B.AIEnabled = !this.character_B.AIEnabled;
            }

            // Render stage sprite
            if (this.textures.ContainsKey(this.CurrentSprite)) {
                Sprite temp_sprite = new Sprite(this.textures[this.CurrentSprite]);
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
                char_object.Render(this.show_boxs);
            }
            UI.DrawBattleUI(this);
            foreach (Character part_object in this.OnSceneParticles) part_object.Render(this.show_boxs);

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
            if (this.training_mode) this.TrainingMode();
            if (this.pause) this.PauseScreen();
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
            UI.ShowFramerate("default small white");
            UI.DrawText(Language.GetText("training mode"), 0, 70, spacing: Config.spacing_small, textureName: "default small white");

            // Show life points
            UI.DrawText(this.character_A.life_points.X.ToString(), -18, -Config.RenderHeight/2, spacing: Config.spacing_small, alignment: "right", textureName: "default small white");
            UI.DrawText(this.character_B.life_points.X.ToString(), 18, -Config.RenderHeight/2, spacing: Config.spacing_small, alignment: "left", textureName: "default small white");
            UI.DrawText((this.character_A.life_points.Y - this.character_A.life_points.X).ToString(), -Config.RenderWidth/2, -Config.RenderHeight/2, spacing: Config.spacing_small, alignment: "left", textureName: "default small red");
            UI.DrawText((this.character_B.life_points.Y - this.character_B.life_points.X).ToString(), Config.RenderWidth/2, -Config.RenderHeight/2, spacing: Config.spacing_small, alignment: "right", textureName: "default small red");

            // Reset frames
            if (this.character_A.hitstop_counter == 0 && this.character_B.hitstop_counter == 0) this.reset_frames += 1;

            // Block: After hit
            if (this.character_B.stun_frames > 0) {
                if (this.block == 1) this.character_B.blocking = true;
                this.reset_frames = 0;
            } else if (this.character_A.stun_frames > 0) {
                if (this.block == 1) this.character_A.blocking = true;
                this.reset_frames = 0;
            }
            
            // Block: Allways
            if (this.block == 2) {
                this.character_A.blocking = true;
                this.character_B.blocking = true;
            }

            // Parry: After hit
            if (this.character_B.stun_frames > 0) {
                if (this.parry == 1) this.character_B.parring = true;
                this.reset_frames = 0;
            } else if (this.character_A.stun_frames > 0) {
                if (this.parry == 1) this.character_A.parring = true;
                this.reset_frames = 0;
            }

            // Parry: Allways
            if (this.parry == 2) {
                this.character_A.parring = true;
                this.character_B.parring = true;
            }

            // Reset chars life, stun and super bar
            if (this.reset_frames >= Config.reset_frames) {
                if (this.character_B.not_acting_all) {
                    if (this.block != 2) this.character_B.blocking = false;
                    if (this.parry != 2) this.character_B.parring = false;
                    if (this.refil_life) {
                        this.character_B.life_points.X = this.character_B.life_points.Y;
                        this.character_B.dizzy_points.X = this.character_B.dizzy_points.Y;
                    }
                    if (this.refil_super) this.character_B.aura_points.X = this.character_B.aura_points.Y;
                }
                if (this.character_A.not_acting_all) {
                    if (this.block != 2) this.character_A.blocking = false;
                    if (this.parry != 2) this.character_A.parring = false;
                    if (this.refil_life) {
                        this.character_A.life_points.X = this.character_A.life_points.Y;
                        this.character_A.dizzy_points.X = this.character_A.dizzy_points.Y;
                    }
                    if (this.refil_super) this.character_A.aura_points.X = this.character_A.aura_points.Y;
                }
                this.reset_frames = Config.reset_frames;
            }
        }
        public void PauseScreen() {
            fade90.Position = new Vector2f(Camera.X - Config.RenderWidth/2, Camera.Y - Config.RenderHeight/2);
            Program.window.Draw(fade90);

            var face_release = InputManager.Key_up("A") || InputManager.Key_up("B") || InputManager.Key_up("C") || InputManager.Key_up("D");
            var face_hold = InputManager.Key_hold("A") || InputManager.Key_hold("B") || InputManager.Key_hold("C") || InputManager.Key_hold("D");

            // Draw options
            UI.DrawText(Language.GetText("Pause"), 0, -75, spacing: Config.spacing_medium, textureName: "default medium");

            if (UI.DrawButton(Language.GetText("Settings"), 0, -45, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: this.pause_pointer.Y == 0, click_font: "default medium click", hover_font: "default medium hover", font: "default medium"))
                Program.ChangeState(Program.Settings);
            
            if (UI.DrawButton(Language.GetText("Controls"), 0, -30, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: this.pause_pointer.Y == 1, click_font: "default medium click", hover_font: "default medium hover", font: "default medium"))
                Program.ChangeState(Program.Controls);
            
            if (UI.DrawButton(Language.GetText("Training"), 0, -15, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: this.pause_pointer.Y == 2, click_font: "default medium click", hover_font: "default medium hover", font: "default medium"))
                this.training_mode = !this.training_mode;
            
            if (training_mode) {
                if (UI.DrawButton(Language.GetText("Reset Characters"), 0, 0, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pause_pointer.Y == 3, click_font: "default small click", hover_font: "default small hover", font: "default small"))
                    this.ResetPlayers();
            
                if (UI.DrawButton(Language.GetText("Show hitboxes"), 0, 10, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pause_pointer.Y == 4, click_font: "default small click", hover_font: "default small hover", font: "default small"))
                    this.show_boxs = !this.show_boxs;
            
                if (UI.DrawButton(block switch { 0 => Language.GetText("Block") + ": " + Language.GetText("Never"), 1 => Language.GetText("Block") + ": " + Language.GetText("After hit"), 2 => Language.GetText("Block") + ": " + Language.GetText("Always"), _ => Language.GetText("Block") + ": " + Language.GetText("Error")}, 0, 20, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pause_pointer.Y == 5, click_font: "default small click", hover_font: "default small hover", font: "default small"))
                    this.block = block >= 2 ? 0 : block + 1;
            
                if (UI.DrawButton(parry switch { 0 => Language.GetText("Parry") + ": " + Language.GetText("Never"), 1 => Language.GetText("Parry") + ": " + Language.GetText("After hit"), 2 => Language.GetText("Parry") + ": " + Language.GetText("Always"), _ => Language.GetText("Parry") + ": " + Language.GetText("Error")}, 0, 30, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pause_pointer.Y == 6, click_font: "default small click", hover_font: "default small hover", font: "default small"))
                    this.parry = parry >= 2 ? 0 : parry + 1;
            
                if (UI.DrawButton(refil_life ? Language.GetText("Life") + ": " + Language.GetText("Refil") : Language.GetText("Life") + ": " + Language.GetText("Keep"), 0, 40, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pause_pointer.Y == 7, click_font: "default small click", hover_font: "default small hover", font: "default small"))
                    this.refil_life = !this.refil_life;
            
                if (UI.DrawButton(refil_super ? Language.GetText("Super") + ": " + Language.GetText("Refil") : Language.GetText("Super") + ": " + Language.GetText("Keep"), 0, 50, spacing: Config.spacing_small, click: face_hold, action: face_release, hover: this.pause_pointer.Y == 8, click_font: "default small click", hover_font: "default small hover", font: "default small"))
                    this.refil_super = !this.refil_super;
            
            } if (UI.DrawButton(Language.GetText("End match"), 0, 70, spacing: Config.spacing_medium, click: face_hold, action: face_release, hover: this.pause_pointer.Y == 9, click_font: "default medium click", hover_font: "default medium red", font: "default medium")) {
                this.Pause();
                Program.winner = Program.Drawn;
                Program.sub_state = Program.MatchEnd;
                this.show_boxs = false;
                this.training_mode = false;
                this.block = 0;
                this.refil_life = true;
                this.refil_super = true;
                this.pause_pointer.Y = 0;
            }       

            // Change option 
            if (InputManager.Key_down("Up") && this.pause_pointer.Y > 0) {
                this.pause_pointer.Y -= 1;
                if (!training_mode && this.pause_pointer.Y < 9 && this.pause_pointer.Y > 2) this.pause_pointer.Y = 2;
            } else if (InputManager.Key_down("Down") && this.pause_pointer.Y < 9) {
                this.pause_pointer.Y += 1;
                if (!training_mode && this.pause_pointer.Y < 9 && this.pause_pointer.Y > 2) this.pause_pointer.Y = 9;
            }
        }

        // Spawns
        public void spawnParticle(String state, float X, float Y, int facing = 1, int X_offset = 0, int Y_offset = 0) {
            var par = new Particle(state, X + X_offset * facing, Y + Y_offset, facing);
            par.ChangeState(state, reset: true);
            par.states = this.particle.states;
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
            } else {
                return;
            }
            var hs = new Hitspark(state, X + X_offset * facing, Y + Y_offset, facing);
            hs.ChangeState(state, reset: true);
            hs.states = this.spark.states;
            this.newParticles.Add(hs);
        }
        public Fireball spawnFireball(string state, float X, float Y, int facing, int team, int life_points = 1, int X_offset = 10, int Y_offset = 0) {        
            var fb = new Fireball(state, life_points, X + X_offset * facing, Y + Y_offset, team, facing, this);
            fb.ChangeState(state, reset: true);
            fb.states = this.fireball.states;
            this.newCharacters.Add(fb);
            return fb;
        }

        // Visuals
        public void DrawShadow(Character char_obj) {
            if (char_obj.shadow_size != -1) {
                this.shadow.Texture = Program.visuals["shadow" + char_obj.shadow_size];
                this.shadow.Position = new Vector2f(char_obj.body.Position.X - this.shadow.GetLocalBounds().Width/2, this.floorLine - this.shadow.GetLocalBounds().Height/2 - 55 );
                this.shadow.Color = this.AmbientLight;
                Program.window.Draw(this.shadow);
            }
        }

        // Auxiliary
        public bool CheckRoundEnd() {
            if (this.training_mode || this.pause ) return false;
            
            bool doEnd = false;

            if (this.round_time == 0) {
                if (character_A.life_points.X <= character_B.life_points.X) {
                    this.rounds_B += 1;
                    doEnd = true;
                } 
                if (character_A.life_points.X >= character_B.life_points.X) {
                    this.rounds_A += 1;
                    doEnd = true;
                } 

                return doEnd;
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
                Program.winner = Program.Player1;
                Program.player1_wins += 1;
                return true;
            }
            else if (this.rounds_B >= Config.max_rounds) {
                Program.winner = Program.Player2;
                Program.player2_wins += 1;
                return true;
            }
            
            return false;
        }
        public void ResetMatch() {
            this.rounds_A = 0;
            this.rounds_B = 0;
        }
        public void Pause() {
            this.pause = !this.pause;
            this.TogglePlayers();
            this.PauseRoundTime();
            this.PauseTimer();
            this.ToggleMusicVolume(this.pause, volume_A: 20f);
            foreach (Character char_object in this.OnSceneCharacters) char_object.animate = !char_object.animate;
            foreach (Character part_object in this.OnSceneParticles) part_object.animate = ! part_object.animate;
        }
        public void Hitstop(string amount, int hit_type, Character character) {
            if (hit_type == Character.PARRY) {
                switch (amount) {
                    case "Light":
                            this.StopFor(lenght: (Config.hit_stop_time * 1/3) + character.current_animation.lenght + Config.parry_advantage * 2, target: character, target_lenght: Config.hit_stop_time * 1/3);
                        break;

                    case "Medium":
                            this.StopFor(lenght: (Config.hit_stop_time * 1/3) + character.current_animation.lenght + Config.parry_advantage * 3/2, target: character, target_lenght: Config.hit_stop_time * 1/3);
                        break;

                    case "Heavy":
                            this.StopFor(lenght: (Config.hit_stop_time * 1/3) + character.current_animation.lenght + Config.parry_advantage, target: character, target_lenght: Config.hit_stop_time * 1/3);
                        break;

                    default:
                        this.StopFor(lenght: (Config.hit_stop_time * 1/3) + character.current_animation.lenght + Config.parry_advantage * 2, target: character, target_lenght: Config.hit_stop_time * 1/3);
                        break;
                }

            } else {
                switch (amount) {
                    case "Light":
                        this.StopFor(Config.hit_stop_time * 1/2);
                        break;

                    case "Medium":
                        this.StopFor(Config.hit_stop_time * 2/3);
                        break;

                    case "Heavy":
                        this.StopFor(Config.hit_stop_time);
                        break;

                    default:
                        this.StopFor(Config.hit_stop_time * 1/2);
                        break;
                }
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

            this.character_B = char_B;
            this.character_B.facing = -1;
            this.character_B.player_index = 2;

            this.character_A.floor_line = this.floorLine;
            this.character_B.floor_line = this.floorLine;
            this.character_A.body.Position.X = this.start_point_A;
            this.character_B.body.Position.X = this.start_point_B;
            this.character_A.stage = this;
            this.character_B.stage = this;

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
        public void SetMusicVolume(float amount = -1) {
            if (amount == -1) amount = Config.Music_Volume;
            this.music.Volume = amount * (Config.Music_Volume / 100);
        }
        public void StopMusic() {
            this.music.Stop();
        }
        public void PauseMusic() {
            this.music.Pause();
        }
        public void PlayMusic() {
            if (this.music.Status == SoundStatus.Stopped || this.music.Status == SoundStatus.Paused){
                this.music.Play();
            }
        }
        public void ToggleMusic() {
            if (this.music.Status == SoundStatus.Paused) this.PlayMusic();
            else this.PauseMusic();
        }
        public void ToggleMusicVolume(bool control, float volume_A = -1, float volume_B = -1) {
            if (control) this.SetMusicVolume(volume_A);
            else this.SetMusicVolume(volume_B);
        }
        
        // Loads☺
        public void LoadCharacters(string charA_name, string charB_name) {        
            var charA = Character.SelectCharacter(charA_name, this);
            var charB = Character.SelectCharacter(charB_name, this);

            this.SetChars(charA, charB);
            
            this.spark.Load();
            this.fireball.Load();
            this.particle.Load();
        }
        public void UnloadCharacters() {
            this.character_A = null;
            this.character_B = null;
        }
        public void LoadTextures() {
            string currentDirectory = Directory.GetCurrentDirectory();
            string full_path = Path.Combine(currentDirectory, this.folder_path, "sprites");

            // Verifica se o diretório existe
            if (!System.IO.Directory.Exists(full_path)) {
                throw new System.IO.DirectoryNotFoundException($"O diretório {full_path} não foi encontrado.");
            }

            // Set shadow            
            this.shadow = new Sprite(Program.visuals["shadow1"]);

            // Verifica se o arquivo binário existe, senão, carrega as texturas e cria ele
            string dat_path = Path.Combine(full_path, "visuals.dat");
            try {
                DataManagement.LoadTexturesFromFile(dat_path, this.textures);
            } catch (Exception e) {
                DataManagement.LoadTexturesFromPath(full_path, this.textures);
                DataManagement.SaveTexturesToFile(dat_path, this.textures);
            }
        }
        public void UnloadTextures() {
            foreach (var image in this.textures.Values)
            {
                image.Dispose(); // Free the memory used by the image
            }
            this.textures.Clear(); // Clear the dictionary
        }
        public void LoadSounds() {
            string currentDirectory = Directory.GetCurrentDirectory();
            string full_sound_path = Path.Combine(currentDirectory, this.folder_path, "sound");

            // Verifica se o diretório existe
            if (!System.IO.Directory.Exists(full_sound_path)) {
                throw new System.IO.DirectoryNotFoundException($"O diretório {full_sound_path} não foi encontrado.");
            }

            // Verifica se o arquivo binário existe, senão, carrega os sons e cria ele
            string dat_path = Path.Combine(full_sound_path, "sounds.dat");
            try {
                DataManagement.LoadSoundsFromFile(dat_path, this.sounds);
            } catch (Exception e) {
                DataManagement.LoadSoundsFromPath(full_sound_path, this.sounds);
                DataManagement.SaveSoundsToFile(dat_path, this.sounds);
            }

            // setta a musica
            this.music = new Sound(sounds["music"]);
        }
        public void UnloadSounds() {
            this.StopMusic();
            foreach (var sound in this.sounds.Values)
            {
                sound.Dispose(); // Free the memory used by the image
            }
            this.sounds.Clear(); 
        }
        public static void LoadThumbs() {
            try {
                DataManagement.LoadTexturesFromFile("Assets/data/stage_thumbs.dat", Program.thumbs);
            } catch (Exception e) {
                var temp_dict = new Dictionary<string, Texture> { };
                foreach (string characterDir in Directory.GetDirectories("Assets/stages")) DataManagement.LoadTexturesFromPath(characterDir, temp_dict);
                DataManagement.SaveTexturesToFile("Assets/data/stage_thumbs.dat", temp_dict);
                foreach (var item in temp_dict) Program.thumbs[item.Key] = item.Value;
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

}