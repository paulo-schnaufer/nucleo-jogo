## Escopo (MVP — pré-seleção, implementar AGORA)
- 1 mapa, 1 personagem jogável, desktop (Windows standalone), 2D top-down com Rigidbody2D.
- 2 tipos de inimigo: rusher ("loop infinito", corpo a corpo) e atirador ("DDoS", à
  distância, mira o Núcleo por padrão — ver DECISIONS.md) + Integridade do Núcleo como
  segundo alvo dos inimigos e condição de derrota (Núcleo a 0 = Game Over).
- 3 ondas.
- Arma inicial: torreta automática (já implementada).
- Pool de upgrades do MVP — 3 opções: 1 arma extra (lâminas orbitais) + 2 passivas
  (velocidade de movimento, dano). Passivos empilham, armas são únicas (ver DECISIONS.md).
- Narrativa mínima: abertura (3 frases) + fechamento simples (sem variantes).
- Assets: paleta e regras já fechadas em STYLE_GUIDE.md; usar CC0 Kenney, sem curadoria
  extensa em outras fontes agora.

## Escopo expandido (SÓ depois do MVP acima rodando fim-a-fim, e só se sobrar tempo)
- Tanque ("vazamento de memória") e boss ("estouro de pilha"), onda 4 em diante.
- Upgrades restantes até completar 7: tiro em leque, regeneração de vida, raio de coleta
  de XP, corrente elétrica.
- Sprites customizados (núcleo/boss em Aseprite).
- Linhas de status entre ondas, variantes de fechamento conforme integridade do núcleo
  restante, easter egg de causa da morte.
- Curadoria ampla de assets fora de Kenney (OpenGameArt, Freesound, itch.io), mixagem/
  limiter de áudio completo.

## Backlog (ideias que NÃO viram código agora, mesmo se parecerem boas)
- Screen-shake leve + chromatic aberration no telegraph do boss (stack overflow).
- Vinheta de tela pulsando em MAGENTA-ESCURO quando Integridade do Núcleo estiver
  crítica (<25%).
- Hit-stop de 1–2 frames quando o Núcleo toma dano crítico.
- Variação adaptativa do spawnInterval conforme a velocidade com que o jogador limpou a
  onda anterior — mesma régua da corrente elétrica: só depois do boss estar testado.