# Cursor Interactivo del Menu

## Resumen

El menu principal necesita un script encargado de convertir el raton en un cursor que interactua con la escena.

Este sistema no pertenece a ningun edificio concreto. Su responsabilidad es detectar que hay bajo el puntero y llamar al contrato comun de interaccion.

Los edificios, por su parte, implementan `IInteractable` y deciden como responder al hover o al click.

## Responsabilidad del Script

Crear un script de menu, por ejemplo `MenuCursorInteractor`, que centralice la entrada del jugador en la escena `Dojo`.

Responsabilidades:

- Leer la posicion del raton.
- Leer clicks de raton.
- Soportar en el futuro toque de pantalla si el juego sale en Android.
- Lanzar raycast desde la camara principal.
- Detectar si el objeto alcanzado implementa `IInteractable`.
- Llamar a `OnHover` y `OnClick` cuando corresponda.

El script no debe saber si ha tocado Arena, Hospital, Dojo o Admision. Solo conoce `IInteractable`.

## Contrato IInteractable

La interfaz comun de edificios interactuables se llamara `IInteractable`.

Concepto:

```csharp
public interface IInteractable
{
    void OnHover(bool isHovering);
    void OnClick();
}
```

Los edificios del menu implementan esta interfaz.

El campo de entrenamiento no la implementa.

## Click

El click es el caso sencillo.

Flujo:

1. El usuario hace click con el raton.
2. Si en el futuro hay version Android, el usuario toca la pantalla.
3. Si existe `_currentSelected`, se llama a `_currentSelected.OnClick()`.

El click no necesita repetir raycast. Usa el interactuable que ya ha calculado el hover en `Update`.

Concepto:

```csharp
if (_currentSelected != null)
{
    _currentSelected.OnClick();
}
```

El `OnClick` del edificio se encargara despues de reproducir su feedback DOTween y lanzar la senal `OpenWindow(X)`.

## Hover

El hover se resuelve desde el mismo script del cursor.

Un raycast por frame en el menu no deberia ser un problema real. La escena tiene camara estatica y pocas interacciones, asi que la solucion inicial puede ser directa y facil de leer.

Direccion:

- En `Update`, lanzar un raycast desde la posicion actual del puntero.
- Si el raycast choca con algo que tiene `IInteractable`, guardar ese interactuable en `_currentSelected`.
- Si no choca con nada interactuable, guardar `_currentSelected = null`.
- Mantener aparte un `cachedSelected`.
- Cuando `_currentSelected` cambia respecto a `cachedSelected`, apagar el hover anterior y encender el nuevo.

Flujo:

1. `Update` calcula `_currentSelected` con un raycast.
2. Si `_currentSelected == cachedSelected`, no se cambia nada.
3. Si `_currentSelected != cachedSelected`, se ejecuta el cambio de seleccion.
4. Si `cachedSelected != null`, se llama `cachedSelected.OnHover(false)` antes de sustituirlo.
5. Se asigna `cachedSelected = _currentSelected`.
6. Si `cachedSelected != null`, se llama `cachedSelected.OnHover(true)`.

Concepto:

```csharp
private IInteractable _currentSelected;
private IInteractable cachedSelected;

private void Update()
{
    _currentSelected = RaycastInteractableUnderPointer();
    RefreshHoverSelection();
}

private void RefreshHoverSelection()
{
    if (_currentSelected == cachedSelected)
    {
        return;
    }

    if (cachedSelected != null)
    {
        cachedSelected.OnHover(false);
    }

    cachedSelected = _currentSelected;

    if (cachedSelected != null)
    {
        cachedSelected.OnHover(true);
    }
}
```

La idea es simple: el raycast puede ocurrir todo el rato, pero el feedback de hover solo cambia cuando cambia el interactuable seleccionado.

## Raycast Sin Layer Dedicada

No vamos a poner una layer especifica al raycast del menu.

Motivo:

- Queremos que el cursor interactue con todo lo que este delante, incluida UI u otros bloqueadores.
- Si hay una pantalla abierta, esa pantalla debe impedir que el usuario haga click en edificios detras.
- La propia escena/UI define que recibe el raycast primero.

Consecuencia:

- El raycast no debe filtrar solo edificios.
- El sistema debe intentar obtener `IInteractable` del objeto alcanzado.
- Si el objeto alcanzado no tiene `IInteractable`, no ocurre nada.

Esto permite que un panel de UI, un modal o cualquier bloqueador pueda interceptar el puntero sin que los edificios respondan por detras.

## Raton y Touch

El primer objetivo es raton.

El diseno debe dejar hueco para touch:

- Click de raton y toque de pantalla pueden alimentar el mismo flujo de `pointerPosition`.
- El raycast no necesita saber si viene de raton o de dedo.
- `IInteractable.OnClick` debe ser independiente del origen de input.

Android queda como posibilidad futura, no como requisito del primer prototipo.

## Reglas

- El script del cursor detecta interacciones.
- Los edificios responden a interacciones.
- Los edificios no consultan input directamente.
- El cursor no conoce tipos concretos de edificio.
- El click solo llama `OnClick` si existe `_currentSelected`.
- El hover debe evitar llamadas repetidas innecesarias al mismo objeto.
