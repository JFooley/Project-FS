using SFML.Graphics;
using SFML.System;
using SFML.Audio;
using Animation_Space;
using Input_Space;
using Stage_Space;
using UI_space;
using Data_space;

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
public abstract class Character : Object_Space.Object {
    // Consts
    public const int NOTHING = -1;
    public const int BLOCK = 0;
    public const int HIT = 1;
    public const int PARRY = 2;

    // Infos
    public string name;
    public int type;
    public string folderPath;
    public string soundFolderPath;
    public float floorLine;
    public Stage stage;

    // Controls
    public int playerIndex { get; set; }

    // Statistics 
    public Vector2i LifePoints = new Vector2i(1000, 1000);
    public Vector2i DizzyPoints = new Vector2i(500, 500);
    public Vector2i SuperPoints = new Vector2i(0, 100);
    public int StunFrames = 0;
    public int move_speed = 0;
    public int dash_speed = 0;
    public int jump_hight = 79;
    public int push_box_width = 25;

    // Object infos
    public string CurrentState;
    public string LastState;
    private Sprite[] LastSprites = new Sprite[3]; // For tracing
    public Color LightTint = new Color(255, 255, 255, 255);

    // Combat logic infos
    public bool notActing => this.State.not_busy && !this.State.low && !this.State.air && !this.onAir;
    public bool notActingLow => this.State.not_busy && this.State.low && !this.State.air && !this.onAir;
    public bool notActingAir => this.State.not_busy && !this.State.low && this.State.air && this.onAir;
    public bool notActingAll => notActing || notActingAir || notActingLow;

    public bool onHit => this.State.on_hit;
    public bool onAir => this.body.Position.Y < this.floorLine;
    public bool crounching => this.State.low;

    public bool canParry => InputManager.Instance.Key_press("Right", input_window: 10, player: this.playerIndex, facing: this.facing) && notActingAll && !this.isBlocking();
    public bool canDash => notActing && !this.State.is_parry;
    public bool hasHit = false; 

    public bool blockingHigh = false;
    public bool blockingLow = false;
    public bool blocking = false;

    // Data
    public Dictionary<string, State> states = new Dictionary<string, State>{};
    public abstract Dictionary<string, Texture> textures { get; protected set;}
    public abstract Dictionary<string, SoundBuffer> sounds { get; protected set;}
    private static List<Sound> active_sounds = new List<Sound>();

    // Visuals
    public Vector2f VisualPosition => new Vector2f(this.body.Position.X - 125, this.body.Position.Y - 250);
    public Texture thumb;
    public int shadow_size = 1;
    public bool has_frame_change => this.LastFrameIndex != this.CurrentFrameIndex || this.CurrentAnimation.frame_counter == 0;

    // Gets
    public string CurrentSprite => CurrentAnimation.GetCurrentFrame().Sprite_index;
    public string CurrentSound => CurrentAnimation.GetCurrentFrame().Sound_index;
    public List<GenericBox> CurrentBoxes => CurrentAnimation.GetCurrentFrame().Boxes;
    public int CurrentFrameIndex => CurrentAnimation.current_frame_index;
    public Animation CurrentAnimation => states[CurrentState].animation;
    public State State => states[CurrentState];
    public int LastFrameIndex = -1;

    // Flags and counters
    public int comboCounter = 0;
    public int hitstopCounter = 0;
    public float damageScaling => Math.Max(0.1f, 1 - comboCounter * 0.1f);
    public bool SA_flag = false;

    public Character(string name, string initialState, float startX, float startY, string folderPath, string soundFolderPath, Stage stage, int type = 0) : base() {
        this.folderPath = folderPath;
        this.soundFolderPath = soundFolderPath;
        this.name = name;
        this.type = type;
        this.CurrentState = initialState;
        this.LastState = initialState;
        this.stage = stage;
        base.body.Position.X = startX; 
        base.body.Position.Y = startY;
        this.floorLine = startY;
    }

