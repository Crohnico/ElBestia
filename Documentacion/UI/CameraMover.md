# CameraMover

## Resumen

`CameraMover` es el sistema encargado de mover la camara entre posiciones predefinidas.

La idea es aprovechar que el menu usa edificios 3D: al abrir una pantalla, la camara puede moverse hacia la zona relacionada con esa pantalla para que la navegacion sea mas diegetica.

Ejemplos:

- Abrir Arena mueve la camara hacia el edificio de Arena.
- Abrir Hospital mueve la camara hacia el Hospital.
- Abrir Admision mueve la camara hacia Admision.
- Cerrar una pantalla puede devolver la camara al plano general del Dojo.

## Snapshot de Camara

Las posiciones de camara se definen como snapshots.

Cada snapshot representa:

- Un valor de enum.
- Una posicion.
- Una rotacion.

Direccion inicial:

- Usar una lista serializable en inspector.
- Cada entrada contiene `enum + transform`.
- En `Awake`, convertir esa lista en un diccionario `enum -> transform` para acceso rapido.

Concepto:

```csharp
public enum CameraSnapshotId
{
    MainMenu,
    Arena,
    Hospital,
    Dojo,
    Admision
}

[Serializable]
public sealed class CameraSnapshot
{
    public CameraSnapshotId Id;
    public Transform Transform;
}
```

## Datos de CameraMover

`CameraMover` contiene:

- Referencia a la camara que va a mover.
- Referencia a un pivote de camara.
- Lista de snapshots configurada desde inspector.
- Diccionario interno para buscar snapshots por enum.
- Duracion del movimiento.
- Ease de DOTween.

Concepto:

```csharp
public sealed class CameraMover : MonoBehaviour
{
    public Camera Camera;
    public Transform Pivot;
    public CameraSnapshot[] Snapshots;

    private Dictionary<CameraSnapshotId, Transform> snapshotsById;
}
```

## Awake

En `Awake`, `CameraMover` transforma la lista en diccionario.

Motivo:

- Inspector trabaja bien con listas.
- Runtime trabaja mejor con diccionarios.
- Al recibir una senal, no queremos buscar linealmente cada vez.

Concepto:

```csharp
private void Awake()
{
    snapshotsById = new Dictionary<CameraSnapshotId, Transform>();

    for (int i = 0; i < Snapshots.Length; i++)
    {
        snapshotsById[Snapshots[i].Id] = Snapshots[i].Transform;
    }
}
```

## Senal CameraMove

`CameraMover` se suscribe a una senal:

```csharp
CameraMove(CameraSnapshotId target)
```

Cuando llega la senal:

1. Busca el snapshot por enum.
2. Si existe, mueve la camara hacia la posicion/rotacion del transform.
3. Al terminar, lanza una senal de completado del movimiento.
4. Si no existe, no hace nada o deja un warning de debug.

Concepto:

```csharp
SignalBus.Subscribe<CameraMoveSignal>(OnCameraMove);
```

La senal podria ser:

```csharp
public readonly struct CameraMoveSignal
{
    public readonly CameraSnapshotId Target;
}
```

Senal de completado:

```csharp
public readonly struct CameraMoveCompletedSignal
{
    public readonly CameraSnapshotId Target;
}
```

## Movimiento Inicial

Por ahora el movimiento puede ser simple con DOTween.

Direccion inicial:

- Si la camara ya esta en el destino solicitado, no se mueve.
- Si la camara ya se esta moviendo hacia el destino solicitado, no reinicia el movimiento.
- Si la camara debe ir a otro destino, usa el pivote como punto de control de una Bezier cuadratica.
- Al llegar al destino, emite `CameraMoveCompletedSignal`.
- La posicion usa una unica curva Bezier `P0 = camara actual`, `P1 = pivote`, `P2 = destino`, para que no haya frenado ni rearranque en el pivote.
- La rotacion se interpola de forma continua hasta la rotacion del destino.
- Ease configurable.

Mas adelante se podra cambiar por:

- Path curvo.
- Waypoints.
- Cinemachine.
- Transiciones con zoom.
- Movimiento distinto por tipo de pantalla.

La primera version debe ser una transicion clara, no un sistema cinematografico complejo.

## Pivote

El pivote es una posicion intermedia comun para las transiciones de camara del menu.

Uso:

```text
CameraMove(Arena)
    P0 camara actual, P1 pivote, P2 Arena
    en una unica transicion continua

CameraMove(Hospital)
    P0 camara actual, P1 pivote, P2 Hospital
    en una unica transicion continua
```

Reglas:

- El pivote se configura como `Transform` en inspector.
- El pivote no es una pantalla ni un destino de UI.
- El pivote actua como punto de control de una Bezier cuadratica, no como punto por el que la camara tenga que pasar exactamente.
- El pivote evita que la camara viaje directamente de un edificio a otro con trayectorias raras.
- El pivote no debe cortar la transicion en dos tweens, porque eso provoca frenado al llegar y aceleracion al salir.
- Si se solicita el mismo destino mientras la camara ya esta alli o ya va hacia alli, la solicitud se ignora.

## Uso con Pantallas

Una pantalla puede pedir movimiento de camara al abrirse mediante una `UIAction`.

Ejemplo:

```text
OpenBehaviour
    UICameraMoveAction(Arena)
    UIMoveAction(WindowPanel, Hidden -> Visible)
```

Esto permite que abrir una ventana no sea solo aparecer un panel, sino tambien mover la camara hacia el edificio correspondiente.

## Reglas

- `CameraMover` escucha senales, no depende de edificios concretos.
- `CameraMover` pasa por el pivote antes de llegar a un nuevo destino.
- Los snapshots se configuran como lista en inspector.
- En `Awake`, la lista se convierte en diccionario.
- Las pantallas o acciones lanzan `CameraMove(enum)`.
- `CameraMover` decide como mover la camara.
- La primera implementacion puede usar DOTween directo.
