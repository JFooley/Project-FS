import json
import os
import tkinter as tk
from dataclasses import dataclass, field
from tkinter import filedialog, messagebox

from PIL import Image, ImageTk

FPS = 60
CANVAS_W = 960
CANVAS_H = 540
GRID_SIZE = 32


# DATA
@dataclass
class Hitbox:
    x1: int
    y1: int
    x2: int
    y2: int
    box_type: int

@dataclass
class FrameData:
    image_path: str
    duration: int = 1
    facing: int = 1
    delta_x: int = 0
    delta_y: int = 0
    hitboxes: list = field(default_factory=list)

class AnimationData:
    def __init__(self):
        self.frames = []

    def add_frame(self, path):
        self.frames.append(FrameData(path))

    def remove_frame(self, index):
        if 0 <= index < len(self.frames):
            self.frames.pop(index)

    def duplicate_frame(self, index):
        if 0 <= index < len(self.frames):
            f = self.frames[index]
            new_frame = FrameData(
                image_path=f.image_path,
                duration=f.duration,
                facing=f.facing,
                delta_x=f.delta_x,
                delta_y=f.delta_y,
                hitboxes=[
                    Hitbox(h.x1, h.y1, h.x2, h.y2, h.box_type)
                    for h in f.hitboxes
                ]
            )
            self.frames.insert(index + 1, new_frame)

    def move_left(self, index):
        if index > 0:
            self.frames[index], self.frames[index - 1] = self.frames[index - 1], self.frames[index]
            return index - 1
        return index

    def move_right(self, index):
        if index < len(self.frames) - 1:
            self.frames[index], self.frames[index + 1] = self.frames[index + 1], self.frames[index]
            return index + 1
        return index

# SERIALIZER
class AnimationSerializer:
    @staticmethod
    def save(animation, filename, prefix=None):

        anim_name = os.path.splitext(os.path.basename(filename))[0]

        data = {
            anim_name: []
        }

        for frame in animation.frames:

            base = os.path.splitext(os.path.basename(frame.image_path))[0]

            sprite_name = f"{prefix}:{base}" if prefix else base

            boxes = []

            for h in frame.hitboxes:
                boxes.append({
                    "type": h.box_type,
                    "pA": {
                        "x": h.x1,
                        "y": h.y1
                    },
                    "pB": {
                        "x": h.x2,
                        "y": h.y2
                    }
                })

            data[anim_name].append({
                "Sprite_index": sprite_name,
                "DeltaX": frame.delta_x / frame.duration,
                "DeltaY": frame.delta_y / frame.duration,
                "Boxes": boxes,
                "lenght": frame.duration,
                "facing": frame.facing,
                "Sound_index": "",
                "hasHit": True
            })

        with open(filename, "w") as f:
            json.dump(data, f, indent=2)

