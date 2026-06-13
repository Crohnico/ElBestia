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
- Lista de snapshots configurada desde inspector.
- Diccionario interno para buscar snapshots por enum.
- Duracion del movimiento.
- Ease de DOTween.

Concepto:

```csharp
public sealed class CameraMover : MonoBehaviour
{
    public Camera Camera;
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
3. Si no existe, no hace nada o deja un warning de debug.

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

## Movimiento Inicial

Por ahora el movimiento puede ser simple con DOTween.

Direccion inicial:

- Tween de posicion de A a B.
- Tween de rotacion de A a B.
- Misma duracion para ambos.
- Ease configurable.

Mas adelante se podra cambiar por:

- Path curvo.
- Waypoints.
- Cinemachine.
- Transiciones con zoom.
- Movimiento distinto por tipo de pantalla.

La primera version debe ser una transicion clara, no un sistema cinematografico complejo.

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
- Los snapshots se configuran como lista en inspector.
- En `Awake`, la lista se convierte en diccionario.
- Las pantallas o acciones lanzan `CameraMove(enum)`.
- `CameraMover` decide como mover la camara.
- La primera implementacion puede usar DOTween directo.
