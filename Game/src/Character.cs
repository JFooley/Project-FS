using SFML.Graphics;
using SFML.System;
using SFML.Audio;
using Animation_Space;
using Input_Space;
using Stage_Space;
using UI_space;
using Data_space;
using System.Runtime.InteropServices;

// ----- Default States -------
// Intro
// Idle

// WalkingForward
// WalkingBackward

// JumpForward
// Jump
// JumpBackward
// JumpFalling
// FallingAfter

// Crouching

// LightP
// LightK
// MediumP
// MediumK

// OnHit
// OnHitLow
// OnBlock
// OnBlockLow

// Parry
// AirParry

// Airboned
// Sweeped
// Falling
// OnGround
// Wakeup

namespace Character_Space {
    public class AI {
        public static Random rand = new Random();
        public Queue<string> moveQueue = new Queue<string>();
        public Queue<string> actionQueue = new Queue<string>();
        public FightState[] states = new FightState[5];

        public void EnqueueMove(string key, int frames = 1) {
            for (int i = 0; i < frames; i++) moveQueue.Enqueue(key);
        }
        public void EnqueueAction(string key, int frames) {
            for (int i = 0; i < frames; i++) actionQueue.Enqueue(key);
        }
    }
    public class FightState {
        public float distance;
        public float enemyDistance;
        public bool enemyIsIdle;
        public bool enemyIsOnHit;
        public bool enemyIsBlocking;
        public bool enemyIsAttacking;
        public bool enemyIsAirborne;
        public bool enemyIsCrouching;
        public bool enemyChangedSide;
        public bool enemyIsDead;
        public bool onCorner;
        public string lastState;
    }
    
    public abstract class Character : Object_Space.Object {
        // Consts
        public const int NOTHING = -1;
        public const int BLOCK = 0;
        public const int HIT = 1;
        public const int PARRY = 2;

        // Infos
        public string name;
        public int type;
        public string folder_path;
        public float floor_line;
        public Stage stage => Program.stage;

        // AI
        public AI BOT = new AI();
        public bool BotEnabled = false;
        public bool AIEnabled = false;

        // Controls
        public int player_index { get; set; }

        // Statistics 
        public Vector2i life_points = new Vector2i(1000, 1000);
        public Vector2i dizzy_points = new Vector2i(500, 500);
        public Vector2i aura_points = new Vector2i(0, 100);
        public Vector2f visual_position => new Vector2f(this.body.Position.X - 125, this.body.Position.Y - 250);
        public int move_speed = 0;
        public int dash_speed = 0;
        public int jump_hight = 79;
        public int push_box_width = 25;

        // Object infos
        public string current_state;
        public string last_state;
        private Sprite[] last_sprites = new Sprite[3]; // For tracing

        // Combat logic infos
        public bool not_acting => this.state.not_busy && !this.state.low && !this.state.air && !this.on_air;
        public bool not_acting_low => this.state.not_busy && this.state.low && !this.state.air && !this.on_air;
        public bool not_acting_air => this.state.not_busy && !this.state.low && this.state.air && this.on_air;
        public bool not_acting_all => not_acting || not_acting_air || not_acting_low;

        public bool on_hit => this.state.on_hit;
        public bool on_air => this.body.Position.Y < this.floor_line;
        public bool crounching => this.state.low;

        public bool can_parry => (not_acting_all && parring) || (not_acting_all && InputManager.Key_press("Left", input_window: Config.parry_window, player: this.player_index, facing: this.facing));
        public bool can_dash => not_acting && !this.state.on_parry;
        public bool has_hit = false; 

        public bool blocking_high = false;
        public bool blocking_low = false;
        public bool blocking = false;
        public bool parring = false;

        // Data
        public Dictionary<string, State> states = new Dictionary<string, State>{};
        public abstract Dictionary<string, Texture> textures { get; protected set;}
        public abstract Dictionary<string, SoundBuffer> sounds { get; protected set;}
        private static List<Sound> active_sounds = new List<Sound>();

