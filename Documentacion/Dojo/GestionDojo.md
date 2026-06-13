# Gestion del Dojo

## Resumen

Este documento habla del `Dojo` como institucion del jugador, no del edificio `Dojo` del menu principal.

El nombre puede traer confusion:

- `Dojo` como institucion: roster, capacidad, progreso, campeones admitidos.
- `DojoBuilding` como edificio del menu: entrada visual a herencias, salas de entrenamiento y gestion interna.

Cuando haya riesgo de confusion, conviene hablar de `Dojo institucion` y `DojoBuilding`.

## Capacidad de Campeones

El dojo tiene un limite de campeones.

Valor inicial:

```text
Capacidad maxima: 10 campeones
```

Por ahora esta capacidad no aumenta.

Mas adelante podra crecer con fama, instalaciones, mejoras o progreso del dojo, pero queda fuera de la primera definicion.

## Roster

El roster del dojo contiene los campeones admitidos por el jugador.

Reglas iniciales:

- Un campeon aceptado en Admision entra al roster del dojo.
- El roster debe guardarse para no perder alumnos al salir, volver al menu o cambiar de escena.
- El roster no puede superar la capacidad maxima.
- La capacidad inicial es 10.
- Por ahora no hay expansion de capacidad.

Pendiente:

- Que feedback exacto recibe el jugador si intenta aceptar un campeon con el dojo lleno.
- Si se permite liberar espacio desde Admision o hay que ir a otra pantalla.
- Si campeones muertos, retirados o hospitalizados cuentan contra la capacidad.

Detalle de persistencia: [GuardadoRoster.md](./GuardadoRoster.md).

## Relacion con Admision

Admision es la fuente inicial de nuevos campeones.

Cuando el jugador acepta un candidato:

1. Se comprueba la capacidad del dojo.
2. Si hay hueco, el candidato pasa al roster.
3. Si no hay hueco, no se admite hasta que exista espacio o se defina otra solucion.

La decision de aceptar un campeon debe importar porque el espacio es limitado.

## Relacion con el Edificio Dojo

El edificio `Dojo` del menu permite abrir una pantalla de gestion de alumnos admitidos.

Prioridad inicial de esa pantalla:

- Ver los alumnos actualmente admitidos.
- Seleccionar un alumno.
- Ver informacion basica del alumno.
- Despedir alumnos para liberar espacio.

Esto permite que el jugador pueda contratar mas candidatos si la capacidad del dojo esta llena.

## Regla de Diseno

La capacidad del dojo fuerza decisiones.

Con un limite inicial de 10, el jugador no deberia aceptar todo sin pensar. Admision no es solo una tienda de personajes: es una decision de cantera.
