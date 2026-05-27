import json
import os
import tkinter as tk
from dataclasses import dataclass, field
from tkinter import filedialog, messagebox, simpledialog

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
    sound: str = ""
    delta_x: int = 0
    delta_y: int = 0
    hitboxes: list = field(default_factory=list)
    has_hit: bool = True

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
                sound=f.sound,
                delta_x=f.delta_x,
                delta_y=f.delta_y,
                hitboxes=[
                    Hitbox(h.x1, h.y1, h.x2, h.y2, h.box_type)
                    for h in f.hitboxes
                ]
            )

            self.frames.insert(index + 1, new_frame)

class AnimationProject:
    def __init__(self):

        self.animations = {}

        self.filename = None

        self.sprite_folder = ""

    def resolve_sprite_path(self, sprite_name):

        sprite_name = sprite_name.split(":")[-1]

        return os.path.join(
            self.sprite_folder,
            sprite_name + ".png"
        )

    def create_animation(self, name):
        self.animations[name] = AnimationData()

    def delete_animation(self, name):
        if name in self.animations:
            del self.animations[name]

    def get_animation(self, name):

        return self.animations.get(name)

    def list_animations(self):

        return list(self.animations.keys())

    def clear(self):

        self.animations.clear()

    def load_json(self, filename):
        self.clear()

        with open(filename, "r") as f:
            data = json.load(f)

        for anim_name, frames_data in data.items():
            animation = AnimationData()

            for frame_data in frames_data:
                sprite = frame_data.get("Sprite_index", "")

                duration = max(
                    1,
                    int(frame_data.get("lenght", 1))
                )

                frame = FrameData(
                    image_path=self.resolve_sprite_path(sprite),
                    duration=duration,
                    facing=int(frame_data.get("facing", 1)),
                    sound=frame_data.get("Sound_index", ""),
                    delta_x=int(frame_data.get("DeltaX", 0)),
                    delta_y=int(frame_data.get("DeltaY", 0)),
                    has_hit=frame_data.get("hasHit", False)
                )

                for box in frame_data.get("Boxes", []):
                    pA = box.get("pA", {})
                    pB = box.get("pB", {})

                    frame.hitboxes.append(
                        Hitbox(
                            int(pA.get("x", 0)),
                            int(pA.get("y", 0)),
                            int(pB.get("x", 0)),
                            int(pB.get("y", 0)),
                            int(box.get("type", 0))
                        )
                    )

                animation.frames.append(frame)

            self.animations[anim_name] = animation

    def save_json(self, filename):
        output = {}
        for anim_name, animation in self.animations.items():

            output[anim_name] = []

            for frame in animation.frames:

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

                sprite_name = os.path.splitext(
                    os.path.basename(frame.image_path)
                )[0]

                output[anim_name].append({
                    "Sprite_index": sprite_name,
                    "DeltaX": frame.delta_x,
                    "DeltaY": frame.delta_y,
                    "Boxes": boxes,
                    "lenght": frame.duration,
                    "facing": frame.facing,
                    "Sound_index": frame.sound,
                    "hasHit": frame.has_hit
                })

        with open(filename, "w") as f:
            json.dump(output, f, indent=2)

