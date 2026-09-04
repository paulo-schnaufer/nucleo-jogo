## Arquivo de sessões concluídas (sem pendência em aberto na época do arquivamento)

### Sessão — Style Guide de Arte (2026-09-04 - 02:10)
Criado Docs/STYLE_GUIDE.md completo: paleta fixa de 13 cores em hex, grade de resolução
(personagens/inimigos, projéteis, ícones de upgrade) com PPU=64 fixo, regra de contorno
(nenhum, justificada), regra de sombreamento (flat, 2 tons), regra de paleta para VFX do
Particle System, e dica de implementação pra unificar assets CC0 via URP Volume sem
repintura manual. Concluída — nenhuma pendência bloqueando sessões seguintes.

### Sessão — Arquitetura do Core Prototype (2026-09-04 - 02:50)

**O que foi feito nesta sessão:**
Leitura de STATUS.md, SCOPE_LOCK.md, DECISIONS.md (vazio) e STYLE_GUIDE.md. Arquitetura
completa do core loop em C#/Unity 6.6, pooling desde o início: `ObjectPool`+`PoolItem`,
`Health` genérico (jogador/inimigo/Núcleo), `CoreIntegrity`, `PlayerStats`,
`PlayerController` (2D top-down), `EnemyBase` (perseguição+contato, extensível pros
tipos), `XPOrb`, `PlayerProgression` (XP/nível, trata level-up múltiplo no mesmo frame),
`UpgradeData`+`UpgradeManager` (pausa+escolha), `AutoTurretWeapon` (arma automática
funcional) + `Projectile` (pooled), `EnemySpawner` (3 ondas — MVP, ver SCOPE_LOCK.md),
`GameManager` (derrota/vitória). 15 scripts comentados em pt-BR, entregues como arquivos
prontos pra colar em `Assets/Scripts`.

**O que ficou pendente/quebrado:**
- Só o comportamento "rusher" está em `EnemyBase`; atirador (DDoS) ainda não tem
  subclasse — é a próxima prioridade (tanque/boss são escopo expandido, não agora).
- UI de escolha de upgrade (3 cards) e HUD (vida jogador/Núcleo, XP) não existem —
  `UpgradeManager` e `Health` já expõem os eventos que essa UI vai consumir.
- Nenhum GameObject/prefab estava criado no Editor no momento em que esta entrada foi
  escrita.

**Validação manual (04/09, pós-integração dos scripts):**
Scripts colados, prefabs criados, loop testado no Editor com 1 inimigo rusher + torreta
automática — spawn, dano e pooling funcionando numa cena de teste.

**Próxima tarefa exata para a próxima sessão:**
1. Implementar a subclasse de `EnemyBase` para o atirador (DDoS) — mira o Núcleo por
   padrão (ver DECISIONS.md).
2. Construir a UI de escolha de upgrade (consumindo `UpgradeManager.OnChoicesReady` /
   `ConfirmChoice`) usando só o pool de 3 upgrades do MVP definido em `SCOPE_LOCK.md`:
   lâminas orbitais, velocidade, dano.
3. Construir a UI de HUD (consumindo `Health.OnHealthChanged`) para jogador e Núcleo.

### Sessão — Sistema de Upgrades do Pool MVP (2026-09-04 10:10)

**O que foi feito nesta sessão:**
- Leitura de STATUS.md e SCOPE_LOCK.md; leitura de PlayerStats.cs, UpgradeManager.cs,
  UpgradeData.cs e AutoTurretWeapon.cs (arquitetura já existente, estendida — não recriada).
- Criados 2 scripts novos, únicos que faltavam pro pool de 3 armas do MVP:
  Assets/Scripts/OrbitalBladesWeapon.cs (lâminas orbitais) e
  Assets/Scripts/FanShotWeapon.cs (tiro em leque). Seguem exatamente o padrão de
  AutoTurretWeapon.cs (mesma guarda de Time.timeScale, mesmo GetComponentInParent<PlayerStats>,
  mesmo fluxo de weaponPrefab via UpgradeManager).
- Confirmado que NENHUMA mudança em PlayerStats.cs/UpgradeManager.cs/UpgradeData.cs foi
  necessária: a arquitetura de ApplyUpgrade() já é genérica o suficiente pra qualquer arma
  nova (só prefab + UpgradeData) e as 4 passivas já são 100% funcionais só com o
  UpgradeData configurado — não escrevi script novo pra elas, seria sistema paralelo à toa.
- Sugeridos nomes de exibição com metáfora de computação pros 7 upgrades (Daemon, Buffer
  Circular, Fork, Overclock, Payload, Redundância, Cache) — só como valor de displayName,
  sem mexer na lógica.

**O que ficou pendente/quebrado:**
- BLOQUEIO REAL pra compilar OrbitalBladesWeapon.cs: o script assume
  Health.TakeDamage(float amount, GameObject source), mas Health.cs não estava na lista de
  leitura desta sessão e nenhum dos 4 arquivos lidos chama esse método diretamente (quem
  causa dano nos outros dois é sempre Projectile, também não lido). Confirmar a assinatura
  real antes de compilar — está marcado com TODO(verify) no arquivo.
