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