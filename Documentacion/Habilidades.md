# Habilidades

## Resumen

Las habilidades de los luchadores seran aleatorias y podran subir de nivel.

Una habilidad no es solo un golpe: es una composicion de acciones que pueden ocurrir antes, durante y despues del cast.

Estructura conceptual:

```csharp
Skill
{
    Icon;
    StatScaling;
    Weapon;
    Range;
    Element;
    EnergyCost;
    PreCast[];
    Cast[];
    PostCast[];
}
```

## Icono

El icono de una habilidad sera procedural.

Debe poder generarse a partir de los datos/seed de la habilidad para que:

- No haya que almacenar un icono unico por skill generada.
- La habilidad sea reconocible visualmente.
- Dos habilidades similares puedan compartir lenguaje visual.

## SkillAction

Una accion de habilidad define un objetivo y un efecto.

```csharp
SkillAction
{
    Target: Self / Enemy;
    Action;
}
```

### Target

**Self**

- La accion afecta al propio campeon.

**Enemy**

- La accion afecta al rival.

## Skill

Campos actuales:

**Stat**

- Stat o stats con las que escala.
- Puede escalar con uno o dos stats.
- Cada escalado usa rangos tipo F, E, D, C, B, A, S, SS.

Ejemplo conceptual:

```text
Fuerza: A
Inteligencia: D
```

**Weapon**

- Puede ser `none`.
- Puede ser una lista de armas compatibles.
- Si requiere arma, la habilidad solo puede usarse cuando el campeon tenga una de esas armas.

**Range**

- Rango de uso.
- Va de 0 a `maxWeaponRange`.
- El combate debe mover al campeon al rango necesario antes de ejecutar la accion.

**Elemento**

- Elemento asociado a la habilidad.
- Ejemplos actuales: agua, electricidad.
- Pendiente definir lista completa.

**Coste de energia**

- Cantidad de energia necesaria para usar la habilidad.
- Pendiente decidir que ocurre si no hay energia suficiente: saltar habilidad, usar basico, esperar o elegir otra.

**Cooldown**

- Las habilidades elaboradas tienen cooldown.
- Rango inicial objetivo: 2-10 segundos.
- El cooldown depende de cuantas fases, acciones y efectos tenga la habilidad.
- El cooldown baja conforme sube el nivel de la habilidad.
- La habilidad base siempre tiene cooldown 0.

**PreCast[]**

- Acciones que ocurren antes del efecto principal.

**Cast[]**

- Acciones principales de la habilidad.

**PostCast[]**

- Acciones que ocurren despues del efecto principal.

## Tipos de Accion

### PreCast y PostCast

Acciones posibles:

- Aumenta el dano del propio campeon.
- Debilita al enemigo.
- Gana X cargas de Y.
- Aplica X cargas de Y.

### Cast

Acciones posibles:

- `DoDamage`.

`DoDamage` puede ser:

- Positivo: ataque o dano.
- Negativo: curacion/heal.

Pendiente:

- Decidir si `DoDamage` negativo siempre targetea a `Self` o si se permite curar al enemigo por efectos raros.
- Separar mas adelante `Damage`, `Heal`, `Shield`, `Cleanse`, etc., si la abstraccion se vuelve confusa.

## Cargas

Las cargas representan estados acumulables.

### Cargas Negativas

- Veneno.
- Quemar.
- Lentitud.
- TBD.

### Cargas Positivas

- Rapidez.
- Robo de vida.
- Contraataque.
- TBD.

Pendiente:

- Duracion por cargas, por tiempo o por usos.
- Si las cargas se consumen o se degradan.
- Maximo de cargas.
- Interacciones entre cargas opuestas.

## Calidad de Habilidad

Las habilidades usan el mismo lenguaje de rangos:

```text
F, E, D, C, B, A, S, SS
```

La calidad/rango de una habilidad determina cuantas fases de accion puede tener.

Regla inicial:

- F: solo tiene una accion, a escoger entre `PreCast`, `Cast` o `PostCast`.
- SS: puede tener las tres fases: `PreCast`, `Cast` y `PostCast`.

Pendiente de tabla completa:

```text
F  = 1 fase
E  = TBD
D  = TBD
C  = TBD
B  = TBD
A  = TBD
S  = TBD
SS = 3 fases
```

Notas de diseno:

