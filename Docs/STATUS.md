# STATUS ATUAL — 2026-09-04

Este bloco é a fonte operacional para os próximos agentes. As sessões antigas abaixo
foram preservadas intencionalmente; consulte `STATUS_ARCHIVE.md` para o histórico mais
antigo. Não trate uma pendência antiga como atual sem conferir o código e a cena.

## Fontes de verdade

1. `SCOPE_LOCK.md`: prioridade de escopo.
2. `DECISIONS.md`: arquitetura e decisões fechadas.
3. Código e cena atuais: comportamento integrado.
4. Este bloco: pendências atuais.
5. `STATUS_ARCHIVE.md`: histórico.

## Estado atual do MVP

O MVP prioritário continua sendo: 1 mapa, 1 jogador, 3 ondas, rusher, atirador,
torreta automática, Núcleo com vida separada, Game Over por Player/Núcleo e 3 upgrades
(lâminas orbitais, velocidade e dano). Pooling é obrigatório para inimigos, projéteis,
XP e VFX. Tanque, boss, ondas 4–6, upgrades extras, narrativa expandida, arte customizada
e áudio completo são pós-MVP, mesmo que parte deles já exista no código.

## Já integrado

- Core: `ObjectPool`, `PoolItem`, `Health`, `CoreIntegrity`, `GameManager`, `EnemySpawner`.
- Player: `PlayerController`, `PlayerStats`, `PlayerProgression`.
- Combate: `AutoTurretWeapon`, `Projectile`, `EnemyBase`, `RusherEnemy`, `AtiradorEnemy`,
	`EnemyProjectile`, `TanqueEnemy` e `BossEnemy`.
- Upgrades oficiais: `UpgradeData`, `UpgradeManager`, `OrbitalBladesWeapon` e
	`FanShotWeapon`.
- Juice: `HitStop`, `ScreenShake`, `ScreenShakeCamera`, `DamageFlash`,
	`PooledImpactEffect` e `PooledDamageNumber`.
- `PooledImpactEffect` procura o `ParticleSystem` em filho e usa
	`ParticleSystem.main.useUnscaledTime`.
- Os cinco campos de feedback do `GameManager` na `Assets/Scenes/Cena.unity` apontam para
	os prefabs em `Assets/Prefabs/Feedback/`.
- `Assets/Scripts/Utils/AudioManager.cs` já existe.
- Não foi introduzido DOTween, New Input System ou outra dependência nova.

O runtime oficial usa `Nucleo.PlayerStats`, `Nucleo.UpgradeManager` e `Nucleo.UpgradeData`.
O sistema alternativo em `Assets/Scripts/Upgrades/` não é o caminho oficial.

## Pendências atuais, por prioridade

### P0 — validar no Unity

1. Abrir `Assets/Scenes/Cena.unity` e confirmar Console sem erros.
2. Testar hit/morte comum, dano ao Núcleo, hit/morte do boss e partículas durante hit-stop.
3. Confirmar que o Player recebe `Health.TakeDamage` e mostra `DamageFlash`.
4. Confirmar no Player: `Health`, Collider2D e SpriteRenderer no objeto ou em filho.
5. Rodar o MVP completo por três ondas.

### P1 — UI mínima

Criar ou confirmar: abertura, HUD do Player, HUD do Núcleo, barra de XP, tela de
level-up com 3 cards, vitória após 3 ondas e derrota. Eventos disponíveis:
`Health.OnHealthChanged`, `Health.OnDeath`, `PlayerProgression.OnXPChanged`,
`PlayerProgression.OnLevelUp`, `UpgradeManager.OnChoicesReady` e
`GameManager.OnGameStateChanged`.

### P1 — áudio

Configurar Audio Mixer com `Master`, `Music`, `SFX` e Compressor no Master; importar SFX
de hit CC0 e música opcional; preencher o `AudioManager`; decidir/integrar
`AudioManager.Instance?.PlayHit()` no ponto de dano/morte. `Docs/CREDITS.txt` está vazio.

### P1 — arte/polimento

