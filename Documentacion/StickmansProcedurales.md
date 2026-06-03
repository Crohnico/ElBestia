# Stickmans Procedurales

## Objetivo

Definir un sistema para generar campeones tipo stickman con identidad visual variable, reconstruible desde datos compactos y compatible con animacion Humanoid.

La fantasia es cercana a Spore en miniatura: el jugador o el generador no esculpe un personaje completo desde cero, sino que combina proporciones, grosores, siluetas y accesorios para producir infinitos luchadores reconocibles.

## Principio Base

El esqueleto manda. La malla se adapta.

Los huesos no deberian "crecer" libremente en tiempo de juego para simular musculatura. Las articulaciones representan limites naturales. Lo que cambia es:

- Longitud base de segmentos, si el rig lo permite en generacion.
- Grosor por segmento.
- Perfil de grosor a lo largo del segmento.
- Redondez o cuadratura.
- Volumen del torso.
- Tamano de cabeza/manos/pies.
- Accesorios y pelo.
- Color/material.

Cada parte debe poder empezar y terminar con un grosor compatible con su articulacion para que el personaje no parezca roto al animar.

## Primer Paso Real

El primer prototipo deberia ser una plantilla viva, no aun un generador completo.

La idea:

1. Pillar un esqueleto Humanoid funcional.
2. Crear un prefab `StickmanTemplate`.
3. Anadir una primitiva visible por cada hueso importante.
4. Anadir primitivas de articulacion donde haya transiciones.
5. Dar a cada pieza un ID estable.
6. Guardar esa configuracion como datos.
7. Reconstruir el mismo muneco desde esos IDs y valores.

Este paso valida lo importante:

- Que el rig anima bien.
- Que las piezas siguen los huesos correctos.
- Que el stickman se lee bien en combate.
- Que las articulaciones tapan cortes.
- Que el sistema se puede reconstruir desde datos compactos.
- Que el shader/postprocesado no destruye la silueta.

No hace falta que este primer muneco sea procedural de verdad. Tiene que ser editable, serializable y animable.

## Plantilla de Huesos y Piezas

Cada pieza del cuerpo deberia tener un ID propio y una relacion explicita con un hueso.

Ejemplo:

```text
body.head          -> HumanBodyBones.Head
body.chest_upper   -> HumanBodyBones.Chest
body.chest_lower   -> HumanBodyBones.Hips
arm.left.upper     -> HumanBodyBones.LeftUpperArm
arm.left.elbow     -> HumanBodyBones.LeftLowerArm / LeftUpperArm junction
arm.left.lower     -> HumanBodyBones.LeftLowerArm
arm.left.hand      -> HumanBodyBones.LeftHand
leg.left.upper     -> HumanBodyBones.LeftUpperLeg
leg.left.knee      -> HumanBodyBones.LeftLowerLeg / LeftUpperLeg junction
leg.left.lower     -> HumanBodyBones.LeftLowerLeg
leg.left.foot      -> HumanBodyBones.LeftFoot
```

El ID no deberia depender del nombre del GameObject en escena. El nombre puede cambiar; el ID es contrato de datos.

Formato conceptual:

```text
StickmanPart
  id
  bone
  primitiveType
  localPosition
  localRotation
  localScale
  profile
  materialSlot
  mirrorOf
```

Para brazos y piernas, conviene definir una pieza izquierda y generar la derecha por espejo cuando sea posible. Si una pieza tiene decoracion asimetrica, entonces se rompe el espejo y se guarda como pieza propia.

## Fase 1: Primitivas Parentadas

La primera version puede usar primitivas normales de Unity:

- Esferas para cabeza y articulaciones.
- Capsulas/cilindros para brazos y piernas.
- Cubos redondeados o capsulas escaladas para torso.
- Esferas achatadas para manos.
- Capsulas o cajas simples para pies.

Cada primitiva va parentada al hueso correspondiente con offset local.

Ventajas:

