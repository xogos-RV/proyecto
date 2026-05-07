# Resumen del Proyecto Unity - Patinete

## Visión General

- **Nombre del Proyecto**: Patinete (juego de exploración y recolección)
- **Motor**: Unity 6000.0.25f1 (Unity 6)
- **Tipo**: Juego 3D en tercera persona
- **Objetivo**: Juego de patinete con mecánicas de exploración urbana y playera, recolección de almejas y evasión de enemigos.

## Estructura del Proyecto

### Carpetas Principales

```
Assets/
├── 3D_MODELS/          # Modelos 3D (patinete, edificios, árboles, almejas, etc.)
├── Animations/         # Animaciones del personaje
├── AudioMixer/         # Mezclas de audio
├── Materials/          # Materiales (asfalto, arena, metal, etc.)
├── Prefabs/           # Prefabs organizados por categoría
│   ├── Water/         # Prefab de agua
│   ├── Enemies/       # Enemigos (coches patrulleros)
│   ├── Players/       # Jugador
│   ├── Objets/        # Objetos interactivos (almejas, conos)
│   └── ...
├── Resources/         # Recursos (audio, fuentes, imágenes)
├── Scenes/            # Escenas del juego
│   ├── Main.unity
│   ├── Negociacion.unity
│   ├── Patinete.unity
│   └── Playa.unity    # Escena de la playa (enfoque principal)
├── Scripts/           # Scripts C# organizados por módulos
│   ├── Cams/          # Controladores de cámara
│   ├── Canvas/        # UI
│   ├── Controls/      # Input del jugador
│   ├── Game/          # Gestores del juego (tiempo, marea, menú)
│   ├── Generators/    # Generadores procedurales
│   ├── NPCs/          # IA de enemigos y NPCs
│   ├── Player/        # Controladores del jugador
│   └── ...
└── Textures/          # Texturas varias
```

### Escenas Disponibles

1. **Main.unity** - Posiblemente menú principal o escena de inicio.
2. **Negociacion.unity** - Escena de negociación (contexto urbano).
3. **Patinete.unity** - Escena principal de patinete en entorno urbano.
4. **Playa.unity** - **Escena de la playa** (enfoque de este análisis).

## Escena de la Playa (Playa.unity)

### Mecánicas Principales

#### 1. Jugador (`PlayerControllerPlaya.cs`)
- **Movimiento**: Correr, caminar, saltar, rotación suave.
- **Escarbado**: Modo de excavación para buscar almejas (reduce velocidad).
- **Física**: Gravedad personalizada, detección de suelo, caídas desde altura.
- **Colisiones**:
  - **Agua**: Detección de entrada/salida (efectos pendientes).
  - **Enemigos**: Colisión con coches patrulleros → knockback y animación de muerte.
- **Animaciones**: Integración con Animator para movimientos, salto, landing y muerte.
- **Audio**: Efectos de pasos (correr/caminar), excavación, aterrizaje, choque.

#### 2. Cámara (`CamControllerPlaya.cs`)
- **Seguimiento**: Tercera persona con posición relativa ajustable (distancia, altura, ángulo).
- **Rotación fija**: Mantiene ángulo inicial constantemente.
- **Suavizado**: Movimiento suave con `SmoothDamp`.

#### 3. Enemigos (`CarPatrolling.cs`)
- **Patrullaje**: IA que recorre waypoints.
- **Detección**: Cambia a estado de alerta al detectar al jugador.
- **Interacción**: Al colisionar, activa knockback en el jugador y reinicia patrullaje.

#### 4. Almejas (Recolectables)
- **Generación procedural** (`GeneradorAmeixas.cs`):
  - Spawnea almejas enterradas en la arena.
  - Filtra por textura del terreno (`Sand_TerrainLayer`).
  - Control de densidad, profundidad y distancia a bordes.
- **Recolección** (`AmeixaController.cs`):
  - Al tocar al jugador, suma al contador global (`GameManager`).
  - Reproduce sonido de recolección y destruye el objeto.

#### 5. Agua y Marea (`TideSimulator.cs`)
- **Simulación de mareas**: Nivel del agua sube y baja cíclicamente.
- **Configuración**: Alturas mínima/máxima, duración del ciclo (12.42h).
- **Integración con tiempo**: Usa `TimeManager` para sincronizar con el ciclo día/noche.

#### 6. Ciclo Día/Noche
- **TimeManager.cs**: Gestor de tiempo interno del juego.
- **SunController.cs**: Controla la posición y rotación del sol/luz direccional.
- **Efectos**: Luz nocturna, cambios ambientales.

#### 7. Generación Procedural
- **GeneradorAmeixas.cs**: Almejas en la arena.
- **GeneradorConos.cs**: Conos de tráfico (probablemente en escena urbana).
- **GeneradorEdificios.cs**: Edificios alrededor del jugador.
- **GeneradorPlanos.cs**: Terrenos o planos base.

