using Animation_Space;

public class State {
    public Animation animation;
    public string post_state;
    public int priority; // Light: 0, Medium: 1, Heavy: 2, Special: 3, Super: 4, Throw: 5, Parry: 6, Particles: 7
    public string hitstop; // None, Light, Medium, Heavy

    // Combat logic
    public bool not_busy;
    public bool busy => !not_busy;
    public bool air;
    public bool low;
    public bool on_hit;
    public bool on_block;
    public bool on_parry;
    public bool can_be_parried;
    public bool can_be_hit;
    public bool can_harm;

    // Anim logic
    public bool trace;
    public bool glow;
    public bool change_on_end;
    public bool change_on_ground;

    // Other logics
    public bool drama_wait;

    public State(List<Frame> frames, string post_state, int priority = -1, bool loop = true, bool change_on_end = true, bool change_on_ground = false, bool can_be_parried = true, bool trace = false, bool glow = false, string hitstop = "Light", bool not_busy = false, bool air = false, bool low = false, bool on_hit = false, bool on_block = false, bool is_parry = false, bool can_be_hit = true, bool can_harm = false, bool drama_wait = false) {
        this.animation = new Animation(frames, loop);
        this.post_state = post_state;
        this.priority = priority;
        this.change_on_end = change_on_end;
        this.change_on_ground = change_on_ground;
        this.trace = trace;
        this.glow = glow;
        this.hitstop = hitstop;
        this.can_be_parried = can_be_parried;
        this.can_be_hit = can_be_hit;
        this.not_busy = not_busy;
        this.air = air;
        this.low = low;
        this.on_hit = on_hit;
        this.on_block = on_block;
        this.on_parry = is_parry;
        this.drama_wait = drama_wait;
        this.can_harm = (!not_busy && !on_hit && !on_block && !is_parry) || can_harm;
    }
}