# EDITOR
class AnimationEditor:
    def __init__(self, root):
        self.loading_frame_data = False

        self.root = root
        self.root.title("Animation Editor")

        self.animation = AnimationData()

        self.current_frame = 0

        self.image_cache = {}
        self.tk_cache = {}

        self.scale = 2

        self.playing = False
        self.play_frame = 0
        self.play_tick = 0

        self.preview_x = 0
        self.preview_y = 0

        self.box_type = 0

        self.box_start = None
        self.temp_rect = None

        self.setup_ui()
        self.setup_bindings()

        self.loop()

    # UI
    def setup_ui(self):

        self.top = tk.Frame(self.root)
        self.top.pack(fill=tk.X)

        tk.Button(self.top, text="Add Frames", command=self.add_frames).pack(side=tk.LEFT)
        tk.Button(self.top, text="Save", command=self.save).pack(side=tk.LEFT)
        tk.Button(self.top, text="Play", command=self.toggle_play).pack(side=tk.LEFT)
        tk.Button(self.top, text="Duplicate", command=self.duplicate_current).pack(side=tk.LEFT)
        tk.Button(self.top, text="Copy Last Frame Hitboxes", command=self.copy_last_frame_hitboxes).pack(side=tk.LEFT)
        tk.Button(self.top, text="Delete", command=self.delete_current).pack(side=tk.LEFT)

        tk.Label(self.top, text="Duration").pack(side=tk.LEFT, padx=8)

        self.duration_var = tk.StringVar(value="1")

        self.duration_entry = tk.Entry(self.top, width=5, textvariable=self.duration_var)
        self.duration_entry.pack(side=tk.LEFT)

        tk.Label(self.top, text="Facing").pack(side=tk.LEFT, padx=8)

        self.facing_var = tk.StringVar(value="1")

        self.facing_entry = tk.Entry(self.top, width=5, textvariable=self.facing_var)
        self.facing_entry.pack(side=tk.LEFT)


        tk.Label(self.top, text="Delta X").pack(side=tk.LEFT, padx=8)

        self.dx_var = tk.StringVar(value="0")

        self.dx_entry = tk.Entry(self.top, width=5, textvariable=self.dx_var)
        self.dx_entry.pack(side=tk.LEFT)

        tk.Label(self.top, text="Delta Y").pack(side=tk.LEFT, padx=8)

        self.dy_var = tk.StringVar(value="0")

        self.dy_entry = tk.Entry(self.top, width=5, textvariable=self.dy_var)
        self.dy_entry.pack(side=tk.LEFT)

        self.canvas = tk.Canvas(
            self.root,
            width=CANVAS_W,
            height=CANVAS_H,
            bg="#202020"
        )

        self.canvas.pack()

        self.timeline = tk.Frame(self.root)
        self.timeline.pack(fill=tk.X)

        self.status = tk.Label(self.root, text="")
        self.status.pack(fill=tk.X)

        self.box_button = tk.Button(
            self.top,
            text="Box: HITBOX",
            command=self.next_box_type
        )
        self.box_button.pack(side=tk.LEFT, padx=8)
        
        self.duration_var.trace_add("write", lambda *args: self.on_field_change())
        self.facing_var.trace_add("write", lambda *args: self.on_field_change())
        self.dx_var.trace_add("write", lambda *args: self.on_field_change())
        self.dy_var.trace_add("write", lambda *args: self.on_field_change())

    # INPUT
    def setup_bindings(self):
        self.root.bind("<Left>", lambda e: self.change_delta(-1, 0))
        self.root.bind("<Right>", lambda e: self.change_delta(1, 0))
        self.root.bind("<Up>", lambda e: self.change_delta(0, -1))
        self.root.bind("<Down>", lambda e: self.change_delta(0, 1))

        self.root.bind("q", lambda e: self.change_box_type(-1))
        self.root.bind("e", lambda e: self.change_box_type(1))

        self.root.bind("<Delete>", lambda e: self.delete_last_hitbox())

        self.canvas.bind("<Button-1>", self.on_click)
        self.canvas.bind("<Motion>", self.on_motion)

    # FILES
    def add_frames(self):

        files = filedialog.askopenfilenames(
            filetypes=[("Images", "*.png;*.jpg;*.jpeg")]
        )

        for f in files:
            self.animation.add_frame(f)
            self.load_image(f)

        self.rebuild_timeline()
        self.load_frame_data()

    def load_image(self, path):

        if path in self.image_cache:
            return

        image = Image.open(path).convert("RGBA")

        image = image.resize((
            image.width * self.scale,
            image.height * self.scale
        ))

        self.image_cache[path] = image
        self.tk_cache[path] = ImageTk.PhotoImage(image)

    # TIMELINE
    def rebuild_timeline(self):

        for w in self.timeline.winfo_children():
            w.destroy()

        for i, frame in enumerate(self.animation.frames):

            color = "orange" if i == self.current_frame else "#404040"

            b = tk.Button(
                self.timeline,
                text=f"{i}\n{frame.duration}f",
                bg=color,
                fg="white",
                command=lambda idx=i: self.select_frame(idx),
                width=6,
                height=3
            )

            b.pack(side=tk.LEFT, padx=2, pady=2)

    # FRAME NAVIGATION
    def select_frame(self, index):

        self.save_frame_data()

        self.current_frame = index

        self.load_frame_data()
        self.rebuild_timeline()

    def next_frame(self):

        if not self.animation.frames:
            return

        self.save_frame_data()

        self.current_frame += 1

        if self.current_frame >= len(self.animation.frames):
            self.current_frame = 0

        self.load_frame_data()
        self.rebuild_timeline()

    def prev_frame(self):

        if not self.animation.frames:
            return

        self.save_frame_data()

        self.current_frame -= 1

        if self.current_frame < 0:
            self.current_frame = len(self.animation.frames) - 1

        self.load_frame_data()
        self.rebuild_timeline()

    # DATA SYNC
    def load_frame_data(self):

        if not self.animation.frames:
            return

        self.loading_frame_data = True

        frame = self.animation.frames[self.current_frame]

        self.duration_var.set(str(frame.duration))
        self.facing_var.set(str(frame.facing))
        self.dx_var.set(str(frame.delta_x))
        self.dy_var.set(str(frame.delta_y))

        self.loading_frame_data = False

    def save_frame_data(self):

        if not self.animation.frames:
            return

        frame = self.animation.frames[self.current_frame]

        try:
            frame.duration = max(1, int(self.duration_var.get()))
        except:
            frame.duration = 1

        try:
            frame.facing = min(max(int(self.facing), -1), 1)
        except:
            frame.facing = 1

        try:
            frame.delta_x = int(self.dx_var.get())
        except:
            frame.delta_x = 0

        try:
            frame.delta_y = int(self.dy_var.get())
        except:
            frame.delta_y = 0

    def change_delta(self, dx, dy):
        if not self.animation.frames:
            return

        frame = self.animation.frames[self.current_frame]

        frame.delta_x += dx
        frame.delta_y += dy

        self.dx_var.set(str(frame.delta_x))
        self.dy_var.set(str(frame.delta_y))
    
    def on_field_change(self):
        if self.loading_frame_data:
            return

        if not self.animation.frames:
            return

        frame = self.animation.frames[self.current_frame]

        try:
            frame.duration = max(1, int(self.duration_var.get()))
        except:
            pass

        try:
            frame.facing = min(max(int(self.facing), -1), 1)
        except:
            pass

        try:
            frame.delta_x = int(self.dx_var.get())
        except:
            pass

        try:
            frame.delta_y = int(self.dy_var.get())
        except:
            pass

        self.rebuild_timeline()

    # PLAYBACK
    def toggle_play(self):
        self.playing = not self.playing

    def update_playback(self):

        if not self.playing:
            return

        if not self.animation.frames:
            return

        frame = self.animation.frames[self.play_frame]

        self.play_tick += 1

        if self.play_tick >= frame.duration:

            self.preview_x += frame.delta_x
            self.preview_y += frame.delta_y

            self.play_tick = 0
            self.play_frame += 1

            if self.play_frame >= len(self.animation.frames):
                self.play_frame = 0
                self.preview_x = 0
                self.preview_y = 0

    # HITBOX
    def change_box_type(self, delta):
        self.box_type += delta

        if self.box_type < 0:
            self.box_type = 2

        if self.box_type > 2:
            self.box_type = 0

        names = {
            0: "HITBOX",
            1: "HURTBOX",
            2: "PUSHBOX"
        }

        self.box_button.config(
            text=f"Box: {names[self.box_type]}"
        )

    def next_box_type(self):

        self.box_type += 1

        if self.box_type > 2:
            self.box_type = 0

        names = {
            0: "HITBOX",
            1: "HURTBOX",
            2: "PUSHBOX"
        }

        self.box_button.config(
            text=f"Box: {names[self.box_type]}"
        )

    def delete_last_hitbox(self):
        if not self.animation.frames:
            return

        frame = self.animation.frames[self.current_frame]

        if frame.hitboxes:
            frame.hitboxes.pop()

    def copy_last_frame_hitboxes(self):
        if not self.animation.frames:
            return
        
        frame: FrameData = self.animation.frames[self.current_frame]
        last_frame: FrameData = self.animation.frames[self.current_frame - 1] if self.current_frame > 0 else None

        if last_frame.hitboxes:
            frame.hitboxes = last_frame.hitboxes.copy() if last_frame else []

    def on_click(self, event):

        if not self.animation.frames:
            return

        frame = self.animation.frames[self.current_frame]

        img_x = CANVAS_W // 2
        img_y = CANVAS_H // 2

        image = self.tk_cache[frame.image_path]

        left = img_x - image.width() // 2
        top = img_y - image.height() // 2

        if self.box_start is None:

            self.box_start = (event.x, event.y)

        else:

            x1, y1 = self.box_start
            x2, y2 = event.x, event.y

            self.box_start = None

            h = Hitbox(
                (x1 - left) // self.scale,
                (y1 - top) // self.scale,
                (x2 - left) // self.scale,
                (y2 - top) // self.scale,
                self.box_type
            )

            frame.hitboxes.append(h)

    def on_motion(self, event):

        if self.temp_rect:
            self.canvas.delete(self.temp_rect)
            self.temp_rect = None

        if self.box_start:

            x1, y1 = self.box_start

            self.temp_rect = self.canvas.create_rectangle(
                x1,
                y1,
                event.x,
                event.y,
                outline=self.box_color(self.box_type),
                dash=(4, 4)
            )

    # EDIT
    def duplicate_current(self):

        if not self.animation.frames:
            return

        self.animation.duplicate_frame(self.current_frame)
        self.rebuild_timeline()

    def delete_current(self):

        if not self.animation.frames:
            return

        self.animation.remove_frame(self.current_frame)

        if self.current_frame >= len(self.animation.frames):
            self.current_frame = max(0, len(self.animation.frames) - 1)

        self.rebuild_timeline()

    # DRAW
    def draw_grid(self):

        for x in range(0, CANVAS_W, GRID_SIZE):
            self.canvas.create_line(x, 0, x, CANVAS_H, fill="#303030")

        for y in range(0, CANVAS_H, GRID_SIZE):
            self.canvas.create_line(0, y, CANVAS_W, y, fill="#303030")

        self.canvas.create_line(CANVAS_W // 2, 0, CANVAS_W // 2, CANVAS_H, fill="red")
        self.canvas.create_line(0, CANVAS_H // 2, CANVAS_W, CANVAS_H // 2, fill="red")

    def draw_editor(self):

        self.canvas.delete("all")

        self.draw_grid()

        if not self.animation.frames:
            return

        if self.playing:
            frame_index = self.play_frame
        else:
            frame_index = self.current_frame

        frame = self.animation.frames[frame_index]

        image = self.tk_cache[frame.image_path]

        if self.playing:

            cx = CANVAS_W // 2 + self.preview_x * self.scale
            cy = CANVAS_H // 2 + self.preview_y * self.scale

        else:

            accumulated_x = 0
            accumulated_y = 0

            for i in range(frame_index + 1):
                f = self.animation.frames[i]
                accumulated_x += f.delta_x
                accumulated_y += f.delta_y

            cx = CANVAS_W // 2 + accumulated_x * self.scale
            cy = CANVAS_H // 2 + accumulated_y * self.scale

        self.canvas.create_image(
            cx,
            cy,
            image=image
        )

        left = cx - image.width() // 2
        top = cy - image.height() // 2

        if not self.playing:
            for h in frame.hitboxes:
                    self.canvas.create_rectangle(
                        left + h.x1 * self.scale,
                        top + h.y1 * self.scale,
                        left + h.x2 * self.scale,
                        top + h.y2 * self.scale,
                        outline=self.box_color(h.box_type),
                        width=2
                    )

        self.status.config(
            text=f"Frame {self.current_frame + 1}/{len(self.animation.frames)} | BoxType {self.box_type}"
        )

    def box_color(self, t):

        colors = {
            0: "red",
            1: "blue",
            2: "white"
        }

        return colors.get(t, "black")

    # LOOP
    def loop(self):

        self.update_playback()
        self.draw_editor()

        self.root.after(1000 // FPS, self.loop)

    # SAVE
    def save(self):

        self.save_frame_data()

        filename = filedialog.asksaveasfilename(
            defaultextension=".json"
        )

        if not filename:
            return

        AnimationSerializer.save(
            self.animation,
            filename
        )

        messagebox.showinfo("Saved", filename)

# MAIN
if __name__ == "__main__":

    root = tk.Tk()

    app = AnimationEditor(root)

    root.mainloop()
