### Sessão — Sistema de Upgrades (2026-09-04 - 05:20)

**O que foi feito nesta sessão:**
- Confirmado (com cache-bust) que SCOPE_LOCK.md real não tem seções "## Escopo (MVP)"
  / "## Escopo expandido" — tem uma única seção "### Escopo" listando 7 upgrades
  (3 armas + 4 passivas), com corrente elétrica já marcada como stretch goal à parte.
  Projeto do sistema seguiu essa versão real do arquivo.
- Criado Assets/Scripts/Upgrades/ completo: UpgradeDefinition (ScriptableObject) +
  UpgradeLevelData, UpgradeManager (sorteio de N opções sem repetir upgrade no
  nível máximo + aplicação de arma/passiva), PlayerStats (stub), e os 7 upgrades:
  DaemonTurret (torreta), RoundRobinBlades + BladeHit (lâminas orbitais), ForkShot
  (tiro em leque), e os 4 hooks de passiva (Overclock/CriticalExploit/Redundancy/Cache)
  aplicados via PlayerStats.
- Nomes com metáfora de computação aplicados aos 7 (Daemon, Round-Robin, Fork(),
  Overclock, Exploit Crítico, Redundância, Cache).

**O que ficou pendente/quebrado:**
- Integração com o runtime oficial concluída para as armas novas: Daemon, Fork() e
  Round-Robin agora usam os contratos reais de Projectile, EnemyBase e ObjectPool;
  a mira usa SyncTransforms e a PhysicsScene2D da cena da arma.
- O sistema alternativo em Nucleo.Upgrades.Core permanece fora do runtime oficial;
  o jogo continua usando Nucleo.UpgradeManager, Nucleo.UpgradeData e Nucleo.PlayerStats.
- Nenhuma UI de level-up foi criada (fora do escopo pedido nesta sessão) — o
  UpgradeManager expõe eventos (OnOptionsRolled, OnUpgradeApplied,
  OnNoUpgradesAvailable) prontos pra uma tela consumir.
- O PlayerStats.cs em Nucleo.Upgrades é um stub legado e permanece fora do runtime;
  o jogador usa Nucleo.PlayerStats, que já concentra os quatro bônus passivos.
- As APIs reais foram confirmadas e integradas: ObjectPool.Instance.Get(prefab,pos,rot),
  Projectile.Launch(direction,speed,damage,owner) e EnemyBase.ApplyDamage(amount,source).
- Nenhum asset (ScriptableObject de fato, ícones, prefabs de projétil/lâmina) foi
  criado dentro da Unity ainda — só o código.
- Valores de balanceamento (cadência, dano, %s) não foram fixados em código —
  ficam no Inspector de cada UpgradeDefinition; nenhum default numérico foi commitado.

**Próxima tarefa exata para a próxima sessão:**
- Criar os 7 assets UpgradeDefinition no Editor (Assets/Data/Upgrades/), preencher
  níveis com valores de teste, criar a UpgradeManager na cena com os 7 arrastados,
  criar prefabs mínimos de projétil/lâmina compatíveis com as assunções acima (ou
  ajustar os scripts pros nomes reais), e então construir a tela de level-up
  (pausa via Time.timeScale = 0, já decidido) que chama RollOptions/ApplyUpgrade.
  