Usar apenas assets aprovados no `LICENSE_CHECKLIST_LOG.md`; PPU 64, Bilinear e sem
compressão agressiva; conferir cruz azul no Generic Items; não usar `aura-design-assets`
nem assets com licença contraditória; configurar URP Volume/Bloom após validar o core.

### P2 — expansão pós-MVP

Tanque/boss no fluxo final, ondas 4–6, upgrades restantes, status entre ondas, variantes,
easter eggs, sprites customizados, música/mixagem completas e callouts de onda.

## Regras para os próximos agentes

- Ler `SCOPE_LOCK.md`, `DECISIONS.md`, `STYLE_GUIDE.md` e este bloco antes de editar.
- Não redesenhar a arquitetura oficial.
- Não usar `Instantiate`/`Destroy` para inimigos, projéteis ou VFX durante gameplay.
- Fazer uma tarefa por sessão, editar o mínimo e validar no Unity.
- Atualizar este bloco com o estado real; preservar detalhes históricos abaixo e em
	`STATUS_ARCHIVE.md`.

## Arquivo histórico abaixo

### Sessão — Narrativa e telas (2026-09-04 12:20)
**O que foi feito nesta sessão:**
- Escrito todo o texto narrativo de NÚCLEO: Última Onda: 3 frases de abertura, 4 linhas de status por limiar de Integridade do Núcleo (75/50/25/10%), 2 variantes de vitória com condição de disparo definida, 2 linhas de tela de derrota, 4 variantes de easter egg de causa de morte (uma por tipo de inimigo).
- Confirmado que SCOPE_LOCK.md e STATUS.md estão consistentes com a revisão técnica anterior, nenhuma decisão de DECISIONS.md precisou ser reaberta.

**O que ficou pendente/quebrado:**
- Texto ainda não integrado ao Unity (nenhum script de disparo de evento foi escrito nesta sessão — só o conteúdo).
- Ideia de callouts de início de onda por tipo de inimigo não implementada, registrada como sugestão abaixo.

**Próxima tarefa exata para a próxima sessão:**
- Selecionar e importar assets CC0 conforme STYLE_GUIDE.md (essa já era a pendência da sessão anterior de style guide, ainda não iniciada) — ou, se a prioridade mudar, implementar os triggers de UI que disparam os textos escritos nesta sessão (evento de limiar de integridade, evento de fim de onda 6, evento de morte por tipo de inimigo).

### Sessão — Simplificação de Áudio (2026-09-04 17:55)

**O que foi feito nesta sessão:**
- Leitura de STATUS.md, SCOPE_LOCK.md e DECISIONS.md (necessário por causa do padrão de singleton estático já usado em `EnemyBase.PlayerTarget/CoreTarget`).
- Projetada a estrutura do Audio Mixer: `Master` (com Compressor atuando como limiter) → `Music` e `SFX` como grupos filhos. Parâmetros do Compressor especificados (Threshold ≈ -10dB, Attack 10ms, Release ≈150ms).
- Criado `AudioManager.cs`: singleton com pool de 8 `AudioSource`s pra reprodução do hit único com pitch aleatório (0.85–1.15) reaproveitado nos 4 tipos de inimigo, mais `AudioSource` dedicada pra faixa única de música ambiente/tensão em loop.
- Arquivo entregue: `AudioManager.cs` (ver anexo desta sessão).

**O que ficou pendente/quebrado:**
- `AudioManager.cs` ainda não foi colocado na cena/commitado no repo (essa sessão não teve acesso de escrita ao projeto Unity, só aos Docs via GitHub raw).
- O `.mixer` asset em si não foi criado no editor — só especificado (passo a passo acima). Alguém precisa abrir o Unity e seguir os 5 passos.
- **Ponto de integração não confirmado**: não sei o nome exato do método em `EnemyBase` (ou classe equivalente) onde dano/morte são resolvidos pros 4 tipos de inimigo — não tive acesso ao código-fonte (Assets fora do escopo desta sessão). `AudioManager.Instance.PlayHit()` precisa ser chamado de lá.
- Nenhum clipe de áudio (hit nem música) foi selecionado/importado ainda — isso é seleção de asset CC0, não coberto por esta sessão.

