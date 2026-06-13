# Menu Principal - Dojo

## Resumen

La escena `Dojo` sera el menu principal del juego.

En vez de presentar un menu tradicional con botones de interfaz, el jugador interactua directamente con los edificios del dojo. La camara del menu permanece estatica y no se puede mover. La navegacion ocurre haciendo click sobre los edificios visibles de la escena.

## Elementos de la Escena

La escena contiene cuatro edificios principales interactuables:

- Arena.
- Hospital.
- Dojo.
- Admision.

Ademas, existira un campo de entrenamiento no interactuable.

El campo de entrenamiento forma parte de la vida visual del menu. Su objetivo es mostrar actividad del dojo, por ejemplo campeones entrenando o entrando al campo, pero no debe comportarse como boton ni abrir una pantalla.

## Interaccion Principal

Los cuatro edificios principales deben ser clickeables.

La camara se mantiene fija, asi que el jugador no navega moviendo la vista. El lenguaje de interaccion debe ser claro: si un edificio se puede usar, debe responder al puntero y al click.

### Hover

Al pasar el cursor por encima de un edificio interactuable:

- Se activa un outline.
- El outline indica que el edificio es seleccionable.
- El feedback debe ser suficiente para distinguir edificios interactuables de decoracion.

El campo de entrenamiento no debe mostrar outline de seleccion.

### Click

Al hacer click sobre un edificio interactuable:

- Se reproduce una animacion corta con DOTween.
- El edificio se hace un poco mas pequeno.
- Despues se hace un poco mas grande.
- Finalmente vuelve a su escala normal.

La animacion debe sentirse como una confirmacion fisica del click, no como una transicion larga.

## Edificios

### Arena

La arena es el lugar donde enviamos campeones a combatir.

Desde aqui el jugador deberia poder:

- Elegir campeones disponibles.
- Entrar en combates.
- Acceder a torneos o enfrentamientos cuando el sistema este disponible.

### Hospital

El hospital es donde internamos a campeones cuando vuelven de combatir y necesitan tratamiento.

Desde aqui el jugador deberia poder:

- Revisar campeones heridos.
- Asignar plazas de tratamiento.
- Gestionar recuperacion, heridas graves o riesgo de muerte cuando esos sistemas existan.

### Dojo

El dojo representa la gestion interna, las herencias y las salas de entrenamiento.

Desde aqui el jugador deberia poder:

- Ver los alumnos admitidos en el dojo.
- Despedir alumnos para liberar espacio de roster.
- Crear o gestionar herencias.
- Acceder a salas de entrenamiento.
- Gestionar mejoras internas relacionadas con el crecimiento de campeones y legado.

### Admision

La admision es el lugar donde reclutamos nuevos campeones.

Desde aqui el jugador deberia poder:

- Ver candidatos disponibles.
- Revisar stats base, movimientos y datos visibles.
- Contratar nuevos campeones para el dojo.

## Campo de Entrenamiento

El campo de entrenamiento no es interactuable.

Su funcion es ambiental y narrativa:

- Mostrar campeones entrando o entrenando.
- Dar sensacion de que el dojo esta vivo.
- Reforzar la fantasia de manager sin convertirse en una opcion de menu.

## Regla de UX

Todo lo que sea clickeable debe tener feedback de hover y click.

Todo lo que no sea clickeable no debe usar el mismo lenguaje visual de seleccion, para no confundir al jugador.

## Documentos Tecnicos

- [EdificiosYSignalBus.md](./EdificiosYSignalBus.md): contrato tecnico de edificios, senales de apertura y `SignalBus`.
- [CursorInteractivo.md](./CursorInteractivo.md): input de raton/touch, raycast, hover y click contra `IInteractable`.
- [../UI/CameraMover.md](../UI/CameraMover.md): movimiento de camara por snapshots para reforzar pantallas diegeticas.