- Os prefabs de OrbitalBladesWeapon e FanShotWeapon (GameObject na cena/Assets, com o
  bladeVisualPrefab da lâmina) ainda não foram montados no Editor — só os scripts existem.
- Os 7 UpgradeData.asset ainda não foram criados no Editor; tabela de configuração
  (displayName/category/passiveType/passiveAmountPerPick) está na resposta desta sessão.
- Observação, não decisão a reabrir: SCOPE_LOCK.md hoje tem só uma seção "### Escopo"
  única — não existem as seções "## Escopo expandido" nem "Backlog" mencionadas na tarefa.
  Se essa separação for adotada, sugiro criar as duas seções na próxima sessão que mexer
  em escopo (corrente elétrica, hoje descrita ali mesmo como stretch goal, seria a primeira
  candidata a ir pra "Escopo expandido").
- Não li DECISIONS.md nesta sessão (não parecia necessário pra este pedido). Se houver lá
  alguma decisão tipo "todo dano de arma passa por Projectile/ObjectPool", ela conflita com
  o desenho de OrbitalBladesWeapon (dano por contato direto, sem projétil) — vale checar.

**Próxima tarefa exata para a próxima sessão:**
- Abrir Health.cs, confirmar/corrigir a assinatura do método de dano usado em
  OrbitalBladesWeapon.cs, montar os prefabs das 2 armas novas (incluindo bladeVisualPrefab)
  e criar os 7 UpgradeData.asset com os valores da tabela desta sessão.

  ### Sessão — IA dos Inimigos, versão contra código real (2026-09-04 10:40)

**O que foi feito nesta sessão:**
- Leitura de STATUS.md, SCOPE_LOCK.md, DECISIONS.md, STYLE_GUIDE.md e, pela primeira vez,
  do código real: Assets/Scripts/EnemyBase.cs, Health.cs, CoreIntegrity.cs.
- Reescrita completa da IA dos 4 tipos de inimigo confirmados em SCOPE_LOCK.md contra as
  assinaturas reais (namespace Nucleo, Health.TakeDamage, ObjectPool.Instance.Get +
  PoolItem.ReturnToPool, PlayerTarget/CoreTarget estáticos). Descartada a versão anterior
  desta entrega, que usava contratos assumidos (stubs de EnemyBase/IDamageable/PoolManager)
  incompatíveis com o projeto real.
- RusherEnemy.cs: sem código — comportamento padrão de EnemyBase já é o "loop infinito".
- AtiradorEnemy.cs: PickNearestTarget() sobrescrito pra sempre mirar o Núcleo (regra
  travada em DECISIONS.md), FixedUpdate() sobrescrito pra manter banda de distância e
  disparar rajada de projéteis, em vez de avançar até encostar.
- TanqueEnemy.cs: usa alvo padrão (sem regra própria em DECISIONS.md); dano de contato
  cresce com o tempo vivo até um teto, reaproveitando o campo protected contactDamage já
  existente na base.
- BossEnemy.cs: ciclo aproxima → telegraph → overflow (dano em área) → cooldown, com
  UnityEvents pro VFX de telegraph âmbar (STYLE_GUIDE seção 5).
- EnemyProjectile.cs: projétil pooled do Atirador, dano via Health.TakeDamage, pool via
  PoolItem.ReturnToPool — mesmo padrão já usado em EnemyBase.SpawnXPOrb.
- Entregue em /mnt/user-data/outputs/ia-inimigos/: os 5 .cs acima + README_INTEGRACAO.md.

**O que ficou pendente/quebrado:**
- PoolItem.cs e ObjectPool.cs não foram lidos diretamente nesta sessão — a API usada
  (Get/ReturnToPool) foi inferida de como EnemyBase.cs já as chama, não é suposição às
  cegas, mas vale conferir a assinatura exata ao integrar.
- Nenhum script foi compilado/testado no Editor (ambiente sem Unity nesta sessão).
- Balanceamento de números (dano, alcance, cooldowns) é chute de design razoável, não
  calibrado em playtest.
- Confirmar se STATUS.md deveria já ter registrado a sessão em que EnemyBase/Health/
  CoreIntegrity foram escritos — a versão atual do arquivo não menciona esse código, o que
  sugere um gap de atualização de outro agente, não desta sessão.

**Próxima tarefa exata para a próxima sessão:**
- Ler PoolItem.cs e ObjectPool.cs reais pra confirmar as assinaturas usadas em
  EnemyProjectile.cs e AtiradorEnemy.cs. Criar os prefabs (Rusher, Atirador + projétil com
  PoolItem, Tanque, Boss) no Editor, configurar Health/moveSpeed/contactDamage por tipo,
  registrar o prefab do projétil no ObjectPool e rodar as 6 ondas fim-a-fim.