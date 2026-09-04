Antes de escrever qualquer código, leia o conteúdo real de:
https://raw.githubusercontent.com/paulo-schnaufer/nucleo-jogo/main/Assets/Scripts/PlayerStats.cs
https://raw.githubusercontent.com/paulo-schnaufer/nucleo-jogo/main/Assets/Scripts/UpgradeManager.cs
https://raw.githubusercontent.com/paulo-schnaufer/nucleo-jogo/main/Assets/Scripts/UpgradeData.cs
https://raw.githubusercontent.com/paulo-schnaufer/nucleo-jogo/main/Assets/Scripts/AutoTurretWeapon.cs
Se algum desses arquivos já existir (é o caso agora), sua tarefa é ESTENDER
o que já existe, nunca criar uma segunda classe PlayerStats/UpgradeManager
paralela em outro namespace ou pasta — mesmo que pareça mais "limpo"
começar do zero. Se você achar que o desenho existente tem uma limitação
real, escreva isso como observação no seu bloco final de STATUS.md, não
como um sistema novo.
Projete o sistema de upgrades de "NÚCLEO: Última Onda". A lista exata de
upgrades a implementar AGORA está em Docs/SCOPE_LOCK.md, seção "## Escopo"
(MVP) — não implemente nada listado em "## Escopo expandido", mesmo que
pareça fácil de encaixar. Entregue scripts de cada upgrade do pool do MVP,
usando o UpgradeData/UpgradeManager/PlayerStats já existentes (acima).
Aplique nomes com leve metáfora de conceitos de computação (ex:
paralelização, cache, redundância).