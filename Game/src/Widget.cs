using UI_space;
using SFML.System;
using SFML.Audio;

public abstract class Widget {
    public Widget() {}
    public virtual void Render() {}
}

public class Selector : Widget {
    private bool has_change => this.pointer != this.previous_pointer;
    private Sound change_sound;
    public Vector2i pointer;
    public Vector2i previous_pointer;
    public List<int> options = new List<int>() { 1 }; // Dimensions of the selector (ex: [1, 2, 2, 1]. Each number is a line and the valor is the number of options in that line)

    public Selector(List<int> options) : base() {
        this.options = options;
        this.pointer = new Vector2i(0, 0);
        this.change_sound = new Sound(Data.sounds["ui:change"]) {Volume = Config.Effect_Volume};
    }

    public void Update(int player = 0) {
        this.previous_pointer = this.pointer;
        if (Input.Key_down("Up", player) || (Input.Key_hold_for("Up", Config.hold_time, player) && UI.ForEach(Config.hold_clock))) {
            this.pointer.Y = this.pointer.Y <= 0 ? options.Count - 1 : pointer.Y - 1;
            this.pointer.X = Math.Min(this.pointer.X, options[this.pointer.Y] - 1);
        } else if (Input.Key_down("Down", player) || (Input.Key_hold_for("Down", Config.hold_time, player) && UI.ForEach(Config.hold_clock))) {
            this.pointer.Y = this.pointer.Y >= options.Count - 1 ? 0 : pointer.Y + 1;
            this.pointer.X = Math.Min(this.pointer.X, options[this.pointer.Y] - 1);
        }

        if (Input.Key_down("Left", player) || (Input.Key_hold_for("Left", Config.hold_time, player) && UI.ForEach(Config.hold_clock))) {
            this.pointer.X = this.pointer.X <= 0 ? options[this.pointer.Y] - 1 : pointer.X - 1;
        } else if (Input.Key_down("Right", player) || (Input.Key_hold_for("Right", Config.hold_time, player) && UI.ForEach(Config.hold_clock))) {
            this.pointer.X = this.pointer.X >= options[this.pointer.Y] - 1 ? 0 : pointer.X + 1;
        }

        if (has_change && Accessibility.navigation_cue) {
            this.change_sound.Play();
        }
    }

    public bool is_on(int x, int y) {
        return this.pointer.X == x && this.pointer.Y == y;
    }
}