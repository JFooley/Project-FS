import re
import json

def split_top_level(s, sep=','):
    parts = []
    current = ""
    depth_paren = 0
    depth_brace = 0

    for c in s:
        if c == '(':
            depth_paren += 1
        elif c == ')':
            depth_paren -= 1
        elif c == '{':
            depth_brace += 1
        elif c == '}':
            depth_brace -= 1

        if c == sep and depth_paren == 0 and depth_brace == 0:
            parts.append(current.strip())
            current = ""
        else:
            current += c

    if current.strip():
        parts.append(current.strip())

    return parts

def parse_boxes(text):
    boxes = []

    matches = re.findall(r'new\s+GenericBox\s*\((.*?)\)', text)

    for m in matches:
        args = split_top_level(m)

        if len(args) != 5:
            continue

        box = {
            "type": int(args[0]),
            "pA": {"x": float(args[1]), "y": float(args[2])},
            "pB": {"x": float(args[3]), "y": float(args[4])},
        }

        boxes.append(box)

    return boxes

def parse_frame(frame_text):
    inner = frame_text.strip()

    # remove "new FrameData("
    inner = re.sub(r'^new\s+FrameData\s*\(', '', inner)
    inner = re.sub(r'\)\s*,?$', '', inner)

    # extrai boxes antes (remove bloco completo)
    boxes_match = re.search(r'boxes\s*:\s*new\s+List<GenericBox>\s*\{(.*?)\}', inner)
    boxes_text = boxes_match.group(1) if boxes_match else ""
    boxes = parse_boxes(boxes_text)

    if boxes_match:
        inner = inner.replace(boxes_match.group(0), "BOXES")

    tokens = split_top_level(inner)

    sprite_index = tokens[0].strip().replace('"', '')
    delta_x = float(tokens[1].replace('f', ''))
    delta_y = float(tokens[2].replace('f', ''))

    length = 1
    sound = ""
    has_hit = True

    for t in tokens[3:]:
        if t.startswith("len:"):
            length = int(t.split(":", 1)[1])

        elif t.startswith("sound:"):
            sound = t.split(":", 1)[1].strip().replace('"', '')

        elif t.startswith("hasHit:"):
            has_hit = t.split(":", 1)[1].strip().lower() == "true"

    return {
        "Sprite_index": sprite_index,
        "DeltaX": delta_x,
        "DeltaY": delta_y,
        "Boxes": boxes,
        "lenght": length,
        "Sound_index": sound,
        "hasHit": has_hit
    }

def parse_file(text):
    result = {}

    # captura listas tipo: var introFrames = new List<Frame> { ... };
    lists = re.findall(
        r'var\s+(\w+)\s*=\s*new\s+List<Frame>\s*\{(.*?)\};',
        text,
        re.S
    )

    for name, body in lists:
        frames_raw = split_top_level(body)

        frames = []
        for f in frames_raw:
            if "new FrameData" in f:
                frames.append(parse_frame(f))

        result[name] = frames

    return result

input_text = """
"""

parsed = parse_file(input_text)

with open("animations.json", "w", encoding="utf-8") as f:
    json.dump(parsed, f, indent=4, ensure_ascii=False)

print("OK -> animations.json gerado")