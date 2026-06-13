# Arena Inicial

## Resumen

Por ahora, al pulsar el edificio `Arena`, solo habra un torneo disponible.

Este torneo es la primera puerta de combate del jugador desde el menu principal. Su objetivo inicial es permitir apuntar discipulos, ejecutar el torneo y ver los combates de los campeones propios.

No se define todavia el sistema completo de torneos. Esta docu solo fija la direccion de la primera version.

## Entrada desde el Menu

Flujo inicial:

1. El jugador pulsa el edificio `Arena`.
2. Se mueve la camara hacia Arena.
3. Se abre la pantalla de Arena.
4. La pantalla muestra el torneo disponible.
5. El jugador selecciona discipulos para apuntarlos.
6. El jugador pulsa `Ejecutar`.
7. El sistema genera/resuelve los combates del torneo.
8. El jugador puede ver los combates de sus discipulos.

## Torneo Disponible

Primera version:

```text
Torneos disponibles: 1
```

Por ahora no hay seleccion entre varios torneos.

Mas adelante podran existir:

- Torneos de distinto nivel.
- Torneos por prestigio.
- Torneos desbloqueados.
- Torneos con reglas especiales.
- Torneos mensuales o eventos concretos.

## Inscripcion de Discipulos

El jugador podra apuntar varios discipulos al mismo torneo.

Intencion:

- El torneo no es solo "elige un campeon y mira una pelea".
- El jugador puede mandar una pequena expedicion de su dojo.
- Cada discipulo participa en su propio recorrido dentro del torneo.

Pendiente:

- Limite exacto de discipulos apuntables.
- Si se puede apuntar a todos los disponibles.
- Si hay coste de inscripcion.
- Si un discipulo lesionado puede apuntarse.
- Si hay requisitos minimos de nivel o estado.

## Ejecucion

La pantalla tendra una accion principal tipo:

```text
Ejecutar
```

Al pulsarla:

- Se confirma la lista de discipulos inscritos.
- Se genera o inicia el torneo.
- Se preparan los combates asociados a los discipulos del jugador.

Por ahora queda abierta la decision de si:

- Los combates se simulan todos de golpe.
- Los combates se van jugando/viendo uno a uno.
- El jugador decide cuales observar.
- Se pueden saltar combates.

## Visualizacion de Combates

La idea inicial es que el jugador pueda ver los combates de sus discipulos.

Direccion provisional:

- Habra un sistema de varios combates.
- El jugador podra cambiar entre combates de sus alumnos.
- Posible control: izquierda/derecha para pasar de un combate a otro.

Ejemplo mental:

```text
Combate de Ana  < izquierda/derecha >  Combate de Rodrigo  < izquierda/derecha >  Combate de Lucia
```

Pendiente:

- Si los combates ocurren en paralelo o son vistas de resultados ya calculados.
- Si izquierda/derecha cambia entre combates activos, combates pendientes o combates ya resueltos.
- Como se muestra el estado de cada discipulo en el torneo.
- Como se vuelve al mapa del torneo.

## Mapa de Torneo

La presentacion objetivo se acerca a un mapa clasico de torneo/bracket.

Referencia visual:

- Mapa grande de emparejamientos.
- Rondas mostradas por columnas.
- Lineas que conectan enfrentamientos.
- Iconos/retratos de campeones.
- Indicadores de vida, estado o resultado.
- Final destacada.
- Sensacion de pergamino/mapa de torneo.

La imagen de referencia apunta a una estructura tipo:

```text
Ronda inicial -> Ronda media -> Cuartos -> Semifinal -> Final
```

No hace falta implementar todavia toda esa complejidad. La primera version debe capturar la idea:

- Hay un bracket.
- Los discipulos del jugador estan ubicados en el bracket.
- El jugador puede entender en que ronda esta cada uno.
- El jugador puede acceder a los combates relevantes.

## Relacion con Arena

El edificio `Arena` del menu no ejecuta el torneo directamente.

Flujo esperado:

1. `ArenaBuilding.OnClick`.
2. Solicita abrir la pantalla de Arena.
3. La pantalla de Arena muestra el torneo inicial.
4. El jugador inscribe discipulos.
5. El jugador pulsa `Ejecutar`.

## Relacion con Combate

El torneo inicial usa el sistema de combate automatico existente.

El jugador no controla el combate directamente.

La pantalla de torneo debe servir como capa superior:

- Inscripcion.
- Bracket.
- Acceso a combates.
- Resultados.

El combate sigue resolviendose en la arena de combate.

## Fuera de Alcance Inicial

No definir todavia:

- Multiples torneos.
- Economia de inscripcion.
- Recompensas detalladas.
- Ranking publico.
- Defensa de titulo.
- Torneo mensual.
- Bracket final completo.
- Navegacion definitiva entre varios combates.

## Reglas

- Al principio solo hay un torneo disponible desde Arena.
- Se podran apuntar varios discipulos al torneo.
- El jugador pulsa `Ejecutar` para iniciar/resolver la participacion.
- El jugador podra ver los combates de sus discipulos.
- Se explorara una navegacion izquierda/derecha entre combates.
- La presentacion apunta a un mapa clasico de torneo con bracket.
- La definicion actual es de alto nivel; el detalle tecnico se documentara antes de implementar.
