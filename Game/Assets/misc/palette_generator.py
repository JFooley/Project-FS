import os
import tkinter as tk
from tkinter import filedialog
from PIL import Image
import numpy as np
import sys
import colorsys

def selecionar_pasta():
    """Abre diálogo para selecionar pasta"""
    root = tk.Tk()
    root.withdraw()
    pasta = filedialog.askdirectory(title="Selecione a pasta com as imagens")
    return pasta

def atualizar_progresso(atual, total, cores_encontradas):
    """Atualiza a barra de progresso no console"""
    barra_largura = 40
    percentual = atual / total
    barras = int(barra_largura * percentual)
    espacos = barra_largura - barras
    
    sys.stdout.write('\r')
    sys.stdout.write(f"[{'█' * barras}{' ' * espacos}] ")
    sys.stdout.write(f"{percentual*100:5.1f}% ")
    sys.stdout.write(f"({atual}/{total} imagens) ")
    sys.stdout.write(f"| Cores: {cores_encontradas}")
    sys.stdout.flush()

def rgb_to_hsv_normalized(rgb):
    """Converte RGB para HSV normalizado (0-1)"""
    r, g, b = rgb[0]/255.0, rgb[1]/255.0, rgb[2]/255.0
    h, s, v = colorsys.rgb_to_hsv(r, g, b)
    return (h, s, v)

def organizar_cores_por_hue_luminosidade(cores_rgb):
    """
    Organiza cores por HUE (matiz) e depois por luminosidade (value)
    Ignora completamente o canal alfa
    """
    # Converter para lista de tuplas RGB (garantir 3 componentes)
    cores_limpas = []
    for cor in cores_rgb:
        if len(cor) >= 3:
            # Pegar apenas R, G, B (ignorar alpha se existir)
            cores_limpas.append((cor[0], cor[1], cor[2]))
    
    # Remover duplicatas
    cores_limpas = list(set(cores_limpas))
    
    # Converter para HSV para ordenação
    cores_com_hsv = []
    for rgb in cores_limpas:
        h, s, v = rgb_to_hsv_normalized(rgb)
        cores_com_hsv.append({
            'rgb': rgb,
            'h': h,      # Hue (0-1)
            's': s,      # Saturação (0-1)
            'v': v       # Valor/Luminosidade (0-1)
        })
    
    # Ordenar primariamente por HUE (matiz)
    # Para cores com HUE similar, ordenar por luminosidade (mais claro primeiro)
    cores_ordenadas_hsv = sorted(cores_com_hsv, key=lambda x: (x['h'], -x['v']))
    
    # Extrair apenas RGB ordenado
    cores_ordenadas_rgb = [cor['rgb'] for cor in cores_ordenadas_hsv]
    
    return cores_ordenadas_rgb