- Rapido.
- Muy visible.
- Facil de depurar.
- Permite probar animaciones inmediatamente.

Limitaciones:

- No hay deformacion suave.
- Las piezas pueden separarse en poses extremas.
- Las capsulas de Unity limitan bastante la silueta.

Esta fase no intenta ser final. Es una maqueta tecnica para encontrar la estructura correcta.

## Fase 2: Capturar la Plantilla

Cuando el prefab base funcione, creamos una utilidad de editor:

- Lee todas las piezas `StickmanPartAuthoring`.
- Extrae ID, hueso, transform local, tipo de primitiva y material.
- Crea un `StickmanBodyTemplateSO`.
- Permite reconstruir el prefab desde el asset.

El objetivo es que podamos borrar el muneco de escena y regenerarlo identico desde el asset.

Esto sera la base de los IDs procedurales:

```text
championVisualId
  templateId
  seed
  archetype
  partOverrides
  accessoryIds
```

## Fase 3: Sustituir Primitivas por Meshes Parametricas

Cuando la plantilla por primitivas este validada, sustituimos piezas una a una:

- Primero extremidades.
- Luego torso.
- Luego manos/pies.
- Por ultimo cabeza y juntas especiales.

Cada pieza conserva el mismo ID. Solo cambia como se genera su geometria.

Esta es la ventaja de empezar con plantilla: el contrato de datos no cambia aunque mejoremos la malla.

## Modelo de Datos

Un campeon visual podria reconstruirse desde:

- `bodySeed`
- `bodyArchetype`
- `headShape`
- `upperTorsoShape`
- `lowerTorsoShape`
- `limbProfiles`
- `jointProfiles`
- `handsFeetProfile`
- `materialProfile`
- `hairProfile`
- `accessoryProfile`

Los valores finales pueden salir de un `ChampionBodyPresetSO` mas pequenos modificadores procedurales.

Ejemplo conceptual:

```text
ChampionBodyPresetSO
  height
  shoulderWidth
  hipWidth
  headRadius
  torsoUpper
    widthTop
    widthBottom
    height
    roundness
    belly
    chest
  torsoLower
    widthTop
    widthBottom
    height
    roundness
  limbs
    upperArm
    forearm
    thigh
    shin
    thicknessStart
    thicknessMid
    thicknessEnd
    roundness
    taper
  joints
    elbowRadius
    kneeRadius
    wristRadius
    ankleRadius
```

## Anatomia Modular

### Segmentos

Los miembros se pueden construir como cadenas:

```text
mano | articulacion | antebrazo | articulacion | brazo | articulacion
```

Esto permite que cada segmento sea una mesh independiente, emparentada o skinned al hueso correspondiente.

Segmentos iniciales:

- Cabeza.
- Cuello opcional.
- Torso superior.
- Torso inferior.
- Brazo superior izquierdo/derecho.
- Antebrazo izquierdo/derecho.
- Mano izquierda/derecha.
- Muslo izquierdo/derecho.
- Espinilla izquierda/derecha.
- Pie izquierdo/derecho.
- Articulaciones: hombro, codo, muneca, cadera, rodilla, tobillo.

### Torso

El torso puede empezar con dos primitivas:

```text
[    ]
 [  ]
```

Con parametros de anchura, redondez y barriga/pecho se consiguen variantes:

```text
Azul:
[    ]
  []

Rojo:
  [  ]
 (     )
```

Esto es una buena base porque da siluetas fuertes sin necesitar anatomia compleja.

## Generacion de Mesh

No conviene hacer un editor libre de vertices al principio. Seria potente, pero caro de mantener y facil de romper.

La ruta recomendada es generar meshes parametricas desde perfiles:

- Un segmento es un volumen entre dos puntos.
- Tiene varias secciones transversales.
- Cada seccion tiene radio X/Y.
- Cada seccion puede interpolar entre redondo y cuadrado.
- La malla se reconstruye al cambiar sliders.

Para una extremidad:

