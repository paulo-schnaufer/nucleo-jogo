# STYLE_GUIDE.md — NÚCLEO: Última Onda

Guia técnico de arte. Objetivo: unificar assets CC0 (Kenney/OpenGameArt)
de origens variadas com 2 sprites customizados, em 2 dias de produção,
sem repintura pixel a pixel de cada asset importado.

Estilo-alvo: sci-fi flat/vetorial limpo (não pixel-art retrô), neon
ciano/magenta sobre fundo escuro, com glow feito no engine (bloom),
não desenhado nos sprites.

---

## 1. Paleta fixa (13 cores)

Travada. Qualquer cor fora desta lista em sprite, UI ou VFX é bug de arte.

| Papel              | Hex       | Uso                                                              |
|---------------------|-----------|-------------------------------------------------------------------|
| BG-VOID             | `#05070D` | Fundo mais profundo (espaço/vazio)                                |
| BG-PAINEL           | `#0B1220` | Fundo de UI, painéis, chão de mapa                                 |
| BG-ELEVADO          | `#16213A` | Superfícies elevadas do mapa, bordas de painel                    |
| CIANO-ESCURO        | `#0A5E6B` | Sombra única de qualquer objeto ciano                              |
| CIANO-BASE          | `#00E5FF` | Jogador, aliados, projétil aliado, UI positiva                    |
| CIANO-GLOW          | `#8FF9FF` | Realce/rim-light de objetos ciano, brilho de VFX aliado            |
| MAGENTA-ESCURO      | `#7A0E45` | Sombra única de qualquer objeto magenta                            |
| MAGENTA-BASE        | `#FF167A` | Inimigos, dano, projétil inimigo                                   |
| MAGENTA-GLOW        | `#FF7FC7` | Realce/rim-light de inimigos, brilho de VFX de morte/hit           |
| CORE-VIOLETA        | `#B45CFF` | Só para o Núcleo (Integridade) — identidade própria, nem ciano nem magenta |
| BRANCO              | `#F5F7FA` | Texto, flash de dano, brilho máximo, HUD                          |
| CINZA-UI            | `#8B93AC` | Texto secundário, UI neutra, "contorno funcional" quando precisar de separação sem usar preto |
| ÂMBAR-ALERTA        | `#FFC93C` | Única exceção à dupla ciano/magenta: telegraph de boss, aviso de Integridade crítica |

Regra de combinação: **um efeito/sprite nunca mistura ciano e magenta
ao mesmo tempo** (isso quebra a leitura "aliado vs. inimigo" no meio
do caos de bullet heaven). Âmbar é a única cor "neutra de perigo"
permitida fora dessa dupla, e só para telegraphs/avisos.

---

## 2. Grade de resolução

**PPU (Pixels Per Unit) fixo em 64 para TODOS os sprites do jogo.**
Configurem isso uma vez num *Texture Importer Preset* e apliquem em
todas as pastas de import — não mexam PPU por asset individual, é a
forma mais rápida de manter escala consistente entre CC0 e sprites
customizados sem redesenhar nada.

**a) Personagens/inimigos** (canvas do sprite, não da hitbox):
| Entidade                     | Canvas       | Relação    |
|-------------------------------|--------------|------------|
| Jogador                       | 64×64 px     | 1×  |
| Rusher (loop infinito)        | 48×48 px     | 0.75× |
| Atirador (DDoS)               | 64×64 px     | 1×  |
| Tanque (memory leak)          | 96×96 px     | 1.5× |
| Boss (stack overflow)         | 192×192 px   | 3×  |

**b) Projéteis** — grade base 16 px:
| Tipo                          | Canvas   |
|--------------------------------|----------|
| Projétil padrão (jogador/atirador) | 16×16 px |
| Projétil pesado (tanque/leque)     | 24×24 px |
| Lâmina orbital                     | 20×20 px |

**c) Ícones de upgrade (UI/HUD)**: canvas mestre **64×64 px** (mesmo
tier do jogador/inimigo comum — mantém peso visual consistente entre
sprite de mundo e ícone de UI). Exibição no HUD pode reduzir para
48×48, mas a arte-fonte é sempre 64×64.

**Import settings padrão (Unity):**
- Filter Mode: **Bilinear** (é flat/vetorial, não pixel-art — Point deixaria serrilhado feio nas curvas neon)
- Compression: **Truecolor / sem compressão agressiva** — cor chapada e saturada (ciano/magenta puro) mostra banding feio em compressão crunch padrão; prioridade é qualidade de cor, não tamanho de build
- Mesh Type: Tight para personagens/inimigos (reduz overdraw), Full Rect para projéteis e partículas
- Pivot: Bottom para personagens/inimigos que pisam no chão; Center para projéteis, ícones e o Núcleo

---

## 3. Regra de contorno: **SEM contorno (nenhum)**

Decisão travada nesta sessão (ver DECISIONS.md).

Justificativa:
1. **Tempo**: retrabalhar contorno de 1px em dezenas de sprites CC0
   de fontes diferentes não cabe em 2 dias. Os packs relevantes da
   Kenney (space/sci-fi) já são flat-vetorial sem contorno — escolher
   "sem contorno" significa **zero retrabalho** na maior parte da base
   de assets.