- Una habilidad de baja calidad aun puede ser util si su accion es buena o barata.
- Una habilidad SS no deberia ser automaticamente perfecta: coste, requisito de arma, rango y elemento pueden balancearla.

## Nivel de Habilidad

Las habilidades suben de nivel:

```text
Nivel 1, 2, 3, ...
```

Pendiente:

- Como ganan experiencia.
- Si suben por uso, por nivel del campeon, por entrenamiento o por herencia.
- Que mejora al subir: potencia, coste, cargas, escalado, precision, velocidad o nuevas fases.

## Skill Base

Todos los campeones tienen una skill base.

- No cuenta para el limite de 3 skills equipadas.
- Siempre esta disponible.
- Cooldown: 0 segundos.
- Coste de energia: 0.
- Es un golpe simple con el arma equipada.
- No usa escalado por rareza `F` a `SS`.

Escalado inicial:

- Hacha: fuerza.
- Arco: agilidad.
- Lanza: agilidad + fuerza.
- Espada: fuerza + agilidad.
- Punhos/staff: pendiente de afinar, base actual fuerza.

El objetivo es que cada vez que a un campeon le toque actuar siempre tenga al menos una accion posible.

## Aprendizaje de Skills

Las skills elaboradas pueden o no existir a nivel 1.

Regla inicial:

- Nivel 1: posibilidad de empezar con una skill elaborada.
- Nivel 5: aprende una skill.
- Nivel 10: aprende otra skill.
- Nivel 15: aprende otra skill.
- A partir de ahi puede seguir aprendiendo, pero solo puede tener 3 equipadas sin contar la base.

## Generacion Aleatoria

Las habilidades se craftearan de forma aleatoria.

La generacion debe elegir:

- Icono/seed visual.
- Stat o stats de escalado.
- Rango de escalado.
- Requisito de arma.
- Rango.
- Elemento.
- Coste de energia.
- Acciones de `PreCast`, `Cast` y `PostCast` segun calidad.
- Nivel inicial.

Pendiente:

- Pesos de generacion.
- Reglas para evitar combinaciones inutiles.
- Reglas para permitir combinaciones raras pero memorables.

## Implementacion Inicial en Unity

Scripts creados:

- `SkillData`: estructura serializable de habilidad.
- `SkillAction`: accion con target, tipo, carga y cantidad.
- `RandomSkillFactory`: genera una habilidad aleatoria inicial.
- `SkillNameGenerator`: genera nombres de habilidades.
- `SkillIconGenerator`: genera una textura procedural 64x64.
- `SkillDescriptionGenerator`: genera una descripcion legible desde acciones y efectos.

El icono procedural inicial usa:

- Color de fondo segun elemento.
- Forma central segun stat principal.
- Borde segun calidad/rango de la habilidad.

Cuando se pulsa `CreateRandomCharacter` en un `ChampionSO`, no se guarda ningun PNG. El icono se genera bajo demanda desde los datos de la skill y se cachea en memoria en el editor.

El inspector muestra cada skill como ficha:

- Nombre.
- Cooldown.
- Escalado.
- Arma.
- Icono procedural cacheado.
- Descripcion.

El nombre procedural inicial intenta comunicar lectura de combate y se genera en ingles:

- Accion/arma: `Cleave`, `Slash`, `Thrust`, `Shot`, `Strike`, etc.
- Efectos/cargas: `Burning`, `Poisonous`, `Slowing`, `Vampiric`, etc.
- Elemento: `Water`, `Fire`, `Earth`, `Wind`, `Thunder`, `Venom`.
- Si hay varios efectos, deben aparecer todos los importantes en el nombre o activar un combo especial.
- Los elementos no reciben adjetivos automaticos como `Wild Fire` o `Muddy Earth` si la habilidad no lo justifica.
- Los efectos combinados pueden crear frases compactas como `Burning Venom`, `Numbing Venom` o `Scorching Chains`.
- Algunas combinaciones prefijadas pueden usar nombres mas iconicos.

Ejemplo:

```text
Water Cleave of Burning Venom
```

Ademas de `skillName` para lectura humana, cada habilidad guarda `skillNameId` para localizacion futura.

Ejemplo:

```text
skill_name.water_cleave.burning_venom
```

Ejemplo de combo especial:

```text
Venomous Dragonfall
skill_name.combo.venomous_dragonfall
```
