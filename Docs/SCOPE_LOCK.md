## Escopo
- 1 mapa, 1 personagem jogável, desktop (Windows standalone). 
- 4 tipos de inimigo (rusher/loop infinito, atirador/DDoS) + **Integridade do Núcleo** como segundo alvo dos inimigos além do jogador.
- 3 ondas.
- 7 upgrades: 3 armas (torreta automática, lâminas orbitais, tiro em leque) + 4 passivas (velocidade, dano, regeneração, raio de coleta).
- Narrativa: abertura (3 frases) + linhas de status entre ondas (desejável) + fechamento com variante conforme integridade do núcleo restante (desejável) + easter egg de causa da morte (desejável).
- Nomes de upgrade com leve metáfora de computação (desejável).
- 2D top-down com Rigidbody2D

### Escopo expandido (APENAS após finalizar o acima), adiciona:
- Tanque, boss, 4ª/5ª/6ª onda além da 3.
- Corrente elétrica, upgrades restantes até completar 7.
- Sprites customizados (núcleo/boss em Aseprite).
- Style guide formal, mixagem/limiter de áudio, variantes narrativas, easter egg.
- Curadoria ampla de assets fora de Kenney.

## Backlog
- Screen-shake leve + chromatic aberration no telegraph do boss (stack overflow), pra reforçar leitura de perigo além da cor âmbar.
- Vinheta de tela pulsando em MAGENTA-ESCURO quando Integridade do Núcleo estiver crítica (<25%).

- Hit-stop de 1–2 frames quando o Núcleo toma dano crítico, pra reforçar o telegraph sem precisar de arte nova.
- Leve variação do spawnInterval da onda conforme a velocidade com que o jogador limpou a anterior (dificuldade adaptativa) — mesma régua da corrente elétrica: só depois do boss estar testado.