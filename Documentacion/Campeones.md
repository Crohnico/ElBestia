# Campeones

## Resumen

Los campeones son luchadores generados de forma aleatoria. El jugador puede ver parte de su informacion antes de contratarlos, pero no conoce todo su potencial real.

Cada campeon debe poder reconstruirse de forma determinista a partir de:

- Nivel.
- ID interno compacto.

Objetivo tecnico:

```text
LVL: 12
ID: ID_BASE-ID_PERKS-ID_SKILLS-ID_EQUIPMENT
```

Con esos datos, el backend o cliente debe poder reconstruir:

- Nombre.
- Stats base y crecimiento.
- Skills.
- Perks.
- Equipamiento.
- Apariencia.

El ID puede estar ofuscado o ser largo a nivel maquina-maquina, pero no deberia ser kilometrico.

## Nombre

El nombre del luchador sera aleatorio.

Regla inicial:

- El nombre completo tiene nombre y apellido.
- El nombre sale de listas de nombres reales/comunes.
- El apellido se forma como `prefijo + sufijo`.
- El generador tiene al menos 100 prefijos y 100 sufijos para apellidos.
- Puede generar apellidos serios o raros tipo `Shadowforce`, `Blindman`, `Handless`, `Hearthless`, `Fireless` o `Handcock`.
- No depende realmente del sexo del campeon.
- Decidir si el nombre forma parte de `ID_BASE` o si se deriva de una seed interna.
- El apodo no se genera automaticamente: queda reservado para que el jugador lo ponga mas adelante.

Pools iniciales de nombres:

- 50 nombres ingleses.
- 50 nombres espanoles.
- 10 nombres franceses.
- 10 nombres alemanes.
- 10 nombres nordicos.
- 10 nombres japoneses.
- 10 nombres coreanos.
- 10 nombres chinos.
- 10 nombres indios.
- 50 nombres portugueses.

Nombres obligatorios incluidos:

- Rodrigo.
- Daniel.
- Dionisio.
- Ruben.
- Isa.
- Nieves.
- Ana.
- Lucia.
- Afro.
- Guillermo.

## ID Interno

Formato propuesto:

```text
ID_BASE-ID_PERKS-ID_SKILLS-ID_EQUIPMENT
```

### ID_BASE

Debe contener o permitir reconstruir:

- Nombre generado.
- Sexo.
- Stats iniciales.
- Crecimiento oculto.
- Apariencia.
- Rasgos base que no sean perks.

### ID_PERKS

Debe contener o permitir reconstruir:

- Perks iniciales.
- Perks heredados.
- Posibles modificadores especiales.

### ID_SKILLS

Debe contener o permitir reconstruir:

- Hasta 3 habilidades.
- Nivel de cada habilidad.
- Datos procedurales necesarios para icono y composicion de acciones.

Regla actual:

- Un campeon solo puede tener 3 skills.

### ID_EQUIPMENT

Debe contener o permitir reconstruir:

- Arma equipada.
- Armadura equipada.
- Otros slots de equipo si se definen mas adelante.

## Stats

### Recursos Principales

**Vida**

- Representa cuanto dano puede recibir el campeon antes de caer.
- Escala principalmente con constitucion.

**Energia**

- Recurso para ejecutar habilidades.
- Tambien puede llamarse poder interno o mana, pendiente de tono final.
- Escala principalmente con inteligencia.

### Stats Base

**Fuerza**

- Aumenta el dano base plano de habilidades.
- Principal stat para builds fisicas o golpes directos.

**Agilidad**

- Aumenta probabilidad de dodge.
- Aumenta velocidad de turno.

**Constitucion**

- Aumenta vida.
- Aporta una base comun a todas las resistencias elementales.
- Puede influir en recuperacion o resistencia a lesiones.

**Inteligencia**

- Aumenta dano overtime: veneno, quemadura y efectos similares.
- Aumenta energia.
- Podria influir en seleccion inteligente de acciones, pendiente.

