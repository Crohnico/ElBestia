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

### Lista Scrollable

La lista de nombres se montara como un `ScrollView` vertical.

Estructura inicial en escena:

```text
SelectionTable
    Canvas
        Background
            ScrollView
                Viewport
                    GridLayout
                        Botones de candidatos
```

Reglas:

- El `ScrollView` ocupa el mismo sitio y tamano que tenia el `GridLayout`.
- El `GridLayout` es el `Content` del `ScrollRect`.
- El scroll horizontal esta desactivado.
- El scroll vertical esta activado.
- Los botones de candidatos se popularan dentro del `GridLayout`.
- El `GridLayout` queda anclado arriba para que los candidatos se rellenen de arriba abajo.
- Al abrir la lista o repoblarla, el scroll debe volver arriba para mostrar primero el candidato `0`.
- Si hay mas candidatos de los que caben en pantalla, el jugador hace scroll hacia abajo para ver los nuevos.
- Al abrirse, la lista selecciona el primer candidato visible.
- Solo puede haber un candidato seleccionado a la vez.
- El boton seleccionado usa el color `#727780`.
- Los botones no seleccionados usan el color `#41444A`.

### Componentes Iniciales

La primera version runtime de Admision usa varios componentes:

- `ChampionAdmisionList`: singleton de candidatos del dia.
- `AdmisionScrollView`: pinta la lista visual dentro del `ScrollView`.
- `AdmisionButton`: boton de candidato con referencia a su texto y a su `ChampionData`.
- `ChampionShowcase`: escaparate visual del campeon seleccionado.

`ChampionAdmisionList`:

- Tiene un `payload` publico.
- Tiene una lista publica de `ChampionData`.
- En `Start` llama a `CreateList`.
- `CreateList` limpia primero la lista y despues genera `payload` campeones nuevos.

`AdmisionScrollView`:

- Tiene una referencia al boton plantilla `AdmisionButton`.
- Tiene una referencia al `content` del `ScrollView`.
- Crea un boton por cada campeon existente en `ChampionAdmisionList`.
- Asigna al texto del boton el nombre guardado en el `ChampionData`.
- Gestiona que solo haya un boton seleccionado.
- Actualiza los colores selected/unselected de los botones.
- Avisa al `ChampionShowcase` cuando cambia el candidato seleccionado.
- Resetea el scroll arriba al repoblar.

`AdmisionButton`:

- Guarda la referencia al `TMP_Text` del boton.
- Guarda el `ChampionData` que representa.
- Expone un metodo `Bind(ChampionData)` para recibir el campeon y actualizar el texto.

`ChampionShowcase`:

- Tiene una referencia al `BaseStickman` de Admision.
- Al recibir un `ChampionData`, carga su aspecto visual en el stickman.
- Usa el sistema visual existente de stickman para aplicar `ChampionData.Appearance`.
- Antes de configurar el aspecto, el root animado del stickman debe estar a escala `1,1,1` para que las mallas procedurales se reconstruyan bien.
- En la primera carga, despues de configurar el primer campeon, el root animado puede volver a `0,0,0` para que las `UIAction` de apertura hagan el pop desde cero.

`ChampionShowcaseDragRotator`:

- Vive en el `StickmanParent` o en el objeto que tenga el `CapsuleCollider` de interaccion.
- Al hacer click izquierdo sobre la capsula empieza a rotar el showcase.
- Mientras el click izquierdo siga pulsado, usa el delta horizontal del raton para rotar de forma continua.
- Al soltar el click izquierdo deja de rotar.

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
