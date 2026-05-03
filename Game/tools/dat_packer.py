import os
import struct
import json

IMAGE_EXTENSIONS = {".png", ".jpg", ".jpeg", ".bmp"}
AUDIO_EXTENSIONS = {".wav", ".ogg", ".flac", ".mp3"}


def _save_dat(file_path, data_dict):
    with open(file_path, "wb") as f:
        f.write(struct.pack("<I", len(data_dict)))

        for key, data in data_dict.items():
            write_csharp_string(f, key)

            f.write(struct.pack("<I", len(data)))
            f.write(data)

def write_csharp_string(f, s: str):
    data = s.encode("utf-8")
    length = len(data)

    # 7-bit encoded int (igual BinaryWriter)
    while True:
        b = length & 0x7F
        length >>= 7
        if length:
            f.write(bytes([b | 0x80]))
        else:
            f.write(bytes([b]))
            break

    f.write(data)

def _load_textures(folder):
    textures = {}

    for root, _, files in os.walk(folder):
        for file in files:
            ext = os.path.splitext(file)[1].lower()

            if ext in IMAGE_EXTENSIONS:
                full_path = os.path.join(root, file)

                rel = os.path.relpath(full_path, folder).replace("\\", "/")
                key = rel.rsplit(".", 1)[0].replace("/", ":")

                with open(full_path, "rb") as f:
                    textures[key] = f.read()

    return textures

def pack_textures(folder, dat_folder):
    textures = _load_textures(folder)
    output = os.path.join(dat_folder, "textures.dat")

    _save_dat(output, textures)

    print(f"[TEXTURES] {len(textures)} packed -> {output}")

def _load_audio(folder):
    audio = {}

    for root, _, files in os.walk(folder):
        for file in files:
            ext = os.path.splitext(file)[1].lower()

            if ext in AUDIO_EXTENSIONS:
                full_path = os.path.join(root, file)

                rel = os.path.relpath(full_path, folder).replace("\\", "/")
                key = rel.rsplit(".", 1)[0].replace("/", ":")

                with open(full_path, "rb") as f:
                    audio[key] = f.read()

    return audio

def pack_audio(folder, dat_folder):
    audio = _load_audio(folder)
    output = os.path.join(dat_folder, "audio.dat")

    _save_dat(output, audio)

    print(f"[AUDIO] {len(audio)} packed -> {output}")

def _write_animation_dat(file_path, data):
    with open(file_path, "wb") as f:
        # quantidade de animações
        f.write(struct.pack("<I", len(data)))

        for anim_name, frames in data.items():
            write_csharp_string(f, anim_name)

            # quantidade de frames
            f.write(struct.pack("<I", len(frames)))

            for frame in frames:
                # sprite
                write_csharp_string(f, frame.get("Sprite_index", ""))

                # delta
                f.write(struct.pack("<f", frame.get("DeltaX", 0.0)))
                f.write(struct.pack("<f", frame.get("DeltaY", 0.0)))

                # length
                f.write(struct.pack("<I", frame.get("lenght", 1)))

                # sound
                write_csharp_string(f, frame.get("Sound_index", ""))

                # hasHit
                f.write(struct.pack("<?", frame.get("hasHit", True)))

                # boxes
                boxes = frame.get("Boxes", [])
                f.write(struct.pack("<I", len(boxes)))

                for b in boxes:
                    f.write(struct.pack("<I", b["type"]))

                    f.write(struct.pack("<f", b["pA"]["x"]))
                    f.write(struct.pack("<f", b["pA"]["y"]))

                    f.write(struct.pack("<f", b["pB"]["x"]))
                    f.write(struct.pack("<f", b["pB"]["y"]))

def pack_animations(json_path, dat_folder, name="animations.dat"):
    with open(json_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    output = os.path.join(dat_folder, name)

    _write_animation_dat(output, data)

    print(f"[ANIMATIONS] {len(data)} animations packed -> {output}")

def main():
    ## Executar em Game pelo vs code com a pasta aberta em Game (nao em Project FS)
    
    # Visuals
    pack_textures("raw/visuals", "assets/visuals")

    # Sounds
    pack_audio("raw/sounds", "assets/sounds")

    # Fonts
    pack_textures("raw/fonts", "assets/fonts")

    # Stages
    pack_textures("raw/stages/Burning Dojo/sprites", "assets/stages/Burning Dojo")
    pack_audio("raw/stages/Burning Dojo/sounds", "assets/stages/Burning Dojo")

    pack_textures("raw/stages/Japan Fields/sprites", "assets/stages/Japan Fields")
    pack_audio("raw/stages/Japan Fields/sounds", "assets/stages/Japan Fields")

    pack_textures("raw/stages/Night Alley/sprites", "assets/stages/Night Alley")
    pack_audio("raw/stages/Night Alley/sounds", "assets/stages/Night Alley")

    pack_textures("raw/stages/NYC Subway/sprites", "assets/stages/NYC Subway")
    pack_audio("raw/stages/NYC Subway/sounds", "assets/stages/NYC Subway")

    pack_textures("raw/stages/Rindo-Kan Dojo/sprites", "assets/stages/Rindo-Kan Dojo")
    pack_audio("raw/stages/Rindo-Kan Dojo/sounds", "assets/stages/Rindo-Kan Dojo")

    pack_textures("raw/stages/The Midnight Duel/sprites", "assets/stages/The Midnight Duel")
    pack_audio("raw/stages/The Midnight Duel/sounds", "assets/stages/The Midnight Duel")

    pack_textures("raw/stages/The Savana/sprites", "assets/stages/The Savana")
    pack_audio("raw/stages/The Savana/sounds", "assets/stages/The Savana")

    pack_textures("raw/stages/Training Stage/sprites", "assets/stages/Training Stage")
    pack_audio("raw/stages/Training Stage/sounds", "assets/stages/Training Stage")

    # Particles
    pack_animations("raw/particles/Hitspark/animations.json", "assets/particles/Hitspark")
    pack_textures("raw/particles/Hitspark/sprites", "assets/particles/Hitspark")
    pack_audio("raw/particles/Hitspark/sounds", "assets/particles/Hitspark")

    pack_animations("raw/particles/Super/animations.json", "assets/particles/Super")
    pack_textures("raw/particles/Super/sprites", "assets/particles/Super")
    pack_audio("raw/particles/Super/sounds", "assets/particles/Super")

    pack_animations("raw/particles/Shungokusatso/animations.json", "assets/particles/Shungokusatso")
    pack_textures("raw/particles/Shungokusatso/sprites", "assets/particles/Shungokusatso")
    pack_audio("raw/particles/Shungokusatso/sounds", "assets/particles/Shungokusatso")

    # Characters
    pack_animations("raw/characters/Ken/animations.json", "assets/characters/Ken")
    pack_textures("raw/characters/Ken/sprites", "assets/characters/Ken")
    pack_audio("raw/characters/Ken/sounds", "assets/characters/Ken")

    pack_animations("raw/characters/Ken-Fireball/animations.json", "assets/characters/Ken-Fireball")
    pack_textures("raw/characters/Ken-Fireball/sprites", "assets/characters/Ken-Fireball")
    pack_audio("raw/characters/Ken-Fireball/sounds", "assets/characters/Ken-Fireball")

main()
