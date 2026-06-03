# Direccion Visual 2D en Entorno 3D

## Objetivo

Conseguir una lectura cercana a la tercera referencia visual: escena 3D con apariencia de ilustracion 2D, contorno claro, color mas plano, sombras dibujadas y camara de combate estable.

No buscamos un cel shading generico. El resultado no debe parecer simplemente "3D con bandas de luz", sino una imagen final tratada como dibujo.

## Diagnostico

La escena actual ya tiene una composicion muy favorable:

- Camara fija durante el combate.
- Escenario casi estatico.
- Movimiento concentrado en personajes, antorchas, telas y pequenos props.
- Plano de combate tipo fighting game.
- UI ya planteada como capa 2D.

Esto nos da margen para mezclar varias tecnicas sin pagar el coste de resolver todos los casos de un juego 3D libre.

## Direccion Recomendada

La ruta mas prometedora es una pipeline hibrida:

1. Base 3D con materiales simples y colores controlados.
2. Postprocesado de ilustracion sobre la imagen final.
3. Contornos calculados con profundidad y normales.
4. Sombras estilizadas por patrones o rampas, no por cel shading tradicional.
5. Camara/proyeccion ajustada para reducir la sensacion de perspectiva 3D.
6. Animacion de personajes con cadencia mas "dibujada" cuando encaje.

El postprocesado estilo Moebius es una buena referencia porque trabaja sobre la escena renderizada como imagen, no solo sobre materiales aislados. La idea clave es producir una pasada final que detecta bordes desde depth/normal buffers, simplifica lectura tonal y puede meter tramas o rayado en zonas de sombra.

## Postprocesado Tipo Ilustracion

### Componentes

- `Depth texture`: permite detectar cambios de distancia y sacar contornos exteriores.
- `Normal texture`: permite detectar cambios de orientacion y sacar lineas internas entre planos.
- `Sobel/edge detection`: filtro de convolucion para convertir cambios fuertes en lineas.
- `Color grading controlado`: paleta mas saturada, menos realista, menos PBR.
- `Shadow pattern`: tramas, rayas o manchas en zonas oscuras segun luminancia.
- `Line jitter opcional`: pequena irregularidad para que el borde no parezca vectorial perfecto.

### Adaptacion a Unity

Si el proyecto usa URP, la ruta natural seria una `ScriptableRendererFeature` con una o varias pasadas:

- Pasada de color principal.
- Pasada de normales si necesitamos mas control que el normal buffer disponible.
- Pasada fullscreen de estilizacion.
- Parametros en un `ScriptableObject` o `VolumeComponent` para iterar en escena.

Parametros iniciales:

- Grosor de linea.
- Umbral de profundidad.
- Umbral de normal.
- Color de tinta.
- Intensidad de posterizacion.
- Saturacion.
- Patrones de sombra: ninguno, hatch, raster, textura.
- Intensidad por capa: escenario, personajes, VFX, UI.

## Camara y Proyeccion

La referencia de Guilty Gear Xrd es especialmente util para nuestro caso. El articulo describe que Arc System Works combino proyeccion perspectiva y ortografica en una proporcion aproximada de 30/70 para evitar que los personajes cambiaran de grosor al moverse por la pantalla. Tambien menciona que Street Fighter IV tomo una decision parecida.

Para El Bestia, esto apunta a tres pruebas:

### Opcion A: Perspectiva Larga

Camara perspectiva alejada con FOV bajo.

Ventajas:

- Facil en Unity.
- Mantiene algo de profundidad.
- Menos riesgo tecnico.

Riesgos:

- No corrige del todo la deformacion lateral.
- Puede seguir oliendo a maqueta 3D.

### Opcion B: Ortografica Controlada

Camara ortografica para la escena principal.

Ventajas:

- Lectura muy 2D.
- Personajes de tamano estable.
- Encaja con combate lateral.

Riesgos:

- El escenario puede perder escala y profundidad.
- Algunos efectos 3D pueden parecer planos.

### Opcion C: Proyeccion Hibrida

Proyeccion personalizada o sistema de doble camara que mezcle sensaciones:

- Personajes casi ortograficos.
- Escenario con ligera perspectiva.
- UI completamente independiente.

Ventajas:

- Es la ruta mas cercana a fighting games 3D que quieren parecer 2D.
- Permite preservar profundidad en el coliseo sin deformar personajes.

Riesgos:

- Mas dificil de depurar.
- Puede complicar sombras, z-buffer, VFX y seleccion de capas.

Recomendacion: empezar con A y B como pruebas rapidas. Si el resultado visual pide mas, explorar C. No empezaria directamente por la proyeccion hibrida salvo que la prueba ortografica mate demasiado el escenario.

## Personajes y Animacion

La apariencia 2D no depende solo del shader. Guilty Gear Xrd tambien se apoya en animacion limitada: el juego renderiza a 60 fps, pero ciertas poses se mantienen varios frames para parecer animacion dibujada.

Para El Bestia:

- El combate puede seguir simulandose a 60 fps.
- La presentacion visual puede cuantizar algunas animaciones.
- Ataques y poses clave pueden tener holds breves.
- Los efectos de impacto pueden ser 2D o 3D modelados, pero con timing de animacion tradicional.

Esto es importante porque un stickman con shader 2D pero interpolacion 3D muy suave puede seguir pareciendo un muneco 3D.

## Escenario

Al tener escenario estatico, se puede aprovechar:

- Lineas dibujadas en textura o decals para piedras, arena y madera.
- Sombras pintadas o semibakeadas.
- Materiales planos con poca variacion especular.
- Props pequenos tratados con el mismo shader de tinta.
- Antorchas con luz visual separada de luz real si hace falta controlar lectura.

La tercera imagen tiene mucha fuerza porque el suelo y las gradas tienen lineas artisticas explicitas. Eso no sale solo de postprocesado. Necesitaremos texturas, decals o un pass especifico de lineas sobre superficies.

## VFX y UI

La UI debe quedar fuera del postprocesado de mundo o recibir una pasada distinta.

Los VFX conviene separarlos por familia:

- Fuego/antorchas: pueden ser sprites o meshes con material emissive, pero sin iluminar demasiado la escena si rompe la consistencia.
- Impactos: mejor estilo 2D, con spritesheet o mesh plano orientado a camara.
- Polvo/humo: puede ser 3D, pero con textura dibujada o animacion por frames.
- Armas: mesh 3D con el mismo tratamiento de tinta que el personaje.

## Riesgos

- Un filtro global puede ensuciar la UI si no se separan capas.
- Los contornos por profundidad fallan en superficies coplanares; por eso hacen falta normales o lineas artisticas.
- Las tramas de sombra pueden vibrar si estan ancladas a pantalla y la camara se mueve.
- Las tramas pueden moirear en resoluciones bajas.
- El escenario necesita direccion artistica, no solo shader.
- Si todo tiene el mismo grosor de linea, personajes y fondo compiten demasiado.

## Plan de Pruebas

1. Crear una escena duplicada de combate visual.
2. Probar camara perspectiva con FOV bajo.
3. Probar camara ortografica equivalente.
4. Crear una primera pasada fullscreen con:
   - Posterizacion suave.
   - Sobel por profundidad.
   - Sobel por normales si esta disponible.
5. Separar capas:
   - Mundo.
   - Personajes.
   - UI.
6. Hacer una captura comparativa con:
   - Sin filtro.
   - Solo contorno.
   - Contorno + color.
   - Contorno + color + tramas.
7. Ajustar line width distinto para escenario y personajes.
8. Probar animacion limitada en un ataque.

## Decision Actual

Si, parece viable acercarnos bastante a la tercera imagen manteniendo 3D.

La clave no sera una unica tecnica milagrosa. La direccion correcta es una combinacion de camara casi 2D, postprocesado de ilustracion, materiales simples, lineas artisticas en el escenario y timing de animacion menos interpolado.

Para un primer prototipo no implementaria todavia una camara hibrida compleja. Primero probaria:

- Camara ortografica.
- Camara perspectiva lejana con FOV bajo.
- Postprocesado Sobel depth/normal.
- Posterizacion y control de saturacion.

Si la ortografica da la lectura 2D pero aplana demasiado el coliseo, entonces tiene sentido investigar la proyeccion hibrida.

## Referencias

- Maxime Heckel, Moebius-style post-processing: https://blog.maximeheckel.com/posts/moebius-style-post-processing/
- 4Gamer, Guilty Gear Xrd rendering article: https://www.4gamer.net/games/216/G021678/20140714079/
- Unity Discussions, camara hibrida ortografica/perspectiva: https://discussions.unity.com/t/hybrid-camera-between-orthographic-and-perspective-mode/694922/5