    // Every Frame methods
    public override void Update() {
        // Render > Behave > Colide > Anima
        base.Update();
        if (this.hitstopCounter <= 0) {
            this.Animate();
            this.DoBehave();
            this.CheckColisions();
        } else this.hitstopCounter -= 1;
    }
    public override void Render(bool drawHitboxes = false) {
        base.Render(drawHitboxes);
        
        // Set current sprite
        var temp_sprite = this.GetCurrentSprite();
        temp_sprite.Position = new Vector2f(this.body.Position.X - (temp_sprite.GetLocalBounds().Width / 2 * this.facing), this.body.Position.Y - temp_sprite.GetLocalBounds().Height);
        temp_sprite.Scale = new Vector2f(this.size_ratio * this.facing, this.size_ratio);
        temp_sprite.Color = this.LightTint;

        // Render tracing
        if (this.State.trace) {
            Program.hueChange.SetUniform("hslInput", new SFML.Graphics.Glsl.Vec3(0.66f, 0.5f, 0.75f));

            for (int i = 0; i < 3; i++) {
                if (LastSprites[i] != null) Program.window.Draw(LastSprites[i], new RenderStates(Program.hueChange));
            }
            
            if (this.has_frame_change) {               
                LastSprites[2] = LastSprites[1];
                LastSprites[1] = LastSprites[0];
                LastSprites[0] = temp_sprite;
            }
        } else LastSprites = new Sprite[3];

        // Render current sprite
        if (this.State.glow && UI.Instance.blink30Hz) {
            Program.hueChange.SetUniform("hslInput", new SFML.Graphics.Glsl.Vec3(0.66f, 0.5f, 0.75f));
            Program.window.Draw(temp_sprite, new RenderStates(Program.hueChange));
        } else Program.window.Draw(temp_sprite);

        // Play sounds
        this.PlayFrameSound();
        
        // Draw Hitboxes
        if (drawHitboxes) {  
            RectangleShape anchorY = new RectangleShape(new Vector2f(0, 10)) {
                Position = new Vector2f(this.body.Position.X, this.body.Position.Y - 60),
                FillColor = SFML.Graphics.Color.Transparent,
                OutlineColor = this.CurrentAnimation.on_last_frame ? Color.Red : Color.White, 
                OutlineThickness = 1.0f
            };
            RectangleShape anchorX = new RectangleShape(new Vector2f(10, 0)) {
                Position = new Vector2f(this.body.Position.X - 5, this.body.Position.Y - 55),
                FillColor = SFML.Graphics.Color.Transparent,
                OutlineColor = this.CurrentAnimation.on_last_frame ? Color.Red : Color.White, 
                OutlineThickness = 1.0f 
            };
            
            Program.window.Draw(anchorX);
            Program.window.Draw(anchorY);

            foreach (GenericBox box in this.CurrentBoxes) {
                // Calcula as coordenadas absolutas da hitbox
                float x1 = box.getRealA(this).X * size_ratio;
                float y1 = box.getRealA(this).Y * size_ratio;
                float x2 = box.getRealB(this).X * size_ratio;
                float y2 = box.getRealB(this).Y * size_ratio;

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
            UI.Instance.DrawText(this.CurrentFrameIndex.ToString(), this.body.Position.X - Camera.Instance.X, this.body.Position.Y - Camera.Instance.Y - 135, spacing: Config.spacing_small, alignment: "center", textureName: "default small");
            UI.Instance.DrawText(this.CurrentState, this.body.Position.X - Camera.Instance.X, this.body.Position.Y - Camera.Instance.Y - 125, spacing: Config.spacing_small, alignment: "center", textureName: "default small");
            UI.Instance.DrawText(this.State.not_busy ? "waiting" : "busy", this.body.Position.X - Camera.Instance.X, this.body.Position.Y - Camera.Instance.Y - 115, spacing: Config.spacing_small, alignment: "center", textureName: "default small");
        }
    }
    public override void Animate() {
        if (!this.animate) return;
        this.CheckStun();

        // Update body.Position
        this.body.Update(this);
        this.body.Position.X += CurrentAnimation.GetCurrentFrame().DeltaX * this.facing;
        this.body.Position.Y += CurrentAnimation.GetCurrentFrame().DeltaY * this.facing;

        // Advance to the next frame and reset hit if necessary
        this.LastFrameIndex = this.CurrentFrameIndex;
        if (CurrentAnimation.AdvanceFrame() && CurrentAnimation.GetCurrentFrame().hasHit == false) this.hasHit = false;

        // Change state, if necessary
        if (State.change_on_end && this.CurrentAnimation.ended) {
            this.ChangeState(this.State.post_state);
        } else if (State.change_on_ground && !this.onAir) {
            this.ChangeState(this.State.post_state);
        }
    }
    
    // Battle methods
    public virtual int ImposeBehavior(Character target, bool parried = false) {
        return -1;
    }
    public bool isBlocking() {
        return this.isBlockingHigh() || this.isBlockingLow();
    }
    public bool isBlockingHigh() {
        if ((this.notActingAll || this.State.on_block) && (this.blockingHigh || this.blocking)) return true;
        return (this.notActing || (this.State.on_block && !this.State.low)) && InputManager.Instance.Key_hold("Left", player: this.playerIndex, facing: this.facing) && !InputManager.Instance.Key_hold("Down", player: this.playerIndex, facing: this.facing);
    }
    public bool isBlockingLow() {
        if ((this.notActingAll || this.State.on_block) && (this.blockingLow || this.blocking)) return true;
        return (this.notActingLow || (this.State.on_block && this.State.low)) && InputManager.Instance.Key_hold("Left", player: this.playerIndex, facing: this.facing) && InputManager.Instance.Key_hold("Down", player: this.playerIndex);
    }
    public void Stun(Character enemy, int advantage, bool hit = true, bool airbone = false, bool sweep = false, bool force = false) {
        this.StunFrames = 0;
        if (hit) { // Hit stun states
            if (sweep) {
                this.ChangeState("Sweeped", reset: true);
                return;

            } else if (airbone || this.LifePoints.X <= 0 || this.CurrentState == "Airboned") {
                this.facing = -enemy.facing;
                this.ChangeState("Airboned", reset: true);
                this.StunFrames = 0;

                if (this.LifePoints.X <= 0) this.SetVelocity(X: -Config.heavy_pushback, Y: 10);
                return;

            } else if (this.crounching) {
                this.facing = -enemy.facing;
                this.ChangeState("OnHitLow", reset: true);

            } else {
                this.facing = -enemy.facing;
                this.ChangeState("OnHit", reset: true);
            }

        } else { // Block stun states
            this.facing = -enemy.facing;
            if (this.crounching) this.ChangeState("OnBlockLow", reset: true);
                else this.ChangeState("OnBlock", reset: true);
        }
    
        // Set stun frames
        if (force) {
            this.StunFrames = Math.Max(advantage, 1);
        } else {
            this.StunFrames = Math.Max(60 / enemy.CurrentAnimation.framerate * (enemy.CurrentAnimation.anim_size - enemy.CurrentFrameIndex) + advantage, 1);
        }
    }
    public void BlockStun(Character enemy, int advantage, bool force = false) {
        this.Stun(enemy, advantage, hit: false, force: force);
    }
    public void CheckStun() {
        if (this.StunFrames > 0) {
            this.StunFrames -= 1;
            if (this.StunFrames == 0 && this.CurrentState != "Airboned") {
                if (this.onAir) this.ChangeState("JumpFalling");
                else if (this.crounching) this.ChangeState("Crouching");
                else this.ChangeState("Idle");
            }
        }
    }
    public void CheckColisions() {               
        // Para cada character no stage
        foreach (var charB in Program.stage.OnSceneCharacters) {
            if (charB == this) continue;
            
            foreach (GenericBox boxA in this.CurrentBoxes) {
                if (boxA.type != GenericBox.HITBOX && boxA.type != GenericBox.PUSHBOX && boxA.type != GenericBox.GRABBOX) continue;
                
                foreach (GenericBox boxB in charB.CurrentBoxes) {
                    if (boxB.type == GenericBox.HURTBOX && boxB.type == GenericBox.PUSHBOX) continue;
                    
                    if (GenericBox.Intersects(boxA, boxB, this, charB)) {
                        if (boxA.type == GenericBox.PUSHBOX && boxB.type == GenericBox.PUSHBOX) {
                            // A body push B
                            GenericBox.Colide(boxA, boxB, this, charB);

                        } else if (this.playerIndex != charB.playerIndex && this.hasHit == false && charB.State.can_be_hit && this.type >= charB.type && (boxA.type == GenericBox.HITBOX || boxA.type == GenericBox.GRABBOX) && boxB.type == GenericBox.HURTBOX) { 
                            // A hit B
                            this.hasHit = true;
                            var hit = this.ImposeBehavior(charB, parried: charB.canParry);
                            
                            if (hit == Character.PARRY) charB.ChangeState("Parry", reset: true);
                            stage.Hitstop(this.State.hitstop, parry: hit == Character.PARRY, character: charB);
                            
                            if (this.playerIndex == 1) stage.character_A.comboCounter += hit == Character.HIT ? 1 : 0; 
                            else stage.character_B.comboCounter += hit == Character.HIT ? 1 : 0;

                            stage.spawnHitspark(hit, boxA.getRealB(this).X - (boxA.width * 1/3), (boxA.getRealA(this).Y + boxA.getRealB(this).Y) / 2 + 125, this.facing);
                        }
                    }
                }
            }
        }
    }

    // Static Methods 
    public static void Push(Character target, Character self, string amount, float X_amount = 0, float Y_amount = 0, bool airbone = false, bool force_push = false) {
        if ((target.body.Position.X <= Camera.Instance.X - Config.corner_limit || target.body.Position.X >= Camera.Instance.X + Config.corner_limit) && !force_push) {
            if (X_amount != 0) {
                self.SetVelocity(X: self.facing * target.facing * X_amount, keep_Y: true);
                target.SetVelocity(X: self.facing * target.facing * X_amount, Y: (target.onAir || airbone) ? Y_amount : 0);
            } else if (amount == "Light") {
                self.SetVelocity(X: self.facing * target.facing * Config.light_pushback, keep_Y: true);
                target.SetVelocity(X: self.facing * target.facing * Config.light_pushback, Y: (target.onAir || airbone) ? Y_amount : 0);
            } else if (amount == "Medium") {
                self.SetVelocity(X: self.facing * target.facing * Config.medium_pushback, keep_Y: true);
                target.SetVelocity(X: self.facing * target.facing * Config.medium_pushback, Y: (target.onAir || airbone) ? Y_amount : 0);
            } else if (amount == "Heavy"){
                self.SetVelocity(X: self.facing * target.facing * Config.heavy_pushback, keep_Y: true);
                target.SetVelocity(X: self.facing * target.facing * Config.heavy_pushback, Y: (target.onAir || airbone) ? Y_amount : 0);
            }
        } else {
            if (X_amount != 0) {
                target.SetVelocity(X: self.facing * target.facing * X_amount, Y: (target.onAir || airbone) ? Y_amount : 0);
            } else if (amount == "Light") {
                target.SetVelocity(X: self.facing * target.facing * Config.light_pushback, Y: (target.onAir || airbone) ? Y_amount : 0);
            } else if (amount == "Medium") {
                target.SetVelocity(X: self.facing * target.facing * Config.medium_pushback, Y: (target.onAir || airbone) ? Y_amount : 0);
            } else if (amount == "Heavy"){
                target.SetVelocity(X: self.facing * target.facing * Config.heavy_pushback, Y: (target.onAir || airbone) ? Y_amount : 0);
            }
        }
    }
    public static void Damage(Character target, Character self, int damage, int dizzy_damage) {
        target.LifePoints.X = (int) Math.Max(target.LifePoints.X - damage * self.damageScaling, 0);
        target.DizzyPoints.X = (int) Math.Max(target.DizzyPoints.X - dizzy_damage * self.damageScaling, 0);
    }
    public static void GetSuperPoints(Character target, Character self, int hit , int target_amount = 3, int self_amount = 10) {
        target.SuperPoints.X = (int) Math.Min(target.SuperPoints.Y, target.SuperPoints.X + target_amount);
        self.SuperPoints.X = (int) Math.Min(self.SuperPoints.Y, hit == 1 ? self.SuperPoints.X + self_amount : self.SuperPoints.X + (self_amount / 3));
    }
    public static bool CheckSuperPoints(Character target, int amount) {
        return target.SuperPoints.X >= amount;
    }
    public static void UseSuperPoints(Character target, int amount) {
        target.SuperPoints.X = (int) Math.Max(0, target.SuperPoints.X - amount);
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
    public void ChangeState(string newState, bool reset = false) {
        if (this.LifePoints.X <= 0 && this.CurrentState == "OnGround" && !reset) return;

        if (newState == "Parry" && this.onAir) newState = "AirParry";

        this.LastState = this.CurrentState;
        if (states.ContainsKey(newState)) {
            if (CurrentState != newState || reset) this.CurrentAnimation.Reset();
            this.CurrentState = newState;
            this.hasHit = false;
        }

        if (this.CurrentState.Contains("Falling")) this.StunFrames = 0;
    }
    public Sprite GetCurrentSprite() {
        if (textures.TryGetValue(this.CurrentSprite, out Texture texture)) {
            return new Sprite(texture);
        }
        return new Sprite(); 
    }
    public void PlayFrameSound() {
        if (this.has_frame_change && !this.CurrentAnimation.playing_sound && this.CurrentSound != "" && sounds.TryGetValue(this.CurrentSound, out SoundBuffer buffer)) {
            var temp_sound = new Sound(buffer) {Volume = Config.Character_Volume};
            temp_sound.Play();
            active_sounds.Add(temp_sound);
            active_sounds.RemoveAll(s => s.Status == SoundStatus.Stopped);
            this.CurrentAnimation.playing_sound = true;
        }
    }
    public void PlaySound(string sound_name) {
        if (sounds.TryGetValue(sound_name, out SoundBuffer buffer)) {
            var temp_sound = new Sound(buffer) {Volume = Config.Character_Volume};
            temp_sound.Play();
            active_sounds.Add(temp_sound);
            active_sounds.RemoveAll(s => s.Status == SoundStatus.Stopped);
            this.CurrentAnimation.playing_sound = true;
        };
    }
    public void Reset(int start_point, int facing, String state = "Idle", bool total_reset = false) {
        this.ChangeState(state, reset: true);
        this.LifePoints.X = this.LifePoints.Y;
        this.DizzyPoints.X = this.DizzyPoints.Y;
        this.SuperPoints.X = total_reset ? 0 : this.SuperPoints.X;
        this.body.Position.X = start_point;
        this.body.Position.Y = this.floorLine;
        this.body.SetVelocity(this, 0, 0, raw_set: true);
        this.facing = facing;
    }
   
    // Visuals load
    public void LoadSpriteImages() {
        string currentDirectory = Directory.GetCurrentDirectory();
        string fullPath = Path.Combine(currentDirectory, this.folderPath);
        
        // Verifica se o diretório existe
        if (!System.IO.Directory.Exists(fullPath)) {
            throw new System.IO.DirectoryNotFoundException($"O diretório {fullPath} não foi encontrado.");
        }

        // Verifica se o arquivo binário existe, senão, carrega as texturas e cria ele
        string datpath = Path.Combine(fullPath, "visuals.dat");
        if (System.IO.File.Exists(datpath)) {
            textures = DataManagement.LoadTextures(datpath);
            
        } else {
            // Obtém todos os arquivos no diretório especificado
            string[] files = System.IO.Directory.GetFiles(fullPath);
            foreach (string file in files) {
                // Tenta carregar a textura
                Texture texture = new Texture(file);
                
                // Obtém o nome do arquivo sem a extensão
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(file);
                
                // Usa o nome do arquivo sem extensão como chave no dicionário
                textures[fileNameWithoutExtension] = texture;
            }

            // Salva o arquivo binário
            DataManagement.SaveTextures(datpath, textures);
        }
    }
    public void UnloadSpriteImages() {
        foreach (var image in textures.Values)
        {
            image.Dispose(); // Free the memory used by the image
        }
        textures.Clear(); // Clear the dictionary
    }

    // Sounds load
    public void LoadSounds() {
        string currentDirectory = Directory.GetCurrentDirectory();
        string fullSoundPath = Path.Combine(currentDirectory, this.soundFolderPath);

        // Verifica se o diretório existe
        if (!System.IO.Directory.Exists(fullSoundPath)) {
            throw new System.IO.DirectoryNotFoundException($"O diretório {fullSoundPath} não foi encontrado.");
        }

        // Verifica se o arquivo binário existe, senão, carrega as texturas e cria ele
        string datpath = Path.Combine(fullSoundPath, "sounds.dat");
        if (System.IO.File.Exists(datpath)) {
            sounds = DataManagement.LoadSounds(datpath);
            
        } else {
            // Obtém todos os arquivos no diretório especificado
            string[] files = System.IO.Directory.GetFiles(fullSoundPath);
            foreach (string file in files) {
                // Obtém o nome do arquivo sem a extensão
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(file);

                // Adiciona no dicionário
                sounds[fileNameWithoutExtension] = new SoundBuffer(file);
            }

            // Salva o arquivo binário
            DataManagement.SaveSounds(datpath, sounds);
        }
    }
    public void UnloadSounds() {
        foreach (var sound in sounds.Values)
        {
            sound.Dispose(); // Free the memory used by the image
        }
        sounds.Clear(); 
    }

    // General Load
    public static Character SelectCharacter(string name, Stage stage) {
        switch (name) {
            case "Ken":
                var Ken_object = new Ken("Intro", 0, stage.floorLine, stage);
                Ken_object.Load();
                return Ken_object;
            
            case "Psylock":
                var Psylock_object = new Psylock("Intro", 0, stage.floorLine, stage);
                Psylock_object.Load();
                return Psylock_object;

            default:
                var Default_object = new Ken("Intro", 0, stage.floorLine, stage);
                Default_object.Load();
                return Default_object;
        }
    }
    public virtual void Load() {}
    public override void Unload() {
        this.UnloadSounds();
        this.UnloadSpriteImages();
    }
}

}
