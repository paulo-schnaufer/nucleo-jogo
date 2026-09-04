### Sessão — Style Guide de Arte (2026-09-04 - 02:10)

**O que foi feito nesta sessão:**
- Leitura de SCOPE_LOCK.md (STATUS.md, DECISIONS.md e STYLE_GUIDE.md vieram vazios/inacessíveis via fetch — tratado como primeira sessão de definição visual).
- Criado Docs/STYLE_GUIDE.md completo: paleta fixa de 13 cores em hex, grade de resolução (personagens/inimigos, projéteis, ícones de upgrade) com PPU=64 fixo, regra de contorno (nenhum, justificada), regra de sombreamento (flat, 2 tons), regra de paleta para VFX do Particle System, e dica de implementação pra unificar assets CC0 via URP Volume sem repintura manual.

**O que ficou pendente/quebrado:**
- Não foi possível confirmar via fetch se STATUS.md/DECISIONS.md/STYLE_GUIDE.md já tinham conteúdo prévio no repo — confirmar manualmente antes da próxima sessão pra não haver conflito.
- Nenhum asset CC0 foi selecionado/baixado ainda; guia é só a especificação.
- Os 2 sprites customizados ainda não foram feitos.

**Próxima tarefa exata para a próxima sessão:**
- Selecionar e importar assets CC0 (packs sci-fi/space da Kenney + busca dirigida no OpenGameArt) para os 4 tipos de inimigo, projéteis e ícones de upgrade, seguindo a grade de resolução e paleta do STYLE_GUIDE.md; configurar o Texture Importer Preset (PPU 64, Bilinear, sem compressão agressiva) e o URP Volume de unificação de cor (seção 6 do guia).

### Sessão — Arquitetura do Core Prototype (2026-09-04 - 02:50)

**O que foi feito nesta sessão:**
Leitura de STATUS.md, SCOPE_LOCK.md, DECISIONS.md (vazio) e STYLE_GUIDE.md. Arquitetura completa do core loop em C#/Unity 6.6, pooling desde o início: `ObjectPool`+`PoolItem`, `Health` genérico (jogador/inimigo/Núcleo), `CoreIntegrity`, `PlayerStats`, `PlayerController` (2D top-down), `EnemyBase` (perseguição+contato, extensível pros 4 tipos), `XPOrb`, `PlayerProgression` (XP/nível, trata level-up múltiplo no mesmo frame), `UpgradeData`+`UpgradeManager` (pausa+escolha), `AutoTurretWeapon` (arma automática funcional) + `Projectile` (pooled), `EnemySpawner` (6 ondas), `GameManager` (derrota/vitória). 15 scripts comentados em pt-BR, entregues como arquivos prontos pra colar em `Assets/Scripts`.

**O que ficou pendente/quebrado:**
- Nada foi testado dentro do Editor (sem acesso ao projeto Unity nesta sessão) — validar compilação e wiring ao colar.
- 4 decisões de design não-técnicas bloqueando trabalho seguro daqui pra frente: 2D vs 3D, regra de alvo Núcleo/jogador por tipo de inimigo, se Núcleo=0 é derrota, regras de stacking de upgrade (detalhe na resposta desta sessão).
- Só o comportamento "rusher" está em `EnemyBase`; atirador (DDoS), tanque (memory leak) e boss (stack overflow) ainda não têm subclasses.
- UI de escolha de upgrade (3 cards) e HUD (vida jogador/Núcleo, XP) não existem — `UpgradeManager` e `Health` já expõem os eventos que essa UI vai consumir.
- Nenhum GameObject/prefab foi criado no Editor ainda — lista completa na resposta desta sessão.

**Próxima tarefa exata para a próxima sessão:**
1. Confirmar as 4 decisões de design pendentes listadas acima.
2. Criar os GameObjects/prefabs da lista desta sessão e colar os 15 scripts.
3. Validar o loop rodando (1 inimigo rusher + torreta funcionando numa cena de teste) e então implementar as subclasses de `EnemyBase` pra atirador/tanque/boss.
4. Construir a UI de escolha de upgrade (consumindo `UpgradeManager.OnChoicesReady`/`ConfirmChoice`) e a UI de HUD (consumindo `Health.OnHealthChanged`).

Inserção manual: validei o loop rodando.