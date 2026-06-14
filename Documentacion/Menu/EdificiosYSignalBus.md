# Edificios y SignalBus

## Enfoque Tecnico

Los edificios del menu principal deben ser autonomos.

Cada edificio sabe como responder al hover y al click, pero no debe conocer directamente que ventana concreta existe ni como se abre. Su responsabilidad termina al lanzar una senal de navegacion.

La apertura real de ventanas pertenece a otro sistema, que escucha senales y decide que UI mostrar.

## SignalBus

El proyecto necesita un `SignalBus` general, independiente de los edificios.

Objetivo:

- Permitir comunicacion desacoplada entre sistemas.
- Evitar que un edificio tenga referencia directa a ventanas, managers o canvases.
- Permitir lanzar senales aunque no haya ningun listener registrado.

API base:

```csharp
SignalBus.Subscribe<TSignal>(Action<TSignal> callback);
SignalBus.Fire<TSignal>(TSignal signal);
```

Regla importante:

- `SignalBus.Fire` no debe quejarse ni lanzar error si no hay nadie suscrito a esa senal.

Esto permite que los edificios funcionen desde el primer prototipo aunque todavia no exista la UI final conectada.

## Senal de Apertura de Ventana

El click de un edificio lanza una senal de apertura:

```csharp
OpenWindow(X)
```

`X` representa la ventana o seccion que se quiere abrir.

Ejemplos:

```text
OpenWindow(Arena)
OpenWindow(Hospital)
OpenWindow(Dojo)
OpenWindow(Admision)
```

Pendiente de implementacion:

- Definir si `X` sera un enum, un id, un ScriptableObject o una clase de senal tipada.

Direccion inicial recomendada:

- Usar un enum simple para el primer prototipo.
- Cambiar a ids o definiciones mas ricas solo cuando la UI lo necesite.

## Contrato IInteractable

Crear una interfaz comun para edificios interactuables.

Concepto:

```csharp
public interface IInteractable
{
    void OnHover(bool isHovering);
    void OnClick();
}
```

Responsabilidades:

- `OnHover`: activar o desactivar feedback de seleccion.
- `OnClick`: reproducir feedback de click y lanzar la senal correspondiente.

Los edificios del menu implementan `IInteractable`. El sistema de cursor del menu solo debe conocer esta interfaz, no los tipos concretos de edificio.

## Implementacion de Edificio

Los edificios del menu usan una implementacion comun:

```text
Building
```

Cada instancia configura en inspector su destino mediante un enum.

La idea base:

- Reciben hover.
- Reciben click.
- Reproducen animacion DOTween de confirmacion.
- Ejecutan acciones configurables de click si se han asignado.
- Lanzan `OpenWindow(X)` con su destino configurado.

### Destinos

El destino se configura con un enum equivalente a:

```text
Arena
Hospital
Dojo
Admision
```

Ejemplos:

```text
OpenWindow(Arena)
OpenWindow(Hospital)
OpenWindow(Dojo)
OpenWindow(Admision)
```

Esto evita tener cuatro scripts identicos para cuatro edificios que solo cambian el destino.

## Feedback Tecnico de Edificio

Cada edificio interactuable debe tener:

- Una forma de recibir hover/click desde raycast, collider o sistema equivalente.
- Referencia al transform que se anima con DOTween al hacer click.
- Destino de ventana asociado.
- Acciones opcionales para anadir efectos al click desde inspector.

La animacion de click debe restaurar siempre la escala original, incluso si el jugador hace click varias veces.

El campo de entrenamiento no implementa `IInteractable` y no lanza senales.