        // Visuals
        public Texture thumb;
        public Color light_tint => Program.stage.AmbientLight;
        public Color own_light = Color.Transparent;
        public int shadow_size = 1;
        public bool has_frame_change => this.last_anim_frame_index != this.current_anim_frame_index;
        public virtual Texture palette {get; protected set;}
        public uint palette_size => this.palette.Size.X;
        public uint palette_quantity => this.palette.Size.Y;
        public uint palette_index = 0;
        public SFML.Graphics.Color current_palette_color;

        // Gets
        public string current_sprite => current_animation.GetCurrentFrame().Sprite_index;
        public string current_sound => current_animation.GetCurrentFrame().Sound_index;
        public List<GenericBox> current_boxes => current_animation.GetCurrentFrame().Boxes;
        public int current_anim_frame_index => current_animation.anim_frame_index;
        public int current_logic_frame_index => current_animation.logic_frame_index;
        public Animation current_animation => states[current_state].animation;
        public State state => states[current_state];
        public int last_anim_frame_index = -1;

        // Flags and counters
        public int combo_counter = 0;
        public int hitstop_counter = 0;
        public float damage_scaling => Math.Max(0.1f, 1 - combo_counter * 0.1f);
        public bool SA_flag = false;

        public Character(string name, string initialState, float startX, float startY, string folderPath, int type = 0) : base() {
            this.folder_path = folderPath;
            this.name = name;
            this.type = type;
            this.current_state = initialState;
            this.last_state = initialState;
            base.body.Position.X = startX; 
            base.body.Position.Y = startY;
            this.floor_line = startY;
            this.current_palette_color = this.palette != null ? this.palette.CopyToImage().GetPixel(0, this.palette_index) : Color.White;
        }
        public Character(string name, string folder_path) {
            this.name = name;
            this.folder_path = folder_path;
            this.current_palette_color = this.palette != null ? this.palette.CopyToImage().GetPixel(0, this.palette_index) : Color.White;
        }

