Antes de escrever qualquer código, leia o conteúdo real de:
https://raw.githubusercontent.com/paulo-schnaufer/nucleo-jogo/main/Assets/Scripts/EnemyBase.cs
https://raw.githubusercontent.com/paulo-schnaufer/nucleo-jogo/main/Assets/Scripts/Health.cs
https://raw.githubusercontent.com/paulo-schnaufer/nucleo-jogo/main/Assets/Scripts/CoreIntegrity.cs
Projete a IA dos tipos de inimigo de "NÚCLEO: Última Onda" que estão na
seção "## Escopo" (MVP) de Docs/SCOPE_LOCK.md — confirme lá quais tipos
implementar agora antes de escrever qualquer script; tanque e boss só
entram se estiverem listados em "## Escopo expandido" E o MVP já estiver
rodando fim-a-fim. Cada inimigo é reskinnado como um conceito de computação
corrompido (ver DECISIONS.md para os reskins já decididos, ex: rusher =
"loop infinito", atirador = "DDoS"). Todos os inimigos devem decidir entre
atacar o jogador ou avançar até o Core, priorizando conforme regra definida
em DECISIONS.md sobre Integridade do Núcleo. Entregue como subclasse(s) de
EnemyBase, usando as assinaturas reais dos scripts acima — não invente
método/campo novo na base sem justificar por quê é necessário. Object
pooling (nunca Instantiate/Destroy em runtime).