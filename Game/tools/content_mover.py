import os
from PIL import Image

# Solicitar o deslocamento X e Y ao usuário
deslocamento_x = int(input("Deslocamento X: "))
deslocamento_y = int(input("Deslocamento Y: "))

# Caminho da pasta onde o script está sendo executado
caminho_pasta = os.getcwd()

# Itera sobre todos os arquivos na pasta
for arquivo in os.listdir(caminho_pasta):
    # Verifica se o arquivo é uma imagem .png
    if arquivo.endswith(".png"):
        caminho_imagem = os.path.join(caminho_pasta, arquivo)
        
        # Abre a imagem
        imagem = Image.open(caminho_imagem)
        
        # Cria uma nova imagem do mesmo tamanho, com fundo transparente
        nova_imagem = Image.new("RGBA", imagem.size, (0, 0, 0, 0))
        
        # Desloca a imagem original em X e Y
        nova_imagem.paste(imagem, (deslocamento_x, deslocamento_y))
        
        # Salva a nova imagem sobre a antiga
        nova_imagem.save(caminho_imagem)

print("Imagens deslocadas e salvas com sucesso!")
