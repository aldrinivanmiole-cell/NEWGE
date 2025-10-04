# EduGame - Elementary Learning Game

A 2D turn-based educational game built with Godot 4 for elementary students. This game connects to a backend classroom management system to provide interactive learning experiences.

## Features

- **Student Authentication**: Login and registration with email/password
- **API Integration**: Connects to backend API at `https://capstoneproject-jq2h.onrender.com`
- **Mobile-First Design**: Optimized for portrait mode on mobile devices (1080x1920)
- **Educational Content**: Support for assignments, subjects, and classroom integration
- **Persistent Data**: Local session management and offline data caching
- **Clean UI**: Wooden frame aesthetic with blue background, suitable for elementary students

## Project Structure

```
godot-project/
├── scenes/
│   ├── auth/
│   │   ├── LoginScreen.tscn      # Student login screen
│   │   └── RegisterScreen.tscn   # Student registration screen
│   ├── ui/                        # UI components
│   ├── game/                      # Gameplay scenes
│   └── main/
│       └── MainMenu.tscn          # Main menu screen
├── scripts/
│   ├── auth/
│   │   ├── LoginController.gd    # Login logic and validation
│   │   └── RegisterController.gd # Registration logic and validation
│   ├── managers/
│   │   ├── APIManager.gd         # HTTP API requests (Singleton)
│   │   ├── GameManager.gd        # Game state management (Singleton)
│   │   └── DataManager.gd        # Local data persistence (Singleton)
│   └── ui/                        # UI scripts
├── assets/
│   ├── textures/                  # Image assets
│   ├── fonts/                     # Font files
│   ├── audio/                     # Sound effects and music
│   └── themes/
│       └── edugame_theme.tres    # UI theme configuration
└── project.godot                  # Godot project configuration
```

## Getting Started

### Prerequisites

- Godot Engine 4.2 or later
- Internet connection for API access

### Installation

1. Clone the repository
2. Open Godot Engine
3. Click "Import" and navigate to the `godot-project` folder
4. Select the `project.godot` file
5. Click "Import & Edit"

### Running the Game

1. Press F5 or click the "Play" button in Godot
2. The game will start at the Login screen
3. You can register a new account or login with existing credentials

## Backend API

The game connects to the backend API at: `https://capstoneproject-jq2h.onrender.com`

### Available Endpoints

- `POST /api/student/login` - Student login
- `POST /api/student/simple-register` - Student registration (no class enrollment)
- `POST /api/student/subjects` - Get available subjects
- `POST /api/student/assignments` - Get assignments for a subject
- `POST /api/submit/{id}` - Submit assignment answers
- `GET /api/leaderboard/{code}` - Get class leaderboard
- `POST /api/student/join-class` - Join a class with code

See [API_INTEGRATION.md](API_INTEGRATION.md) for detailed API documentation.

## Configuration

### Display Settings

The game is configured for mobile portrait mode:
- Resolution: 1080x1920
- Orientation: Portrait
- Stretch Mode: Viewport with aspect ratio keep

### Autoload Singletons

Three manager scripts are autoloaded:
- **APIManager**: Handles all HTTP requests
- **GameManager**: Manages game state and navigation
- **DataManager**: Handles local data storage

## Development

### Adding New Scenes

1. Create scene in appropriate folder under `scenes/`
2. Attach a controller script if needed
3. Use the theme: `res://assets/themes/edugame_theme.tres`

### Making API Calls

Use the APIManager singleton:

```gdscript
# Example: Student login
APIManager.student_login(email, password, device_id)
APIManager.request_completed.connect(_on_api_response)

func _on_api_response(success: bool, data: Dictionary, error_message: String):
    if success:
        print("Success: ", data)
    else:
        print("Error: ", error_message)
```

### Local Data Storage

Use the DataManager singleton:

```gdscript
# Save data
DataManager.save_setting("key", value)

# Load data
var value = DataManager.load_setting("key", default_value)
```

## Export Settings

### Android

1. Install Android Build Template
2. Configure export settings in Project -> Export
3. Set minimum SDK to API 21 (Android 5.0)
4. Enable Internet permission

## License

This project is part of a capstone educational system.

## Support

For issues or questions, please refer to the project documentation or contact the development team.
