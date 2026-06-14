# Alcance del Proyecto

## Identidad del Proyecto

**Nombre provisional:** El Bestia

**Referencia principal:** El Bruto.

**Genero base:** Manager/idle async de luchadores con permadeath, progresion de dojo y torneos.

**Plataforma objetivo:** PC.

**Fantasia principal:** Dirigir un dojo de campeones impredecibles. El jugador no controla los combates directamente: contrata, observa, arriesga, invierte y crea una herencia marcial entre generaciones.

**Promesa jugable:** Tus luchadores tienen vida propia. Crecen, pelean, se lesionan, ganan gloria o mueren. Tu trabajo es decidir a quien contratar, a que torneo enviarlo, cuando retirarlo y como convertir sus victorias y cicatrices en futuro para el dojo.

## Pilares

### 1. Campeones Autonomos

Los personajes evolucionan por si mismos. El jugador no decide cada punto de stat ni controla sus acciones durante el combate.

- El jugador selecciona quien pelea.
- El campeon decide, de forma automatica, como pelea.
- La mejora del campeon ocurre como consecuencia de participar, sobrevivir y subir de nivel.
- Parte de la calidad real del campeon esta oculta al jugador.

### 2. Riesgo Real

Perder importa. Un luchador puede volver herido, quedar fuera durante un tiempo, necesitar tratamiento urgente o morir.

- Las derrotas pueden dejar secuelas.
- La muerte debe ser posible y emocionalmente relevante.
- El jugador debe sentir tension al apuntar a un luchador prometedor a un torneo peligroso.
- La gestion medica y la capacidad del hospital forman parte del nucleo del juego.

### 3. Herencia del Dojo

La muerte y la retirada no cortan la progresion del jugador. El dojo acumula legado.

- Los luchadores pueden jubilarse.
- Un luchador jubilado puede ensenar una habilidad, pasiva o perk a futuras generaciones.
- Las instalaciones permiten transformar campeones viejos o valiosos en progreso permanente.
- El jugador mejora aunque pierda individuos, siempre que gestione bien su cantera y su legado.

### 4. Incertidumbre con Lectura

El jugador no conoce todos los numeros internos, pero puede desarrollar intuicion.

- Antes de contratar, ve stats base y movimientos.
- No ve claramente el potencial real de crecimiento.
- El crecimiento oculto debe poder intuirse con el tiempo por rendimiento, ritmo de mejora y comportamiento.
- La aleatoriedad debe generar historias, no solo frustracion.

## Sensacion Objetivo

### Palabras Clave

- Impredecible.
- Punitivo.
- Dinastico.
- Competitivo.
- Observar con tension.
- Gestionar riesgo.

### El Jugador Deberia Sentir

- Que cada campeon tiene personalidad por sus stats, apariencia, historia, comportamiento y frases de combate.
- Que mandar a pelear a alguien importante es emocionante y peligroso.
- Que una derrota puede doler, pero tambien abrir decisiones interesantes.
- Que el dojo progresa como institucion incluso si sus campeones mueren.
- Que los grandes campeones se convierten en leyendas recordadas por el sistema.
- Que mirar un combate automatico puede ser tan tenso como jugarlo directamente.

### El Juego No Deberia Sentirse

- Como un RPG donde optimizas cada punto manualmente.
- Como un autobattler completamente transparente y calculable.
- Como un juego donde perder solo significa esperar o repetir.
- Como una experiencia donde la muerte es aleatoria sin preparacion ni respuesta posible.
- Como un simulador pasivo sin decisiones relevantes entre combates.

### Ritmo

- Sesiones cortas para contratar, revisar luchadores, apuntar a torneos y ver resultados.
- Combates observables, tensos y relativamente rapidos.
- Torneos async que el jugador puede jugar cuando quiera, salvo eventos concretos especiales.
- Momentos de calma en la gestion del dojo, instalaciones, hospital, jubilaciones y eleccion de torneos.
- Momentos de tension al esperar resultados, resolver heridas graves o decidir triaje.

## Loop Jugable

### Loop de 10 Segundos

Durante un combate, el jugador observa:

