import json
import re
from pathlib import Path
import librosa
import numpy as np
from kokoro import KPipeline
import soundfile as sf

###### ONLY WORKS WITH PYTHON <3.13

INPUT_DIR = Path("assets/languages")
OUTPUT_DIR = Path("raw/languages")
AUDIO_EXTENSION = "wav"
SAMPLE_RATE = 24000
VOICE_MAP = {
    "PT": "pf_dora",
    "EN": "af_heart",
    "ES": "ef_dora",
}

def sanitize_filename(text: str) -> str:
    """
    Converte a chave em um nome de arquivo válido.
    """
    text = text.lower().strip()

    text = re.sub(r"[^\w\s-]", "", text)
    text = re.sub(r"[\s]+", "_", text)

    return text

def load_pipeline(language_code: str):
    """
    Cria pipeline Kokoro para o idioma.
    """
    lang = language_code.lower()

    if lang.startswith("pt"):
        lang = "p"
    elif lang.startswith("en"):
        lang = "a"
    elif lang.startswith("es"):
        lang = "e"
    elif lang.startswith("jp") or lang.startswith("ja"):
        lang = "j"
    elif lang.startswith("ru"):
        lang = "r"
    else:
        raise ValueError(f"Idioma não suportado: {language_code}")

    return KPipeline(lang_code=lang)

def generate_audio(pipeline, voice: str, text: str, output_path: Path):
    """
    Gera áudio utilizando Kokoro.
    """

    generator = pipeline(
        text,
        voice=voice,
        speed=1.0,
        split_pattern=r"\n+"
    )

    audio_chunks = []

    for _, _, audio in generator:
        audio_chunks.extend(audio)

    audio_array = np.array(audio_chunks, dtype=np.float32)

    # Remove silêncio do início e fim
    trimmed_audio, _ = librosa.effects.trim(
        audio_array,
        top_db=35
    )

    sf.write(
        str(output_path),
        trimmed_audio,
        SAMPLE_RATE
    )

def process_translation_file(json_path: Path):

    language_code = json_path.stem.upper()

    if language_code not in VOICE_MAP:
        print(f"[IGNORADO] Voz não configurada para {language_code}")
        return

    print(f"\n=== Processando idioma: {language_code} ===")

    voice = VOICE_MAP[language_code]

    pipeline = load_pipeline(language_code)

    with open(json_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    output_lang_dir = OUTPUT_DIR / language_code
    output_lang_dir.mkdir(parents=True, exist_ok=True)

    total = len(data)
    current = 0

    for key, translated_text in data.items():

        current += 1

        if not translated_text.strip():
            continue

        filename = sanitize_filename(key)
        output_file = output_lang_dir / f"{filename}.{AUDIO_EXTENSION}"

        if output_file.exists():
            print(f"[{current}/{total}] Pulando existente: {output_file.name}")
            continue

        try:
            print(f"[{current}/{total}] Gerando {language_code}: {output_file.name}")

            generate_audio(
                pipeline=pipeline,
                voice=voice,
                text=translated_text,
                output_path=output_file
            )

        except Exception as e:
            print(f"Erro em '{key}': {e}")

def main():

    if not INPUT_DIR.exists():
        print(f"Pasta não encontrada: {INPUT_DIR}")
        return

    json_files = list(INPUT_DIR.glob("*.json"))

    if not json_files:
        print("Nenhum arquivo json encontrado.")
        return

    for json_file in json_files:
        process_translation_file(json_file)

    print("\nConcluído.")

if __name__ == "__main__":
    main()