### Elementos Ambientales

- **Agua**: Prefab con shader simple (posiblemente de `IgniteCoders/Simple Water Shader`).
- **Terreno**: Texturas de arena, capas para spawn de almejas.
- **Cielo**: Skybox y iluminación dinámica.
- **NavMesh**: Superficie de navegación para enemigos (`NavMesh-NavMesh Surface.asset`).

### Sistemas de Juego

- **GameManager.cs**: Gestor global del estado del juego, contador de almejas.
- **GameMenu.cs** y **MenuNavigation.cs**: Menús y navegación UI.
- **CountAmeixas.cs**: UI que muestra cantidad de almejas recolectadas.

## Scripts Clave (Resumen)

| Script | Módulo | Descripción |
|--------|--------|-------------|
| `PlayerControllerPlaya.cs` | Player | Control principal del jugador en la playa |
| `CamControllerPlaya.cs` | Cams | Cámara en tercera persona para playa |
| `TideSimulator.cs` | Game | Simula mareas basadas en tiempo |
| `AmeixaController.cs` | NPCs | Lógica de recolección de almejas |
| `GeneradorAmeixas.cs` | Generators | Genera almejas en el terreno de arena |
| `CarPatrolling.cs` | NPCs | IA de patrullaje para coches enemigos |
| `TimeManager.cs` | Game | Gestor de tiempo interno (ciclo día/noche) |
| `SunController.cs` | Game | Controla el movimiento del sol |
| `GameManager.cs` | Game | Gestor global del juego (vidas, puntuación, almejas) |
| `PlayerInput.cs` | Controls | Procesa input del teclado/mando |

## Assets y Prefabs Destacados

### Modelos 3D
- `patinete_OK.fbx` - Modelo del patinete (varias versiones).
- `arbol.fbx` - Árboles para decoración.
- `casa_*.fbx` - Edificios (chalet, casa baja, casa vieja, fábrica).
- `bolsa_almejas.fbx` - Bolsa de almejas (posiblemente objeto).
- `farola.fbx` - Farolas de iluminación.
- `cono.fbx` - Conos de tráfico.

### Prefabs
- `Water.prefab` - Agua con posible shader de olas.
- Prefabs de enemigos, almejas, edificios, partículas.

### Materiales
- `asfalto.mat`, `Sand_TerrainLayer` - Texturas de terreno.
- Materiales metálicos, de goma, colores para patinete.

### Audio
- Efectos: pasos, excavación, choque, recolección de monedas (usado para almejas).

## Estado Actual (Según README.md)

### Tareas de la Playa

1. **Estado de alerta** ⌛ - Pendiente.
2. **Vigilancia (visión enemigo)** ✅ - Completado (implementado en `CarPatrolling.cs` y `DecalCollision.cs`).
3. **Caídas altura** - Pendiente (parcialmente implementado en `PlayerControllerPlaya.cs` con detección de caídas).
4. **Almejas. Movimiento** - Pendiente (las almejas están estáticas, falta movimiento/animación).
5. **Cielo - luz nocturna** - Pendiente (parcialmente implementado con `SunController.cs` y `TimeManager.cs`).
6. **Escalar paredes** - Pendiente.

### Otros Aspectos
- **Knockback** implementado pero con mejoras pendientes (calcular altura de terreno al caer).
- **Efectos de agua** detectados pero sin lógica completa.
- **Game over** pendiente tras colisión con enemigo.

## Conclusión y Observaciones

### Fortalezas
- Arquitectura modular bien organizada (scripts por responsabilidad).
- Generación procedural funcional para almejas y otros objetos.
- Sistema de tiempo y marea integrado, añade profundidad al ambiente.
- Física de movimiento y knockback implementada con animaciones.

### Áreas de Mejora
1. **Completar tareas pendientes** del README, especialmente movimiento de almejas y efectos de agua.
2. **Pulir IA enemiga**: Mejorar transiciones entre estados, añadir visión cónica.
3. **Optimización**: Los generadores procedurales podrían usar pooling (`ObjectPool.cs` disponible).
4. **UI/UX**: Añadir más feedback visual (barra de tiempo, contador de almejas más visible).
5. **Sonido**: Implementar más variedad de efectos y música ambiental.

### Posibles Extensiones
- **Misiones**: Recolectar X almejas antes de que suba la marea.
- **Más enemigos**: Aves, cangrejos, etc.
- **Herramientas**: Pala para excavar más rápido, detector de almejas.
- **Multiplayer**: Cooperativo para recolectar almejas juntos.

---

**Resumen elaborado el**: 19/12/2025  
**Base de análisis**: Exploración de archivos del proyecto Unity en `j:/programacion/FP_a__Distancia/_JUEGOS/PROYECTO/proyecto/PROJECT/prototipos/patinete`