```text
start joint radius -> profile A -> profile B -> end joint radius
```

Para torso:

```text
top width/depth -> mid deformation -> bottom width/depth
roundness 0 = caja
roundness 1 = capsula/ovalo
belly/chest desplazan o escalan secciones intermedias
```

Esto evita esculpir manualmente y mantiene todos los personajes dentro de reglas animables.

## Fusion Visual Entre Primitivas

Hay tres niveles posibles:

### Nivel 1: Solape Controlado

Las piezas se solapan un poco en las articulaciones. Con material plano, contorno y camara fija puede bastar.

Ventajas:

- Simple.
- Robusto.
- Facil de animar.

Riesgo:

- Puede verse como piezas pegadas si el contorno marca demasiado las uniones.

### Nivel 2: Juntas Esfericas

Las articulaciones son esferas/capsulas que tapan las transiciones.

Ventajas:

- Muy compatible con stickman.
- Refuerza el lenguaje visual.
- No requiere fusion real de malla.

Riesgo:

- Si todas las juntas son iguales, los personajes pierden variedad.

### Nivel 3: Fusion por Shader

Usar una tecnica tipo metaball/SDF o blending visual para suavizar uniones.

Ventajas:

- Aspecto organico.
- Encaja con la idea de primitivas fusionadas.

Riesgos:

- Mucho mas complejo.
- Puede complicar contornos, sombras y z-buffer.
- Dificil si cada pieza sigue siendo una mesh separada con rig Humanoid.

Recomendacion: prototipar niveles 1 y 2 primero. Reservar fusion por shader para torso/cuello/hombros si el resultado lo pide.

## Rig y Humanoid

La forma mas segura es mantener un rig Humanoid convencional y generar piezas alrededor de cada hueso.

Opciones:

- Mesh por segmento parentada al hueso.
- Mesh por segmento con skinning simple a uno o dos huesos.
- Mesh combinada generada y skinned al esqueleto completo.

Para el MVP visual, usaria mesh por segmento parentada o con pesos muy simples. Los stickmans toleran mejor la separacion entre piezas que un humano realista.

Cuando haya ataques y deformaciones mas exigentes, se puede pasar a mesh combinada o pesos compartidos.

## Editor

El editor deberia ser de datos, no de vertices.

### `ChampionBodyPresetSO`

ScriptableObject con sliders agrupados:

- Global:
  - altura
  - escala general
  - anchura de hombros
  - anchura de caderas
  - tamano de cabeza
- Torso:
  - pecho
  - barriga
  - cintura
  - redondez
  - cuadratura
- Brazos:
  - longitud
  - grosor superior
  - grosor inferior
  - taper
  - articulaciones
- Piernas:
  - longitud
  - muslo
  - espinilla
  - pie
  - articulaciones
- Estilo:
  - color principal
  - roughness/smoothness visual
  - grosor de linea preferido
  - patron de sombra opcional
- Decoracion:
  - pelo
  - barba
  - casco/sombrero
  - arma inicial
  - accesorios ligeros

### Preview

El inspector necesita un preview:

- Boton `Randomize`.
- Boton `Apply Seed`.
- Boton `Bake Mesh`.
- Vista en pose T/A.
- Vista en idle.
- Vista en pose de combate.
- Warning si una combinacion rompe articulaciones o solapes.

### Sliders Avanzados

Los sliders no deberian ser todos lineales y sueltos. Conviene tener arquetipos:

- Delgado.
- Fuerte.
- Barrigon.
- Cabezon.
- Cuadrado.
- Redondo.
- Larguirucho.
- Compacto.

Cada arquetipo rellena los valores y luego el jugador/generador modifica dentro de rangos seguros.

## Decoracion

Los pelos y accesorios de assets existentes pueden funcionar si pasan por el mismo tratamiento visual:

- Material compatible con el shader/postprocesado.
- Paleta limitada.
- Contorno coherente.
- Escala exagerada si hace falta para lectura.
- Evitar mucho detalle fino que luego se pierda con el filtro 2D.

