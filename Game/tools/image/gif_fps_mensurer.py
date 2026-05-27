from PIL import Image
import numpy as np
from tkinter import Tk, filedialog

FPS = 60
DIFF_THRESHOLD = 0


def frames_equal(a, b, threshold=0):
    diff = np.abs(a.astype(np.int16) - b.astype(np.int16))
    return np.max(diff) <= threshold


# abre explorador de arquivos
root = Tk()
root.withdraw()

gif_path = filedialog.askopenfilename(
    title="Selecione o GIF",
    filetypes=[("GIF files", "*.gif")]
)

if not gif_path:
    print("Nenhum arquivo selecionado.")
    exit()


gif = Image.open(gif_path)

frames = []
durations = []

try:
    while True:
        frame = gif.convert("RGBA")

        frames.append(np.array(frame))
        durations.append(gif.info.get("duration", 100))

        gif.seek(gif.tell() + 1)

except EOFError:
    pass


unique_frames = []
frame_mapping = []

for frame in frames:
    found = False

    for i, unique in enumerate(unique_frames):
        if frames_equal(frame, unique, DIFF_THRESHOLD):
            frame_mapping.append(i)
            found = True
            break

    if not found:
        unique_frames.append(frame)
        frame_mapping.append(len(unique_frames) - 1)


print("\n=== FRAMES ÚNICOS ===")
print(len(unique_frames))

print("\n=== SEQUÊNCIA DA ANIMAÇÃO ===")
print(frame_mapping)

print("\n=== REPETIÇÕES CONSECUTIVAS ===")

runs = []

current = frame_mapping[0]
count = 1

for f in frame_mapping[1:]:
    if f == current:
        count += 1
    else:
        runs.append((current, count))
        current = f
        count = 1

runs.append((current, count))

for frame_id, count in runs:
    print(f"Frame {frame_id}: {count}x")


print("\n=== DURAÇÃO EM FRAMES @60FPS ===")

for i, duration_ms in enumerate(durations):
    game_frames = round((duration_ms / 1000) * FPS)

    print(
        f"GIF frame {i:02d} "
        f"=> {game_frames} frame(s)"
    )

input()