        // Every Frame methods
        public override void Update() {
            // Render > Behave > Colide > Anima
            base.Update();
            if (this.hitstop_counter <= 0) {
                if (this.animate) this.Animate();
                if (this.behave) {
                    this.DoBehave();
                    this.CheckColisions();
                }
            }
            else this.hitstop_counter -= 1;
        }
        public override void Render(bool drawHitboxes = false) {
            base.Render(drawHitboxes);
            
            // Set current sprite
            var temp_sprite = this.GetCurrentSprite();
            temp_sprite.Position = new Vector2f(this.body.Position.X - (temp_sprite.GetLocalBounds().Width / 2 * this.facing), this.body.Position.Y - temp_sprite.GetLocalBounds().Height);
            temp_sprite.Scale = new Vector2f(this.facing, 1f);

            // Draw tracing
            if (this.state.trace) {
                Program.hueChange.SetUniform("hslInput", new SFML.Graphics.Glsl.Vec3(0.66f, 0.5f, 0.75f));

                for (int i = 0; i < 3; i++) {
                    if (last_sprites[i] != null) Program.window.Draw(last_sprites[i], new RenderStates(Program.hueChange));
                }
                
                if (this.has_frame_change) {               
                    last_sprites[2] = last_sprites[1];
                    last_sprites[1] = last_sprites[0];
                    last_sprites[0] = temp_sprite;
                }
            } else last_sprites = new Sprite[3];

            // Draw current sprite
            if (this.palette != null) {
                var light = this.own_light == Color.Transparent ? this.light_tint : this.own_light;
                Program.window.Draw(temp_sprite, this.SetSwaperShader(light));
            } else {
                Program.window.Draw(temp_sprite);
            }

            // Aplly aura effect
            if (this.state.glow && UI.blink30Hz) {
                Program.hueChange.SetUniform("hslInput", new SFML.Graphics.Glsl.Vec3(0.66f, 0.5f, 0.75f));
                Program.window.Draw(temp_sprite, new RenderStates(shader: Program.hueChange));
            } 

            // Play sounds
            this.PlayFrameSound();
            
            // Draw Hitboxes
            if (drawHitboxes) {  
                RectangleShape anchorY = new RectangleShape(new Vector2f(0, 10)) {
                    Position = new Vector2f(this.body.Position.X, this.body.Position.Y - 60),
                    FillColor = SFML.Graphics.Color.Transparent,
                    OutlineColor = this.current_animation.on_last_frame ? Color.Red : Color.White, 
                    OutlineThickness = 1.0f
                };
                RectangleShape anchorX = new RectangleShape(new Vector2f(10, 0)) {
                    Position = new Vector2f(this.body.Position.X - 5, this.body.Position.Y - 55),
                    FillColor = SFML.Graphics.Color.Transparent,
                    OutlineColor = this.current_animation.on_last_frame ? Color.Red : Color.White, 
                    OutlineThickness = 1.0f 
                };
                
                Program.window.Draw(anchorX);
                Program.window.Draw(anchorY);

                foreach (GenericBox box in this.current_boxes) {
                    // Calcula as coordenadas absolutas da hitbox
                    float x1 = box.getRealA(this).X;
                    float y1 = box.getRealA(this).Y;
                    float x2 = box.getRealB(this).X;
                    float y2 = box.getRealB(this).Y;

                    // Cria o retângulo da hitbox
                    Color color;
                    switch (box.type) {
                        case 0:
                            color = SFML.Graphics.Color.Red;
                            break;
                        case 1:
                            color = SFML.Graphics.Color.Blue;
                            break;
                        case 2:
                            color = SFML.Graphics.Color.White;
                            break;
                        default:
                            color = SFML.Graphics.Color.Green;
                            break;
                    }

                    RectangleShape hitboxRect = new RectangleShape(new Vector2f(x2 - x1, y2 - y1)) {
                        Position = new Vector2f(x1, y1),
                        FillColor = SFML.Graphics.Color.Transparent,
                        OutlineColor = color, 
                        OutlineThickness = 1.0f 
                    };

                    // Desenha o retângulo da hitbox na janela
                    Program.window.Draw(hitboxRect);
                }

                // Draw debug info
                if (Config.debug) {
                    UI.DrawText(this.current_logic_frame_index + "/" + this.current_animation.lenght, this.body.Position.X - Camera.X, this.body.Position.Y - Camera.Y - 145, spacing: Config.spacing_small, alignment: "center", textureName: "default small");
                    UI.DrawText(this.current_anim_frame_index.ToString(), this.body.Position.X - Camera.X, this.body.Position.Y - Camera.Y - 135, spacing: Config.spacing_small, alignment: "center", textureName: "default small");
                    UI.DrawText(this.current_state, this.body.Position.X - Camera.X, this.body.Position.Y - Camera.Y - 125, spacing: Config.spacing_small, alignment: "center", textureName: "default small");
                    UI.DrawText(this.state.not_busy ? "waiting" : "busy", this.body.Position.X - Camera.X, this.body.Position.Y - Camera.Y - 115, spacing: Config.spacing_small, alignment: "center", textureName: "default small");
                }
            }
        }
        public override void Animate() {   
            // Update body.Position
            this.body.Update(this);
            this.body.Position.X += current_animation.GetCurrentFrame().DeltaX * this.facing;
            this.body.Position.Y += current_animation.GetCurrentFrame().DeltaY * this.facing;

            // Advance to the next frame and reset hit if necessary
            this.last_anim_frame_index = this.current_anim_frame_index;
            if (current_animation.AdvanceFrame() && current_animation.GetCurrentFrame().hasHit == false) this.has_hit = false;

            // Change state, if necessary
            if ((state.change_on_end && this.current_animation.ended) || (state.change_on_ground && !this.on_air)) {
                this.ChangeState(this.state.post_state);
            }
        }
        public void Bot() {
            if (!this.BotEnabled || !this.behave) return;

            // Realiza as ações programadas
            if (this.BOT.moveQueue.Count > 0)
                InputManager.SetKey(this.BOT.moveQueue.Dequeue(), player: this.player_index, facing: this.facing);
            if (this.BOT.actionQueue.Count > 0)
                InputManager.SetKey(this.BOT.actionQueue.Dequeue(), player: this.player_index, facing: this.facing);

            // IA do bot
            if (this.AIEnabled) {
                var enemy = Program.stage.character_A.player_index == this.player_index ? Program.stage.character_B : Program.stage.character_A;
                
                var AIstate = new FightState();

                // Distância até o inimigo
                AIstate.distance = Math.Abs(this.body.Position.X - enemy.body.Position.X);
                AIstate.enemyDistance = AIstate.distance / Config.RenderWidth;

                // Estado
                AIstate.lastState = this.last_state;
                AIstate.enemyIsIdle = enemy.state.not_busy;
                AIstate.enemyIsAttacking = enemy.state.can_harm && !enemy.state.not_busy;
                AIstate.enemyIsAirborne = enemy.state.air;
                AIstate.enemyIsCrouching = enemy.state.low;
                AIstate.enemyIsBlocking = enemy.state.on_block;
                AIstate.enemyIsOnHit = enemy.state.on_hit || enemy.state.on_parry;
                AIstate.enemyChangedSide = enemy.facing == this.facing;
                AIstate.enemyIsDead = enemy.life_points.X == 0;
                AIstate.onCorner = this.body.Position.X < (0.2f * Config.RenderWidth) || this.body.Position.X >= (Program.stage.length - (0.2f * Config.RenderWidth));
                
                // Atualiza o array de estados de luta
                this.BOT.states[4] = this.BOT.states[3];
                this.BOT.states[3] = this.BOT.states[2];
                this.BOT.states[2] = this.BOT.states[1];
                this.BOT.states[1] = this.BOT.states[0];
                this.BOT.states[0] = AIstate;

                // Seleciona as ações, caso já tenha realizado todas
                if (this.BOT.moveQueue.Count == 0 && AIstate != null)
                    SelectMovement(AIstate);
                if (this.BOT.actionQueue.Count == 0 && AIstate != null)
                    SelectAction(AIstate);
            }
        }
        public virtual void SelectMovement(FightState state) {}
        public virtual void SelectAction(FightState state) {}

