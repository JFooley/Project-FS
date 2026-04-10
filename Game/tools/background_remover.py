import os
import glob
from PIL import Image

def remove_background(image_path):
    # Abre a imagem
    img = Image.open(image_path).convert("RGBA")
    data = img.getdata()
    
    # Obtém a cor do primeiro pixel do canto superior esquerdo
    background_color = data[0]
    
    # Cria uma nova lista para os dados da imagem
    new_data = []
    
    for item in data:
        # Se o pixel corresponde à cor de fundo, torna-o transparente
        if item[:3] == background_color[:3]:
            new_data.append((255, 255, 255, 0))
        else:
            new_data.append(item)
    
    # Atualiza a imagem com os novos dados
    img.putdata(new_data)
    
    # Salva a imagem com o fundo removido, substituindo a original
    img.save(image_path, "PNG")

def process_all_pngs_in_folder(folder_path):
    # Obtém todos os arquivos PNG na pasta
    png_files = glob.glob(os.path.join(folder_path, "*.png"))
    
    for png_file in png_files:
        remove_background(png_file)
        print(f"Processed {png_file}")

# Exemplo de uso:
current_folder = os.path.dirname(os.path.realpath(__file__))
process_all_pngs_in_folder(current_folder)
