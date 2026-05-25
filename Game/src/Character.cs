using SFML.Graphics;
using SFML.System;
using SFML.Audio;

using UI_space;

// ----- Default States -------
// Intro
// Win

// Idle
// WalkingForward
// WalkingBackward

// Jump
// JumpForward
// JumpBackward
// JumpFalling
// Landing

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

public class AI {
    public static Random rand = new Random();
    public int difficulty = 0; // lower = harder
    public Queue<string> moveQueue = new Queue<string>();
    public Queue<string> actionQueue = new Queue<string>();

    public void EnqueueMove(string key, int frames = 1) {
        for (int i = 0; i < frames; i++) moveQueue.Enqueue(key);
    }
    public void EnqueueAction(string key, int frames) {
        for (int i = 0; i < frames; i++) actionQueue.Enqueue(key);
    }
}
public struct FightState {
    public float enemyDistance;
    public bool enemyIsIdle;
    public bool enemyIsOnHit;
    public bool enemyIsBlocking;
    public bool enemyIsAttacking;
    public bool enemyIsGrabbing;
    public bool enemyIsAirborne;
    public bool enemyIsCrouching;
    public bool enemyChangedSide;
    public bool enemyIsDead;
    public bool enemyOnCorner;
    public bool onCorner;
    public string lastState;
}

public abstract class Character : Object {
    protected static Frame[][] F(params Frame[][] frames) => frames;

    // Consts
    public const int NOTHING = -1;
    public const int BLOCK = 0;
    public const int HIT = 1;
    public const int PARRY = 2;
    public const int GRAB = 3;
    public const int TECH = 4;

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
    public Vector2f visual_position => new Vector2f(this.body.position.X - 125, this.body.position.Y - 250);
    public Vector2f visual_center => new Vector2f(this.body.position.X, this.body.position.Y - 125);
    public int jump_hight = 100;
    public int push_box_width = 25;

    // State info
    public string current_state;

    public string last_state;
    public Vector2f last_position;
    private Sprite[] last_sprites = new Sprite[3]; 

    public string? next_state;
    public bool next_state_reset;
    public int? next_state_lenght;
    
    public int next_frame_hitstop = 0;

    // Combat logic infos
    public bool not_acting => this.state.not_busy && !this.state.low && !this.state.air && !this.on_air;
    public bool not_acting_low => this.state.not_busy && this.state.low && !this.state.air && !this.on_air;
    public bool not_acting_air => this.state.not_busy && !this.state.low && this.state.air && this.on_air;
    public bool not_acting_all => not_acting || not_acting_air || not_acting_low;

    public bool on_parry => this.state.on_parry;
    public bool on_block => this.state.on_block;
    public bool on_hit => this.state.on_hit;
    public bool on_air => this.body.position.Y < this.floor_line;
    public bool crounching => this.state.low;

    public bool can_parry => (not_acting_all && parring) || (not_acting_all && Input.Key_press("Left", input_window: this.state.air? Config.parry_window/2 : Config.parry_window, player: this.player_index, facing: this.facing));
    public bool can_dash => not_acting && !this.state.on_parry;
    public bool can_jump => not_acting && this.current_state != "Landing";
    public bool has_hit = false;

    public bool blocking_high = false;
    public bool blocking_low = false;
    public bool blocking = false;
    public bool parring = false;

    // Data
    public Dictionary<string, State> states = new Dictionary<string, State>{};
    public abstract Dictionary<string, Texture> textures { get; protected set;}
    public abstract Dictionary<string, SoundBuffer> sounds { get; protected set;}
    public abstract Dictionary<string, Frame[]> animations { get; protected set;}
    private static List<Sound> active_sounds = new List<Sound>();

    // Visuals
    public Texture thumb;
    public Color light_tint => Program.stage.AmbientLight;
    public Color own_light = Color.Transparent;

    public int shadow_size = 2;
    public bool has_frame_change = false;

    public virtual Texture palette {get; protected set;}
    public uint palette_size => this.palette.Size.X;
    public uint palette_quantity => this.palette.Size.Y;
    public uint palette_index = 0;
    public Color current_palette_color;

    // Gets
    public string current_sprite => current_animation.GetCurrentFrame().sprite_index;
    public string current_sound => current_animation.GetCurrentFrame().sound_index;
    public List<GenericBox> current_boxes => current_animation.GetCurrentFrame().Boxes;
    public int current_anim_frame_index => current_animation.anim_frame_index;
    public int current_logic_frame_index => current_animation.logic_frame_index;
    public Animation current_animation => states[current_state].animation;
    public State state => states[current_state];

