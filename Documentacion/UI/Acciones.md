# Acciones de UI

## Resumen

Las acciones de UI usan un patron command.

Una `UIAction` representa una operacion concreta sobre la interfaz: mover, escalar, hacer fade, encadenar otras acciones o cualquier comportamiento reusable que una pantalla pueda ejecutar desde inspector.

## Contrato UIAction

Contrato base:

```csharp
public interface UIAction
{
    bool Execute();
    void InstantExecute();
}
```

`Execute` devuelve si la accion ha terminado.

Regla base:

```csharp
bool Execute()
{
    return true;
}
```

El `return true` por defecto evita que una accion vacia o simple atasque el behaviour.

Cada accion concreta decide cuando considera que ha terminado:

- Una accion instantanea puede devolver `true` en el mismo frame.
- Una accion de desplazamiento devuelve `false` mientras no llega al destino.
- Una accion de escala devuelve `false` hasta completar la animacion.
- Una accion compuesta devuelve `true` cuando terminan sus acciones hijas.

## Acciones Encadenadas

En Unity podemos hacer que cada `UIAction` tenga su propio array de `UIAction`.

Objetivo:

- Permitir encadenar acciones desde inspector.
- Evitar tener que escribir un behaviour nuevo para cada secuencia.
- Poder crear acciones compuestas que ejecuten otras al completarse.

Ejemplo:

```text
Mover panel de A a B
Al completar:
    Escalar panel de 0 a 1
Al completar:
    Marcar accion completa
```

Estructura conceptual:

```csharp
public abstract class UIActionBase : MonoBehaviour, UIAction
{
    public UIActionBase[] OnCompleteActions;

    public abstract bool Execute();
    public abstract void InstantExecute();
}
```

Regla:

- Una accion puede tener acciones hijas.
- Las acciones hijas se ejecutan cuando la accion principal termina.
- La accion principal no devuelve `true` hasta que sus hijas hayan terminado tambien.

## Flujo de Accion Compuesta

Una accion compuesta tiene dos fases:

1. Ejecutar su efecto propio.
2. Ejecutar sus acciones hijas.

Concepto:

```csharp
public bool Execute()
{
    if (!ownActionCompleted)
    {
        ownActionCompleted = ExecuteOwnAction();
        return false;
    }

    return ExecuteChildActions();
}
```

`ExecuteChildActions` ejecuta el array de acciones hijas igual que un behaviour:

- Llama a `Execute` en cada hija.
- Guarda cuales han terminado.
- Devuelve `true` solo cuando todas han terminado.

## InstantExecute

`InstantExecute` debe dejar la accion y sus hijas en estado final.

Regla:

- Primero fuerza el estado final de la accion principal.
- Despues llama `InstantExecute` en todas las acciones hijas.

Esto permite saltar animaciones sin dejar una secuencia a medias.

## Accion DoTween Scale From To

Accion para escalar un elemento usando DOTween.

Nombre propuesto:

```text
UIScaleAction
```

Responsabilidad:

- Escalar un `Transform` desde una escala inicial hasta una escala final.
- Devolver `false` mientras la animacion esta en curso.
- Devolver `true` cuando DOTween completa la animacion y sus acciones hijas han terminado.

Datos:

- `target`: transform que se escala.
- `fromScale`: escala inicial.
- `toScale`: escala final.
- `duration`: duracion.
- `ease`: curva/ease de DOTween.
- `OnCompleteActions`: acciones hijas opcionales.

Comportamiento:

1. Al empezar, asigna `target.localScale = fromScale`.
2. Lanza un tween hacia `toScale`.
3. Mientras el tween no termina, devuelve `false`.
4. Cuando termina, ejecuta acciones hijas si existen.
5. Cuando todo termina, devuelve `true`.

`InstantExecute`:

- Mata o ignora el tween activo.
- Asigna `target.localScale = toScale`.
- Ejecuta `InstantExecute` en acciones hijas.

## Accion DoTween Move From To

Accion para mover un elemento usando DOTween.

Nombre propuesto:

```text
UIMoveAction
```

Responsabilidad:

- Mover un `Transform` desde una posicion inicial hasta una posicion final.
- Permitir usar referencias a transforms como puntos de origen/destino.
- Permitir usar `Vector3` directamente si no hay referencias.
- Devolver `false` mientras la animacion esta en curso.
- Devolver `true` cuando DOTween completa la animacion y sus acciones hijas han terminado.

Datos:

- `target`: transform que se mueve.
- `fromTransform`: referencia opcional de origen.
- `toTransform`: referencia opcional de destino.
- `fromPosition`: fallback `Vector3` si no hay `fromTransform`.
- `toPosition`: fallback `Vector3` si no hay `toTransform`.
- `useLocalPosition`: si mueve en local o world space.
- `duration`: duracion.
- `ease`: curva/ease de DOTween.
- `OnCompleteActions`: acciones hijas opcionales.

Regla de posicion:

- Si existe `fromTransform`, se usa su posicion.
- Si no existe `fromTransform`, se usa `fromPosition`.
- Si existe `toTransform`, se usa su posicion.
- Si no existe `toTransform`, se usa `toPosition`.

Comportamiento:

1. Al empezar, coloca `target` en la posicion inicial.
2. Lanza un tween hacia la posicion final.
3. Mientras el tween no termina, devuelve `false`.
4. Cuando termina, ejecuta acciones hijas si existen.
5. Cuando todo termina, devuelve `true`.

`InstantExecute`:

- Mata o ignora el tween activo.
- Coloca `target` directamente en la posicion final.
- Ejecuta `InstantExecute` en acciones hijas.

## Accion Camera Move To

Accion para pedir un movimiento de camara a un snapshot concreto.

Nombre propuesto:

```text
UICameraMoveAction
```

Responsabilidad:

- Lanzar una senal `CameraMove(enum)`.
- Pedir al `CameraMover` que mueva la camara hacia un snapshot.
- Permitir que una pantalla acompane su apertura/cierre con movimiento diegetico de camara.

Datos:

- `targetSnapshot`: enum del snapshot de camara.
- `waitForCompletion`: si la accion debe esperar a que termine el movimiento o completarse tras lanzar la senal.
- `OnCompleteActions`: acciones hijas opcionales.

Direccion inicial:

- Para el primer prototipo, puede lanzar la senal y devolver `true`.
- Si necesitamos encadenar animaciones despues de que termine la camara, se anadira confirmacion de completado desde `CameraMover`.

Concepto:

```csharp
public bool Execute()
{
    SignalBus.Fire(new CameraMoveSignal(targetSnapshot));
    return ExecuteChildActions();
}
```

Detalle tecnico del sistema que recibe la senal: [CameraMover.md](./CameraMover.md).

## Ejemplo de Secuencia

Ejemplo: abrir una ventana que entra desde abajo y luego hace pop.

```text
UIMoveAction
    target: WindowPanel
    fromTransform: HiddenBottomPoint
    toTransform: CenterPoint
    duration: 0.25
    OnCompleteActions:
        UIScaleAction
            target: WindowPanel
            fromScale: 0
            toScale: 1
            duration: 0.15
```

La `UIMoveAction` solo se considera completada cuando:

1. Termina su movimiento.
2. Termina su `UIScaleAction` hija.

Ejemplo: abrir la pantalla de Arena moviendo primero la camara.

```text
UICameraMoveAction
    targetSnapshot: Arena
    OnCompleteActions:
        UIMoveAction
            target: WindowPanel
            fromTransform: HiddenBottomPoint
            toTransform: CenterPoint
            duration: 0.25
```

## Reglas

- `UIAction` es el contrato comun.
- Las acciones pueden tener acciones hijas.
- Una accion con hijas no termina hasta que sus hijas terminan.
- `InstantExecute` fuerza el estado final de la accion y de sus hijas.
- `UIScaleAction` usa DOTween para escalar de `fromScale` a `toScale`.
- `UIMoveAction` usa DOTween para mover de origen a destino.
- `UIMoveAction` puede usar transforms o `Vector3` como fallback.
- `UICameraMoveAction` lanza una senal `CameraMove(enum)` para que `CameraMover` mueva la camara.
