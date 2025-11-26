O Project Fighting System (Project FS) é um protótipo de fighting game feito em C# usando SFML com a plataforma de desenvolvimento .NET

Implementado:
- Sistema de input segregrado para player 1 e 2
- Sistema de detecção de motion inputs (meia lua, etc)
- Sistema de som
- Animações
- Camera dinâmica
- Janela de tamanho dinamico
- Sistema genérico personalizável de estados
- Stages animados com behaviour próprio
- Hitboxes e sistema para criar hitboxes
- Colisão
- Sistema para inferir estado no oponente
- Cancels
- Block
- Auto orientação da direção lógica dos personagens
- Pushback em golpes
- Lógica básica de combate (tempo, rounds, gatilhos de fim de round, resets)
- Orientação visual dos personagens (olharem para o lado correto)
- Padronização dos controles
- Hitstop
- Pushback usando as pushboxes (jogadores se "empurram" ao colidirem)
- Particulas de dano e block
- UI
- Estados de intervalo entre rounds
- Dano e stun ao interagir
- Auto detecção e assingment de controles:  começa com ambos no teclado e vai trocando a medida que controles forem sendo conectados ou desconectados.
- Pushback no corner: o personagem que está batendo receber o pushback (apenas o movimento no eixo X) pois o que está sendo atingido não pode se afastar mais devido ao corner.
- Capacidade de realizar Super Arts
- Hitstop variável de acordo com a intensidade do golpe
- Multiplos hits por animação (com reinício definido no frame)
- Limitação da tela: Jogadores não podem se afastar um do outro mais do que o tamanho da tela.
- Particulas visuais independentes (Super Art, etc)
- Hitstun e blockstun
- Sistema de contagem de combo
- Damage scaling de acordo com o combo
- Física complexa
- Modo treino configurável 
- Menu de pausa
- Lógica do jogo mais robusta
- Menu de seleção de stage e personagem
- Tela pós batalha com rematch instantâneo
- Tela de configurações com permanência dos dados
- Sistema de prioridade de golpes
- Serialização dos dados (poupa tempo de load)
- Imposição de movimento unificada (Agora usa apenas o método Push)
- Trail visual para Super e movimentos EX
- Sistema de mudança sutil na cor da sprite de acordo com cada stage (simula uma iluminação global simples)
- Uso de shaders
- Load unificado e mais eficiente (usando arquivos binários)
- Inteligência artificial dos BOTs

Deprecated:
- Física simples

Em desenvolvimento:
- Conteúdo audiovisual melhorado
- Mecanica de agarrão
- Modularização de janelas de menus e botões selecionáveis.


