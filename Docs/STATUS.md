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

**Decisões novas (copiar para DECISIONS.md):**
- Contorno de sprite: **nenhum** (não 1px). Motivo: maioria dos assets CC0 (Kenney) já é flat sem contorno; retrabalhar contorno em todos os assets não cabe no prazo; leitura visual sustentada por paleta fixa + shading flat + bloom.
- PPU fixo em 64 para todos os sprites, com canvas variando por tier de entidade (jogador/atirador 64px, rusher 48px, tanque 96px, boss 192px, projétil padrão 16px, ícone 64px).
- Núcleo (Integridade) recebe cor própria (CORE-VIOLETA, `#B45CFF`), distinta de jogador (ciano) e inimigos (magenta).
- VFX: regra de no máximo 2 cores por Particle System, nunca ciano+magenta no mesmo efeito; âmbar reservado exclusivamente para telegraphs/avisos críticos.