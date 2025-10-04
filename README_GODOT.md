# NEWGE - Educational Game Project

This repository contains educational game projects for elementary students, connecting to a classroom management backend.

## Projects

### 1. Godot 4 Project (NEW - Recommended)

Location: `/godot-project`

A complete Godot 4 implementation of the educational game with:
- Mobile-first design (Portrait mode 1080x1920)
- Student authentication (login/register)
- Backend API integration
- Clean UI with wooden frame aesthetic
- Turn-based educational gameplay foundation

**See [godot-project/README.md](godot-project/README.md) for details.**

### 2. Unity Project (Legacy)

Location: `/Assets`, `/ProjectSettings`, `/Packages`

The original Unity implementation with classroom integration features.

## Quick Start (Godot)

1. Install Godot Engine 4.2 or later
2. Open Godot and import the `godot-project` folder
3. Press F5 to run
4. Test login/register functionality

**Detailed setup instructions: [godot-project/SETUP.md](godot-project/SETUP.md)**

## Backend API

Both projects connect to: `https://capstoneproject-jq2h.onrender.com`

API documentation: [godot-project/API_INTEGRATION.md](godot-project/API_INTEGRATION.md)

## Features

- ✅ Student authentication (login/register)
- ✅ Backend API integration
- ✅ Mobile-optimized UI
- ✅ Session management
- ✅ Local data caching
- 🚧 Subject management (framework ready)
- 🚧 Assignment loading (framework ready)
- 🚧 Turn-based gameplay (in development)
- 🚧 Leaderboards (in development)

## Project Structure

```
NEWGE/
├── godot-project/          # Godot 4 implementation (NEW)
│   ├── scenes/            # Game scenes
│   ├── scripts/           # GDScript code
│   ├── assets/            # Resources and themes
│   ├── project.godot      # Godot project file
│   ├── README.md          # Godot project documentation
│   ├── SETUP.md           # Setup instructions
│   └── API_INTEGRATION.md # API documentation
├── Assets/                # Unity project assets (Legacy)
├── ProjectSettings/       # Unity project settings (Legacy)
├── Packages/              # Unity packages (Legacy)
└── DYNAMIC_SUBJECTS_API.md # API requirements
```

## Development

### Godot Project (Recommended for new development)

```bash
cd godot-project
# Open in Godot Engine 4.2+
```

See [godot-project/SETUP.md](godot-project/SETUP.md) for complete setup guide.

### Unity Project (Maintenance mode)

Open with Unity 2021.3 or later. See Unity assets for existing features.

## Documentation

- **Godot Project**: [godot-project/README.md](godot-project/README.md)
- **Setup Guide**: [godot-project/SETUP.md](godot-project/SETUP.md)
- **API Integration**: [godot-project/API_INTEGRATION.md](godot-project/API_INTEGRATION.md)
- **API Requirements**: [DYNAMIC_SUBJECTS_API.md](DYNAMIC_SUBJECTS_API.md)

## Technology Stack

### Godot Project
- **Engine**: Godot 4.2+
- **Language**: GDScript
- **Platform**: Mobile (Android/iOS)
- **Backend**: FastAPI + Flask (existing)

### Unity Project
- **Engine**: Unity 2021.3+
- **Language**: C#
- **Platform**: Mobile (Android)

## License

Educational project for capstone system.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## Support

For issues or questions:
- Check documentation in `godot-project/`
- Review API documentation
- Contact development team