- Carga de acciones de cada campeon.
- Pausas del combate cuando toca ejecutar una accion.
- Movimiento hacia rango valido.
- Resolucion de habilidad, golpe, defensa, curacion o efecto.
- Reanudacion del cronometro hasta la siguiente accion.

El jugador no interviene directamente. La tension viene de entender que podria pasar y no poder corregir al campeon.

### Loop de 1 Minuto

El jugador revisa un combate o tramo de torneo:

- Ver que acciones usa cada campeon.
- Detectar si un luchador es prometedor, torpe, resistente, agresivo o fragil.
- Sufrir heridas, remontadas, malas decisiones automaticas y golpes decisivos.
- Confirmar si el campeon avanza, pierde o muere.

### Loop de 10-20 Minutos

El jugador gestiona una sesion de dojo:

- Contratar luchadores de nivel 1 viendo stats base y movimientos.
- Elegir a que torneos apuntar cada campeon.
- Cobrar recompensas segun hasta donde llegan.
- Gestionar retornos tras combate: intacto, heridas leves, moderadas, graves, muerte o riesgo de muerte.
- Asignar plazas limitadas de hospital.
- Mejorar instalaciones.
- Jubilar campeones para transmitir habilidades, pasivas o perks.

### Loop de Campana / Meta

El jugador construye un dojo que sobrevive a sus campeones:

- Crear una linea de luchadores cada vez mejor preparada.
- Desbloquear torneos superiores mediante victorias previas.
- Ganar equipamiento como premio de torneos.
- Escalar hacia torneos de prestigio.
- Competir por registros publicos de campeones y puntuacion de dojo.
- Preparar campeones capaces de defender titulos.

## Contratacion y Campeones

Detalle tecnico: [Campeones.md](./Campeones.md).

### Contratacion

El jugador puede contratar luchadores de nivel 1.

Antes de contratar, puede ver:

- Stats base.
- Movimientos iniciales.
- Habilidades iniciales generadas aleatoriamente.

No puede ver con precision:

- Potencial real de crecimiento por stat.
- Calidad futura del campeon.
- Si sera una estrella, un luchador mediocre o alguien util solo como puente generacional.

### Stats Base

Los campeones empiezan con una cantidad total de puntos de stats, por ejemplo 50 puntos a nivel 1.

- Esos puntos se reparten aleatoriamente entre las stats disponibles.
- Las stats exactas se definiran mas adelante.
- El reparto inicial puede hacer que dos campeones del mismo nivel se sientan muy distintos.

### Lore Inicial

Cada campeon recibe un origen tipo RimWorld dividido en:

- Nacimiento.
- Ninez.
- Juventud.

Cada etapa aplica pequenos modificadores iniciales y genera texto de historia. Esto permite que dos campeones con stats parecidas se sientan distintos por contexto, no solo por numeros.

### LLM Local para Identidad

Direccion actual:

- El juego se hara solo para PC para poder integrar un LLM dentro del juego.
- El LLM se usara para generar texto de identidad y expresion de los campeones, no para resolver reglas de combate.
- El modelo objetivo sera pequeno, aproximadamente entre 1B y 4B parametros.
- La motivacion es evitar gasto recurrente en tokens y no depender de una API externa para generar textos de campeon.
- Primero se generan personalidad, nacimiento, ninez y juventud como datos estructurados.
- Despues se llama una vez al LLM y el resultado se guarda en la data del personaje.
- Uso previsto:
  - Biografia breve del campeon, explicando como se forjo y por que llega al edificio de admision del Dojo.
  - 10 frases de inicio de combate.
  - 10 frases al golpear.
  - 10 frases al recibir golpe.
  - 10 frases al ganar.
  - 10 frases al perder.
- Esta integracion se considera factible, pero debe mantenerse acotada para no romper el determinismo de gameplay ni convertir el sistema de combate en una caja negra.

### Crecimiento Oculto

Cada campeon tiene un crecimiento oculto por atributo.

Modelo base propuesto:

- Al subir de nivel, la experiencia necesaria para subir se aplica a cada stat.
- Cada stat tiene un multiplicador propio por campeon.
- El potencial puede expresarse internamente con rangos tipo F, E, D, C, B, A, S, SS.
- Una stat SS crece muy rapido.
- Una stat F crece muy lento.