    // Flags and counters
    public int combo_counter = 0;
    public int hitstop_counter = 0;
    public float damage_scaling => Math.Max(0.1f, 1 - combo_counter * 0.1f);

    public Character(string name, string initialState, float startX, float startY, string folderPath, int type = 0) : base() {
        this.folder_path = folderPath;
        this.name = name;
        this.type = type;
        this.last_state = initialState;
        base.body.position.X = startX; 
        base.body.position.Y = startY;
        this.floor_line = startY;
        this.current_palette_color = this.palette != null ? this.palette.CopyToImage().GetPixel(new Vector2u(0, this.palette_index)) : Color.White;
        this.current_state = initialState;
        this.ChangeState(initialState, reset: true);
    }
    public Character(string name, string initialState, string folder_path) {
        this.name = name;
        this.folder_path = folder_path;
        this.current_palette_color = this.palette != null ? this.palette.CopyToImage().GetPixel(new Vector2u(0, this.palette_index)) : Color.White;
        this.current_state = initialState;
        this.ChangeState(initialState, reset: true);
    }

    // Every Frame methods
    public override void Update() { // Animate > Render > Bot > Behave > Colide > Advance Frame > Update state
        if (this.hitstop_counter <= 0) {
            this.Bot();
            this.Behave();
            this.CheckColisions();
        }
    }
    public override void Render(bool drawHitboxes = false) {
        base.Render(drawHitboxes);

        // Play sounds
        this.PlayFrameSound();

        // Set current sprite
        var temp_sprite = this.GetCurrentSprite();
        temp_sprite.Position = new Vector2f(this.body.position.X - (temp_sprite.GetLocalBounds().Width / 2 * this.facing), this.body.position.Y - temp_sprite.GetLocalBounds().Height);
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
        if (this.state.glow && UI.blink30Hz) {
            Program.hueChange.SetUniform("hslInput", new SFML.Graphics.Glsl.Vec3(0.66f, 0.5f, 0.75f));
            Program.window.Draw(temp_sprite, new RenderStates(shader: Program.hueChange));
        } else if (Accessibility.high_contrast) {
            Program.window.Draw(temp_sprite, this.SetHighContrastShader(this.current_palette_color));
        } else if (this.palette != null) {
            Program.window.Draw(temp_sprite, this.SetSwaperShader(this.own_light == Color.Transparent ? this.light_tint : this.own_light));
        } else {
            Program.window.Draw(temp_sprite);
        }


        // Draw Hitboxes
        if (drawHitboxes) {  
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
        }
    }
    public override void Animate() {
        if (this.hitstop_counter > 0 || !this.animate) return;

        this.last_position = this.body.position;

        // Physics
        this.body.Update(this);

        // Baked animation
        this.body.position.X += current_animation.GetCurrentFrame().delta_X * this.facing;
        this.body.position.Y += current_animation.GetCurrentFrame().delta_Y;
    }
    public void UpdateState() {
        if (!animate) return;

        // If it's in hitstop or have hitstop to set
        if (this.hitstop_counter > 0) {
            this.hitstop_counter--;
        } else if (this.next_frame_hitstop > 0) {
            this.hitstop_counter = this.next_frame_hitstop;
            this.next_frame_hitstop = 0;
        }

        // If there's a change of state
        if (this.next_state != null) {
            if (this.life_points.X <= 0 && !Stage.training_mode && this.current_state == "OnGround" && !this.next_state_reset) return;

            this.last_state = this.current_state;
            if (states.ContainsKey(this.next_state)) {
                this.has_hit = false;
                this.current_state = this.next_state;
                if (this.current_state != this.last_state || this.next_state_reset) this.current_animation.Reset();
                if (this.next_state_lenght != null) this.current_animation.lenght = this.next_state_lenght.Value;
                if (this.state.variation_amount > 1) this.state.variation_index = AI.rand.Next(0, this.state.variation_amount);
            }

            this.next_state = null;
            this.next_state_reset = false;
            this.next_state_lenght = null;
        }
    }
    public void AdvanceFrame() {
        if (this.hitstop_counter > 0 || this.next_state != null || !this.animate) return;

        // Advance to the next frame and reset hit if necessary
        this.has_frame_change = current_animation.AdvanceFrame();
        if (this.has_frame_change) {
            this.facing *= current_animation.GetCurrentFrame().facing;
            if (current_animation.GetCurrentFrame().has_hit == false) this.has_hit = false;
        }

        if ((state.change_on_end && this.current_animation.ended) || (state.change_on_ground && !this.on_air)) {
            this.ChangeState(this.state.post_state);
        }
    }
    public void Bot() {
        if (!this.BotEnabled || !this.behave) return;

        // Realiza as ações programadas
        if (this.BOT.moveQueue.Count > 0)
            Input.SetKey(this.BOT.moveQueue.Dequeue(), player: this.player_index, facing: this.facing);
        if (this.BOT.actionQueue.Count > 0)
            Input.SetKey(this.BOT.actionQueue.Dequeue(), player: this.player_index, facing: this.facing);

        // IA do bot
        if (this.AIEnabled) {
            var enemy = Program.stage?.character_A.player_index == this.player_index ? Program.stage.character_B : Program.stage?.character_A;

            var AIstate = new FightState {
                enemyDistance = Math.Abs(this.body.position.X - enemy.body.position.X) / Config.RenderWidth,
                lastState = this.last_state,
                enemyIsIdle = enemy.state.not_busy,
                enemyIsAttacking = enemy.state.will_hit && enemy.state.busy,
                enemyIsGrabbing = enemy.state.is_grab,
                enemyIsAirborne = enemy.state.air,
                enemyIsCrouching = enemy.state.low,
                enemyIsBlocking = enemy.state.on_block,
                enemyIsOnHit = enemy.state.on_hit || enemy.state.on_parry,
                enemyChangedSide = enemy.facing == this.facing,
                enemyIsDead = enemy.life_points.X == 0,
                enemyOnCorner = enemy.body.position.X < (0.2f * Config.RenderWidth) || enemy.body.position.X >= (Program.stage?.length - (0.2f * Config.RenderWidth)),
                onCorner = this.body.position.X < (0.2f * Config.RenderWidth) || this.body.position.X >= (Program.stage?.length - (0.2f * Config.RenderWidth)) 
            };

            // Seleciona as ações, caso já tenha realizado todas
            if (this.BOT.moveQueue.Count == 0)
                SelectMovement(AIstate);
            if (this.BOT.actionQueue.Count == 0)
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
        else if (target.state.is_grab && this.state.is_grab) return Character.TECH;
        else return this.ImposeBehavior(target);
    }
    public bool isBlocking() {
        return this.isBlockingHigh() || this.isBlockingLow();
    }
    public bool isBlockingHigh() {
        if ((this.not_acting_all || this.state.on_block) && (this.blocking_high || this.blocking)) return true;
        return (this.not_acting || (this.state.on_block && !this.state.low)) && Input.Key_hold("Left", player: this.player_index, facing: this.facing) && !Input.Key_hold("Down", player: this.player_index, facing: this.facing);
    }
    public bool isBlockingLow() {
        if ((this.not_acting_all || this.state.on_block) && (this.blocking_low || this.blocking)) return true;
        return (this.not_acting_low || (this.state.on_block && this.state.low)) && Input.Key_hold("Left", player: this.player_index, facing: this.facing) && Input.Key_hold("Down", player: this.player_index);
    }
    public void Stun(Character enemy, int advantage, bool hit = true, bool airbone = false, bool sweep = false, bool force_crounch = false, bool force_stand = false, bool raw_value = false) {
        this.facing = this.GetFacingTo(enemy);
        int lenght = raw_value ? advantage : enemy.current_animation.lenght - enemy.current_logic_frame_index + advantage;

        if (hit || this.life_points.X <= 0) { // Hit stun states
            if (sweep) {
                this.ChangeState("Sweeped", reset: true);
                return;

            } else if (airbone || (this.life_points.X <= 0 && !Program.stage.MustWait()) || (this.state.air && this.state.on_hit)) {
                this.ChangeState("Airboned", reset: true);
                if (this.life_points.X <= 0) this.SetVelocity(X: -Config.heavy_pushback, Y: 50);
                return;

            } else if ((this.crounching && !force_stand) || force_crounch) {
                this.ChangeState("OnHitLow", reset: true, lenght: lenght);
                
            } else {
                this.ChangeState("OnHit", reset: true, lenght: lenght);
            }

        } else { // Block stun states
            if (this.crounching) this.ChangeState("OnBlockLow", reset: true, lenght: lenght);
            else this.ChangeState("OnBlock", reset: true, lenght: lenght);
        }
    }
    public void BlockStun(Character enemy, int advantage, bool raw_value = false) {
        this.Stun(enemy, advantage, hit: false, raw_value: raw_value);
    }
    public void CheckColisions() {   
        foreach (var enemy in Program.stage.OnSceneCharacters) {
            if (enemy == this) continue;
            
            foreach (GenericBox boxA in this.current_boxes) {
                if (boxA.type != GenericBox.HITBOX && boxA.type != GenericBox.PUSHBOX) continue;
                
                foreach (GenericBox boxB in enemy.current_boxes) {
                    if ((boxA.type == GenericBox.HITBOX && boxB.type != GenericBox.HURTBOX) || (boxA.type == GenericBox.PUSHBOX && boxB.type != GenericBox.PUSHBOX)) continue;
                    
                    if (GenericBox.Intersects(boxA, boxB, this, enemy)) {
                        // A push B
                        if (boxA.type == GenericBox.PUSHBOX && boxB.type == GenericBox.PUSHBOX) {
                            GenericBox.Colide(boxA, boxB, this, enemy);

                        // A hit B
                        } else if (this.player_index != enemy.player_index && this.has_hit == false && enemy.state.can_be_hit && this.type >= enemy.type && boxA.type == GenericBox.HITBOX && boxB.type == GenericBox.HURTBOX) { 
                            this.has_hit = true;
                            
                            int hit_type = DefineColisionType(enemy);

                            if (hit_type == Character.NOTHING) {
                                continue;
                                
                            } else if (hit_type == Character.PARRY) {
                                enemy.ChangeState(enemy.on_air ? "AirParry" : "Parry", reset: true, lenght: Config.parry_lenght);
                                enemy.aura_points.X = Math.Min(enemy.aura_points.Y, enemy.aura_points.X + 10);
                                
                            } else if (hit_type == Character.TECH) {
                                enemy.facing = enemy.GetFacingTo(this);
                                this.AddAcceleration(-3);
                                enemy.AddAcceleration(-3);
                            }
                            
                            stage.Hitstop(this.state.hitstop, hit_type: hit_type, on_hit_char: enemy, hitting_char: this);
                            
                            if (this.player_index == 1) stage.character_A.combo_counter += hit_type == Character.HIT ? 1 : 0; 
                            else stage.character_B.combo_counter += hit_type == Character.HIT ? 1 : 0;

                            stage.PSHitspark(hit_type, boxA.getRealB(this).X - (boxA.width * 1/3), (boxA.getRealA(this).Y + boxA.getRealB(this).Y) / 2 + 125, this.GetFacingTo(enemy), weight: this.state.hitstop);
                            return;
                        }
                    }
                }
            }
        }
    }

    // Static Methods 
    public static void Push(Character target, Character self, float X_amount, float Y_amount = 0, bool airbone = false, bool fixed_height = false, bool force_push = false) {
        if ((target.body.position.X <= Camera.X - Config.corner_limit || target.body.position.X >= Camera.X + Config.corner_limit) && !force_push) {
            self.SetVelocity(X: -X_amount, keep_Y: true);
            target.SetVelocity(X: -X_amount, Y: (target.on_air || airbone) ? Y_amount : 0, fixed_height: fixed_height);
        } else {
            target.SetVelocity(X: -X_amount, Y: (target.on_air || airbone) ? Y_amount : 0, fixed_height: fixed_height);
        }
    }
    public static void Damage(Character target, Character self, int damage, int dizzy_damage) {
        // NOTE: Before stun and blockstun
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
    public static void UseAuraPoints(Character target, int amount) {
        target.aura_points.X = (int) Math.Max(0, target.aura_points.X - amount);
    }
    
    // Physics
    public void SetVelocity(float X = 0, float Y = 0, bool raw_set = false, bool fixed_height = false, bool keep_X = false, bool keep_Y = false) {
        this.body.SetVelocity(X, Y, raw_set: raw_set, fixed_height: fixed_height, keep_X: keep_X, keep_Y: keep_Y);
    }
    public void AddVelocity(float X = 0, float Y = 0, bool raw_set = false, bool fixed_height = false) {
        this.body.AddVelocity(X, Y, raw_set: raw_set, fixed_height: fixed_height);
    }
    public void SetAcceleration(float X = 0, float Y = 0, bool keep_X = false, bool keep_Y = false, bool raw_set = false) {
        this.body.SetAcceleration(X, Y, keep_X: keep_X, keep_Y: keep_Y, raw_set: raw_set);
    }
    public void AddAcceleration(float X = 0, float Y = 0) {
        this.body.AddAcceleration(X, Y);
    }
    
    // Auxiliar methods
    public void ChangeState(string new_state, bool reset = false, int? lenght = null) {
        this.next_state = new_state;
        this.next_state_reset = reset;
        this.next_state_lenght = lenght;
    }
    public Sprite GetCurrentSprite() {
        if (textures.TryGetValue(this.current_sprite, out Texture texture)) {
            return new Sprite(texture);
        }
        return null; 
    }
    public void PlayFrameSound() {
        if (!this.current_animation.playing_sound && this.current_sound != "") this.PlaySound(this.current_sound, follow_player: true);
    }
    public void PlaySound(string sound_string, float panning = 0, bool follow_player = true) {
        string[] sound = sound_string.Split(' ');
        if (sounds.TryGetValue(sound[0], out SoundBuffer buffer)) {
            var temp_sound = new Sound(buffer) {
                Volume = Accessibility.distance_cue ? Config.Character_Volume * 0.7f : Config.Character_Volume,
                Pan = follow_player && Accessibility.spacialized_audio? (this.body.position.X - Camera.X) / (Config.RenderWidth*0.5f) : panning,
                Pitch = sound_string.Contains("--RP") ? 1 + (float) AI.rand.Next(-1, 2) / 10 : 1,
            };
            temp_sound.Play();
            active_sounds.Add(temp_sound);
            active_sounds.RemoveAll(s => s.Status == SoundStatus.Stopped);
            this.current_animation.playing_sound = true;
        };
    }
    public virtual void Reset(int start_point, int facing, String state = "Idle", bool total_reset = false) {
        this.ChangeState(state, reset: true);
        this.life_points.X = this.life_points.Y;
        this.dizzy_points.X = total_reset ? dizzy_points.Y : this.dizzy_points.X;
        this.aura_points.X = total_reset ? 0 : this.aura_points.X;
        this.body.position.X = start_point;
        this.body.position.Y = this.floor_line;
        this.body.SetVelocity(0, 0, raw_set: true);
        this.facing = facing;
    }
    public RenderStates SetSwaperShader(Color light, int palette_index = -1) {
        Program.paletteSwaper.SetUniform("palette", this.palette);
        Program.paletteSwaper.SetUniform("palette_size", this.palette_size);
        Program.paletteSwaper.SetUniform("palette_quantity", this.palette_quantity);
        Program.paletteSwaper.SetUniform("palette_index", palette_index == -1 ? this.palette_index : (uint) palette_index);
        Program.paletteSwaper.SetUniform("light", new Vector3f(light.R / 255f, light.G / 255f, light.B / 255f));
        return new RenderStates(Program.paletteSwaper);
    }
    public RenderStates SetHighContrastShader(Color fillColor) {
        Program.highContrastShader.SetUniform("direction", this.player_index);
        Program.highContrastShader.SetUniform("fillColor", new Vector3f(fillColor.R, fillColor.G, fillColor.B));
        return new RenderStates(Program.highContrastShader);
    }
    public int GetFacingTo(Character target) => target.body.position.X > this.body.position.X ? 1 : -1;
    public static string[] S(params string[] strings) => strings;

    // Loads
    public void LoadTextures() {
        Data.LoadTexturesDat(Path.Combine(this.folder_path, "textures.dat"), this.textures);
    }
    public void UnloadTextures() {
        foreach (var image in textures.Values) {
            image.Dispose(); // Free the memory used by the image
        }
        textures.Clear(); // Clear the dictionary
    }
    public void LoadSounds() {
        this.sounds = Data.LoadSoundsDat(Path.Combine(this.folder_path, "audio.dat"));
    }
    public void UnloadSounds() {
        foreach (var sound in sounds.Values) {
            sound.Dispose(); // Free the memory used by the image
        }
        sounds.Clear(); 
    }
    public void LoadAnimations() {
        this.animations = Data.LoadAnimationDat(Path.Combine(this.folder_path, "animations.dat"));
    }
    public void UnloadAnimations() {
        animations.Clear();
    }
    
    public void LoadPalette() {
        this.palette = new Texture(Path.Combine(this.folder_path, "palette.bmp"));
    }
    public void SetPalette(int add_to_index = 0) {
        var temp_index = (int) (this.palette_index + add_to_index) % this.palette_quantity;

        if (temp_index < 0) temp_index += this.palette_quantity;

        this.palette_index = (uint) temp_index;

        this.current_palette_color = this.palette.CopyToImage().GetPixel(new Vector2u(0, this.palette_index));
    }
    public void SetThumb() {
        try {
            this.textures.TryGetValue("thumb", out Texture thumb);
            this.thumb = thumb ;
        } catch (Exception) {
            this.thumb = new Texture(new Vector2u(1,1));
        }
    }
    
    // General Load
    public virtual Character Copy() {
        return null;
    }
    public virtual void Load() { }
    public override void Unload() {
        this.UnloadSounds();
        this.UnloadTextures();
        this.UnloadAnimations();
    }
    
}