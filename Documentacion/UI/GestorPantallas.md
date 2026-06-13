# Gestor de Pantallas

## Resumen

El unico sistema que puede abrir y cerrar pantallas es el gestor de UI.

Nombre propuesto:

```text
UIScreenManager
```

El resto de sistemas no abren ni cierran ventanas directamente. Solo solicitan al `UIScreenManager` que lo haga mediante senales.

Esto evita que edificios, botones, sistemas de gameplay o acciones de UI tengan referencias directas a pantallas concretas.

## UISectionId

Necesitamos identificar a que conjunto de pantallas pertenece una solicitud.

Nombre propuesto:

```text
UISectionId
```

`UISectionId` representa el ambito o seccion de UI que debe procesar la peticion.

Ejemplos:

```text
MainMenu
Admision
Hospital
Dojo
Arena
```

La idea es que pueda existir mas de un `UIScreenManager` en el proyecto, pero cada uno gestiona solo su seccion.

Ejemplo:

- El menu principal tiene un `UIScreenManager` para pantallas generales del Dojo.
- Admision puede tener su propio `UIScreenManager` mas pequeno para pantallas internas de reclutamiento.
- Hospital puede tener otro gestor para sus subpantallas.

## Responsabilidad del UIScreenManager

`UIScreenManager` se encarga de:

- Escuchar solicitudes de abrir/cerrar pantalla.
- Filtrar solicitudes por `UISectionId`.
- Saber cual es la pantalla actual de su seccion.
- Cerrar la pantalla actual antes de abrir otra si corresponde.
- Abrir la pantalla solicitada.
- Cerrar pantallas.
- Protegerse contra callbacks viejos usando un token de cancelacion.

No se encarga de:

- Detectar clicks de edificios.
- Ejecutar animaciones internas de una pantalla.
- Decidir como se mueve la camara.
- Contener logica concreta de admision, hospital, arena o dojo.

## Datos Internos

Datos conceptuales:

```csharp
public sealed class UIScreenManager : MonoBehaviour
{
    public UISectionId SectionId;
    public UIScreenId InitialScreenId;

    private UIScreenId currentScreenId;
    private int lastCancelToken;
}
```

`SectionId` define que conjunto gestiona.

`InitialScreenId` permite abrir una pantalla inicial al arrancar si hace falta.

`currentScreenId` guarda la pantalla abierta actualmente dentro de esa seccion.

`lastCancelToken` invalida callbacks antiguos cuando llega una orden nueva antes de que termine una transicion.

## Senal de Solicitud

El resto de sistemas pide abrir o cerrar pantallas con una senal.

Nombre propuesto:

```text
RequestUIScreenSignal
```

Datos:

```csharp
public readonly struct RequestUIScreenSignal
{
    public readonly UISectionId SectionId;
    public readonly UIScreenId ScreenId;
    public readonly bool Show;
}
```

Regla:

- Si `Show == true`, el gestor intenta abrir la pantalla.
- Si `Show == false`, el gestor intenta cerrar la pantalla.
- Si `SectionId` no coincide con el gestor, el gestor ignora la senal.

Ejemplos:

```text
RequestUIScreen(MainMenu, Admision, true)
RequestUIScreen(MainMenu, Hospital, true)
RequestUIScreen(Admision, CandidateList, true)
RequestUIScreen(Admision, CandidateDetails, true)
```

## Sin AIEvent

No queremos el flujo de `AIEvent`.

Ese sistema venia de otro proyecto para abrir pantallas por voz o eventos externos. En El Bestia, la apertura de pantallas debe pasar por senales explicitas de UI/gameplay.

Por tanto:

- No se documenta `AIEventSignal`.
- No se implementa handler de voz.
- No se mezcla el gestor de UI con sistemas externos que no pertenezcan a la navegacion actual.

## Apertura de Pantalla

Flujo de `ShowScreen(id)`:

1. Incrementa `lastCancelToken`.
2. Guarda el token local.
3. Si no hay pantalla actual, abre la pantalla solicitada.
4. Si hay pantalla actual distinta:
   - Pide cerrar la pantalla actual.
   - Al terminar el cierre, comprueba que el token sigue vigente.
   - Actualiza `currentScreenId`.
   - Pide abrir la nueva pantalla.