Para mantener el sistema procedural, cada accesorio debe declarar:

- Punto de anclaje.
- Offset.
- Rango de escalado.
- Si se refleja al cambiar de lado.
- Si admite color del campeon.
- Rareza o tags de estilo.

## Relacion Con Stats y Lore

La apariencia puede derivar de identidad sin volverse determinista aburrida:

- Fuerza alta aumenta probabilidad de torso ancho o brazos gruesos.
- Agilidad alta aumenta probabilidad de silueta fina y piernas largas.
- Vitalidad alta aumenta volumen o juntas grandes.
- Lore de desierto puede favorecer telas, vendas, pelo seco, ornamentos.
- Perks elementales pueden cambiar acentos visuales.

Importante: el jugador no deberia poder leer todos los numeros exactos desde la silueta. La silueta comunica personalidad, no una hoja de calculo.

## Riesgos

- Demasiados sliders pueden producir personajes feos antes que variados.
- Si el contorno marca cada pieza, el personaje se puede ver desmontado.
- Si fusionamos demasiado, se pierde la gracia de stickman.
- Accesorios realistas pueden chocar con cuerpo simple.
- El rig Humanoid puede imponer proporciones menos extremas de lo que queremos.
- Las armas deben respetar manos simples y poses exageradas.

## Paso a Paso del Generador

### Paso 0: Escena de Prueba

Crear una escena aislada `StickmanGeneratorSandbox`.

Debe tener:

- Una camara comoda de trabajo, sin fijar aun la proyeccion visual final.
- Luz basica.
- Suelo simple.
- Un Animator con idle, caminar y una animacion de ataque.
- Un material plano provisional.
- Botones de editor o menu para regenerar.

Objetivo: guarrear con el generador de stickmans sin tocar `Arena`, que sigue siendo la escena final de combate.

### Paso 1: Elegir el Rig Base

Crear o importar un esqueleto Humanoid limpio.

Requisitos:

- Huesos Humanoid bien mapeados.
- Pose T o A estable.
- Escala conocida.
- Animator funcional.
- Jerarquia limpia y sin geometria final acoplada.

Resultado esperado:

- Prefab `StickmanRigBase`.
- Una animacion idle funcionando.
- Una animacion de ataque funcionando.

Decision: el rig base es el contrato fisico. La generacion visual cuelga de el.

### Paso 2: Definir IDs Canonicos

Crear una lista cerrada de IDs para las piezas iniciales.

Ejemplo minimo:

```text
body.head
body.neck
body.torso.upper
body.torso.lower
arm.left.upper
arm.left.elbow
arm.left.lower
arm.left.hand
arm.right.upper
arm.right.elbow
arm.right.lower
arm.right.hand
leg.left.upper
leg.left.knee
leg.left.lower
leg.left.foot
leg.right.upper
leg.right.knee
leg.right.lower
leg.right.foot
```

Reglas:

- El ID no cambia aunque renombremos GameObjects.
- El ID no incluye datos variables como grosor o color.
- Las piezas simetricas pueden declarar `mirrorOf`, pero siguen teniendo ID propio.
- Los accesorios usan otro namespace: `hair.*`, `weapon.*`, `cloth.*`, `prop.*`.

Resultado esperado:

- Enum o constantes para IDs.
- Documento/lista de hueso asociado por ID.

### Paso 3: Crear Componentes de Autoria

Crear un componente de editor/runtime para marcar piezas.

Concepto:

```text
StickmanPartAuthoring
  partId
  humanBone
  primitiveType
  materialSlot
  mirrorOf
  isJoint
  allowProceduralOverride
```

Cada pieza visible del prefab lleva uno.

Resultado esperado:

- Podemos seleccionar cualquier primitiva y saber que pieza logica representa.
- Podemos recorrer el prefab y encontrar todas las piezas.

### Paso 4: Montar la Plantilla Manual