def gerar_paleta_unidimensional_organizada(pasta_imagens, arquivo_saida='paleta_hue.bmp'):
    """
    Gera uma paleta unidimensional com cores RGB organizadas por HUE e luminosidade
    Ignora completamente o canal alfa
    Dimensões: (num_cores x 1)
    Saída sempre em formato BMP
    """
    
    if not os.path.exists(pasta_imagens):
        print(f"Pasta não encontrada: {pasta_imagens}")
        return
    
    # Garantir que a saída seja BMP
    if not arquivo_saida.lower().endswith('.bmp'):
        arquivo_saida = arquivo_saida.rsplit('.', 1)[0] + '.bmp'
    
    # Estrutura para armazenar cores únicas (apenas RGB, sem alfa)
    cores_set = set()
    
    # Listar arquivos de imagem
    extensoes = {'.png', '.jpg', '.jpeg', '.bmp', '.gif', '.tiff', '.webp'}
    arquivos = []
    
    for arquivo in os.listdir(pasta_imagens):
        if os.path.splitext(arquivo)[1].lower() in extensoes:
            arquivos.append(arquivo)
    
    if not arquivos:
        print("Nenhuma imagem encontrada na pasta!")
        return
    
    total_imagens = len(arquivos)
    print(f"Encontradas {total_imagens} imagens para processar")
    print("=" * 60)
    
    # Processar cada imagem com barra de progresso
    for i, arquivo in enumerate(arquivos, 1):
        caminho = os.path.join(pasta_imagens, arquivo)
        
        try:
            with Image.open(caminho) as img:
                # Converter sempre para RGB (descartar alpha completamente)
                img_rgb = img.convert('RGB')
                
                # Converter para array numpy
                img_array = np.array(img_rgb)
                
                # Adicionar cores ao set (apenas RGB)
                for linha in img_array:
                    for pixel in linha:
                        # Adicionar como tupla RGB (sem alpha)
                        cores_set.add(tuple(pixel))
                
                # Atualizar progresso
                atualizar_progresso(i, total_imagens, len(cores_set))
                
        except Exception as e:
            print(f"\nErro ao processar {arquivo}: {e}")
            atualizar_progresso(i, total_imagens, len(cores_set))
    
    # Linha final
    print("\n" + "=" * 60)
    
    # Converter set para lista
    cores_lista = list(cores_set)
    
    if not cores_lista:
        print("Nenhuma cor encontrada!")
        return
    
    # ORGANIZAR CORES POR HUE E LUMINOSIDADE
    print("🎨 Organizando cores por HUE → Luminosidade...")
    cores_organizadas = organizar_cores_por_hue_luminosidade(cores_lista)
    
    # Calcular dimensões para paleta UNIDIMENSIONAL
    num_cores = len(cores_organizadas)
    largura = num_cores
    altura = 1  # Altura fixa de 1 pixel
    
    print(f"\n📐 Criando paleta {largura}x{altura} pixels")
    print(f"   (Imagem unidimensional - cada cor ocupa 1 pixel de largura)")
    print(f"   Organização: HUE → Luminosidade (mais claro primeiro)")
    print(f"   Formato de saída: BMP")
    print(f"   Alpha: IGNORADO (apenas RGB)")
    
    # Criar imagem unidimensional RGB (sem alpha)
    paleta_img = Image.new('RGB', (largura, altura), color=(0, 0, 0))
    pixels_paleta = paleta_img.load()
    
    print(f"\n🎨 Salvando paleta em BMP...")
    
    # Preencher com cores ORGANIZADAS
    for idx, cor in enumerate(cores_organizadas):
        # Em uma imagem 1D, todas as cores estão na linha y=0
        pixels_paleta[idx, 0] = cor  # Já é RGB (sem alpha)
    
    # Salvar paleta sempre como BMP
    paleta_img.save(arquivo_saida, 'BMP')
    
    print(f"\n✅ Paleta organizada salva como: {arquivo_saida}")
    print(f"📏 Dimensões: {largura}x{altura} pixels")
    print(f"📦 Formato: BMP")
    
    # Mostrar amostra das cores organizadas
    print(f"\n🌈 Amostra de cores organizadas (primeiras 15):")
    print("-" * 70)
    print("Idx |   R   G   B   |   HUE°   Sat%  Lum%")
    print("-" * 70)
    
    for i, cor in enumerate(cores_organizadas[:15]):
        r, g, b = cor
        h, s, v = rgb_to_hsv_normalized(cor)
        hue_deg = int(h * 360)
        sat_percent = int(s * 100)
        lum_percent = int(v * 100)
        print(f"{i+1:3d} | {r:3d} {g:3d} {b:3d} | {hue_deg:6d}° {sat_percent:5d}% {lum_percent:5d}%")
    
    if len(cores_organizadas) > 15:
        print(f"  ... e mais {len(cores_organizadas) - 15} cores")
    
    return arquivo_saida

def main():
    print("=" * 60)
    print("🎨 GERADOR DE PALETA UNIDIMENSIONAL RGB 🎨")
    print("=" * 60)
    print("Cores organizadas por: HUE → Luminosidade")
    print("Alpha: IGNORADO (apenas RGB)")
    print("Formato de saída: BMP")
    print("Dimensões: Largura = Nº de cores, Altura = 1")
    print()
    
    # Selecionar pasta
    print("📁 Selecione a pasta com suas imagens/sprites...")
    pasta = selecionar_pasta()
    
    if not pasta:
        print("❌ Nenhuma pasta selecionada.")
        return
    
    print(f"📂 Pasta selecionada: {os.path.basename(pasta)}")
    print()
    
    # Perguntar nome do arquivo de saída (sempre BMP)
    nome_saida = "palette.bmp"
    
    print()
    
    # Gerar paleta
    arquivo_paleta = gerar_paleta_unidimensional_organizada(pasta, nome_saida)
    
    print()
    print("=" * 60)
    input("🎉 Pressione Enter para sair...")

if __name__ == "__main__":
    main()