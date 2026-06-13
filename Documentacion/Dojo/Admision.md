# Admision

## Resumen

Admision es el sistema desde el que el jugador recluta nuevos campeones para el dojo.

La pantalla de Admision se abre desde el edificio `Admision` del menu principal.

Flujo de entrada:

1. El jugador pulsa el edificio `Admision`.
2. Se mueve la camara hacia Admision.
3. Se abre la UI de Admision.
4. La pantalla solicita una lista de candidatos del dia.

## Lista Diaria de Candidatos

Admision genera una lista de campeones candidatos cada dia.

Valor inicial:

```text
Candidatos diarios: 5
```

Mas adelante, la cantidad de candidatos aumentara conforme el dojo sea mas famoso.

Por ahora:

- Se generan 5 candidatos.
- La fama del dojo no modifica todavia la cantidad.
- La lista se recarga cada dia.

## Que es un Dia

Por ahora, un dia ocurre cada vez que se carga el menu principal.

Ejemplo:

```text
Menu principal -> Combate -> Volver al menu principal = nuevo dia
```

Consecuencia:

- Cada vez que se carga la escena/menu principal, Admision debe poder generar o solicitar una nueva lista.
- Volver de combate al menu principal refresca la lista diaria.

Pendiente:

- Si la lista diaria se genera al cargar el menu principal o al abrir Admision por primera vez ese dia.
- Si la lista se mantiene igual al cerrar y reabrir Admision dentro del mismo dia.

Direccion inicial recomendada:

- Al cargar el menu principal se considera nuevo dia.
- Admision solicita `X` campeones al cargar su pantalla o al inicializar su estado diario.
- Dentro del mismo dia, cerrar y reabrir Admision no deberia regenerar candidatos si ya fueron solicitados.

## Solicitud de Candidatos

Admision necesita solicitar un numero `X` de campeones al cargar.

Valor inicial:

```text
X = 5
```

La solicitud genera campeones candidatos, no campeones admitidos.

Un candidato solo entra al dojo si el jugador pulsa `Aceptar`.

## Pantalla de Admision

La pantalla de Admision tendra una estructura inicial:

- Lista scrollable de nombres.
- Peana central con el campeon seleccionado.
- Datos del campeon seleccionado.
- Boton `Aceptar`.
- Boton `Expulsar`.

La colocacion final no esta cerrada.

Referencia mental inicial:

```text
Lista de nombres a la izquierda.
Peana con personaje en el centro.
Datos del campeon seleccionado a la derecha.
```

Por ahora, lo importante es conservar:

- Lista de nombres.
- Peana del campeon seleccionado.
- Botones de decision.

## Seleccion de Candidato

Al seleccionar un nombre de la lista:

- Se actualiza el campeon mostrado en la peana.
- Se actualizan los datos visibles del campeon.
- Los botones `Aceptar` y `Expulsar` actuan sobre ese candidato seleccionado.

Los datos visibles del campeon deberian respetar lo ya definido para contratacion:

- Stats base visibles.
- Movimientos o habilidades iniciales visibles.
- Informacion oculta de potencial no visible.

## Aceptar

El boton `Aceptar` admite al candidato en el dojo.

Flujo:

1. El jugador selecciona un candidato.
2. Pulsa `Aceptar`.
3. Se comprueba si el dojo tiene capacidad.
4. Si hay hueco, el candidato entra al roster del dojo.
5. El candidato deja de estar disponible en la lista de Admision.

Si no hay hueco:

- No se admite al candidato.
- Queda pendiente definir el feedback exacto.
- El jugador deberia poder liberar espacio desde la pantalla del edificio `Dojo`, despidiendo alumnos admitidos.

## Expulsar

El boton `Expulsar` quita al candidato de la lista de Admision.

Flujo:

1. El jugador selecciona un candidato.
2. Pulsa `Expulsar`.
3. El candidato desaparece de la lista diaria.
4. No entra al roster del dojo.

Expulsar no genera automaticamente un sustituto en la misma lista.

La lista completa se refresca el proximo dia.

## Relacion con UI

Admision puede tener su propio `UIScreenManager` interno.

Ejemplo:

```text
MainMenu UIScreenManager
    Abre pantalla Admision

Admision UIScreenManager
    Gestiona lista de candidatos, detalle, confirmaciones futuras
```

La pantalla principal de Admision puede formar parte de la seccion `MainMenu`, pero si crece en subpantallas se usara una seccion propia `Admision`.

## Reglas

- Admision genera candidatos, no campeones admitidos.
- El valor inicial es 5 candidatos por dia.
- Un dia equivale por ahora a cargar el menu principal.
- Aceptar mete al candidato en el roster si hay capacidad.
- Expulsar elimina al candidato de la lista diaria.
- El dojo tiene capacidad inicial de 10 campeones.
- El roster admitido debe persistir entre sesiones/escenas.
- La fama del dojo aumentara la cantidad de candidatos mas adelante, pero no ahora.