5. Si la pantalla actual ya es la solicitada:
   - Puede relanzar apertura o ignorar, segun decision final.

Concepto:

```csharp
private void ShowScreen(UIScreenId id)
{
    int token = ++lastCancelToken;

    if (currentScreenId != id && currentScreenId != UIScreenId.None)
    {
        CloseCurrent(() =>
        {
            if (token != lastCancelToken)
            {
                return;
            }

            currentScreenId = id;
            OpenScreen(id, token);
        }, token);

        return;
    }

    currentScreenId = id;
    OpenScreen(id, token);
}
```

## Cierre de Pantalla

Flujo de `HideScreen(id)`:

1. Incrementa `lastCancelToken`.
2. Pide cerrar la pantalla indicada.
3. Al terminar el cierre, comprueba que el token sigue vigente.
4. Si la pantalla cerrada era la actual, asigna `currentScreenId = None`.

Concepto:

```csharp
private void HideScreen(UIScreenId id)
{
    int token = ++lastCancelToken;

    CloseScreen(id, () =>
    {
        if (token != lastCancelToken)
        {
            return;
        }

        if (currentScreenId == id)
        {
            currentScreenId = UIScreenId.None;
        }
    }, token);
}
```

## Senal Interna de Toggle

Para comunicarse con pantallas concretas, el gestor puede emitir una senal interna:

```text
UIScreenToggleSignal
```

Datos:

```csharp
public readonly struct UIScreenToggleSignal
{
    public readonly UISectionId SectionId;
    public readonly UIScreenId ScreenId;
    public readonly bool Show;
    public readonly Action OnComplete;
    public readonly int Token;
}
```

`UIScreen` escucha esta senal y solo responde si:

- Coincide su `SectionId`.
- Coincide su `ScreenId`.

Cuando termina su `OpenBehaviour` o `CloseBehaviour`, llama a `OnComplete`.

## Cerrar Todas las Pantallas

Puede existir una senal:

```text
UICloseAllScreensSignal
```

Datos:

```csharp
public readonly struct UICloseAllScreensSignal
{
    public readonly UISectionId SectionId;
}
```

El gestor solo cierra todas las pantallas de su seccion.

Uso:

- Cerrar todas las pantallas del menu principal.
- Salir de Admision y limpiar subpantallas internas.
- Cerrar modales antes de cambiar de contexto.

## Relacion con Menu y Edificios

Los edificios no abren ventanas directamente.

Flujo esperado:

1. El usuario hace click en un edificio.
2. El edificio ejecuta su feedback.
3. El edificio lanza una senal `RequestUIScreenSignal`.
4. El `UIScreenManager` de `MainMenu` recibe la senal.
5. El gestor abre/cierra la pantalla correspondiente.
6. La pantalla puede usar `UICameraMoveAction` para mover la camara hacia el edificio.

Ejemplo:

```text
AdmisionBuilding.OnClick
    RequestUIScreen(MainMenu, Admision, true)

UIScreenManager(MainMenu)
    Cierra pantalla actual
    Abre pantalla Admision

Pantalla Admision
    OpenBehaviour
        UICameraMoveAction(Admision)
        UIMoveAction(Panel, Hidden -> Visible)
```

## Gestores Anidados por Seccion

Puede haber un gestor principal y gestores secundarios.

Ejemplo:

```text
MainMenu UIScreenManager
    Gestiona: Arena, Hospital, Dojo, Admision

Admision UIScreenManager
    Gestiona: CandidateList, CandidateDetails, RecruitConfirm
```

El gestor principal abre la seccion Admision.

Dentro de Admision, otro gestor se encarga de las subpantallas de reclutamiento.

La separacion evita que el gestor principal conozca todos los detalles internos de cada edificio.

## Reglas

- Solo `UIScreenManager` puede abrir y cerrar pantallas.
- Otros sistemas solo solicitan cambios mediante senales.
- Cada gestor filtra por `UISectionId`.
- Puede haber varios gestores, uno por conjunto de UI.
- `AIEventSignal` queda fuera del sistema.
- `lastCancelToken` evita que callbacks viejos reabran o cierren pantallas cuando ya llego otra orden.
- `UIScreenToggleSignal` es la orden concreta que reciben las pantallas.
