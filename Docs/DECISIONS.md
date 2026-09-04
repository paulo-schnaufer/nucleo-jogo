## Decisões
- Atirador ("DDoS") mirar o Núcleo de propósito em vez do jogador
- Núcleo a 0 = Game Over
- Upgrades passivos empilham, armas são únicas

- Contorno de sprite: **nenhum** (não 1px). Motivo: maioria dos assets CC0 (Kenney) já é flat sem contorno; retrabalhar contorno em todos os assets não cabe no prazo; leitura visual sustentada por paleta fixa + shading flat + bloom.
- PPU fixo em 64 para todos os sprites, com canvas variando por tier de entidade (jogador/atirador 64px, rusher 48px, tanque 96px, boss 192px, projétil padrão 16px, ícone 64px).
- Núcleo (Integridade) recebe cor própria (CORE-VIOLETA, `#B45CFF`), distinta de jogador (ciano) e inimigos (magenta).
- VFX: regra de no máximo 2 cores por Particle System, nunca ciano+magenta no mesmo efeito; âmbar reservado exclusivamente para telegraphs/avisos críticos.

- Input via Input Manager legado (`Input.GetAxisRaw`), não o New Input System — decisão técnica, motivo: zero setup extra no prazo de 2 dias.
- Pausa do jogo (upgrade/game over) via `Time.timeScale = 0`, sem flags de pausa manuais espalhadas — física e corrotinas já respeitam isso.
- `ObjectPool` chaveado pelo prefab de origem (não string/tag), via componente auxiliar `PoolItem` adicionado automaticamente — evita bug de digitação de chave.
- Referências de alvo (`EnemyBase.PlayerTarget`/`CoreTarget`) como campos estáticos simples em vez de service locator — simplificação aceita pro escopo de 1 mapa/1 jogador/1 Núcleo do protótipo.

- Lâminas orbitais causam dano por contato direto via Health (proposto:
  TakeDamage(float, GameObject)) com cooldown por par (lâmina, inimigo) — não usam
  Projectile/ObjectPool, diferente das outras 2 armas. Justificativa: são um hitbox
  persistente, não um projétil disparado; forçar o padrão de Projectile pra isso exigiria
  gambiarra (instanciar/destruir projétil fake a cada frame de contato).
- Nomes de exibição dos 7 upgrades (metáfora de computação): Daemon (torreta), Buffer
  Circular (lâminas), Fork (leque), Overclock (velocidade), Payload (dano), Redundância
  (regeneração), Cache (raio de coleta).

- Tanque (memory leak) ganha dano de contato crescente com o tempo vivo, até um teto —
  reforça o reskin sem mexer em HP/velocidade nem criar campo novo na base.
- Boss (stack overflow) usa ciclo de 4 fases (aproxima/telegraph/overflow/cooldown) com
  UnityEvents OnTelegraphStart/OnOverflowFire pra desacoplar VFX/Animator da lógica de IA.
- Confirmado (não é decisão nova, é registro): Rusher e Tanque usam PickNearestTarget()
  padrão da base sem override; só o Atirador tem regra de alvo própria.

- Autor `aura-design-assets` (OpenGameArt) excluído **categoricamente** do projeto — todos os 3 packs dele (não só o já sinalizado), por: arquivos atualmente indisponíveis por suspeita de licenciamento na própria plataforma, histórico de disclosure de IA incompleta corrigida só sob cobrança do moderador, e um dos 3 packs com campo de licença formal (CC-BY 3.0/4.0) inconsistente com o texto do corpo (CC0). Ícones de upgrade saem do "Generic Items" da Kenney (retint manual pra paleta).
- Regra geral de arte adicionada: nenhum ícone com cruz vermelha em fundo branco (símbolo protegido por tratado internacional, independente de licença de copyright/CC0) — vale pro Generic Items e pra qualquer asset futuro.

- Limiar de vitória: Variante A do texto de vitória dispara com Integridade do Núcleo ≥50% ao fim da onda 6; Variante B dispara com <50%.
- Limiares de status de integridade fixados em 75/50/25/10%, disparo único por limiar (sem repetição).

- Juice de combate usa coroutines nativas, sem DOTween; hit-stop respeita a pausa de
  upgrade/game over e não dispara em hit não-letal de inimigo comum.
- Screen shake usa o máximo entre eventos simultâneos, nunca soma amplitudes.
- Damage numbers ficam restritos ao boss até o HP dos inimigos comuns ser confirmado.

- Estrutura do Audio Mixer travada em 2 grupos filhos de Master: `Music` e `SFX` (sem subgrupos por tipo de inimigo — o pitch aleatório é o único diferenciador entre os 4).
- Limiter implementado via efeito nativo `Compressor` no grupo `Master` (Unity não tem nó "Limiter" dedicado); parâmetros iniciais: Threshold -10dB / Attack 10ms / Release 150ms / Make Up Gain 0–3dB, a reajustar de ouvido com os clipes finais.
- Reprodução do hit via pool fixo de `AudioSource`s com `Play()` (não `PlayOneShot()`), pelo motivo técnico documentado no script: `AudioSource.pitch` afeta todas as reproduções em andamento na mesma fonte, então pitch por instância exige fonte dedicada por instância concorrente.
- `AudioManager` como singleton estático com `DontDestroyOnLoad`, mesmo padrão de simplificação já aceito em DECISIONS.md pra referências estáticas.