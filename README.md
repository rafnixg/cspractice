# PracCentral — CS2 Practice Plugin

Un plugin monolítico y modular para Counter-Strike 2 (Source 2 Engine) que unifica metodologías de entrenamiento táctico y mecánico bajo una sola instancia de ejecución.

## Características

- **Arquitectura Modular**: Diseño basado en módulos desacoplados con estado gestionado por FSM (Finite State Machine).
- **Grenade Module**: Captura y replicación de lanzamientos de granadas con precisión determinística.
- **Prefire Module**: Generador de objetivos automáticos basado en datos JSON por mapa.
- **Aim Module**: Reglas de entrenamiento (headshot-only, bullet economy, etc.).
- **Thread-Safe**: Todas las operaciones en el game loop se sincronizan a través de `Server.NextFrame()`.
- **.NET 8.0 Compliant**: Compatible con .NET 8 y Linux (sin dependencias Win32).

## Estructura del Proyecto

```
src/PracCentral/
├── Core/                   # Fachada, FSM, contratos
├── Modules/               # Módulos de dominio (Grenade, Prefire, Aim)
├── Services/             # Servicios transversales
│   ├── Threading/        # Dispatcher del hilo principal
│   ├── Storage/          # Persistencia JSON
│   ├── Validation/       # Sanitización de input
│   └── Engine/           # Bridges para ConVars y eventos
├── Adapters/             # Adaptadores para CounterStrikeSharp
├── Plugin/               # Plugin loader (punto de entrada)
├── Infrastructure/       # Logging y diagnósticos
├── Config/              # Constantes y valores por defecto
└── Models/              # DTOs y esquemas JSON

data/
└── prefire/             # Archivos de configuración de mapas
```

## Instalación

### Requisitos
- .NET 8.0 SDK
- CounterStrikeSharp 1.0.356+
- CS2 Dedicated Server con MetaMod:Source

### Build
```bash
cd src/PracCentral
dotnet build -c Release
```

El ensamblado compilado estará en `bin/Release/net8.0/PracCentral.dll`.

## Uso

### Comandos disponibles
- `!prac` — Abre el menú de PracCentral
- `!grenade` — Activa el módulo de Granadas
- `!prefire` — Activa el módulo de Prefire
- `!aim` — Activa el módulo de Aim
- `!idle` — Vuelve al modo Idle

### Configuración del plugin
En el primer arranque se crea automáticamente `config/praccentral.json` dentro del directorio del plugin con configuración base por modo.

Ejemplo:
```json
{
  "version": "1.0",
  "aim": {
    "headshotOnlyEnabled": true,
    "bulletEconomyEnabled": true
  },
  "prefire": {
    "kickBotsOnLoad": true,
    "kickBotsOnUnload": true,
    "dataDirectory": "data\\prefire"
  },
  "grenade": {
    "saveLastThrow": true
  }
}
```

También se crea `config/command-aliases.json` para mapear alias de comandos a comandos reales.

Ejemplo:
```json
{
  ".gr": ".grenade",
  ".pf": ".prefire",
  ".hs": ".aim"
}
```

### Ejemplo: Prefire Mode
1. Copia tu configuración de prefire a `data/prefire/de_mirage.json`
2. Ejecuta `!prefire` en el servidor
3. El módulo cargará los bots automáticamente

Estructura del archivo JSON:
```json
{
  "mapName": "de_mirage",
  "nodes": [
    {
      "id": 1,
      "vectorPos": {"x": -984.12, "y": -1420.55, "z": -165.21},
      "vectorAng": {"pitch": 0.0, "yaw": 90.0, "roll": 0.0},
      "weaponId": "weapon_ak47",
      "botDifficulty": 3
    }
  ]
}
```

## Arquitectura Técnica

### Ciclo de vida del módulo
1. **Load**: Se invoca al activar el módulo
2. **Unload**: Se invoca al desactivar o cambiar de módulo
3. **Dispose**: Limpieza explícita de recursos

### Transiciones de estado
```
Idle ← → Grenade
Idle ← → Prefire
Idle ← → Aim
```

Durante cada transición:
- Se llama a `Unload()` en el módulo activo
- Se restauran los ConVars almacenados
- Se limpian todas las suscripciones de eventos

### Thread Safety
Todas las modificaciones a las entidades del juego ocurren dentro del hilo principal:
```csharp
mainThreadDispatcher.Enqueue(() =>
{
    // Código seguro aquí (sincrónico con el game loop)
});
```

## Testing

```bash
dotnet test tests/PracCentral.Tests
```

Pruebas incluidas:
- Sanitización de input
- Roundtrip JSON (read/write)
- Transiciones de estado
- Limpieza de módulos

## Performance

- Overhead de hooks < 0.5ms/frame (target)
- Operaciones de I/O delegadas a async (no bloquean)
- GC administrado por .NET + manejo de memory leaks nativos

## Roadmap (6 Sprints)

- [x] Sprint 1: Bootstrap y FSM
- [x] Sprint 2: Persistencia y validación
- [ ] Sprint 3: Prefire MVP (full integration)
- [ ] Sprint 4: Aim MVP (damage hooks)
- [ ] Sprint 5: Grenade MVP (trajectory replay)
- [ ] Sprint 6: Hardening y profiling

## Referencias

- [CounterStrikeSharp API](https://github.com/roflmuffin/CounterStrikeSharp)
- [MatchZy Plugin](https://github.com/shobhit-pathak/MatchZy)
- [Source 2 Engine](https://www.valvesoftware.com/)

## Licencia

MIT License — Libremente distribuible y modificable.

## Contributing

Pull requests bienvenidos. Para cambios mayores, abre un issue primero.
