# Documentacion - El Bestia

Esta carpeta recoge la definicion del proyecto: vision, alcance, sensacion de juego, loop jugable, sistemas y progresion a largo plazo.

## Documentos

- [Alcance.md](./Alcance.md): vision general, pilares, loop jugable y profundidad.
- [Campeones.md](./Campeones.md): datos de luchadores, stats, identidad, apariencia e ID determinista.
- [Habilidades.md](./Habilidades.md): estructura de skills, acciones, cargas, rareza/calidad y escalado.
- [Combate.md](./Combate.md): manager de combate, flujo de prueba, cronometro, turnos y reglas de fin.
- [Probabilidades.md](./Probabilidades.md): reglas Pareto para crecimiento, rarezas y cantidades.

## Forma de trabajo

La documentacion se ira escribiendo por capas:

1. Sensacion objetivo: como debe sentirse jugar.
2. Loop jugable: que hace el jugador minuto a minuto y sesion a sesion.
3. Profundidad: decisiones, progresion, late game y rejugabilidad.
4. Alcance MVP: que entra en la primera version jugable.
5. Fuera de alcance: ideas buenas que se aparcan para no deformar el proyecto.

## Principio tecnico

Siempre que sea razonable, los campeones deben poder reconstruirse desde datos compactos: nivel + ID interno. Esto permite torneos async, almacenamiento ligero y envio maquina-maquina sin serializar objetos enormes.
