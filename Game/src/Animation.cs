using Character_Space;
using SFML.System;

namespace Animation_Space {

    public class Animation {
        public List<Frame> Frames { get; private set; }

        // logic
        private int frame_counter;
        public int anim_frame_index;
        public int logic_frame_index;
        public bool on_last_frame = false;
        public bool ended = false;
        public bool playing_sound = false;

        // infos
        public int lenght;
        public bool loop;
        public int framerate;

        public Animation(List<Frame> frames, bool loop = true) {
            this.Frames = frames;
            this.anim_frame_index = 0;
            this.lenght = 0;
            foreach (var frame in frames) { this.lenght += frame.lenght; };
            this.loop = loop;
            this.framerate = 60;
            this.logic_frame_index = 0;
        }

        public FrameData GetCurrentFrame() {
            if (anim_frame_index > Frames.Count() - 1) return this.Frames.Last().GetType() == typeof(FrameData) ? (FrameData) this.Frames.Last() : new FrameData();
            else return this.Frames[anim_frame_index].GetType() == typeof(FrameData) ? (FrameData) this.Frames[anim_frame_index] : new FrameData();
        }

        public Frame GetCurrentFrameSimple() {
            if (anim_frame_index > Frames.Count() - 1) return this.Frames.Last();
            else return this.Frames[anim_frame_index];
        }

        public bool AdvanceFrame() {
            if (this.ended && this.loop) this.Reset();

            bool advanced = false;

            logic_frame_index++;
            frame_counter++;

            if (frame_counter >= this.Frames[anim_frame_index].lenght) { // Frame length reached
                frame_counter = 0;

                if (logic_frame_index >= this.lenght) { // Animation end reached
                    logic_frame_index -= 1;
                    ended = true;

                } else if (anim_frame_index < Frames.Count() - 1) { // Advance animation frame
                    anim_frame_index++;
                    playing_sound = false;
                    advanced = true;
                }
            }

            this.on_last_frame = (anim_frame_index == Frames.Count() - 1) && (frame_counter >= this.Frames[anim_frame_index].lenght - 1); // On last frame check

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
    
        // public static Animation LoadFromFile() {}
        // public static void SaveToFile() {}
    }

    public class GenericBox {
        public const int HITBOX = 0;
        public const int HURTBOX = 1;
        public const int PUSHBOX = 2;
        public const int GRABBOX = 3;

        public Vector2f pA;
        public Vector2f pB;

        public float width;
        public float height;

        public int type; // 0 Hitbox, 1 Hurtbox, 2 Pushbox, 3 Grabbox
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
                charA.body.Position.X -= overlapX / 2;
                charB.body.Position.X += overlapX / 2;
            } else {
                // B está à esquerda de A; mova-os para afastá-los até o limite da sobreposição
                charA.body.Position.X += overlapX / 2;
                charB.body.Position.X -= overlapX / 2;
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
        public bool hasHit;
        public float DeltaX;
        public float DeltaY;
        public List<GenericBox> Boxes;

        public FrameData(int sprite_index, float deltaX, float deltaY, List<GenericBox> boxes, int len = 1, string sound = "", bool hasHit = true) {
            this.Sprite_index = sprite_index.ToString();
            this.DeltaX = deltaX;
            this.DeltaY = deltaY;
            this.Boxes = boxes;
            this.Sound_index = sound;
            this.lenght = len;
            this.hasHit = hasHit;
        }

        public FrameData(string sprite_index, float deltaX, float deltaY, List<GenericBox> boxes, int len = 1, string sound = "", bool hasHit = true) {
            this.Sprite_index = sprite_index;
            this.DeltaX = deltaX;
            this.DeltaY = deltaY;
            this.Boxes = boxes;
            this.Sound_index = sound;
            this.lenght = len;
            this.hasHit = hasHit;
        }

        public FrameData() {}
    }

    public class Frame {
        public int lenght;
        public string Sprite_index;
        public string Sound_index;

        protected Frame() {}

        public Frame(int sprite_index, int len = 1, string sound = "") {
            this.Sprite_index = sprite_index.ToString();
            this.Sound_index = sound;
            this.lenght = len;
        }

        public Frame(string sprite_index, int len = 1, string sound = "") {
            this.Sprite_index = sprite_index;
            this.Sound_index = sound;
            this.lenght = len;
        }
    }

}