El jugador no ve directamente estos rangos. Debe inferirlos por rendimiento y evolucion.

La experiencia a siguiente nivel usa una curva sin cap:

- A nivel bajo, todos los rangos piden XP parecida.
- A largo plazo, el rango oculto empieza a pesar mucho mas.
- Objetivo de balance: un campeon excelente puede rondar nivel 70 con una inversion de juego parecida a la que deja a uno mediocre cerca de nivel 40.
- La ficha del campeon muestra `XP actual / XP siguiente nivel`, pero no revela el rango oculto como informacion de jugador.

## Combate

Detalle tecnico: [Combate.md](./Combate.md).

### Formato

El combate es idle/autonomo. El jugador solo observa.

Presentacion objetivo:

- Dos campeones frente a frente.
- Escenario tipo dojo de combate.
- Camara fija lateral.
- Estetica por decidir: 2D o 3D ortogonal.

### Sistema de Action Bar

Cada campeon genera su proximo momento de accion en funcion de su velocidad de accion.

Ejemplo:

- Campeon A tira su tiempo de accion y actuara en el segundo 3.
- Campeon B tira su tiempo de accion y actuara en el segundo 4.
- El cronometro avanza.
- En el segundo 3, el tiempo se pausa y A selecciona una accion.
- A se mueve hasta el rango necesario: cuerpo a cuerpo, distancia media o rango.
- A ejecuta la accion.
- El cronometro se reanuda.
- Si antes de que A vuelva a su sitio llega el segundo 4, el tiempo se pausa.
- B ejecuta su accion desde su situacion actual o tras moverse a rango.

Este sistema permite que movimiento, rango y tiempos se crucen de forma emergente.

### Defensa, Bloqueo e Indice de Golpe

Los campeones tendran indices defensivos y ofensivos separados:

- `Dodge Rating`: indice bruto de esquiva.
- `Block Rating`: indice bruto de bloqueo.
- `Hit Rating`: indice bruto de golpe, usado para pelear contra esquiva y bloqueo.

Estos indices no son porcentajes planos. El inspector puede mostrar un porcentaje derivado para lectura humana, pero los perks suman al indice.

Ejemplos:

- Un perk `+9 Dodge Rating` aumenta el indice de esquiva, no un `+9%`.
- Un perk `+9 Hit Rating` aumenta la presion del atacante contra defensas.
- Los perks de traspasar bloqueo hacen que parte del dano bloqueado atraviese la defensa.
- Un perk legendario tipo `Ghost Strike` permite repetir un golpe esquivado con potencia reducida.

### Resistencias Elementales y Perks Raros

Los elementos actuales son fuego, agua, electricidad, veneno, tierra, aire y madera.

Cada campeon tiene resistencia elemental por rating. El porcentaje derivado tiende hacia un maximo aproximado del 70%, para que la resistencia normal nunca llegue a inmunidad total.

La inmunidad o absorcion real debe venir de perks legendarios unicos:

- Fire Eater.
- Tide Drinker.
- Storm Drinker.
- Venom Saint.
- Stone Eater.
- Sky Lung.
- Root Drinker.

Estos perks pertenecen al grupo unico `elemental_absorption`: si un campeon aprende uno, no puede aprender otro de absorcion elemental.

Tambien se contemplan perks de escuela peligrosa:

- Aplicar dots elementales a ambos combatientes.
- Crear explosiones elementales que golpean a ambos.
- Empezar debilitado y despertar al estar herido.
- Invertir buffs y debuffs recibidos.

### Seleccion de Acciones

Decision abierta:

- Opcion A: seleccion completamente aleatoria, mas cercana a El Bruto.
- Opcion B: seleccion ponderada por contexto.
- Opcion C: stat de inteligencia que mejora decisiones como curarse cuando toca.

Direccion actual preferida: mantener una base aleatoria por gracia, sorpresa y personalidad, y valorar mas adelante si algunas pasivas o stats modifican pesos sin convertirlo en control total.

### Condicion de Victoria

Base actual:

- El que mata o derrota al otro gana.
- El torneo puede definir reloj, duracion y regla al llegar a 0.

Decision abierta:

- Combate al mejor de 1.
- Combate al mejor de 3.
- Diferentes formatos segun torneo.