**Aguante**

- Aumenta resistencia a quedar malherido.
- Aumenta resistencia a danos.
- Puede ser clave para campeones que sobreviven a torneos largos.

### Stats Derivadas

**Resistencia elemental**

- Una resistencia por elemento.
- Reduce o mitiga dano/efectos de ese elemento.
- Funciona como indice, no como porcentaje plano.
- El porcentaje derivado tiene una tendencia maxima propuesta del 70%, para evitar llegar al 100% por acumulacion normal.
- Elementos actuales: fuego, agua, electricidad, veneno, tierra, aire y madera.
- La constitucion suma por igual a todas las resistencias; despues cada elemento puede tener pequenas influencias secundarias de otros stats.

**Velocidad**

- Derivada principalmente de agilidad.
- Se usa para calcular frecuencia o tirada de turno.

**Proficiency con armas**

- Competencia con cada tipo de arma.
- Afecta a la probabilidad de critico y al dano de habilidades que usen ese arma.

**Critical Rating**

- Indice bruto de critico.
- Se transforma en porcentaje de critico con una formula derivada.
- Se ve modificado por proficiency del arma.

**Dodge Rating**

- Indice bruto de esquiva.
- Se transforma en porcentaje de esquiva con una formula derivada.
- Los perks que dan `+X Dodge Rating` no dan directamente `+X%`.

**Block Rating**

- Indice bruto de bloqueo.
- Se transforma en porcentaje/capacidad de bloqueo con una formula derivada.
- Deriva principalmente de aguante y fuerza.

**Hit Rating**

- Indice bruto de golpe.
- Lucha contra la esquiva y el bloqueo del rival.
- No es `+X%` directo: los perks que dan `+X Hit Rating` aumentan el indice, y el combate lo transformara despues en presion contra defensas.
- Deriva principalmente de agilidad e inteligencia.

## Crecimiento

No hay limite de nivel.

Consecuencia de diseno:

- Cuanto mas sobreviva un campeon, mas poderoso puede volverse.
- El nivel determina a que torneos puede o no puede apuntarse.
- Esto evita abusar de torneos inferiores con campeones demasiado fuertes.

El crecimiento por stat sigue el sistema de rangos:

```text
F, E, D, C, B, A, S, SS
```

- F crece muy lento.
- SS crece muy rapido.
- El jugador no ve directamente estos rangos.
- Debe inferirlos por rendimiento, ritmo de mejora y supervivencia.

### Experiencia y Siguiente Nivel

Cada campeon guarda progreso como:

```text
XP actual / XP necesaria para siguiente nivel
```

La XP necesaria no tiene cap de nivel.

Formula inicial:

```text
baseCost = 80 + level^1.35 * 34
latePressure = 1 - exp(-(level - 1) / 35)
xpToNext = baseCost * lerp(1, growthFactor, latePressure)
```

`growthFactor` depende del rango global oculto del campeon:

```text
F  = 1.35
E  = 1.20
D  = 1.08
C  = 1.00
B  = 0.82
A  = 0.58
S  = 0.28
SS = 0.08
```

Intencion de diseno:

- A nivel 1 la diferencia entre rangos apenas se nota.
- La ventaja se abre conforme sobreviven y juegan mas torneos.
- Un campeon excelente debe poder acercarse a nivel 70 con una inversion parecida a la que deja a un campeon mediocre alrededor de nivel 40.
- El jugador no ve el rango directamente, pero puede intuirlo porque algunos campeones suben de nivel con mucha mas fluidez a largo plazo.

## Apariencia

Cada campeon tendra una apariencia generada por partes.

Campos actuales:

- Sexo: H/M.
- Pelo.
- Cabeza.
- Torso.
- Brazos.
- Piernas.
- Pies.

Ejemplo para cabeza:

```text
Head = random 0 - HeadSO.Length
```

## Recursos de Partes Visuales

Las partes visuales se guardaran como ScriptableObjects en `Resources`.