Sobre `StickmanRigBase`, crear un prefab `StickmanTemplate_Primitives`.

Piezas:

- Cabeza: esfera.
- Torso superior: capsula/cubo escalado.
- Torso inferior: capsula/cubo escalado.
- Brazos: capsulas o cilindros.
- Piernas: capsulas o cilindros.
- Manos: esferas achatadas.
- Pies: capsulas/cajas.
- Articulaciones: esferas en hombros, codos, munecas, caderas, rodillas y tobillos.

Cada pieza va parentada al hueso que le corresponde. Al principio no hace falta skinning.

Resultado esperado:

- El muneco se mueve con idle y ataque.
- Las piezas no se quedan atras.
- La silueta ya se parece a un stickman.

### Paso 5: Capturar a ScriptableObject

Crear `StickmanBodyTemplateSO`.

Debe guardar:

```text
templateId
rigPrefab
parts[]
  partId
  humanBone
  primitiveType
  localPosition
  localRotation
  localScale
  materialSlot
  mirrorOf
```

Crear una herramienta de editor:

- `Capture From Selected Stickman`.
- `Rebuild Selected Stickman From Template`.
- `Validate Template`.

Resultado esperado:

- Podemos convertir el prefab manual en datos.
- Podemos regenerar el mismo personaje desde datos.
- Podemos detectar IDs duplicados o huesos sin asignar.

### Paso 6: Reconstruccion Runtime

Crear un servicio o componente:

```text
StickmanBodyBuilder
  Build(template, visualProfile, parent)
```

Primera version:

- Instancia el rig.
- Busca transforms de huesos con `Animator.GetBoneTransform`.
- Crea primitivas.
- Aplica transform local.
- Aplica material.
- Parent a cada hueso.

Resultado esperado:

- Dado un template, aparece un stickman completo.
- El resultado reconstruido se comporta igual que el prefab manual.

### Paso 7: Perfil Visual Minimo

Crear `StickmanVisualProfile`.

Debe poder modificar sin cambiar la plantilla:

```text
seed
primaryColor
secondaryColor
globalScale
headScale
torsoUpperScale
torsoLowerScale
armThickness
legThickness
jointScale
handScale
footScale
roundnessBias
```

Primera version: los cambios son escalados sobre primitivas.

Resultado esperado:

- El mismo template produce personajes distintos.
- Todavia no generamos meshes custom.
- Ya podemos crear 10 siluetas con seeds.

### Paso 8: Random Determinista

Crear una funcion:

```text
StickmanVisualProfile Generate(seed, archetype, stats, lore)
```

Entradas:

- Seed del campeon.
- Arquetipo visual.
- Stats relevantes.
- Lore/perks opcionales.

Salidas:

- Perfil visual compacto.

Reglas:

- Mismo seed + mismos datos = mismo cuerpo.
- El random no debe depender del orden de ejecucion de Unity.
- Cada parametro debe tener rangos seguros.

Resultado esperado:

- Cuerpos reproducibles.
- Variacion suficiente sin romper animacion.

### Paso 9: Arquetipos

Crear presets de alto nivel:

- Delgado.
- Fuerte.
- Barrigon.
- Cabezon.
- Cuadrado.
- Redondo.
- Larguirucho.
- Compacto.

Cada arquetipo define:

- Rango de altura.
- Rango de cabeza.
- Rango de torso.
- Rango de extremidades.
- Probabilidad de accesorios.
- Paletas preferidas.

Resultado esperado:

- El generador no produce ruido puro.
- Los personajes tienen intencion visual.

### Paso 10: Sustituir Extremidades por Meshes Parametricas

Crear generador de mesh para segmentos:

```text
SegmentMeshProfile
  length
  radiusStartX/Y
  radiusMidX/Y
  radiusEndX/Y
  roundness
  taper
  subdivisions
```

Primero aplicarlo a:

- Brazo superior.
- Antebrazo.
- Muslo.
- Espinilla.