## Habilidades

Detalle tecnico: [Habilidades.md](./Habilidades.md).

Las habilidades se generan aleatoriamente y pueden subir de nivel.

Cada campeon puede tener hasta 3 skills.

Una habilidad se compone de:

- Icono procedural.
- Stat o stats de escalado.
- Requisito de arma.
- Rango.
- Elemento.
- Coste de energia.
- Acciones de `PreCast`.
- Acciones de `Cast`.
- Acciones de `PostCast`.

La calidad de la habilidad usa rangos `F, E, D, C, B, A, S, SS` y afecta a cuantas fases puede tener.

## Heridas, Muerte y Hospital

### Resultado Tras Combate

Al volver de un combate, un campeon puede:

- Volver intacto.
- Volver con heridas leves y necesitar poco tiempo de recuperacion.
- Volver con heridas moderadas y quedar fuera mas tiempo.
- Volver con heridas graves.
- No volver.
- Volver tan grave que morira en dias si el jugador no logra salvarlo.

### Hospital

El hospital debe sentirse prioritario.

- Permite tratar heridas graves.
- Reduce mortalidad o secuelas.
- Tiene capacidad limitada.
- Puede obligar a aplicar triaje cuando varios campeones necesitan ayuda.
- Mejorar el hospital es una inversion clave del dojo.

### Triaje

Cuando no hay capacidad suficiente, el jugador debe decidir:

- Salvar al campeon con mas potencial.
- Salvar al campeon con mas legado.
- Salvar al campeon mas joven.
- Salvar al campeon que puede competir pronto.
- Dejar morir o empeorar a alguien por falta de recursos.

Esta decision debe ser dura, clara y memorable.

## Dojo e Instalaciones

El dinero se gana segun lo lejos que llegue un campeon en un torneo.

Ese dinero se invierte en instalaciones.

Instalaciones confirmadas o sugeridas:

- Hospital: tratamiento, recuperacion, supervivencia.
- Sala de jubilacion/maestros: permite retirar campeones y transmitir habilidades, pasivas o perks.
- Instalaciones de entrenamiento: por definir.
- Capacidad de roster: por definir.
- Exploracion/reclutamiento: por definir.

## Torneos

### Torneos Async

Los torneos se juegan contra campeones de otros jugadores o de la maquina si no hay suficientes.

- Se usa la data de los personajes.
- No requiere que ambos jugadores esten conectados.
- El torneo se genera para el jugador y puede jugarlo cuando quiera.
- Existen excepciones: algunos torneos concretos seran "reales" o con calendario especial.

### Recompensas

Segun lo lejos que llegue el campeon:

- Gana mas o menos dinero.
- Puede conseguir equipamiento.
- Puede desbloquear acceso a torneos superiores.
- Puede ganar prestigio para el dojo.

### Equipamiento

El equipamiento se consigue como premio de torneos.

- Armas y armaduras dependen del nivel/prestigio del torneo.
- El equipo de mayor grado deberia venir de torneos de alto prestigio.
- El nivel del campeon determina a que torneos puede apuntarse.

### Escalera de Torneos

Modelo de progresion:

- Ganar el torneo base desbloquea un torneo superior para ese campeon.
- Ganar ese torneo desbloquea el siguiente.
- La cadena escala hasta torneos de elite.
- Algunos torneos pueden requerir titulos concretos, nivel, equipamiento, ranking o historial.

### Torneo Mensual y Defensa de Titulo

Late game competitivo:

- Existe un torneo de maximo prestigio que se celebra una vez al mes.
- El ganador queda obligado a defender el titulo en cada edicion.
- La recompensa incluye armas y armaduras de grado mas alto.
- El campeon ganador queda registrado publicamente.

## Profundidad y Late Game

### Fuentes de Profundidad

- Evaluar campeones con informacion incompleta.
- Decidir cuando arriesgar a un luchador prometedor.
- Gestionar heridas y capacidad limitada del hospital.
- Retirar campeones antes de que mueran para preservar legado.
- Construir generaciones futuras mediante habilidades heredadas.
- Elegir rutas de torneos segun riesgo, recompensa y estado fisico.
- Decidir que equipamiento merece cada campeon.
- Entender el comportamiento automatico de cada build.
- Preparar campeones para defender titulos, no solo ganarlos.

