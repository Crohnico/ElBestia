# Combate

## Resumen

El combate es idle/autonomo. El jugador no controla al campeon durante la pelea: observa la simulacion.

El sistema debe ser lo bastante determinista como para poder reproducir combates async usando datos compactos de campeones, pero tambien debe producir momentos impredecibles y dramaticos.

## Combat Manager

Habra un `CombatManager` al que se le envian dos combatientes.

Responsabilidades:

- Recibir IDs/niveles o datos reconstruidos de los dos campeones.
- Cargar campeon A y campeon B.
- Instanciarlos en sus lados del escenario.
- Gestionar cuenta atras.
- Gestionar reloj de combate.
- Resolver turnos/action slots.
- Ejecutar movimiento hasta rango.
- Ejecutar habilidades.
- Detectar fin de combate.
- Informar resultado al sistema de torneo.

## Herramientas de Inspector

Para facilitar pruebas, el manager tendra botones en el inspector.

### Boton 1: Load Champions

Carga e instancia los dos campeones.

Debe permitir probar:

- Carga desde IDs.
- Carga desde datos mock/locales.
- Instanciacion visual en cada lado.
- Aplicacion de apariencia.
- Aplicacion de skills/equipo/perks.

### Boton 2: Start Combat

Arranca el combate.

Flujo:

```text
3
2
1
GO
```

Despues del `GO` empieza el reloj.

## Reloj de Combate

El combate tendra un timer para evitar combates infinitos.

Valor inicial propuesto:

```text
60 segundos
```

El reloj debe ser configurable por torneo.

El torneo define:

- Si hay reloj.
- Duracion.
- Que ocurre cuando llega a 0.

## Reglas al Llegar a 0

Posibles comportamientos:

**Gana quien tenga mas vida**

- Al llegar a 0, se comparan vidas actuales.
- Gana el campeon con mas vida restante.

**Muerte subita por desgaste**

- Al llegar a 0, empieza a bajar poco a poco la vida de ambos campeones.
- El combate continua hasta que uno caiga.

**Sin limite**

- El reloj esta desactivado.
- El combate termina solo por derrota/muerte u otra condicion del torneo.

Pendiente:

- Desempate si ambos tienen la misma vida.
- Si se compara vida absoluta o porcentaje de vida.
- Si algunos torneos tienen reglas especiales.

## Crono Unico

El combate usa un `CombatCrono` como fuente unica de tiempo jugable.

Todo lo que pertenezca al combate debe depender de ese crono:

- Reloj de partida.
- Timeline de accion.
- Cooldowns visuales de habilidades.
- Movimiento hacia rango.
- Movimiento de vuelta a posicion inicial.

Cuando el crono se pausa porque un campeon esta ejecutando una accion, esos sistemas tambien se paran. `Time.deltaTime` queda reservado para cosas puramente visuales que deban seguir aunque el combate este pausado.

API inicial:

```text
CurrentTime
DeltaTime
Pause()
Resume()
ResetTime()
```

## Action Timeline

La UI del crono de accion tiene tres puntos:

- `LeftPlayerInitPos`.
- `RightPlayerInitPos`.
- `Center`.

Al preparar combate se instancia un marcador UI por campeon como hijo de su posicion inicial. El marcador muestra por ahora el icono de la skill base como avatar temporal del campeon.

Cada campeon genera una `CharacterIntention`:

```text
champID
timeStamp
```

El `timeStamp` es absoluto contra `CombatCrono.CurrentTime`. Cuando `CurrentTime >= timeStamp`, la timeline pausa el crono y pide al `ChampionBehaviour` que ejecute accion.

Flujo:

1. El campeon tira su proximo delay de accion.
2. La timeline calcula `timeStamp = CurrentTime + delay`.
3. El marcador UI viaja desde su inicio hasta `Center` usando el crono.
4. Al llegar al centro, se pausa el crono.
5. `ChampionBehaviour` elige skill aleatoria.
6. Se dispara el cooldown visual de esa skill.
7. Al completar la accion, se calcula la siguiente intention.
8. Se reanuda el crono.

Si ambos campeones llegan en el mismo instante, se desempata con dado.

## Flujo Implementado de una Accion

Esta seccion describe el flujo actual y debe tratarse como referencia al modificar `ArenaGameManager`, `ActionTimeLine`, `ChampionBehaviour` o la presentacion visual del combate.

1. `ActionTimeLine` detecta que un campeon ha alcanzado su slot.
2. Se pausa `CombatCrono`.
3. Ambos campeones reciben la orden visual de mirar hacia el rival.
4. Se aplican los efectos de inicio de turno y el campeon elige skill.
5. Se ejecuta `PreCast`.
6. Si el `Cast` necesita alcance enemigo, el campeon gira primero y avanza hasta rango.
7. El campeon reproduce el slash correspondiente a su arma.
8. En el porcentaje configurado del slash se crea el proyectil o SFX.
9. El `Cast` se resuelve cuando el proyectil impacta. Tras resolver el dano real, activa `HIT` si el rival sobrevive o `DEATH` si queda a `0`.
10. Se resuelven contraataques pendientes, cargas de fin de turno, `PostCast`, ecos y perks posteriores.
11. Se ordena al campeon volver al origen y la accion se marca como completada inmediatamente.
12. `ActionTimeLine` agenda el siguiente slot del actor y reanuda `CombatCrono`.
13. El campeon vuelve al origen en paralelo al avance normal del combate.
14. Al llegar al origen, gira para volver a mirar al rival. Este giro tampoco bloquea el cronometro ni los siguientes turnos.