        // Battle methods
        public virtual int ImposeBehavior(Character target) {
            return -1;
        }
        public virtual int DefineColisionType(Character target) {
            if (target.can_parry && this.state.can_be_parried) return Character.PARRY;
            else return this.ImposeBehavior(target);
        }
        public bool isBlocking() {
            return this.isBlockingHigh() || this.isBlockingLow();
        }
        public bool isBlockingHigh() {
            if ((this.not_acting_all || this.state.on_block) && (this.blocking_high || this.blocking)) return true;
            return (this.not_acting || (this.state.on_block && !this.state.low)) && InputManager.Key_hold("Left", player: this.player_index, facing: this.facing) && !InputManager.Key_hold("Down", player: this.player_index, facing: this.facing);
        }
        public bool isBlockingLow() {
            if ((this.not_acting_all || this.state.on_block) && (this.blocking_low || this.blocking)) return true;
            return (this.not_acting_low || (this.state.on_block && this.state.low)) && InputManager.Key_hold("Left", player: this.player_index, facing: this.facing) && InputManager.Key_hold("Down", player: this.player_index);
        }
        public void Stun(Character enemy, int advantage, bool hit = true, bool airbone = false, bool sweep = false, bool force_crounch = false, bool force_stand = false, bool force = false) {
            if (hit || this.life_points.X == 0) { // Hit stun states
                if (sweep) {
                    this.ChangeState("Sweeped", reset: true);
                    return;

                } else if (airbone || (this.life_points.X <= 0 && !Program.stage.MustWait())|| this.current_state == "Airboned") {
                    this.ChangeState("Airboned", reset: true);

                    if (this.life_points.X <= 0) this.SetVelocity(X: -Config.heavy_pushback, Y: 50);
                    return;

                } else if ((this.crounching && !force_stand) || force_crounch) {
                    this.ChangeState("OnHitLow", reset: true);

                } else {
                    this.ChangeState("OnHit", reset: true);
                }

                this.facing = -enemy.facing;
                this.body.Velocity.X = Math.Abs(this.body.Velocity.X) * enemy.facing;

            } else { // Block stun states
                this.facing = -enemy.facing;
                if (this.crounching) this.ChangeState("OnBlockLow", reset: true);
                    else this.ChangeState("OnBlock", reset: true);
            }
        
            // Set stun frames
            if (force) {
                this.current_animation.lenght = Math.Max(advantage, 1);
            } else {
                this.current_animation.lenght = Math.Max(enemy.current_animation.lenght - enemy.current_logic_frame_index + advantage, 1);
                
            }
        }
        public void BlockStun(Character enemy, int advantage, bool force = false) {
            this.Stun(enemy, advantage, hit: false, force: force);
        }
        public void CheckColisions() {               
            // Para cada character no stage
            foreach (var charB in Program.stage.OnSceneCharacters) {
                if (charB == this) continue;
                
                foreach (GenericBox boxA in this.current_boxes) {
                    if (boxA.type != GenericBox.HITBOX && boxA.type != GenericBox.PUSHBOX && boxA.type != GenericBox.GRABBOX) continue;
                    
                    foreach (GenericBox boxB in charB.current_boxes) {
                        if (boxB.type == GenericBox.HURTBOX && boxB.type == GenericBox.PUSHBOX) continue;
                        
                        if (GenericBox.Intersects(boxA, boxB, this, charB)) {
                            if (boxA.type == GenericBox.PUSHBOX && boxB.type == GenericBox.PUSHBOX) {
                                // A body push B
                                GenericBox.Colide(boxA, boxB, this, charB);

                            } else if (this.player_index != charB.player_index && this.has_hit == false && charB.state.can_be_hit && this.type >= charB.type && (boxA.type == GenericBox.HITBOX || boxA.type == GenericBox.GRABBOX) && boxB.type == GenericBox.HURTBOX) { 
                                // A hit B
                                this.has_hit = true;
                                
                                int hit_type = DefineColisionType(charB);

                                if (hit_type == Character.NOTHING) continue;
                                if (hit_type == Character.PARRY) {
                                    charB.ChangeState("Parry", reset: true);
                                    charB.aura_points.X = Math.Min(charB.aura_points.Y, charB.aura_points.X + 10);
                                }
                                
                                stage.Hitstop(this.state.hitstop, hit_type: hit_type, character: charB);
                                
                                if (this.player_index == 1) stage.character_A.combo_counter += hit_type == Character.HIT ? 1 : 0; 
                                else stage.character_B.combo_counter += hit_type == Character.HIT ? 1 : 0;

                                stage.spawnHitspark(hit_type, boxA.getRealB(this).X - (boxA.width * 1/3), (boxA.getRealA(this).Y + boxA.getRealB(this).Y) / 2 + 125, this.facing);
                            }
                        }
                    }
                }
            }
        }

