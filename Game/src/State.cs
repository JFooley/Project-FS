public class State {
    private Animation[] _animations;
    public int variation_index = 0;
    public int variation_amount => _animations.Length;
    public Animation animation => _animations[variation_index];

    public string post_state;
    public int priority; // Light: 0, Medium: 1, Heavy: 2, Special: 3, Super: 4, Throw: 5, Parry: 6, Particles: 7
    public string hitstop; // None, Light, Medium, Heavy
    public int range; // Reach of the atack

    // Combat logic
    public bool idle;
    public bool busy => !idle;
    public bool air;
    public bool low;
    public bool overhead;
    public bool on_hit;
    public bool on_block;
    public bool on_parry;
    public bool can_be_parried;
    public bool can_be_hit;
    public bool will_hit;
    public bool is_grab;
    public bool has_gravity;

    // Anim logic
    public bool trace;
    public bool glow;
    public bool change_on_end;
    public bool change_on_ground;
    public bool drama_wait;

    public State(Frame[][] frames, string post_state, int priority = -1, int range = 110, bool loop = true, bool change_on_end = true, bool change_on_ground = false, bool can_be_parried = true, bool trace = false, bool glow = false, string hitstop = "Light", bool idle = false, bool air = false, bool low = false, bool overhead = false, bool on_hit = false, bool on_block = false, bool is_parry = false, bool can_be_hit = true, bool will_hit = false, bool is_grab = false, bool drama_wait = false, bool has_gravity = true) {
        this._animations = frames.Select(f => new Animation(f, loop)).ToArray();
        this.post_state = post_state;
        this.priority = priority;
        this.range = range;
        this.change_on_end = change_on_end;
        this.change_on_ground = change_on_ground;
        this.trace = trace;
        this.glow = glow;
        this.hitstop = hitstop;
        this.can_be_parried = can_be_parried && !is_grab;
        this.can_be_hit = can_be_hit;
        this.idle = idle;
        this.air = air;
        this.low = low;
        this.overhead = overhead;
        this.on_hit = on_hit;
        this.on_block = on_block;
        this.on_parry = is_parry;
        this.drama_wait = drama_wait;
        this.has_gravity = has_gravity;
        this.will_hit = will_hit || is_grab;
        this.is_grab = is_grab;
    }
}
