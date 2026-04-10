import os
import json
from PIL import Image, ImageTk
import tkinter as tk
from tkinter import filedialog, messagebox

class AnimationData:
    """Classe para armazenar os dados da animação"""
    def __init__(self):
        self.frames_data = []  # Lista de dicionários com dados de cada frame
    
    def add_frame(self, file_path, movement_x, movement_y, hitboxes):
        """Adiciona dados de um frame"""
        frame_data = {
            'file': file_path,
            'movement_x': movement_x,
            'movement_y': movement_y,
            'hitboxes': hitboxes.copy() if hitboxes else []
        }
        self.frames_data.append(frame_data)
    
    def update_frame(self, frame_index, movement_x, movement_y, hitboxes):
        """Atualiza dados de um frame existente"""
        if 0 <= frame_index < len(self.frames_data):
            self.frames_data[frame_index]['movement_x'] = movement_x
            self.frames_data[frame_index]['movement_y'] = movement_y
            self.frames_data[frame_index]['hitboxes'] = hitboxes.copy() if hitboxes else []
    
    def get_frame(self, frame_index):
        """Retorna dados de um frame"""
        if 0 <= frame_index < len(self.frames_data):
            return self.frames_data[frame_index]
        return None
    
    def save_to_file(self, filename):
        """Salva os dados em um arquivo JSON"""
        with open(filename, 'w') as f:
            json.dump(self.frames_data, f, indent=2)
    
    def load_from_file(self, filename):
        """Carrega os dados de um arquivo JSON"""
        with open(filename, 'r') as f:
            self.frames_data = json.load(f)