        // Static Methods 
        public static void Push(Character target, Character self, string amount, float X_amount = 0, float Y_amount = 0, bool airbone = false, bool force_push = false) {
            if ((target.body.Position.X <= Camera.X - Config.corner_limit || target.body.Position.X >= Camera.X + Config.corner_limit) && !force_push) {
                if (X_amount != 0) {
                    self.SetVelocity(X: self.facing * target.facing * X_amount, keep_Y: true);
                    target.SetVelocity(X: self.facing * target.facing * X_amount, Y: (target.on_air || airbone) ? Y_amount : 0);
                } else if (amount == "Light") {
                    self.SetVelocity(X: self.facing * target.facing * Config.light_pushback, keep_Y: true);
                    target.SetVelocity(X: self.facing * target.facing * Config.light_pushback, Y: (target.on_air || airbone) ? Y_amount : 0);
                } else if (amount == "Medium") {
                    self.SetVelocity(X: self.facing * target.facing * Config.medium_pushback, keep_Y: true);
                    target.SetVelocity(X: self.facing * target.facing * Config.medium_pushback, Y: (target.on_air || airbone) ? Y_amount : 0);
                } else if (amount == "Heavy"){
                    self.SetVelocity(X: self.facing * target.facing * Config.heavy_pushback, keep_Y: true);
                    target.SetVelocity(X: self.facing * target.facing * Config.heavy_pushback, Y: (target.on_air || airbone) ? Y_amount : 0);
                }
            } else {
                if (X_amount != 0) {
                    target.SetVelocity(X: self.facing * target.facing * X_amount, Y: (target.on_air || airbone) ? Y_amount : 0);
                } else if (amount == "Light") {
                    target.SetVelocity(X: self.facing * target.facing * Config.light_pushback, Y: (target.on_air || airbone) ? Y_amount : 0);
                } else if (amount == "Medium") {
                    target.SetVelocity(X: self.facing * target.facing * Config.medium_pushback, Y: (target.on_air || airbone) ? Y_amount : 0);
                } else if (amount == "Heavy"){
                    target.SetVelocity(X: self.facing * target.facing * Config.heavy_pushback, Y: (target.on_air || airbone) ? Y_amount : 0);
                }
            }
        }
        public static void Damage(Character target, Character self, int damage, int dizzy_damage) {
            target.life_points.X = (int) Math.Max(target.life_points.X - damage * self.damage_scaling, 0);
            target.dizzy_points.X = (int) Math.Max(target.dizzy_points.X - dizzy_damage * self.damage_scaling, 0);
        }
        public static void AddAuraPoints(Character target, Character self, int hit , int target_amount = 3, int self_amount = 10) {
            target.aura_points.X = (int) Math.Min(target.aura_points.Y, target.aura_points.X + target_amount);
            self.aura_points.X = (int) Math.Min(self.aura_points.Y, hit == 1 ? self.aura_points.X + self_amount : self.aura_points.X + (self_amount / 3));
        }
        public static bool CheckAuraPoints(Character target, int amount) {
            return target.aura_points.X >= amount;
        }
        public static void UseSuperPoints(Character target, int amount) {
            target.aura_points.X = (int) Math.Max(0, target.aura_points.X - amount);
        }
        