class AnimationEditor:
    def __init__(self, root, project, anim_name):
        self.loading_frame_data = False
        self.root = root
        self.root.title("Animation Editor")
        self.project = project
        self.current_animation_name = anim_name
        self.animation = self.project.get_animation(anim_name)
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
        self.load_all_images()
        self.refresh_animation_selector()
        self.rebuild_timeline()
        self.load_frame_data()
        self.loop()

    def setup_ui(self):
        self.top = tk.Frame(self.root)
        self.top.pack(fill=tk.X)

        self.anim_var = tk.StringVar()

        self.anim_menu = tk.OptionMenu(self.top, self.anim_var, "")
        self.anim_menu.pack(side=tk.LEFT, padx=4)

        tk.Button(self.top, text="New Animation", command=self.create_animation).pack(side=tk.LEFT,)
        tk.Button(self.top, text="Delete Animation", command=self.delete_animation).pack(side=tk.LEFT)

        tk.Button(self.top, text="Export json", command=self.save_project).pack(side=tk.LEFT)

        tk.Button(self.top, text="Add", command=self.add_frames).pack(side=tk.LEFT)
        tk.Button(self.top, text="Duplicate", command=self.duplicate_current_frame).pack(side=tk.LEFT)
        tk.Button(self.top, text="Delete", command=self.delete_current_frame).pack(side=tk.LEFT)

        tk.Button(self.top, text="Copy last boxes", command=self.copy_last_frame_hitboxes).pack(side=tk.LEFT)

        self.box_button: tk.Button = tk.Button(self.top, text="Box: HITBOX", command=self.next_box_type)
        self.box_button.pack(side=tk.LEFT, padx=8)

        tk.Button(self.top, text="Play", command=self.toggle_play).pack(side=tk.LEFT, padx=8)

        self.duration_var = tk.StringVar(value="1")
        self.duration_entry = tk.Entry(self.top, width=5, textvariable=self.duration_var)
        self.duration_entry.pack(side=tk.RIGHT)
        tk.Label(self.top, text="Duration").pack(side=tk.RIGHT, padx=4)

        self.sound_var = tk.StringVar(value="")
        self.sound_entry = tk.Entry(self.top, width=20, textvariable=self.sound_var)
        self.sound_entry.pack(side=tk.RIGHT)
        tk.Label(self.top, text="Sound").pack(side=tk.RIGHT, padx=4)

        self.facing_button: tk.Button = tk.Button(self.top, text="Facing: " + str(len(self.animation.frames) > self.current_frame and self.animation.frames[self.current_frame].facing or 0), command=self.toggle_facing)
        self.facing_button.pack(side=tk.RIGHT, padx=4)

        self.has_hit_button: tk.Button = tk.Button(self.top, text="Has hit: " + str(len(self.animation.frames) > self.current_frame and self.animation.frames[self.current_frame].has_hit or False), command=self.toggle_has_hit)
        self.has_hit_button.pack(side=tk.RIGHT, padx=4)

        self.dx_var = tk.StringVar(value="0")
        self.dx_entry = tk.Entry(self.top, width=5, textvariable=self.dx_var)
        self.dx_entry.pack(side=tk.RIGHT)
        tk.Label(self.top, text="Delta X").pack(side=tk.RIGHT, padx=4)

        self.dy_var = tk.StringVar(value="0")
        self.dy_entry = tk.Entry(self.top, width=5, textvariable=self.dy_var)
        self.dy_entry.pack(side=tk.RIGHT)
        tk.Label(self.top, text="Delta Y").pack(side=tk.RIGHT, padx=4)

        self.canvas = tk.Canvas(self.root, width=CANVAS_W, height=CANVAS_H, bg="#202020")
        self.canvas.pack()

        self.timeline = tk.Frame(self.root)
        self.timeline.pack(fill=tk.X)

        self.status = tk.Label(self.root, text="")
        self.status.pack(fill=tk.X)

        self.duration_var.trace_add("write", lambda *args: self.on_field_change())
        self.sound_var.trace_add("write", lambda *args: self.on_field_change())
        self.dx_var.trace_add("write", lambda *args: self.on_field_change())
        self.dy_var.trace_add("write", lambda *args: self.on_field_change())

    def setup_bindings(self):
        self.root.bind("<Left>", lambda e: None if self.entry_has_focus() else self.change_delta(-1, 0))
        self.root.bind("<Right>", lambda e: None if self.entry_has_focus() else self.change_delta(1, 0))
        self.root.bind("<Up>", lambda e: None if self.entry_has_focus() else self.change_delta(0, -1))
        self.root.bind("<Down>", lambda e: None if self.entry_has_focus() else self.change_delta(0, 1))

        self.root.bind("q", lambda e: None if self.entry_has_focus() else self.change_box_type(-1))
        self.root.bind("e", lambda e: None if self.entry_has_focus() else self.change_box_type(1))

        self.root.bind("a", lambda e: None if self.entry_has_focus() else self.prev_frame())
        self.root.bind("d", lambda e: None if self.entry_has_focus() else self.next_frame())

        self.root.bind("w", lambda e: None if self.entry_has_focus() else self.change_duration(1))
        self.root.bind("s", lambda e: None if self.entry_has_focus() else self.change_duration(-1))

        self.root.bind("<Delete>", lambda e: None if self.entry_has_focus() else self.delete_last_hitbox())

        self.canvas.bind("<Button-1>", self.on_click)
        self.canvas.bind("<Button-3>", lambda e: self.change_box_type(1))
        self.canvas.bind("<Motion>", self.on_motion)

        self.root.bind("<space>", lambda e: None if self.entry_has_focus() else self.toggle_play())

        self.root.bind("<Button-1>", self.clear_focus, add="+")


    def toggle_has_hit(self):
        if not self.animation.frames:
            return

        frame: FrameData = self.animation.frames[self.current_frame]
        frame.has_hit = not frame.has_hit

        self.on_field_change()

    def toggle_facing(self):
        if not self.animation.frames:
            return

        frame: FrameData = self.animation.frames[self.current_frame]
        frame.facing *= -1

        self.on_field_change()

    def change_delta(self, dx, dy):
        if not self.animation.frames:
            return

        frame: FrameData = self.animation.frames[self.current_frame]

        frame.delta_x += dx
        frame.delta_y += dy

        self.dx_var.set(str(frame.delta_x))
        self.dy_var.set(str(frame.delta_y))
    
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

    def change_duration(self, delta):
        if not self.animation.frames:
            return

        frame = self.animation.frames[self.current_frame]

        frame.duration = max(1, frame.duration + delta)

        self.duration_var.set(str(frame.duration))

        self.rebuild_timeline()

    def on_field_change(self):

        if self.loading_frame_data:
            return

        self.save_frame_data()
        
        self.rebuild_timeline()

    def clear_focus(self, event):
        widget = event.widget

        entry_widgets = {
            self.duration_entry,
            self.sound_entry,
            self.dx_entry,
            self.dy_entry
        }

        if widget not in entry_widgets:
            self.root.focus_set()
            self.save_frame_data()

    def entry_has_focus(self):
        focused = self.root.focus_get()

        return focused in {
            self.duration_entry,
            self.sound_entry,
            self.dx_entry,
            self.dy_entry
        }

    def on_click(self, event):

        if not self.animation.frames:
            return

        frame_index = (
            self.play_frame
            if self.playing
            else self.current_frame
        )

        frame = self.animation.frames[frame_index]

        image = self.tk_cache.get(frame.image_path)

        if not image:
            return

        accumulated_x = 0
        accumulated_y = 0

        for i in range(frame_index):
            f = self.animation.frames[i]

            accumulated_x += f.delta_x
            accumulated_y += f.delta_y

        img_x = (CANVAS_W // 2) + (accumulated_x * self.scale)
        img_y = (CANVAS_H // 2) + (accumulated_y * self.scale)

        left = img_x - image.width() // 2
        top = img_y - image.height() // 2

        if self.box_start is None:

            self.box_start = (event.x, event.y)

        else:

            x1, y1 = self.box_start

            x2, y2 = event.x, event.y

            self.box_start = None

            frame.hitboxes.append(
                Hitbox(
                    (x1 - left) // self.scale,
                    (y1 - top) // self.scale,
                    (x2 - left) // self.scale,
                    (y2 - top) // self.scale,
                    self.box_type
                )
            )

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


    def refresh_animation_selector(self):

        menu = self.anim_menu["menu"]

        menu.delete(0, "end")

        for name in self.project.list_animations():

            menu.add_command(
                label=name,
                command=lambda n=name: self.load_animation_by_name(n)
            )

        self.anim_var.set(self.current_animation_name)

    def load_animation_by_name(self, name):

        self.save_frame_data()

        self.current_animation_name = name

        self.animation = self.project.get_animation(name)

        self.current_frame = 0

        self.load_all_images()

        self.rebuild_timeline()

        self.load_frame_data()

        self.anim_var.set(name)


    def create_animation(self):
        name = simpledialog.askstring(
            "Animation",
            "Animation name:"
        )

        if not name:
            return

        if name in self.project.animations:
            return

        self.project.create_animation(name)

        self.refresh_animation_selector()

        self.load_animation_by_name(name)

    def delete_animation(self):
        if not self.animation.frames:
            return

        confirm = messagebox.askyesno(
            "Delete Animation",
            f"Are you sure you want to delete the animation '{self.current_animation_name}'?"
        )

        if not confirm:
            return

        self.project.delete_animation(self.current_animation_name)

        self.refresh_animation_selector()


    def add_frames(self):

        files = filedialog.askopenfilenames(
            filetypes=[("Images", "*.png;*.jpg;*.jpeg")]
        )

        for f in files:

            self.animation.add_frame(f)

            self.load_image(f)

        self.rebuild_timeline()

    def load_all_images(self):

        for frame in self.animation.frames:
            self.load_image(frame.image_path)

    def load_image(self, path):

        if path in self.image_cache:
            return

        if not os.path.exists(path):
            return

        image = Image.open(path).convert("RGBA")

        image = image.resize((
            image.width * self.scale,
            image.height * self.scale
        ))

        self.image_cache[path] = image

        self.tk_cache[path] = ImageTk.PhotoImage(image)


    def rebuild_timeline(self):

        for w in self.timeline.winfo_children():
            w.destroy()

        for i, frame in enumerate(self.animation.frames):

            color = (
                "orange"
                if i == self.current_frame
                else "#404040"
            )

            b = tk.Button(self.timeline, text=f"{i}\n{frame.duration}f", bg=color, fg="white", command=lambda idx=i: self.select_frame(idx), width=6, height=3)
            b.pack(side=tk.LEFT)


    def select_frame(self, index):

        self.save_frame_data()

        self.current_frame = index

        self.load_frame_data()

        self.rebuild_timeline()

    def next_frame(self):

        if not self.animation.frames:
            return

        self.current_frame += 1

        if self.current_frame >= len(self.animation.frames):
            self.current_frame = 0

        self.load_frame_data()

        self.rebuild_timeline()

    def prev_frame(self):

        if not self.animation.frames:
            return

        self.current_frame -= 1

        if self.current_frame < 0:
            self.current_frame = len(self.animation.frames) - 1

        self.load_frame_data()

        self.rebuild_timeline()

    def duplicate_current_frame(self):

        if not self.animation.frames:
            return

        self.animation.duplicate_frame(self.current_frame)

        self.rebuild_timeline()

    def delete_current_frame(self):

        if not self.animation.frames:
            return

        self.animation.remove_frame(self.current_frame)

        self.current_frame = max(
            0,
            min(
                self.current_frame,
                len(self.animation.frames) - 1
            )
        )

        self.rebuild_timeline()


    def load_frame_data(self):

        if not self.animation.frames:
            return

        self.loading_frame_data = True

        frame: FrameData = self.animation.frames[self.current_frame]

        self.duration_var.set(str(frame.duration))
        self.dx_var.set(str(frame.delta_x))
        self.dy_var.set(str(frame.delta_y))
        self.sound_var.set(str(frame.sound))
        self.has_hit_button.config(text="Has hit: " + str(frame.has_hit))
        self.facing_button.config(text="Facing: " + str(frame.facing))

        self.loading_frame_data = False

    def save_frame_data(self):
        if not self.animation.frames:
            return

        frame: FrameData = self.animation.frames[self.current_frame]

        try:
            frame.duration = int(self.duration_var.get())
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

        try:
            frame.sound = str(self.sound_var.get())
        except:
            pass

        self.has_hit_button.config(text="Has hit: " + str(frame.has_hit))
        self.facing_button.config(text="Facing: " + str(frame.facing))


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

            self.play_tick = 0

            self.play_frame += 1

            if self.play_frame >= len(self.animation.frames):
                self.play_frame = 0

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
    

    def draw_grid(self):
        center_x = CANVAS_W // 2
        center_y = CANVAS_H // 2

        start_x = center_x % GRID_SIZE
        start_y = center_y % GRID_SIZE

        for x in range(start_x, CANVAS_W, GRID_SIZE):

            color = "#505050" if x == center_x else "#303030"

            self.canvas.create_line(
                x,
                0,
                x,
                CANVAS_H,
                fill=color
            )

        for y in range(start_y, CANVAS_H, GRID_SIZE):

            color = "#505050" if y == center_y else "#303030"

            self.canvas.create_line(
                0,
                y,
                CANVAS_W,
                y,
                fill=color
            )

        self.canvas.create_line(
            center_x,
            0,
            center_x,
            CANVAS_H,
            fill="red",
            width=2
        )

        self.canvas.create_line(
            0,
            center_y,
            CANVAS_W,
            center_y,
            fill="red",
            width=2
        )

    def draw_editor(self):
        self.canvas.delete("all")
        self.draw_grid()

        if not self.animation.frames:
            return

        frame_index = (
            self.play_frame
            if self.playing
            else self.current_frame
        )

        frame = self.animation.frames[frame_index]
        image = self.tk_cache.get(frame.image_path)

        if not image:
            return

        accumulated_x = 0
        accumulated_y = 0

        for i in range(frame_index + 1):
            f = self.animation.frames[i]

            accumulated_x += f.delta_x
            accumulated_y += f.delta_y

        cx = (CANVAS_W // 2) + (accumulated_x * self.scale)
        cy = (CANVAS_H // 2) + (accumulated_y * self.scale)

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
            text=f"{self.current_animation_name} | Frame {self.current_frame + 1}/{len(self.animation.frames)}"
        )

    def box_color(self, t):

        colors = {
            0: "red",
            1: "blue",
            2: "white"
        }

        return colors.get(t, "white")


    def loop(self):

        self.update_playback()

        self.draw_editor()

        self.root.after(
            1000 // FPS,
            self.loop
        )

    def save_project(self):

        self.save_frame_data()

        filename = filedialog.asksaveasfilename(
            defaultextension=".json"
        )

        if not filename:
            return

        self.project.save_json(filename)

        messagebox.showinfo(
            "Saved",
            filename
        )

class ProjectSelector:
    def __init__(self, root):

        self.root = root

        self.project = AnimationProject()

        self.frame = tk.Frame(root)

        self.frame.pack(
            fill=tk.BOTH,
            expand=True,
            padx=16,
            pady=16
        )

        tk.Button(self.frame, text="New Project", width=30, height=2, command=self.new_project).pack(pady=8)

        tk.Button(self.frame, text="Open Project", width=30, height=2, command=self.open_project).pack(pady=8)

    def new_project(self):

        name = simpledialog.askstring(
            "Animation",
            "First animation name:"
        )

        if not name:
            return

        self.project.create_animation(name)

        self.open_editor(name)

    def open_project(self):

        filename = filedialog.askopenfilename(
            filetypes=[("Json", "*.json")]
        )

        if not filename:
            return

        sprite_folder = filedialog.askdirectory(
            title="Select Sprite Folder"
        )

        if not sprite_folder:
            return

        self.project.sprite_folder = sprite_folder

        self.project.load_json(filename)

        names = self.project.list_animations()

        if not names:
            return

        self.open_editor(names[0])

    def open_editor(self, anim_name):

        self.frame.destroy()

        AnimationEditor(
            self.root,
            self.project,
            anim_name
        )

if __name__ == "__main__":

    root = tk.Tk()

    ProjectSelector(root)

    root.mainloop()