### Decisiones Interesantes

- Contratar a un luchador con buenos movimientos pero stats base raras.
- Enviar a un campeon lesionado a un torneo rentable o dejarlo descansar.
- Usar una plaza de hospital en un veterano legendario o en una promesa joven.
- Jubilar a un campeon antes de exprimirlo mas.
- Guardar dinero para instalaciones o invertir en mejoras inmediatas.
- Apuntar a un torneo seguro para farmear o a uno peligroso para buscar gloria.
- Equipar al favorito o reforzar a un luchador secundario.

### Progresion

- Progresion horizontal: habilidades, pasivas, perks heredables, estilos de campeon y equipamiento.
- Progresion vertical: niveles de campeon, calidad de instalaciones, torneos desbloqueados y prestigio.
- Desbloqueos: torneos, instalaciones, mejoras medicas, capacidad, equipo de mayor grado.
- Especializacion: dojos que priorizan supervivencia, agresividad, cantera, legado, economia o elite competitiva.

### Late Game

El late game debe girar alrededor de:

- Campeones historicos.
- Torneos de prestigio.
- Defensa de titulos.
- Registro publico de ganadores.
- Puntuacion del dojo segun torneos ganados por sus campeones.
- Equipamiento de maximo grado.
- Decisiones de legado cada vez mas dolorosas.

## Alcance MVP

Version minima que demuestra que el nucleo funciona.

### Incluye

- Contratar luchadores de nivel 1.
- Ver stats base y movimientos antes de contratar.
- Nombres de luchador generados aleatoriamente.
- Generar crecimiento oculto por stat.
- Generar hasta 3 skills aleatorias por campeon.
- Generar skill base garantizada con cooldown 0.
- Generar cooldowns de skills elaboradas.
- Generar perks iniciales con rareza.
- Sistema basico de combate automatico 1v1 con action bar.
- `CombatManager` con carga de campeones y arranque manual desde inspector para pruebas.
- Torneo basico async contra datos locales/de maquina.
- Recompensa economica segun ronda alcanzada.
- Sistema de heridas simple: intacto, leve, grave, muerto.
- Hospital con capacidad limitada.
- Una instalacion de jubilacion que permita transmitir una habilidad, pasiva o perk.
- Mejora basica de al menos una instalacion del dojo.
- Registro simple de campeones y torneos ganados.

### No Incluye Inicialmente

- PvP online en tiempo real.
- Control directo del campeon durante combate.
- Sistema completo de rankings publicos.
- Torneo mensual real con calendario persistente.
- Gran cantidad de torneos encadenados.
- Todas las categorias de heridas y secuelas.
- Balance final de economia, crecimiento y mortalidad.

## Riesgos de Diseno

- Que la muerte se sienta injusta si el jugador no tuvo formas de anticipar o mitigar riesgo.
- Que el combate idle sea aburrido si no comunica bien tension, tiempos y consecuencias.
- Que el crecimiento oculto sea demasiado opaco y parezca puro azar.
- Que el jugador se encarine demasiado poco si los campeones mueren antes de crear historia.
- Que el hospital sea obligatorio pero no interesante si solo reduce porcentajes.
- Que la herencia rompa el balance si permite acumular poder sin suficientes costes.
- Que los torneos async pierdan gracia si se sienten como listas generadas sin identidad.

## Preguntas Abiertas

- Que significa exactamente morir frente a quedar derrotado?
- El combate sera mejor de 1, mejor de 3 o depende del torneo?
- La seleccion de acciones sera aleatoria pura, ponderada o influida por inteligencia?
- Que habilidades, pasivas y perks pueden heredarse?
- Que coste tiene jubilar a un campeon?
- Las heridas pueden dejar secuelas permanentes?
- Como se calcula la velocidad de accion?
- El juego sera 2D, 3D ortogonal o una mezcla?
- Como se presentara el registro publico de campeones y dojos?
- Cuanto dura una temporada competitiva?
- Cual sera la lista definitiva de elementos?
- Como se compactara exactamente el ID interno para que sea estable y no kilometrico?