        // Physics
        public void SetVelocity(float X = 0, float Y = 0, bool raw_set = false, bool keep_X = false, bool keep_Y = false) {
            this.body.SetVelocity(this, X, Y, raw_set: raw_set, keep_X: keep_X, keep_Y: keep_Y);
        }
        public void AddVelocity(float X = 0, float Y = 0, bool raw_set = false) {
            this.body.AddVelocity(this, X, Y, raw_set: raw_set);
        }
        public void SetForce(float X = 0, float Y = 0, int T = 0, bool keep_X = false, bool keep_Y = false) {
            this.body.SetForce(this, X, Y, T, keep_X: keep_X, keep_Y: keep_Y);
        }
        public void AddForce(float X = 0, float Y = 0, int T = 0) {
            this.body.AddForce(this, X, Y, T);
        }
        
        // Auxiliar methods
        public void ChangeState(string new_state, bool reset = false, int variation = 0) {
            if (this.life_points.X <= 0 && !Program.stage.training_mode && this.current_state == "OnGround" && !reset) return;

            if (new_state == "Parry" && this.on_air) new_state = "AirParry";

            this.last_state = this.current_state;
            if (states.ContainsKey(new_state)) {
                if (current_state != new_state || reset) this.current_animation.Reset();
                this.current_state = new_state;
                this.has_hit = false;
            }
        }
        public Sprite GetCurrentSprite() {
            if (textures.TryGetValue(this.current_sprite, out Texture texture)) {
                return new Sprite(texture);
            }
            return new Sprite(); 
        }
        public void PlayFrameSound() {
            if (!this.current_animation.playing_sound && this.current_sound != "" && sounds.TryGetValue(this.current_sound, out SoundBuffer buffer)) {
                var temp_sound = new Sound(buffer) {Volume = Config.Character_Volume};
                temp_sound.Play();
                active_sounds.Add(temp_sound);
                active_sounds.RemoveAll(s => s.Status == SoundStatus.Stopped);
                this.current_animation.playing_sound = true;
            }
        }
        public void PlaySound(string sound_name) {
            if (sounds.TryGetValue(sound_name, out SoundBuffer buffer)) {
                var temp_sound = new Sound(buffer) {Volume = Config.Character_Volume};
                temp_sound.Play();
                active_sounds.Add(temp_sound);
                active_sounds.RemoveAll(s => s.Status == SoundStatus.Stopped);
                this.current_animation.playing_sound = true;
            };
        }
        public void Reset(int start_point, int facing, String state = "Idle", bool total_reset = false) {
            this.ChangeState(state, reset: true);
            this.life_points.X = this.life_points.Y;
            this.dizzy_points.X = total_reset ? dizzy_points.Y : this.dizzy_points.X;
            this.aura_points.X = total_reset ? 0 : this.aura_points.X;
            this.body.Position.X = start_point;
            this.body.Position.Y = this.floor_line;
            this.body.SetVelocity(this, 0, 0, raw_set: true);
            this.facing = facing;
        }
        public static RenderStates SetSwaperShader(Texture palette, uint palette_size, uint palette_quantity, uint palette_index, Color light) {
            Program.paletteSwaper.SetUniform("palette", palette);
            Program.paletteSwaper.SetUniform("palette_size", palette_size);
            Program.paletteSwaper.SetUniform("palette_quantity", palette_quantity);
            Program.paletteSwaper.SetUniform("palette_index", palette_index);
            Program.paletteSwaper.SetUniform("light", new Vector3f(light.R / 255f, light.G / 255f, light.B / 255f));
            return new RenderStates(Program.paletteSwaper);
        }
        public RenderStates SetSwaperShader(Color light, int palette_index = -1) {
            Program.paletteSwaper.SetUniform("palette", this.palette);
            Program.paletteSwaper.SetUniform("palette_size", this.palette_size);
            Program.paletteSwaper.SetUniform("palette_quantity", this.palette_quantity);
            Program.paletteSwaper.SetUniform("palette_index", palette_index == -1 ? this.palette_index : (uint) palette_index);
            Program.paletteSwaper.SetUniform("light", new Vector3f(light.R / 255f, light.G / 255f, light.B / 255f));
            return new RenderStates(Program.paletteSwaper);
        }
        
