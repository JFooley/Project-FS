import os
import re
from PIL import Image

def merge_frames():
    diretorio = os.getcwd()

    # Pega todos os PNGs
    arquivos = [
        f for f in os.listdir(diretorio)
        if f.lower().endswith(".png")
    ]

    # Natural sort
    arquivos.sort(
        key=lambda s: [
            int(text) if text.isdigit() else text.lower()
            for text in re.split(r'(\d+)', s)
        ]
    )

    if not arquivos:
        print("Nenhum PNG encontrado.")
        return

    frames = []

    for arquivo in arquivos:
        img = Image.open(arquivo).convert("RGBA")

        frame = Image.new("RGBA", img.size, (0, 0, 0, 0))
        frame.alpha_composite(img)

        frames.append(frame)

    gif_frames = []

    for frame in frames:
        pal = frame.convert("P", palette=Image.ADAPTIVE)

        mask = Image.eval(
            frame.getchannel("A"),
            lambda a: 255 if a <= 128 else 0
        )

        pal.paste(255, mask)

        pal.info["transparency"] = 255
        pal.info["disposal"] = 2

        gif_frames.append(pal)

    gif_frames[0].save(
        "result.gif",
        save_all=True,
        append_images=gif_frames[1:],
        loop=0,
        duration=100,
        optimize=False,
        disposal=2
    )

    print("GIF criado com sucesso.")

merge_frames()