Regla importante: el retorno al origen y su giro final son presentacion/movimiento posterior a la accion. Nunca se debe retrasar `FinishAction`, la reanudacion de `CombatCrono` o el siguiente slot esperando a que terminen.

### Fases de Skill

- `PreCast`: ocurre antes del ataque principal. Mas adelante puede tener animacion propia.
- `Cast`: su callback de completado coincide con el impacto del proyectil/SFX, no con el inicio ni el final del slash.
- `PostCast`: consecuencia inmediata posterior al impacto.

### Orientacion Visual

La arena usa un offset de camara de `32` grados:

- Campeon izquierdo hacia el rival: `Y = 32`.
- Campeon derecho hacia el rival: `Y = 180 + 32`.
- Campeon izquierdo volviendo a casa: `Y = 180 + 32`.
- Campeon derecho volviendo a casa: `Y = 32`.

Los campeones siempre completan el giro correspondiente antes de empezar a desplazarse. `FORWARD` solo usa valores entre `0` y `1`; nunca se desplazan visualmente hacia atras.

## Flujo de Turnos

Tras cargar ambos campeones y empezar el combate:

1. Ambos campeones tiran su dado de turno.
2. Cada campeon reserva su slot de accion en el cronometro.
3. El cronometro avanza hasta el siguiente slot.
4. El tiempo se pausa.
5. El campeon activo selecciona una habilidad/accion.
6. El campeon se mueve al rango necesario.
7. Ejecuta la accion.
8. Se aplican efectos, dano, curaciones y cargas.
9. El campeon tira o calcula su siguiente slot.
10. El cronometro continua.

## Empates de Slot

Si ambos campeones reservan accion en el mismo instante:

- Se tira un dado de desempate.
- El ganador del desempate actua primero.

Pendiente:

- Si agilidad modifica el dado de desempate.
- Si algunos estados como rapidez o lentitud modifican prioridad.
- Si el segundo campeon mantiene su accion aunque el primero lo interrumpa, lo mate o le aplique control.

## Seleccion de Habilidad

Direccion actual:

- La seleccion sera principalmente aleatoria.
- Mantener gracia tipo El Bruto.
- Evitar control directo del jugador.

Posibles modificadores futuros:

- Inteligencia mejora pesos de decision.
- Perks fuerzan o bloquean ciertas acciones.
- Estados como poca vida aumentan probabilidad de cura.

## Defensas y Precision

El combate separa indices internos de porcentajes finales.

- `Dodge Rating` es el indice bruto de esquiva.
- `Block Rating` es el indice bruto de bloqueo.
- `Hit Rating` es el indice bruto de golpe y se enfrenta a las defensas del rival.
- Los porcentajes mostrados en inspector son una lectura derivada, no la cantidad plana que suma un perk.

Direccion actual:

- Si un perk dice `+9 Dodge Rating`, suma 9 al indice de esquiva.
- Si un perk dice `+9 Hit Rating`, suma 9 al indice de golpe.
- La resolucion final de esquiva/bloqueo debe comparar la defensa del objetivo contra la presion de golpe del atacante.

Perks especiales previstos:

- `Ghost Strike`: si un ataque es esquivado, se repite al 50% de potencia, con minimo 5 de dano. Puede encadenarse hasta que falle el golpe reducido a 5.
- `Block Pierce`: permite que parte del dano bloqueado atraviese el bloqueo. La familia empieza en rareza rara y escala hasta legendaria.

## Elementos y Resistencias

Cada campeon tiene resistencia por elemento:

- Fire.
- Water.
- Electricity.
- Poison.
- Earth.
- Air.
- Wood.

La resistencia elemental funciona como rating interno. La lectura humana se transforma en porcentaje con tendencia a un maximo propuesto del 70%, parecido a resistencias defensivas de MOBAs: cada punto ayuda, pero nunca convierte al campeon en inmune por acumulacion normal.

La inmunidad real debe venir de perks especiales, no de subir rating.

Perks especiales:

- `Elemental Absorption`: perk legendario unico. El dano y dot de ese elemento curan al campeon. La descripcion debe indicar que no puede aprender otra absorcion elemental.
- `Elemental Self Dot On Action`: aplica un dot elemental a ambos campeones despues de actuar.
- `Elemental Explosion On Hit`: cada golpe crea una explosion elemental que afecta a ambos combatientes.
- `Inverted Buffs`: invierte buffs/debuffs recibidos, por ejemplo velocidad en lentitud o lentitud en velocidad.

Dots elementales actuales:

- Fire: `Burn`.
- Water: `Drowning`.
- Electricity: `Shock`.
- Poison: `Poison`.
- Earth: `Crush`.
- Air: `WindShear`.
- Wood: `Splinter`.
- Falta de energia obliga a elegir otra accion o esperar.

## Rango y Movimiento

Cada habilidad tiene un rango valido.

Antes de ejecutar:

- Si el campeon ya esta en rango, ejecuta.
- Si no esta en rango, se mueve al punto necesario.
- El rango visual minimo de ejecucion es `1.5u`, aunque la skill tenga un rango numerico inferior.
- El movimiento hacia rango forma parte de la accion activa y ocurre mientras `CombatCrono` esta pausado.
- El movimiento de vuelta ocurre despues de completar la accion, con `CombatCrono` reanudado, y puede solaparse con futuros slots.

## Condicion Base de Victoria

Base:

- El que mate o derrote al otro gana.

Condiciones dependientes de torneo:

- Mejor de 1.
- Mejor de 3.
- Ganar por vida al terminar el reloj.
- Muerte subita.
- Reglas especiales futuras.