class FrameMovementApp:
    def __init__(self, root, files, repeticao, scale=2):
        self.root = root
        self.files = files
        self.scale = int(scale)
        self.repeticao = repeticao
        
        # Armazenamento dos dados da animação
        self.animation_data = AnimationData()
        
        # Inicializa dados para todos os frames
        for file_path in files:
            self.animation_data.add_frame(file_path, 0, 0, [])

        self.window_width = 400 * self.scale
        self.window_height = 300 * self.scale
        self.canvas = tk.Canvas(root, width=self.window_width, height=self.window_height, bg="white")
        self.canvas.pack()

        # Controles adicionais
        control_frame = tk.Frame(root)
        control_frame.pack(pady=5)
        
        # Botão para copiar hitboxes do frame anterior
        self.copy_btn = tk.Button(control_frame, text="Copiar Hitboxes do Frame Anterior", 
                                  command=self.copy_previous_hitboxes)
        self.copy_btn.pack(side=tk.LEFT, padx=5)
        
        # Botões de navegação
        nav_frame = tk.Frame(root)
        nav_frame.pack(pady=5)
        
        self.prev_btn = tk.Button(nav_frame, text="← Frame Anterior", command=self.previous_frame)
        self.prev_btn.pack(side=tk.LEFT, padx=5)
        
        self.next_btn = tk.Button(nav_frame, text="Próximo Frame →", command=self.next_frame)
        self.next_btn.pack(side=tk.LEFT, padx=5)
        
        # Label para mostrar frame atual/total
        self.frame_label = tk.Label(root, text=f"Frame: 1/{len(files)}")
        self.frame_label.pack()

        # Dados do frame atual
        self.current_frame = 0
        self.X = 0
        self.Y = 0
        self.current_x = self.window_width // 2 
        self.current_y = self.window_height // 3 * 2
        self.absolute_x = self.current_x  # Posição absoluta acumulada
        self.absolute_y = self.current_y  # Posição absoluta acumulada
        self.last_x = 0
        self.last_y = 0

        self.box_type = 0
        self.hitboxes = []
        self.temp_rectangle = None
        self.box_start = None

        # Texto de posição
        self.Position_label = tk.Label(root, text=f"Movimento X: {self.X} Y: {self.Y}")
        self.box_type_label = tk.Label(root, text=f"Box Type: {self.box_type}")
        self.mouse_pos_label = tk.Label(root, text=f"{0},{0}")
        self.absolute_pos_label = tk.Label(root, text=f"Posição Absoluta: X: {self.absolute_x}, Y: {self.absolute_y}")
        self.Position_label.pack()
        self.box_type_label.pack()
        self.mouse_pos_label.pack()
        self.absolute_pos_label.pack()

        # Botão para salvar progresso
        self.save_btn = tk.Button(root, text="Salvar Progresso", command=self.save_progress)
        self.save_btn.pack(pady=5)

        self.canvas.focus_set()

        # Bind mouse and keyboard events
        self.canvas.bind("<Return>", lambda e: self.next_frame())
        self.canvas.bind("<space>", lambda e: self.next_frame())
        self.canvas.bind("p", lambda e: self.previous_frame())  # Atalho para frame anterior
        self.canvas.bind("P", lambda e: self.previous_frame())  # Atalho para frame anterior

        self.canvas.bind("<Left>", self.move_left)
        self.canvas.bind("<Right>", self.move_right)
        self.canvas.bind("<Up>", self.move_up)
        self.canvas.bind("<Down>", self.move_down)

        self.canvas.bind("<Button-1>", self.on_left_click)
        self.canvas.bind("<Button-3>", self.on_right_click)
        self.canvas.bind("<Motion>", self.on_mouse_move)
        self.canvas.bind("<BackSpace>", self.delete_last_box)
        self.canvas.bind("c", self.copy_previous_hitboxes_key)
        self.canvas.bind("C", self.copy_previous_hitboxes_key)

        # Carrega o primeiro frame
        self.load_current_frame()

    def save_current_data(self):
        """Salva os dados do frame atual na estrutura"""
        self.animation_data.update_frame(
            self.current_frame, 
            self.X // self.scale, 
            self.Y // self.scale, 
            self.hitboxes
        )

    def calculate_absolute_position(self):
        """Calcula a posição absoluta acumulada"""
        total_x = self.window_width // 2
        total_y = self.window_height // 3 * 2
        
        for i in range(self.current_frame + 1):
            frame_data = self.animation_data.get_frame(i)
            if frame_data:
                total_x += frame_data['movement_x'] * self.scale
                total_y += frame_data['movement_y'] * self.scale
        
        self.absolute_x = total_x
        self.absolute_y = total_y
        self.absolute_pos_label.config(text=f"Posição Absoluta: X: {(total_x - self.window_width // 2) // self.scale}, Y: {(total_y - self.window_height // 3 * 2) // self.scale}")

    def load_current_frame(self):
        """Carrega os dados do frame atual da estrutura"""
        frame_data = self.animation_data.get_frame(self.current_frame)
        if frame_data:
            self.X = frame_data['movement_x'] * self.scale
            self.Y = frame_data['movement_y'] * self.scale
            self.hitboxes = frame_data['hitboxes'].copy() if frame_data['hitboxes'] else []
        
        # Calcula posição absoluta
        self.calculate_absolute_position()
        
        # Carrega as imagens
        self.load_images()
        self.redraw()
        self.frame_label.config(text=f"Frame: {self.current_frame + 1}/{len(self.files)}")

    def load_images(self):
        """Carrega as imagens atual e anterior"""
        self.current_image_string = self.files[self.current_frame]
        self.previous_image_string = self.files[self.current_frame - 1] if self.current_frame > 0 else None

        current_image = Image.open(self.current_image_string)
        current_image = current_image.resize((int(current_image.width * self.scale), int(current_image.height * self.scale)))
        self.current_photo = ImageTk.PhotoImage(current_image)

        if self.previous_image_string:
            previus_image = Image.open(self.previous_image_string)
            previus_image = previus_image.resize((int(previus_image.width * self.scale), int(previus_image.height * self.scale)))
            previus_image.putalpha(64)  # 25% opacity
            self.previus_photo = ImageTk.PhotoImage(previus_image)
        else:
            self.previus_photo = None

    def next_frame(self):
        """Avança para o próximo frame"""
        self.save_current_data()
        
        if self.current_frame < len(self.files) - 1:
            self.current_frame += 1
        else:
            self.current_frame = 0  # Volta para o primeiro frame
        
        self.load_current_frame()

    def previous_frame(self):
        """Volta para o frame anterior"""
        self.save_current_data()
        
        if self.current_frame > 0:
            self.current_frame -= 1
        else:
            self.current_frame = len(self.files) - 1  # Vai para o último frame
        
        self.load_current_frame()

    def redraw(self):
        self.canvas.delete("all")

        # Draw grid
        self.create_grid()

        # Calcula posição para desenho
        draw_x = self.absolute_x
        draw_y = self.absolute_y
        
        # Desenha linha de trajetória
        self.draw_trajectory()

        # Draw the previous image with 25% opacity if it exists
        if self.previus_photo != None:
            prev_frame_data = self.animation_data.get_frame(self.current_frame - 1 if self.current_frame > 0 else len(self.files) - 1)
            if prev_frame_data:
                prev_abs_x = draw_x - self.X  # Posição anterior
                prev_abs_y = draw_y - self.Y
                self.canvas.create_image(prev_abs_x, prev_abs_y, anchor=tk.CENTER, image=self.previus_photo, tags="sprite")

        # Draw current image
        if self.current_photo != None:
            self.canvas.create_image(draw_x, draw_y, anchor=tk.CENTER, image=self.current_photo, tags="sprite")
        
        # Calcular a posição do canto superior esquerdo da imagem
        img_top_left_x = draw_x - (self.current_photo.width() // 2)
        img_top_left_y = draw_y - (self.current_photo.height() // 2)

        # Draw hitboxes
        for x1, y1, x2, y2, box_type in self.hitboxes:
            self.canvas.create_rectangle(
                img_top_left_x + x1 * self.scale, 
                img_top_left_y + y1 * self.scale, 
                img_top_left_x + x2 * self.scale, 
                img_top_left_y + y2 * self.scale, 
                outline=self.get_box_color(box_type)
            )

        # Draw labels
        self.Position_label.config(text=f"Movimento Relativo X: {self.X // self.scale} Y: {self.Y // self.scale}")
        self.box_type_label.config(text=f"Box Type: {self.box_type}")

    def draw_trajectory(self):
        """Desenha a trajetória do movimento através dos frames"""
        points = []
        current_x = self.window_width // 2
        current_y = self.window_height // 3 * 2
        
        # Coleta todos os pontos da trajetória
        for i in range(len(self.files)):
            frame_data = self.animation_data.get_frame(i)
            if frame_data:
                points.append((current_x, current_y))
                current_x += frame_data['movement_x'] * self.scale
                current_y += frame_data['movement_y'] * self.scale
        
        # Desenha a trajetória
        if len(points) > 1:
            for i in range(len(points) - 1):
                x1, y1 = points[i]
                x2, y2 = points[i + 1]
                # Linha cinza para trajetória
                self.canvas.create_line(x1, y1, x2, y2, fill="gray", width=2)
                # Ponto na posição de cada frame
                self.canvas.create_oval(x1 - 3, y1 - 3, x1 + 3, y1 + 3, fill="blue")
            
            # Último ponto
            x_last, y_last = points[-1]
            self.canvas.create_oval(x_last - 3, y_last - 3, x_last + 3, y_last + 3, fill="red")

    def create_grid(self):
        # Draw a grid on the canvas, scaled
        step_large = 100 * self.scale
        step_small = 10 * self.scale
        
        # Linhas grandes
        for x in range(0, int(self.window_width), int(step_large)):
            self.canvas.create_line(x, 0, x, self.window_height, fill="black", width=2)
        for y in range(0, int(self.window_height), int(step_large)):
            self.canvas.create_line(0, y, self.window_width, y, fill="black", width=2)

        # Linhas pequenas
        for x in range(0, int(self.window_width), int(step_small)):
            self.canvas.create_line(x, 0, x, self.window_height, fill="lightgrey")
        for y in range(0, int(self.window_height), int(step_small)):
            self.canvas.create_line(0, y, self.window_width, y, fill="lightgrey")

        origin_x = self.window_width // 2
        origin_y = self.window_height // 3 * 2
        self.canvas.create_line(origin_x - 5 * self.scale, origin_y, origin_x + 5 * self.scale, origin_y, fill="red")
        self.canvas.create_line(origin_x, origin_y - 5 * self.scale, origin_x, origin_y + 5 * self.scale, fill="red")

    def get_box_color(self, box_type):
        colors = ["red", "blue", "white"]
        return colors[box_type] if box_type < len(colors) else "white"

    def print_current_data(self):
        """Imprime os dados do frame atual no formato desejado"""
        frame_name = os.path.splitext(self.files[self.current_frame])[-2]
        hitbox_str = ', '.join([f'new GenericBox({t}, {int(x1)}, {int(y1)}, {int(x2)}, {int(y2)})' for x1, y1, x2, y2, t in self.hitboxes])
        print(f'new FrameData("{frame_name}", {(self.X // self.scale) / self.repeticao}f, {(self.Y // self.scale) / self.repeticao}f, new List<GenericBox> {{ {hitbox_str} }}),')
    
    def copy_previous_hitboxes(self):
        """Copia as hitboxes do frame anterior para o frame atual"""
        if self.current_frame > 0:
            prev_data = self.animation_data.get_frame(self.current_frame - 1)
        else:
            prev_data = self.animation_data.get_frame(len(self.files) - 1)  # Último frame se for o primeiro
            
        if prev_data and prev_data['hitboxes']:
            self.hitboxes = prev_data['hitboxes'].copy()
            self.redraw()
            print(f"Copiadas {len(self.hitboxes)} hitboxes do frame anterior")
        else:
            messagebox.showinfo("Informação", "Não há hitboxes no frame anterior para copiar.")
    
    def copy_previous_hitboxes_key(self, event=None):
        """Atalho de teclado para copiar hitboxes"""
        self.copy_previous_hitboxes()
    
    def save_progress(self):
        """Salva o progresso atual em um arquivo"""
        self.save_current_data()
        
        # Solicita local para salvar
        filename = filedialog.asksaveasfilename(
            defaultextension=".json",
            filetypes=[("JSON files", "*.json"), ("All files", "*.*")]
        )
        
        if filename:
            self.animation_data.save_to_file(filename)
            messagebox.showinfo("Sucesso", f"Progresso salvo em {filename}")
    
    # Eventos de movimento
    def move_left(self, event):
        self.X -= 1 * self.scale
        self.redraw()
        self.calculate_absolute_position()

    def move_right(self, event):
        self.X += 1 * self.scale
        self.redraw()
        self.calculate_absolute_position()

    def move_up(self, event):
        self.Y -= 1 * self.scale
        self.redraw()
        self.calculate_absolute_position()

    def move_down(self, event):
        self.Y += 1 * self.scale
        self.redraw()
        self.calculate_absolute_position()

    def on_left_click(self, event):
        if not self.box_start:
            # Início da nova caixa
            self.box_start = [event.x, event.y]
        else:
            # Fim da caixa
            x1, y1 = self.box_start
            x2, y2 = event.x, event.y
            self.box_start = None
            
            # Calcular a posição do canto superior esquerdo da imagem
            img_top_left_x = self.absolute_x - (self.current_photo.width() // 2)
            img_top_left_y = self.absolute_y - (self.current_photo.height() // 2)

            # Ajuste para salvar as hitboxes com a posição relativa ao personagem
            adjusted_x1 = (x1 - img_top_left_x) // self.scale
            adjusted_y1 = (y1 - img_top_left_y) // self.scale
            adjusted_x2 = (x2 - img_top_left_x) // self.scale
            adjusted_y2 = (y2 - img_top_left_y) // self.scale
            
            # Adicionar a caixa à lista de hitboxes com a posição ajustada
            self.hitboxes.append((adjusted_x1, adjusted_y1, adjusted_x2, adjusted_y2, self.box_type))
            self.redraw()

    def on_right_click(self, event):
        # Cycle through box types
        self.box_type = (self.box_type + 1) % 3
        self.box_type_label.config(text=f"Box Type: {self.box_type}")

    def on_mouse_move(self, event):
        # Calcular a posição do canto superior esquerdo da imagem
        img_top_left_x = self.absolute_x - (self.current_photo.width() // 2)
        img_top_left_y = self.absolute_y - (self.current_photo.height() // 2)

        self.mouse_pos_label.config(text=f"{(event.x - img_top_left_x) // self.scale} - {(event.y - img_top_left_y) // self.scale}")

        if self.box_start:
            if self.temp_rectangle:
                self.canvas.delete(self.temp_rectangle)
            x1, y1 = self.box_start
            x2, y2 = event.x, event.y
            self.temp_rectangle = self.canvas.create_rectangle(x1, y1, x2, y2, outline=self.get_box_color(self.box_type), dash=(2, 2))

    def delete_last_box(self, event):
        if self.hitboxes:
            self.hitboxes.pop()
            self.redraw()


class ImageSelectorApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Selecionar Frames da Animação")
        
        self.files = []
        
        # Frame principal
        main_frame = tk.Frame(root)
        main_frame.pack(padx=20, pady=20)
        
        # Título
        tk.Label(main_frame, text="Selecione os frames da animação", font=("Arial", 12, "bold")).pack(pady=10)
        
        # Lista de frames selecionados
        tk.Label(main_frame, text="Frames selecionados:").pack(anchor=tk.W)
        
        self.listbox_frame = tk.Frame(main_frame)
        self.listbox_frame.pack(fill=tk.BOTH, expand=True, pady=5)
        
        self.listbox = tk.Listbox(self.listbox_frame, width=50, height=15)
        self.listbox.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        
        scrollbar = tk.Scrollbar(self.listbox_frame, orient=tk.VERTICAL)
        scrollbar.config(command=self.listbox.yview)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.listbox.config(yscrollcommand=scrollbar.set)
        
        # Controles
        btn_frame = tk.Frame(main_frame)
        btn_frame.pack(pady=10)
        
        tk.Button(btn_frame, text="Adicionar Imagens", command=self.add_images).pack(side=tk.LEFT, padx=5)
        tk.Button(btn_frame, text="Remover Selecionado", command=self.remove_selected).pack(side=tk.LEFT, padx=5)
        tk.Button(btn_frame, text="Limpar Lista", command=self.clear_list).pack(side=tk.LEFT, padx=5)
        
        # Botões de reordenação
        order_frame = tk.Frame(main_frame)
        order_frame.pack(pady=5)
        
        tk.Button(order_frame, text="Mover para Cima", command=self.move_up).pack(side=tk.LEFT, padx=2)
        tk.Button(order_frame, text="Mover para Baixo", command=self.move_down).pack(side=tk.LEFT, padx=2)
        
        # Frame para configurações
        config_frame = tk.Frame(main_frame)
        config_frame.pack(pady=10)
        
        tk.Label(config_frame, text="Framerate:").pack(side=tk.LEFT, padx=5)
        self.framerate_var = tk.StringVar(value="60")
        framerate_entry = tk.Entry(config_frame, textvariable=self.framerate_var, width=10)
        framerate_entry.pack(side=tk.LEFT, padx=5)
        
        tk.Label(config_frame, text="Escala (inteiro):").pack(side=tk.LEFT, padx=5)
        self.scale_var = tk.StringVar(value="2")
        scale_entry = tk.Entry(config_frame, textvariable=self.scale_var, width=10)
        scale_entry.pack(side=tk.LEFT, padx=5)
        
        # Botão para carregar progresso salvo
        tk.Button(main_frame, text="Carregar Progresso Salvo", command=self.load_progress).pack(pady=5)
        
        # Botão iniciar
        tk.Button(main_frame, text="Iniciar Criação da Animação", command=self.start_animation, 
                 bg="green", fg="white", font=("Arial", 10, "bold")).pack(pady=20)
        
        # Instruções
        tk.Label(main_frame, text="Instruções:", font=("Arial", 10, "bold")).pack(anchor=tk.W, pady=5)
        tk.Label(main_frame, text="1. Adicione as imagens na ordem desejada (pode repetir)", 
                wraplength=400, justify=tk.LEFT).pack(anchor=tk.W)
        tk.Label(main_frame, text="2. Ajuste o framerate e escala conforme necessário", 
                wraplength=400, justify=tk.LEFT).pack(anchor=tk.W)
        tk.Label(main_frame, text="3. Clique em 'Iniciar Criação da Animação' quando terminar", 
                wraplength=400, justify=tk.LEFT).pack(anchor=tk.W)
        
        # Adiciona tooltip para mostrar caminho completo
        self.listbox.bind('<<ListboxSelect>>', self.show_file_path)

    def show_file_path(self, event):
        """Mostra o caminho completo do arquivo selecionado"""
        selection = self.listbox.curselection()
        if selection:
            index = selection[0]
            if 0 <= index < len(self.files):
                # Atualiza o título da janela com o caminho do arquivo
                filename = os.path.basename(self.files[index])
                self.root.title(f"Selecionar Frames da Animação - {filename}")

    def add_images(self):
        files = filedialog.askopenfilenames(
            title="Selecione as imagens",
            filetypes=[("Imagens", "*.png;*.jpg;*.jpeg;*.gif;*.bmp")]
        )
        
        for file in files:
            self.files.append(file)
            self.listbox.insert(tk.END, os.path.basename(file))
    
    def remove_selected(self):
        selection = self.listbox.curselection()
        if selection:
            index = selection[0]
            self.listbox.delete(index)
            self.files.pop(index)
    
    def clear_list(self):
        if messagebox.askyesno("Confirmar", "Tem certeza que deseja limpar a lista de frames?"):
            self.listbox.delete(0, tk.END)
            self.files.clear()
    
    def move_up(self):
        selection = self.listbox.curselection()
        if selection and selection[0] > 0:
            index = selection[0]
            # Move na lista de arquivos
            self.files[index], self.files[index-1] = self.files[index-1], self.files[index]
            # Move na listbox
            item = self.listbox.get(index)
            self.listbox.delete(index)
            self.listbox.insert(index-1, item)
            self.listbox.select_set(index-1)
    
    def move_down(self):
        selection = self.listbox.curselection()
        if selection and selection[0] < len(self.files) - 1:
            index = selection[0]
            # Move na lista de arquivos
            self.files[index], self.files[index+1] = self.files[index+1], self.files[index]
            # Move na listbox
            item = self.listbox.get(index)
            self.listbox.delete(index)
            self.listbox.insert(index+1, item)
            self.listbox.select_set(index+1)
    
    def load_progress(self):
        """Carrega um progresso salvo anteriormente"""
        filename = filedialog.askopenfilename(
            title="Carregar progresso salvo",
            filetypes=[("JSON files", "*.json"), ("All files", "*.*")]
        )
        
        if filename:
            try:
                with open(filename, 'r') as f:
                    data = json.load(f)
                
                # Limpa a lista atual
                self.files.clear()
                self.listbox.delete(0, tk.END)
                
                # Adiciona os arquivos do progresso salvo
                for frame_data in data:
                    file_path = frame_data['file']
                    if os.path.exists(file_path):
                        self.files.append(file_path)
                        self.listbox.insert(tk.END, os.path.basename(file_path))
                    else:
                        messagebox.showwarning("Aviso", f"Arquivo não encontrado: {file_path}")
                
                messagebox.showinfo("Sucesso", f"Progresso carregado de {filename}")
                
            except Exception as e:
                messagebox.showerror("Erro", f"Erro ao carregar arquivo: {str(e)}")
    
    def start_animation(self):
        if not self.files:
            messagebox.showerror("Erro", "Por favor, selecione pelo menos uma imagem.")
            return
        
        try:
            framerate = float(self.framerate_var.get())
            scale = float(self.scale_var.get())
            
            if framerate <= 0:
                messagebox.showerror("Erro", "O framerate deve ser maior que 0.")
                return
            
            if scale <= 0:
                messagebox.showerror("Erro", "A escala deve ser maior que 0.")
                return
                
            # Converter scale para inteiro
            scale = int(scale)
            
        except ValueError:
            messagebox.showerror("Erro", "Por favor, insira valores numéricos válidos para framerate e escala.")
            return
        
        self.root.destroy()
        
        # Inicia a aplicação principal
        root = tk.Tk()
        app = FrameMovementApp(root, self.files, 60/framerate, scale=scale)
        root.mainloop()
        
        # Imprime todos os dados ao final
        print("\n" + "="*50)
        print("DADOS FINAIS DA ANIMAÇÃO:")
        print("="*50)
        for i in range(len(app.files)):
            frame_data = app.animation_data.get_frame(i)
            if frame_data:
                frame_name = os.path.splitext(app.files[i])[-2]
                hitbox_str = ', '.join([f'new GenericBox({t}, {int(x1)}, {int(y1)}, {int(x2)}, {int(y2)})' 
                                      for x1, y1, x2, y2, t in frame_data['hitboxes']])
                print(f'new FrameData("{frame_name}", {frame_data["movement_x"] / app.repeticao}f, {frame_data["movement_y"] / app.repeticao}f, new List<GenericBox> {{ {hitbox_str} }}),')
        print("="*50)
        
        input("------------- Finalizado -------------")
        input()


if __name__ == "__main__":
    root = tk.Tk()
    selector = ImageSelectorApp(root)
    root.mainloop()