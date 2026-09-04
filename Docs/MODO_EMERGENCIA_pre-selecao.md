# Modo Emergência — Build para Pré-Seleção
**Prazo real:** 05/09, 23h59. **Agora:** 04/09, ~01h00. **Relógio disponível:** ~47h.
**Horas úteis realistas** (descontando sono e vida): ~26-30h, não 60-67h.

## Reframing crítico
Isso é a **eliminatória interna**, não a exposição de 21-24/09. Se passar, você recupera
~16 dias de folga até a exposição para completar o plano consolidado original (arte
customizada, 4 inimigos, boss, 7 upgrades, áudio completo). O que entrega amanhã só
precisa ser **jogável, honesto e demonstrar o conceito** — não o produto final. Julgamento
aqui é "isso tem jogo aqui dentro?", não "isso está pronto para o estande?".

**O plano de 67h da revisão anterior vira Fase 2 (pós-pré-seleção).** Este documento
substitui a Parte 3 do guia de orquestração *apenas para as próximas ~30h*.

## Orçamento de horas sugerido (agora até 05/09 23h59, com sono)
| Bloco | Janela | Horas úteis |
|---|---|---|
| Dormir agora | 04/09 01h-08h | — |
| Core loop + Integridade do Núcleo | 04/09 08h-15h | 6h |
| 2 inimigos reskinnados + 3 upgrades | 04/09 15h-20h | 4h |
| Import de assets Kenney (sem curadoria longa) | 04/09 20h-22h | 1.5h |
| Descanso/comida | 04/09 22h-23h | — |
| Narrativa mínima + UI básica | 05/09 08h-11h | 2.5h |
| SFX mínimo (sem música se apertar) | 05/09 11h-13h | 1.5h |
| Juice básico (shake + hit-stop) | 05/09 13h-15h | 1.5h |
| Testes + correção de bugs | 05/09 15h-19h | 3h |
| Build final, checagem em máquina limpa, upload | 05/09 19h-22h | — |
| **Buffer final antes do prazo** | 05/09 22h-23h59 | **1h+** |

Isso soma ~20h de trabalho ativo, deixando margem para imprevisto e sono real. Ajuste
conforme seu ritmo, mas **proteja o buffer final** — build corrompida ou upload de última
hora é o único jeito de zerar o projeto por bobagem.

## Escopo travado da pré-seleção (não é o escopo final do jogo)

**Mantém (crítico):**
- Player + movimento, 1 arma automática (torreta), spawner com object pooling.
- Integridade do Núcleo como HP separado — é o diferencial mais barato que você tem
  contra "clone genérico"; não cortar isso mesmo sob pressão.
- 2 tipos de inimigo (rusher/loop infinito + atirador/negação de serviço) — reskin
  conceitual é só nome + leve ajuste de comportamento, custo baixo.
- 3 upgrades (1 arma extra ex: lâminas orbitais, 2 passivas: velocidade + dano) — o
  suficiente para mostrar variação de partida sem inflar teste/balance.
- Condição de fim clara: sobreviver a 3 ondas = "vitória" com 2 linhas de fechamento.
  Sem boss agora — boss entra na Fase 2.
- Abertura (3 frases) + 1 linha de status na virada de onda + fechamento. Sem variantes,
  sem easter egg — isso é Fase 2.
- Assets 100% Kenney.nl (CC0 puro) — zero tempo gasto validando licença ambígua agora.
  OpenGameArt/Freesound/itch.io ficam para a Fase 2, quando há tempo de checar
  licença com calma.
- 1-2 SFX (hit + level-up), reaproveitados via pitch variado. Música é opcional — se
  sobrar 20-30min, uma faixa CC0 do próprio Kenney (ele também tem alguns loops);
  senão, silêncio ambiente é aceitável para uma pré-seleção.

**Corta explicitamente (vira backlog Fase 2, registrar em SCOPE_LOCK.md):**
- Tanque, boss, 4ª/5ª/6ª onda além da 3.
- Corrente elétrica, upgrades restantes até completar 7.
- Sprites customizados (núcleo/boss em Aseprite).
- Style guide formal, mixagem/limiter de áudio, variantes narrativas, easter egg.
- Curadoria ampla de assets fora de Kenney.

## Ajuste no protocolo multiagente para as próximas 30h
- **Não** rode o Prompt de Kickoff Universal apontando pro repo inteiro. Dê a URL raw
  só de `Docs/STATUS.md` (mantenha-o com no máximo a última entrada — arquive o resto
  em `STATUS_ARCHIVE.md`).
- Pule as etapas de curadoria/licenciamento em massa (Gemini) — só Kenney agora, decisão
  já tomada, não precisa de rodada de busca.
- Use Copilot para tudo que for "colar script e fazer compilar" — é onde ele economiza
  mais tempo de relógio real, que é o recurso mais escasso agora.
- Um único Claude (o mais "descansado" em quota) fica responsável por projetar o core
  completo de uma vez (loop + Integridade do Núcleo + 2 inimigos + 3 upgrades) numa
  sessão só, para minimizar handoffs nas próximas 6-8h mais críticas.

## Depois de submeter
Se passar na pré-seleção, retome o **plano consolidado de 67h** do guia de orquestração
original como Fase 2, com o backlog acima como ponto de partida: boss, inimigos
restantes, upgrades restantes, arte customizada, áudio completo, narrativa expandida.
