using SFML.System;

    public class Animation {
        private Frame[] frames { get; set; }

        // logic
        private int frame_counter;
        public int anim_frame_index;
        public int logic_frame_index;
        public bool on_last_frame = false;
        public bool on_first_frame => logic_frame_index == 0;
        public bool ended = false;
        public bool playing_sound = false;
        private bool advanced = false;

        // infos
        public int lenght;
        public int original_lenght;
        public bool loop;

        public Animation(Frame[] frames, bool loop = true) {
            this.frames = frames;
            this.anim_frame_index = 0;
            this.lenght = 0;
            foreach (var frame in frames) { this.lenght += frame.lenght; };
            this.original_lenght = this.lenght;
            this.loop = loop;
            this.logic_frame_index = 0;
        }

        public FrameData GetCurrentFrame() {
            return this.frames.Last().GetType() == typeof(FrameData) ? (FrameData) this.frames[Math.Min(anim_frame_index, frames.Count() - 1)] : new FrameData();
        }
        public Frame GetCurrentFrameSimple() {
            if (anim_frame_index > frames.Count() - 1) return this.frames.Last();
            else return this.frames[anim_frame_index];
        }
        
        public bool AdvanceFrame() {
            if (this.ended && this.loop) this.Reset();

            advanced = false;

            logic_frame_index++;
            frame_counter++;

            if (frame_counter >= this.frames[anim_frame_index].lenght) { // Frame length reached
                frame_counter = 0;

                if (logic_frame_index >= this.lenght) { // Animation end reached
                    logic_frame_index -= 1;
                    ended = true;

                } else if (anim_frame_index < frames.Count() - 1) { // Advance animation frame
                    anim_frame_index++;
                    playing_sound = false;
                    advanced = true;
                }
            }

            this.on_last_frame = this.logic_frame_index == this.lenght - 1; // On last frame check

            return advanced;
        }
        public void Reset() {
            anim_frame_index = 0;
            logic_frame_index = 0;
            frame_counter = 0;
            on_last_frame = false;
            playing_sound = false;
            ended = false;
        }
    }

    public class GenericBox {
        public const int HITBOX = 0;
        public const int HURTBOX = 1;
        public const int PUSHBOX = 2;

        public Vector2f pA;
        public Vector2f pB;

        public float width;
        public float height;

        public int type;
        public int quadsize = 250;

        public GenericBox(int type, int x1, int y1, int x2, int y2) {   
            this.type = type;
            this.pA.X = x1;
            this.pB.X = x2;
            this.pA.Y = y1;
            this.pB.Y = y2;
            this.width = x2 - x1;
            this.height = y2 - y1;
        }

        public static bool Intersects(GenericBox boxA, GenericBox boxB, Character charA, Character charB) {
            return (boxA.getRealA(charA).X < boxB.getRealB(charB).X) &&
                (boxA.getRealB(charA).X > boxB.getRealA(charB).X) &&
                (boxA.getRealA(charA).Y < boxB.getRealB(charB).Y) &&
                (boxA.getRealB(charA).Y > boxB.getRealA(charB).Y);
        }

        public static void Colide(GenericBox boxA, GenericBox boxB, Character charA, Character charB) {
            // Calcule a sobreposição entre boxA e boxB no eixo X
            var overlapX = Math.Min(boxA.getRealB(charA).X, boxB.getRealB(charB).X) - Math.Max(boxA.getRealA(charA).X, boxB.getRealA(charB).X);

            // Verifique quem está à esquerda
            if (boxA.getRealA(charA).X < boxB.getRealA(charB).X) {
                // A está à esquerda de B; mova-os para afastá-los até o limite da sobreposição
                charA.body.position.X -= overlapX / 2;
                charB.body.position.X += overlapX / 2;
            } else {
                // B está à esquerda de A; mova-os para afastá-los até o limite da sobreposição
                charA.body.position.X += overlapX / 2;
                charB.body.position.X -= overlapX / 2;
            }
        }

        // Get methods
        public Vector2f getRealA(Character charA) {
            float X = charA.facing == -1 ? charA.visual_position.X - this.pB.X + this.quadsize : charA.visual_position.X + this.pA.X;
            float Y = charA.visual_position.Y + this.pA.Y;

            return new Vector2f(X, Y);
        }
        public Vector2f getRealB(Character charA) {
            float X = charA.facing == -1 ? charA.visual_position.X - this.pA.X + this.quadsize : charA.visual_position.X + this.pB.X;
            float Y = charA.visual_position.Y + this.pB.Y;

            return new Vector2f(X, Y);
        } 
    }

    public class FrameData : Frame{
        public bool has_hit;
        public float delta_X;
        public float delta_Y;
        public int facing;
        public List<GenericBox> Boxes;

        public FrameData(string sprite_index, float deltaX, float deltaY, List<GenericBox> boxes, int len = 1, int facing = 1, string sound = "", bool hasHit = true) {
            this.sprite_index = sprite_index;
            this.delta_X = deltaX;
            this.delta_Y = deltaY;
            this.Boxes = boxes;
            this.sound_index = sound;
            this.facing = facing;
            this.lenght = len;
            this.has_hit = hasHit;
        }

        public FrameData() {
            this.Boxes = new List<GenericBox>(){};
        }
    }

    public class Frame {
        public int lenght;
        public string sprite_index;
        public string sound_index;

        protected Frame() {
            this.sprite_index = ""; 
            this.sound_index = "";
        }

        public Frame(int sprite_index, int len = 1, string sound = "") {
            this.sprite_index = sprite_index.ToString();
            this.sound_index = sound;
            this.lenght = len;
        }

        public Frame(string sprite_index, int len = 1, string sound = "") {
            this.sprite_index = sprite_index;
            this.sound_index = sound;
            this.lenght = len;
        }
    }

