# Gestor de Pantallas

## Resumen

El unico sistema que puede abrir y cerrar pantallas es el gestor de UI.

Nombre propuesto:

```text
UIScreenManager
```

El resto de sistemas no abren ni cierran ventanas directamente. Solo solicitan al `UIScreenManager` que lo haga mediante senales.

Esto evita que edificios, botones, sistemas de gameplay o acciones de UI tengan referencias directas a pantallas concretas.

## Responsabilidad del UIScreenManager

`UIScreenManager` se encarga de:

- Escuchar solicitudes de abrir/cerrar pantalla.
- Saber cual es la pantalla actual.
- Volver a la pantalla inicial cuando el usuario pulsa `Esc`.
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
    public bool OpenInitialScreen;
    public UIScreenId InitialScreenId;

    private UIScreenId currentScreenId;
    private bool hasCurrentScreen;
    private int lastCancelToken;
}
```

`OpenInitialScreen` decide si se abre una pantalla al arrancar.

`InitialScreenId` permite elegir esa pantalla inicial cuando `OpenInitialScreen` esta activo.

`currentScreenId` guarda la pantalla abierta actualmente.

`hasCurrentScreen` indica si hay una pantalla abierta, sin necesitar un valor `None` en el enum de pantallas.

`lastCancelToken` invalida callbacks antiguos cuando llega una orden nueva antes de que termine una transicion.

## Volver a la Pantalla Inicial

Si el usuario pulsa `Esc`, el gestor intenta abrir `InitialScreenId`.

Reglas:

- Si `InitialScreenId` ya es la pantalla actual, no se relanza la apertura.
- `Esc` debe poder cancelar una navegacion en curso y volver a la pantalla inicial.
- La lectura de `Esc` usa el Input System de Unity, no `UnityEngine.Input`.

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
    public readonly UIScreenId ScreenId;
    public readonly bool Show;
}
```

Regla:

- Si `Show == true`, el gestor intenta abrir la pantalla.
- Si `Show == false`, el gestor intenta cerrar la pantalla.

Ejemplos:

```text
RequestUIScreen(Admision, true)
RequestUIScreen(Hospital, true)
```

IDs iniciales de pantalla:

```text
MainMenu
Arena
Dojo
Hospital
Admision
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

    if (hasCurrentScreen && currentScreenId != id)
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

    if (hasCurrentScreen && currentScreenId == id)
        {
            hasCurrentScreen = false;
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
    public readonly UIScreenId ScreenId;
    public readonly bool Show;
    public readonly Action OnComplete;
    public readonly int Token;
}
```

`UIScreen` escucha esta senal y solo responde si:

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
}
```

El gestor cierra su pantalla actual.

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
Building.OnClick
    RequestUIScreen(Admision, true)

UIScreenManager(MainMenu)
    Cierra pantalla actual
    Abre pantalla Admision

Pantalla Admision
    OpenBehaviour
        UICameraMoveAction(Admision)
        UIMoveAction(Panel, Hidden -> Visible)
```

## Reglas

- Solo `UIScreenManager` puede abrir y cerrar pantallas.
- Otros sistemas solo solicitan cambios mediante senales.
- `AIEventSignal` queda fuera del sistema.
- `lastCancelToken` evita que callbacks viejos reabran o cierren pantallas cuando ya llego otra orden.
- `UIScreenToggleSignal` es la orden concreta que reciben las pantallas.