**Próxima tarefa exata para a próxima sessão:**
- Sessão de scripts de inimigo: localizar o método de dano/morte comum aos 4 `EnemyBase` (rusher/atirador/tanque/boss) e adicionar a chamada `AudioManager.Instance?.PlayHit();` nele. Decidir e registrar se o hit toca em CADA dano recebido, só na morte, ou nos dois (recomendação: nos dois, por simplicidade de hookup, já que o orçamento é de 1 clipe único mesmo).
- Sessão de assets/editor: criar o `NucleoMixer.mixer` seguindo os 5 passos acima, importar 1 clipe de hit + 1 faixa de música (CC0), popular os campos do `AudioManager` no Inspector.

### Sessão — Juice e feedback de combate (2026-09-04 17:50)
**O que foi feito nesta sessão:**
- Implementados `HitStop` e `ScreenShake` com coroutines nativas, debounce global de
	60ms, pausa em tempo real e guarda contra a pausa de upgrade/game over.
- Integrados os eventos globais de `Health` para aplicar feedback em morte comum, boss
	e dano ao Núcleo; hit não-letal de inimigo comum não congela nem treme.
- Implementado `DamageFlash` branco de 90ms para objetos com `Health`.
- Implementados hooks pooled opcionais para partículas de impacto e números de dano do
	boss, com capacidade de 20 e reciclagem do item mais antigo.
- Números de dano ficam restritos ao boss; usam tempo não escalado, offset aleatório,
	subida, pop-in e fade.
- Nenhuma dependência nova foi adicionada; a implementação usa coroutines nativas.

**O que ficou pendente/quebrado:**
- Ainda não existem prefabs de partículas de impacto nem de número de dano no projeto;
	os campos correspondentes de `GameManager` permanecem opcionais até serem criados no
	Unity com a paleta e os módulos definidos em `STYLE_GUIDE.md`.
- `ScreenShakeCamera` é adicionado automaticamente à Main Camera no primeiro shake;
	não há follow de câmera existente para integrar.
- Corrigida a cena principal: os cinco campos de feedback do `GameManager` estavam
	nulos. Eles agora apontam para os prefabs em `Assets/Prefabs/Feedback/`.
- `PooledImpactEffect` agora encontra `ParticleSystem` em filho e usa tempo não
	escalado, compatível com o hit-stop; `DamageFlash` também procura renderer filho.

**Próxima tarefa exata para a próxima sessão:**
- Criar/configurar os prefabs pooled de impacto e damage number no Unity, preencher os
	campos de feedback do `GameManager` e testar os cinco eventos em Play Mode.

**Decisões novas:**
- Juice usa coroutines nativas, sem DOTween, porque DOTween não era dependência planejada.
- Hit-stop não ocorre em hit não-letal de inimigo comum; mortes têm prioridade sobre o
	debounce para preservar o feedback de boss.
- Screen shake combina eventos simultâneos pelo máximo de amplitude/duração, nunca por soma.
- Damage numbers permanecem somente no boss até o HP dos inimigos comuns ser confirmado.


### Sessão — UI mínima P1 (2026-09-04)

**O que foi feito nesta sessão:**
Leitura de STATUS.md, SCOPE_LOCK.md e do código real (Health.cs, CoreIntegrity.cs,
PlayerProgression.cs, UpgradeManager.cs, UpgradeData.cs, GameManager.cs, EnemySpawner.cs).
Entregues 4 scripts novos em Assets/Scripts/UI/ (namespace Nucleo.UI): UITheme.cs,
HUDController.cs, UpgradeChoiceUI.cs, NarrativeUIController.cs — cobrindo exatamente o
item P1 "UI mínima" (abertura, HUD Player, HUD Núcleo, XP, level-up 3 cards, vitória,
derrota). Escopo deliberadamente sem variantes de texto por integridade nem easter egg
por tipo de inimigo (isso é P2). Nenhum script de gameplay existente foi alterado.

**O que ficou pendente/quebrado:**
- Ícones dos 3 upgrades do MVP (UpgradeData.icon) precisam existir pra UpgradeChoiceUI
  mostrar algo — depende da sessão de arte (P1 "arte/polimento"), ainda não fechada.
- As barras de vida e XP não funcionam realmente.