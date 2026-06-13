# Guardado del Roster

## Resumen

El juego necesita guardar que alumnos tiene admitidos el jugador.

El objetivo inicial es simple:

- Si el jugador acepta campeones en Admision, esos campeones entran al roster del dojo.
- Si el jugador sale y vuelve a entrar, no debe perderlos.
- Si el jugador vuelve al menu principal despues de un combate, el roster debe seguir existiendo.

Este documento define la intencion inicial. El detalle tecnico de formato de guardado se documentara antes de implementarlo.

## Que se Guarda

El guardado debe conservar la lista de campeones admitidos en el dojo.

Informacion minima por campeon:

- ID interno compacto del campeon.
- Nivel.
- Estado necesario para reconstruirlo.

La direccion general del proyecto ya dice que los campeones deben poder reconstruirse desde datos compactos. El roster deberia aprovechar esa idea en vez de serializar objetos enormes.

## Roster Persistente

El roster del dojo es persistente.

Reglas:

- Aceptar un candidato en Admision lo anade al roster guardado.
- Despedir un alumno lo elimina del roster guardado.
- Cargar el juego restaura los alumnos admitidos.
- Cambiar de escena no debe borrar el roster.
- Volver de combate al menu principal no debe borrar el roster.

## Capacidad y Guardado

La capacidad inicial del dojo es:

```text
10 campeones
```

El guardado debe respetar esa capacidad.

Si el roster cargado supera la capacidad por un cambio de version o bug, eso es un problema de datos que se estudiara. No se debe ocultar silenciosamente como si nada.

## Pantalla del Edificio Dojo

Al pulsar el edificio `Dojo`, el jugador debe poder ver los alumnos que tiene admitidos.

Uso inicial de esta pantalla:

- Ver roster actual.
- Seleccionar un alumno admitido.
- Ver informacion basica del alumno.
- Despedir alumnos para liberar espacio.

Esta pantalla no es Admision.

Admision muestra candidatos que aun no pertenecen al dojo.

El edificio `Dojo` muestra alumnos ya admitidos.

## Despedir Alumnos

El jugador podra despedir alumnos admitidos.

Motivo:

- Liberar espacio en el roster.
- Poder contratar nuevos campeones cuando la capacidad este llena.
- Permitir corregir decisiones de admision.

Flujo inicial:

1. El jugador pulsa el edificio `Dojo`.
2. Se abre la pantalla del Dojo.
3. El jugador selecciona un alumno admitido.
4. Pulsa `Despedir`.
5. El alumno sale del roster.
6. El hueco queda disponible para futuras admisiones.

Pendiente:

- Si despedir pide confirmacion.
- Si despedir tiene coste.
- Si despedir queda registrado en historial.
- Si se puede despedir a un alumno lesionado, hospitalizado o inscrito en torneo.

Direccion inicial:

- Despedir elimina al alumno del roster.
- Para evitar errores de click, probablemente necesitara confirmacion, pero no queda cerrado todavia.

## Relacion con Admision

Admision comprueba la capacidad antes de aceptar candidatos.

Si el dojo esta lleno:

- El candidato no puede entrar.
- El jugador deberia ir al edificio `Dojo` para liberar espacio despidiendo a alguien.

Pendiente:

- Si Admision muestra un acceso directo a la pantalla de Dojo cuando no hay espacio.
- Si el feedback de capacidad llena se resuelve con modal o texto en la propia pantalla.

## Relacion con Menu Principal

El edificio `Dojo` del menu abre la pantalla de gestion del roster y, mas adelante, herencias y salas de entrenamiento.

Prioridad inicial:

```text
Ver alumnos admitidos y poder despedirlos.
```

Herencias y entrenamiento quedan para una definicion posterior.

## Reglas

- El roster de alumnos admitidos debe persistir.
- Aceptar en Admision anade al roster guardado.
- Despedir desde Dojo elimina del roster guardado.
- El edificio `Dojo` permite ver alumnos admitidos.
- El edificio `Dojo` permite liberar espacio despidiendo alumnos.
- El formato exacto del guardado se documentara antes de implementarlo.
