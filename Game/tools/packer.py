import os
import struct

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

def main():
    ## Executar em Game pelo vs code
    
    # Visuals
    pack_textures("Game/raw/visuals", "Game/assets/visuals")

    # Stages
    pack_textures("Game/raw/stages/Burning Dojo/sprites", "Game/assets/stages/Burning Dojo")
    pack_audio("Game/raw/stages/Burning Dojo/sounds", "Game/assets/stages/Burning Dojo")

    pack_textures("Game/raw/stages/Japan Fields/sprites", "Game/assets/stages/Japan Fields")
    pack_audio("Game/raw/stages/Japan Fields/sounds", "Game/assets/stages/Japan Fields")

    pack_textures("Game/raw/stages/Night Alley/sprites", "Game/assets/stages/Night Alley")
    pack_audio("Game/raw/stages/Night Alley/sounds", "Game/assets/stages/Night Alley")

    pack_textures("Game/raw/stages/NYC Subway/sprites", "Game/assets/stages/NYC Subway")
    pack_audio("Game/raw/stages/NYC Subway/sounds", "Game/assets/stages/NYC Subway")

    pack_textures("Game/raw/stages/Rindo-Kan Dojo/sprites", "Game/assets/stages/Rindo-Kan Dojo")
    pack_audio("Game/raw/stages/Rindo-Kan Dojo/sounds", "Game/assets/stages/Rindo-Kan Dojo")

    pack_textures("Game/raw/stages/The Midnight Duel/sprites", "Game/assets/stages/The Midnight Duel")
    pack_audio("Game/raw/stages/The Midnight Duel/sounds", "Game/assets/stages/The Midnight Duel")

    pack_textures("Game/raw/stages/The Savana/sprites", "Game/assets/stages/The Savana")
    pack_audio("Game/raw/stages/The Savana/sounds", "Game/assets/stages/The Savana")

    pack_textures("Game/raw/stages/Training Stage/sprites", "Game/assets/stages/Training Stage")
    pack_audio("Game/raw/stages/Training Stage/sounds", "Game/assets/stages/Training Stage")

    # Particles
    pack_textures("Game/raw/particles/Hitspark/sprites", "Game/assets/particles/Hitspark")
    pack_audio("Game/raw/particles/Hitspark/sounds", "Game/assets/particles/Hitspark")

    pack_textures("Game/raw/particles/Particle/sprites", "Game/assets/particles/Particle")
    pack_audio("Game/raw/particles/Particle/sounds", "Game/assets/particles/Particle")

    # Fonts
    pack_textures("Game/raw/fonts", "Game/assets/fonts")

    # Characters
    pack_textures("Game/raw/characters/Ken/sprites", "Game/assets/characters/Ken")
    pack_audio("Game/raw/characters/Ken/sounds", "Game/assets/characters/Ken")

    pack_textures("Game/raw/characters/Ken-Fireball/sprites", "Game/assets/characters/Ken-Fireball")
    pack_audio("Game/raw/characters/Ken-Fireball/sounds", "Game/assets/characters/Ken-Fireball")

main()