Ejemplo:

```text
HeadSO
```

Cada recurso de partes debe exponer un singleton con una API parecida a:

```csharp
GetResource(int id)
GetAmount()
```

Uso esperado:

- El ID del campeon guarda el indice de cada parte.
- Al cargar el campeon, se accede al SO correspondiente.
- El sistema instancia o asigna las piezas visuales segun esos indices.

Recursos previstos:

- HairSO.
- HeadSO.
- TorsoSO.
- ArmsSO.
- LegsSO.
- FeetSO.

Pendiente:

- Definir si sexo modifica tablas visuales separadas o solo filtra recursos compatibles.
- Definir si las partes influyen en gameplay o son puramente visuales.
- Definir fallback si un ID apunta a una parte que ya no existe tras cambios de contenido.

## Lore Inicial

Cada campeon genera tres piezas de historia:

- Nacimiento.
- Ninez.
- Juventud.

Cada pieza sale de ScriptableObjects en `Resources/Lore`.

Los titulos de lore funcionan como los perks en el inspector: al pasar el puntero por encima se muestra el fragmento de historia y los modificadores mecanicos que aporta.

Catalogo inicial:

- 200 entradas de nacimiento.
- 200 entradas de ninez.
- 200 entradas de juventud.

Cada entrada puede aplicar modificadores iniciales:

- Stats base.
- Proficiency de arma.
- Combat stats.
- Resistencia elemental.

Estos modificadores no cambian la tirada base pura del campeon. El modelo mental es:

```text
stat final = (base tirado + suma de todos los aditivos) * producto de todos los multiplicadores
```

La procedencia no cambia la matematica. Un `+4 Fuerza` de lore y un `+1 Fuerza` de perk son simplemente `+5 Fuerza` total. Un `x0.8 Fuerza` de lore y un `x2 Fuerza` de perk son simplemente `x1.6 Fuerza` total.

Para visualizarlo en el inspector se guardan dos conceptos:

- `baseStats`: la tirada pura de nivel 1, sin lore ni perks.
- `stats`: resultado final despues de aplicar todos los modificadores de lore y perks.

El color del inspector compara `baseStats` contra `stats`. Si el resultado final sube, se ve verde oscuro; si baja, rojo oscuro; si acaba igual que la tirada base, se ve normal. El tooltip de los stats base muestra `Base roll`, `Additive total`, `Multiplier total`, `Final` y la formula usada.

Tambien se genera un texto corto en ingles combinando las tres etapas:

```text
Nombre nacio ... Durante la ninez ... En su juventud ...
```

## Implementacion Inicial en Unity

Scripts creados:

- `ChampionSO`: asset principal de campeon para visualizacion y pruebas.
- `NameGeneratorSO`: generador de nombres internacionales por sexo.
- `HairSO`, `HeadSO`, `TorsoSO`, `ArmsSO`, `LegsSO`, `FeetSO`: catalogos de partes visuales en `Resources/CharacterParts`.

Flujo de uso:

1. Ejecutar `Tools/El Bestia/Create Default Data Assets` para crear los SO base en `Resources`.
2. Crear un asset desde `Create/El Bestia/Champion`.
3. Pulsar el boton `CreateRandomCharacter` en el inspector del campeon.
4. Si se cambian perks a mano durante pruebas, pulsar `Reapply Perks / XP` para recalcular stats finales y experiencia sin regenerar nombre, ID, arma o skills.

El boton genera:

- ID interno compacto.
- Nombre.
- Apodo de jugador vacio.
- Sexo y partes visuales por indice.
- Lore inicial: nacimiento, ninez, juventud y texto de historia.
- Stats base.
- Combat stats: vida, energia, dodge rating, block rating, critical rating, hit rating y proficiency.
- Stats base puras y stats finales tras todos los modificadores, para poder comparar tirada original contra resultado real.
- XP actual y XP necesaria para siguiente nivel.
- Crecimiento oculto.
- Arma equipada.
- De 1 a 3 perks iniciales con probabilidad Pareto.
- Skill base garantizada.
- Entre 0 y 1 habilidades elaboradas iniciales a nivel 1.
- Iconos procedurales cacheados en memoria para visualizar skills en inspector.