2. Consistência visual sai mais barata via **paleta fixa + shading
   flat + glow do engine (bloom)** do que via linha de contorno manual.
   Bloom "dá o pop" de destaque em cima de qualquer sprite, de graça,
   sem tocar em pixel.
3. Num bullet heaven com tela cheia de inimigos/projéteis/partículas,
   uma linha de 1px se perde visualmente no meio do bloom e da
   quantidade de elementos — silhueta de cor chapada + contraste
   ciano/magenta contra fundo escuro lê melhor que contorno fino.
4. Os 2 sprites customizados devem ser desenhados **sem tinta de
   contorno**, flat, para se misturarem sem costura com a maioria
   Kenney.

Exceção controlada: se algum asset do OpenGameArt vier com contorno
próprio (pixel-art) e não houver tempo de trocar por outro, **não
redesenhem** — apliquem tint no contorno existente puxando pra
`BG-VOID`/`CIANO-ESCURO`/`MAGENTA-ESCURO` (conforme facção) via
Sprite Renderer color, pra ele ler como sombra da paleta em vez de
contorno preto destoante.

---

## 4. Regra de sombreamento: flat, sem gradiente

- Máximo **2 tons por objeto**: 1 tom base + 1 tom de sombra (a versão
  "-ESCURO" da mesma família na paleta). Nunca gradiente suave, nunca
  blur de ambient occlusion.
- Realce (highlight) é opcional e, quando usado, é **1 tom só**
  (`CIANO-GLOW` ou `MAGENTA-GLOW`), aplicado em rim-light ou uma
  aresta-chave — nunca como blend suave.
- Sem dithering pesado — mantenham preenchimento sólido.
- Essa regra vale para **sprites** (personagens, inimigos, props,
  ícones). VFX de partícula tem exceção técnica — ver seção 5.

---

## 5. Regra de paleta para VFX (Unity Particle System)

**Regra geral**: cada Particle System usa no máximo 2 cores da
paleta (1 base + 1 glow/branco da mesma família). Nunca misturar
ciano e magenta no mesmo efeito.

| Origem do efeito                                  | Cores                          |
|------------------------------------------------------|---------------------------------|
| Tiro/aliado, upgrades ativos, pickup, torreta         | `CIANO-BASE` + `CIANO-GLOW`     |
| Hit/morte de inimigo, projétil inimigo, burst de DDoS | `MAGENTA-BASE` + `MAGENTA-GLOW` |
| Integridade do Núcleo (efeitos do próprio Núcleo)     | `CORE-VIOLETA` + `BRANCO`       |
| Telegraph de boss / aviso de Integridade crítica      | `ÂMBAR-ALERTA` (+ `BRANCO`, opcional) — nunca combinado com ciano/magenta no mesmo efeito |
| Flash de dano genérico (qualquer entidade)            | `BRANCO`, pulso de tint ~0.08–0.12s, independe de facção |

**Notas técnicas Unity:**
- Material: `Particles/Unlit` (URP) com **Blend Mode = Additive**
  para tudo que é "glow" (reforça o tema neon, soma luz em vez de
  cobrir). Para debris/impacto sólido, usar Alpha Blend em vez de
  Additive (Additive em objeto opaco estoura e vira branco).
- Textura: **um único** sprite radial neutro (círculo branco com
  fade de alpha) serve pra ciano, magenta, âmbar e violeta — a cor
  vem do *Start Color* / *Color over Lifetime*, não da textura. Isso
  economiza produção: uma textura, quatro usos.
- Color over Lifetime: no máximo 2 keys de cor (cor base → mesma cor
  com alpha 0, ou cor base → branco → transparente). Nunca gradiente
  multi-hue/rainbow.
- Performance: bullet heaven = muitos inimigos/projéteis simultâneos.
  Limitem Max Particles por sistema e usem pooling. Prefiram poucas
  partículas grandes translúcidas a muitas partículas pequenas
  opacas — mantém o glow sem matar o frame rate.

---

## 6. Unificando assets CC0 sem repintar (dica de implementação)

Com Kenney + OpenGameArt misturados, o jeito rápido de dar cara única
sem tocar em cada arquivo é um **URP Volume global** na câmera:

- **Color Adjustments**: leve aumento de Saturation e Contrast pra
  nivelar o "peso" visual entre pacotes diferentes.
- **Split Toning**: Shadows puxando pra `CIANO-ESCURO`/tons de
  `BG-*`; Highlights neutro ou levemente puxado pra `CIANO-BASE`.
- **Bloom**: threshold baixo o bastante pra pegar `CIANO-BASE` e
  `MAGENTA-BASE`, mas não tão baixo que estoure `BRANCO`/UI.

Isso dá um "banho" de cor consistente em cima de qualquer asset CC0
sem editar pixel nenhum. Os **2 sprites customizados** devem seguir a
paleta/grade à risca desde o traço — eles servem de régua visual pra
decidir quais assets CC0 combinam (prefiram Kenney/estilo vetorial
flat aos assets do OpenGameArt que já vierem com contorno/pixel-art
pesado, salvo a exceção da seção 3).