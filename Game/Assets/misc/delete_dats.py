import os
import glob

def delete_dat_files():
    # Define o diretório pai (um nível acima)
    parent_dir = os.path.join('..')
    
    # Encontra todos os arquivos .dat recursivamente
    dat_files = glob.glob(os.path.join(parent_dir, '**', '*.dat'), recursive=True)
    
    # Mostra os arquivos que serão deletados
    print(f"Encontrados {len(dat_files)} arquivo(s) .dat:")
    for file_path in dat_files:
        print(f"  - {file_path}")
    
    # Confirmação antes de deletar
    if dat_files:
        confirm = input("\nDeseja apagar todos estes arquivos? (s/N): ")
        if confirm.lower() == 's':
            # Deleta os arquivos
            deleted_count = 0
            for file_path in dat_files:
                try:
                    os.remove(file_path)
                    print(f"Deletado: {file_path}")
                    deleted_count += 1
                except Exception as e:
                    print(f"Erro ao deletar {file_path}: {e}")
            
            print(f"\nTotal de arquivos deletados: {deleted_count}")
        else:
            print("Operação cancelada.")
    else:
        print("Nenhum arquivo .dat encontrado.")

if __name__ == "__main__":
    delete_dat_files()