        // Loads
        public void LoadTextures(bool do_index = false) {
            string currentDirectory = Directory.GetCurrentDirectory();
            string full_path = Path.Combine(currentDirectory, this.folder_path, "sprites");

            if (!System.IO.Directory.Exists(full_path)) {
                throw new System.IO.DirectoryNotFoundException($"O diretório {full_path} não foi encontrado.");
            }

            string dat_path = Path.Combine(full_path, "visuals.dat");
            try {
                DataManagement.LoadTexturesFromFile(dat_path, this.textures);
            } catch (Exception e) {
                DataManagement.LoadTexturesFromPath(full_path, this.textures);
                if (do_index) DataManagement.IndexTextureColors(this.textures, this.palette);
                DataManagement.SaveTexturesToFile(dat_path, this.textures);
            }


        }
        public void UnloadTextures() {
            foreach (var image in textures.Values)
            {
                image.Dispose(); // Free the memory used by the image
            }
            textures.Clear(); // Clear the dictionary
        }
        public void LoadSounds() {
            string currentDirectory = Directory.GetCurrentDirectory();
            string full_sound_path = Path.Combine(currentDirectory, this.folder_path, "sounds");

            if (!System.IO.Directory.Exists(full_sound_path)) {
                throw new System.IO.DirectoryNotFoundException($"O diretório {full_sound_path} não foi encontrado.");
            }

            string dat_path = Path.Combine(full_sound_path, "sounds.dat");
            try {
                DataManagement.LoadSoundsFromFile(dat_path, this.sounds);
            } catch (Exception e) {
                DataManagement.LoadSoundsFromPath(full_sound_path, this.sounds);
                DataManagement.SaveSoundsToFile(dat_path, this.sounds);
            }
        }
        public void UnloadSounds() {
            foreach (var sound in sounds.Values)
            {
                sound.Dispose(); // Free the memory used by the image
            }
            sounds.Clear(); 
        }
        public void LoadPalette() {
            string currentDirectory = Directory.GetCurrentDirectory();
            this.palette = new Texture(Path.Combine(currentDirectory, this.folder_path, "palette.bmp"));
        }
        public void SetThumb() {
            try {
                this.textures.TryGetValue("thumb", out Texture thumb);
                this.thumb = thumb ;
            } catch (Exception) {
                this.thumb = new Texture(1,1);
            }
        }
        public void ChangePalette(int add_to_index = 0) {
            var temp_index = (int) (this.palette_index + add_to_index) % this.palette_quantity;

            if (temp_index < 0) temp_index += this.palette_quantity;

            this.palette_index = (uint) temp_index;

            this.current_palette_color = this.palette.CopyToImage().GetPixel(0, this.palette_index);
        }
        public void UpdatePalette() {
            this.current_palette_color = this.palette.CopyToImage().GetPixel(0, this.palette_index);
        }
        
        // General Load
        public virtual Character Copy() {
            return null;
        }
        public virtual void Load() { }
        public override void Unload() {
            this.UnloadSounds();
            this.UnloadTextures();
        }
    }

}