Mantener los mismos IDs.

Resultado esperado:

- Ya no dependemos de capsulas de Unity.
- Podemos hacer brazos mas cuadrados, finos, gruesos o con taper.

Primera tool creada:

- `StickmanSegmentMesh`: componente que genera una malla entre dos articulaciones.
- Entrada: transform de inicio y transform de final.
- `Particiones`: numero de puntos de control invisibles a lo largo del segmento.
- Cada particion tiene un slider de radio.
- La malla interpola esos radios y redondea los extremos para funcionar como capsula deformable.
- No muestra vertebras en escena; las particiones solo existen como controles de inspector.

### Paso 11: Sustituir Torso por Mesh Parametrica

Crear `TorsoMeshProfile`.

Parametros:

- Anchura superior.
- Anchura inferior.
- Profundidad superior.
- Profundidad inferior.
- Pecho.
- Barriga.
- Cintura.
- Redondez.
- Cuadratura.

Resultado esperado:

- Conseguimos siluetas tipo azul y rojo.
- El torso marca identidad sin complicar extremidades.

### Paso 12: Accesorios

Crear `StickmanAccessorySO`.

Debe guardar:

```text
accessoryId
anchorBone
prefab
localPosition
localRotation
localScale
mirrorMode
allowedArchetypes
materialMode
```

Primeras familias:

- Pelo.
- Barba.
- Vendas.
- Hombreras simples.
- Cinturon.
- Arma.

Resultado esperado:

- Podemos decorar sin tocar el cuerpo base.
- Los assets externos se integran con material/paleta comun.

### Paso 13: Material y Render

Aplicar un material comun a todas las piezas.

Debe permitir:

- Color principal.
- Color secundario.
- Control de suavidad.
- Compatibilidad con contorno/postprocesado.
- Opcion de excluir uniones internas si el outline las ensucia.

Resultado esperado:

- El stickman se lee como un unico personaje.
- No parece una suma de primitivas sin direccion artistica.

### Paso 14: Bake Opcional

Cuando el generador funcione, decidir si conviene bakear.

Opciones:

- Mantener piezas separadas para debug y modularidad.
- Combinar meshes por material para rendimiento.
- Bakear asset por campeon importante.
- Generar runtime solo para personajes temporales.

Decision inicial:

- No bakear en la primera version.
- Optimizar cuando tengamos varios campeones simultaneos y mediciones.

### Paso 15: Validacion

Crear una herramienta `Generate Stickman Sheet`.

Debe generar capturas o una escena con:

- 20 seeds.
- 8 arquetipos.
- Pose idle.
- Pose ataque.
- Vista combate.
- Vista cercana.

Criterios de aceptacion:

- Cada campeon se distingue en silueta.
- Ninguna articulacion se rompe en ataque.
- La cabeza/manos/pies se leen a tamano de juego.
- El postprocesado no une ni destruye formas importantes.
- Los accesorios no ocultan armas ni UI.

## Plan de Prototipo

1. Crear un `ChampionBodyPresetSO` minimo.
2. Generar una extremidad parametrica entre dos transforms.
3. Generar torso superior e inferior con secciones deformables.
4. Anadir articulaciones esfericas.
5. Parentar piezas a un rig Humanoid de prueba.
6. Probar idle, caminar y ataque.
7. Aplicar material plano + contorno.
8. Crear 10 seeds y comparar siluetas.
9. Anadir pelo/accesorio simple.
10. Decidir si hace falta fusion por shader.

## Decision Actual

La idea es solida.

Yo evitaria empezar por un editor de meshes libre. Empezaria por un editor de presets con sliders que generan meshes parametricas. Eso da el 80% de la fantasia Spore, pero mantiene el sistema determinista, animable y facil de reconstruir desde un ID.

El primer objetivo no debe ser que el jugador pueda hacer cualquier forma, sino que el sistema pueda producir siluetas muy distintas sin romper animacion ni lectura 2D.