Nota: la habilidad elaborada de nivel 1 puede no aparecer. A nivel 5/10/15 se generan slots equipados hasta un maximo de 3, sin contar la skill base.

El inspector colorea los valores modificados:

- Normal: valor final igual a la tirada base.
- Verde oscuro: valor final mayor que la tirada base.
- Rojo oscuro: valor final menor que la tirada base.

## Perks

Los perks son ScriptableObjects.

Rarezas:

- Gris: normal.
- Verde: raro.
- Azul: muy raro.
- Morado: epico.
- Amarillo/dorado: legendario.

Ejemplo de cadena de arma:

- Novice Swordsman: +5 proficiency con espada.
- Skilled Swordsman: +10 proficiency con espada.
- Veteran Swordsman: +15 proficiency con espada.
- Master Swordsman: +20 proficiency con espada.
- Sword Saint: +25 proficiency con espada.

Ejemplos legendarios:

- Vampirism: cura un 25% del dano infligido.
- Double Down: duplica las cargas aplicadas.
- Berserker: empieza el combate con 10 cargas de velocidad.
- Thorn Reversal: puede contraatacar al recibir golpes.
- Ghost Strike: al ser esquivado, repite el golpe instantaneamente al 50% de potencia, minimo 5 de dano. Puede repetirse hasta que falle un golpe reducido a 5 de dano.

Familias adicionales:

- Vida: aumenta la vida maxima del campeon.
- `Life Touched` empieza en `+10 Life`; los bonus planos de vida deben sentirse visibles incluso a nivel 1.
- Experiencia: aumenta la experiencia ganada.
- Stats base planos: bonus tipo `+2 Strength`, buenos para torneos iniciales aunque escalan peor que multiplicadores a largo plazo.
- Hit Rating: aumenta el indice de golpe para atravesar defensas activas.
- Resistencia elemental: aumenta el indice de resistencia contra un elemento concreto.
- Absorcion elemental: perks legendarios unicos que hacen que un elemento cure en vez de hacer dano. Si un campeon tiene una absorcion elemental, no puede aprender otra de la misma familia unica.
- Dots elementales: cada elemento tiene su propia version de dano persistente o presion elemental.
- Supervivencia postcombate: reduce probabilidad de heridas fatales.
- Mitigacion de lesiones: reduce la gravedad de lesiones recibidas.
- Block Pierce: desde raro a legendario, permite que parte del dano bloqueado atraviese la defensa.
- Kamikaze elemental: perks que dan poder a cambio de perjudicar tambien al propio campeon.
- Personalidad de combate: nombres con sabor, pero descripcion y efecto mecanico explicito.

Ejemplos de perks kamikaze:

- Living Bonfire: despues de cada accion aplica quemadura a ambos campeones.
- Elemental Detonation: cada golpe crea una explosion elemental que golpea a ambos combatientes.
- Lazy Genius: empieza con stats reducidas, pero al bajar de media vida despierta y se vuelve mucho mas fuerte.
- Backtalker: invierte buffs y debuffs recibidos.

Regla de escritura:

- El nombre debe dar personalidad.
- La descripcion debe explicar el sabor.
- La misma descripcion debe terminar o incluir un resumen mecanico claro, por ejemplo `Effect: +5 Dodge Rating`, `Effect: +10% XP gain` o `Effect: -15% fatal injury chance`.

Implementacion inicial:

- `PerkSO`: definicion de perk.
- `PerkRarity`: rareza.
- `PerkGenerator`: selecciona perks iniciales.
- `Tools/El Bestia/Rebuild Default Perks`: genera el catalogo inicial.
- Catalogo inicial generado: 470 perks en `Assets/Resources/Perks`.
