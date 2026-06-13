# AGENTS.md - El Bestia

## Regla principal

Antes de proponer, documentar o implementar cambios en este proyecto, lee la documentacion existente.

Punto de entrada obligatorio:

- `Documentacion/README.md`

Despues lee los documentos especificos del area afectada.

## Documentacion por area

Menu principal:

- `Documentacion/Menu/README.md`
- `Documentacion/Menu/EdificiosYSignalBus.md`
- `Documentacion/Menu/CursorInteractivo.md`

UI:

- `Documentacion/UI/README.md`
- `Documentacion/UI/Pantallas.md`
- `Documentacion/UI/Acciones.md`
- `Documentacion/UI/CameraMover.md`
- `Documentacion/UI/GestorPantallas.md`

Combate:

- `Documentacion/Combate.md`
- `Documentacion/EstadosAlterados.md`

Campeones, generacion y progresion:

- `Documentacion/Campeones.md`
- `Documentacion/Habilidades.md`
- `Documentacion/Probabilidades.md`

Visual:

- `Documentacion/DireccionVisual2DEn3D.md`
- `Documentacion/StickmansProcedurales.md`

## Forma de trabajar

- La documentacion es la fuente de verdad de diseno mientras el sistema no este implementado.
- Preferir documentar antes de programar.
- Si se va a implementar algo que todavia no esta documentado, parar y proponer documentarlo primero.
- No implementar sistemas nuevos hasta que el usuario este conforme con la documentacion correspondiente.
- Si el usuario pide "solo documentacion", no tocar codigo.
- Si el usuario esta definiendo una idea, apuntarla en la docu antes de convertirla en implementacion.
- No crear documentos genericos tipo "plan de hoy" salvo que el usuario lo pida explicitamente.
- Mantener la documentacion organizada por carpetas y con `README.md` como indice, no como vertedero de documentos largos.
- Los `README.md` son para humanos: contexto rapido, mapa del area e indice de documentos.
- No dejar informacion importante solo en un `README.md`; si una decision es relevante, debe vivir tambien en un documento especifico.
- No usar los `README.md` como lugar de instrucciones internas para la IA.
- Si una decision cambia, actualizar la docu anterior para que no queden dos verdades.

## Preferencias de codigo

- Preferir estructuras limpias y faciles de leer.
- Evitar programacion defensiva innecesaria. Si algo esta mal configurado o mal programado, es aceptable que falle claramente para poder estudiarlo y corregir la causa.
- No esconder errores importantes con silencios, fallbacks excesivos o comprobaciones que tapen problemas de diseno.
- Preferir nombres claros antes que abreviaturas.
- Comentar numeros magicos para explicar por que existen y evitar que se borren o cambien por accidente mas adelante.
- Mantener scripts cortos cuando sea razonable.
- A partir de unas 150 lineas, plantearse si el script podria dividirse.
- Hasta unas 400 lineas puede ser admisible si hay una razon clara, aunque no sea lo ideal.
- Valorar mucho Single Responsibility.
- Usar patron Strategy cuando ayude a separar comportamientos variables sin llenar una clase de condicionales.
- Usar custom inspectors con botones de prueba siempre que se cree una funcionalidad interactiva o un sistema nuevo que lo permita.
- Cada funcionalidad importante debe poder probarse de forma individual desde inspector cuando sea razonable.
- Ejemplos de botonera esperada: abrir ventana, cerrar ventana, ejecutar `OpenBehaviour`, ejecutar `CloseBehaviour`, probar una `UIAction`, simular `OnClick`, simular hover, disparar una senal.
- Las botoneras de inspector son herramientas de prototipado y debug, no sustituyen la arquitectura runtime.

## Convenciones actuales

- El menu principal vive en la escena `Dojo`.
- Los edificios del menu implementaran `IInteractable`.
- Los edificios no abren pantallas directamente: lanzan senales.
- `SignalBus.Fire` no debe fallar si no hay suscriptores.
- Solo `UIScreenManager` puede abrir y cerrar pantallas.
- Las pantallas se organizan con `UIScreen`, `OpenBehaviour`, `BaseBehaviour` y `CloseBehaviour`.
- Las acciones de UI usan `UIAction` y pueden encadenar acciones hijas.
