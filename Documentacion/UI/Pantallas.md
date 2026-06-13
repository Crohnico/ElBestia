# Pantallas de UI

## Resumen

La UI del juego se organiza alrededor de pantallas.

Cada pantalla tiene un gestor principal llamado `UIScreen` y tres behaviours:

- `OpenBehaviour`.
- `BaseBehaviour`.
- `CloseBehaviour`.

El objetivo es que abrir, mantener y cerrar una pantalla sean fases separadas, componibles desde inspector y faciles de reutilizar.

## UIScreen

`UIScreen` es el gestor de una pantalla concreta.

Responsabilidades:

- Conocer sus tres behaviours.
- Ejecutar `OpenBehaviour` cuando la pantalla se abre.
- Mantener o activar `BaseBehaviour` cuando la pantalla ya esta abierta.
- Ejecutar `CloseBehaviour` cuando la pantalla se cierra.
- Coordinar callbacks de finalizacion entre fases.

Estructura conceptual:

```csharp
public sealed class UIScreen : MonoBehaviour
{
    public UIScreenBehaviour OpenBehaviour;
    public UIScreenBehaviour BaseBehaviour;
    public UIScreenBehaviour CloseBehaviour;
}
```

## UIScreenBehaviour

`UIScreenBehaviour` es el contrato base para comportamientos de pantalla.

Cada behaviour representa una fase o conjunto de acciones de UI.

Datos principales:

- `isActive`: bool publico que indica si el behaviour esta ejecutandose.
- `onComplete`: callback al que se pueden anadir acciones cuando el behaviour termina.
- `uiActions`: array publico de acciones, editable desde inspector.

Flujo base:

```csharp
public class UIScreenBehaviour : MonoBehaviour
{
    public bool IsActive;
    public Action OnComplete;
    public UIAction[] UiActions;

    private bool[] completedActions;

    private void Update()
    {
        if (IsActive)
        {
            Tick();
        }
    }
}
```

Regla:

- `Update` no hace trabajo si `isActive` es falso.
- Si `isActive` es verdadero, llama a `Tick`.

## Ejecucion del Behaviour

Cuando un behaviour se ejecuta:

1. Recibe o prepara su `onComplete`.
2. Inicializa el array de acciones completadas.
3. Marca `isActive = true`.
4. En cada `Tick`, ejecuta sus `UIAction`.
5. Cada accion devuelve si ya ha terminado.
6. Si todas las acciones han terminado, el behaviour llama a `onComplete`.
7. Despues se desactiva.

Concepto:

```csharp
public void Execute(Action onComplete)
{
    this.onComplete = onComplete;
    completedActions = new bool[uiActions.Length];
    isActive = true;
}

public void Tick()
{
    bool allCompleted = true;

    for (int i = 0; i < uiActions.Length; i++)
    {
        completedActions[i] = uiActions[i].Execute();
        if (!completedActions[i])
        {
            allCompleted = false;
        }
    }

    if (allCompleted && uiActions.Length > 0)
    {
        onComplete?.Invoke();
        Kill();
    }
}
```

## Kill

`Kill` detiene el behaviour y limpia su estado de ejecucion.

Concepto:

```csharp
public void Kill()
{
    isActive = false;
    completedActions = new bool[uiActions.Length];
    onComplete = null;
}
```

Uso:

- Al completar todas las acciones.
- Al cerrar o interrumpir una pantalla.
- Al cambiar de estado de forma forzada.

## InstantExecution

Cada behaviour debe poder ejecutar sus acciones de forma instantanea.

Esto sirve para saltar animaciones o dejar una pantalla en estado final sin esperar transiciones.

Concepto:

```csharp
public void InstantExecution()
{
    for (int i = 0; i < uiActions.Length; i++)
    {
        uiActions[i].InstantExecute();
    }

    completedActions = new bool[uiActions.Length];
    for (int i = 0; i < completedActions.Length; i++)
    {
        completedActions[i] = true;
    }
}
```

## UIAction

Las acciones de UI usan un patron command.

Cada accion sabe como ejecutarse y cuando se considera completada.

Detalle tecnico: [Acciones.md](./Acciones.md).

Contrato:

```csharp
public interface UIAction
{
    bool Execute();
    void InstantExecute();
}
```

## Execute

`Execute` devuelve un bool.

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
- Una accion de fade devuelve `false` mientras no termina la transicion.
- Una accion de escala devuelve `false` hasta completar la animacion.

Cuando devuelve `true`, el behaviour marca esa accion como completada.

## InstantExecute

`InstantExecute` fuerza la accion a su estado final.

Ejemplos:

- Un desplazamiento coloca el elemento directamente en la posicion destino.
- Un fade asigna directamente la opacidad final.
- Una escala asigna directamente la escala final.

Esto permite que una pantalla pueda saltarse animaciones sin quedar en estado intermedio.

## Relacion Entre Pantalla y Behaviour

Flujo recomendado de pantalla:

1. `UIScreen.Open()` ejecuta `OpenBehaviour`.
2. Cuando `OpenBehaviour` termina, se activa o ejecuta `BaseBehaviour`.
3. `BaseBehaviour` representa el estado normal de la pantalla abierta.
4. `UIScreen.Close()` detiene o ignora `BaseBehaviour` segun haga falta.
5. `UIScreen.Close()` ejecuta `CloseBehaviour`.
6. Cuando `CloseBehaviour` termina, la pantalla queda cerrada.

## Inspector

Los behaviours deben exponer su array de acciones para configurarlo desde inspector.

Esto permite montar una pantalla sin escribir una clase nueva para cada animacion:

- Open: fade in, mover panel, escalar titulo.
- Base: comportamiento pasivo o acciones persistentes.
- Close: fade out, mover panel fuera, apagar bloqueadores.

## Reglas

- `UIScreen` coordina fases.
- `UIScreenBehaviour` ejecuta listas de acciones.
- `UIAction` encapsula una accion concreta.
- Una accion decide cuando esta completada.
- `Execute` debe devolver `true` por defecto para no bloquear.
- `InstantExecute` deja la accion en su estado final.
- Los behaviours solo hacen `Tick` mientras estan activos.
