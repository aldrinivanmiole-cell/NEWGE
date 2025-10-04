# Godot 4 EduGame - Project Overview

## Project Summary

This is a complete Godot 4 implementation of a 2D turn-based educational game for elementary students. The game connects to an existing backend API at `https://capstoneproject-jq2h.onrender.com` to provide classroom-integrated learning experiences.

## Current Status

### ✅ Completed Features

1. **Project Setup**
   - Godot 4.2+ compatible project configuration
   - Mobile-first portrait layout (1080x1920)
   - Android export presets configured
   - Proper autoload singletons setup

2. **Authentication System**
   - Login screen with email/password validation
   - Registration screen with full form validation
   - Password confirmation checking
   - Email format validation using RegEx
   - Error handling and display
   - Loading states with indicators

3. **Backend Integration**
   - Complete API manager with HTTPRequest
   - All student endpoints implemented:
     - Student login (`POST /api/student/login`)
     - Student registration (`POST /api/student/simple-register`)
     - Get subjects (`POST /api/student/subjects`)
     - Get assignments (`POST /api/student/assignments`)
     - Submit answers (`POST /api/submit/{id}`)
     - Get leaderboard (`GET /api/leaderboard/{code}`)
     - Join class (`POST /api/student/join-class`)

4. **Session Management**
   - GameManager singleton for state management
   - DataManager singleton for local persistence
   - Session data saved using ConfigFile
   - Device ID generation and storage
   - Auto-login check on startup

5. **UI System**
   - Custom theme matching design requirements:
     - Blue background (#4A90E2)
     - Wooden frame brown (#8B4513)
     - Green accents (#228B22)
   - Large touch-friendly buttons (80-100px height)
   - Clear readable fonts (24-28pt)
   - Mobile-optimized layouts
   - ScrollContainer for smaller screens

6. **Documentation**
   - Comprehensive README.md
   - Detailed SETUP.md guide
   - Complete API_INTEGRATION.md
   - Code comments throughout

### 🚧 Framework Ready (Not Yet Implemented)

1. **Subject Management**
   - API endpoints integrated
   - Framework ready for subject loading
   - Dynamic subject buttons

2. **Assignment System**
   - API endpoints integrated
   - Framework ready for assignment loading
   - Question display system

3. **Gameplay Mechanics**
   - Turn-based game structure planned
   - Question/answer system framework
   - Progress tracking ready

4. **Additional Features**
   - Leaderboard display
   - Class joining UI
   - Settings screen
   - Offline mode with caching

## Architecture

### Manager Pattern (Singletons)

The project uses three autoloaded manager scripts:

#### 1. APIManager.gd
**Purpose**: Centralized HTTP request handling

**Key Features**:
- Single HTTPRequest node shared by all requests
- Signal-based response handling
- Automatic JSON parsing
- Error handling and logging
- All backend endpoints as simple function calls

**Usage Example**:
```gdscript
APIManager.student_login(email, password, device_id)
APIManager.request_completed.connect(_on_api_response)
```

#### 2. GameManager.gd
**Purpose**: Game state and navigation

**Key Features**:
- Student session data storage
- Scene transition management
- Login state checking
- Points tracking
- Device ID generation

**Usage Example**:
```gdscript
GameManager.start_session(student_data)
GameManager.goto_main_menu()
var logged_in = GameManager.is_logged_in()
```

#### 3. DataManager.gd
**Purpose**: Local data persistence

**Key Features**:
- ConfigFile-based storage
- Session save/load
- Settings management
- Data caching with expiration
- Device ID storage

**Usage Example**:
```gdscript
DataManager.save_session(session_data)
var session = DataManager.load_session()
DataManager.cache_subjects(subjects)
```

### Scene Structure

```
scenes/
├── auth/
│   ├── LoginScreen.tscn          # Entry point
│   └── RegisterScreen.tscn       # New user signup
├── main/
│   └── MainMenu.tscn             # Main navigation hub
├── ui/                            # Reusable UI components (future)
└── game/                          # Gameplay scenes (future)
```

### Script Structure

```
scripts/
├── auth/
│   ├── LoginController.gd        # Login logic
│   └── RegisterController.gd     # Registration logic
├── main/
│   └── MainMenuController.gd     # Main menu logic
├── managers/                      # Autoloaded singletons
│   ├── APIManager.gd
│   ├── GameManager.gd
│   └── DataManager.gd
└── ui/                            # UI components (future)
```

## API Integration

### Authentication Flow

1. User enters credentials on LoginScreen
2. LoginController validates input locally
3. APIManager.student_login() called
4. Backend responds with student data
5. GameManager.start_session() saves data
6. DataManager persists session locally
7. Navigate to MainMenu

### Session Persistence

- On app start: DataManager loads last session
- If valid session exists: Auto-restore student data
- User stays logged in between app launches
- Logout clears all session data

### Offline Support

- Sessions cached locally
- Subjects and assignments can be cached
- Cache expires after 1 hour
- Graceful fallback when offline

## Mobile Optimization

### Portrait Mode
- Resolution: 1080x1920
- Orientation locked to portrait
- Stretch mode: viewport with aspect keep

### Touch Controls
- Large button sizes (80-100px minimum)
- Adequate spacing between elements
- Touch emulation enabled for testing
- ScrollContainer for long content

### Performance
- Mobile renderer selected
- Texture compression for mobile
- Efficient resource management
- Minimal draw calls

## Design System

### Colors

| Element | Color | Hex Code |
|---------|-------|----------|
| Background | Blue | #4A90E2 |
| Wooden Frame | Brown | #8B4513 |
| Green Accent | Green | #228B22 |
| Button Text | White | #FFFFFF |
| Input Text | Dark Gray | #333333 |

### Typography

- **Headings**: 28pt, bold
- **Body Text**: 24pt, regular
- **Button Text**: 28pt, bold
- **Input Placeholder**: 24pt, gray

### Component Sizes

- **Large Buttons**: 100px height
- **Medium Buttons**: 80px height
- **Input Fields**: 80px height
- **Minimum Touch Target**: 80x80px

## File Organization

### Assets

```
assets/
├── textures/          # Images and sprites (to be added)
├── fonts/             # Custom fonts (to be added)
├── audio/             # Sound effects and music (to be added)
└── themes/
    └── edugame_theme.tres    # UI theme with styleboxes
```

### Configuration Files

- `project.godot` - Main project configuration
- `export_presets.cfg` - Android/iOS export settings
- `.gitignore` - Git ignore patterns
- `icon.svg` - App icon

## Development Guidelines

### Code Style

1. **Naming Conventions**
   - PascalCase for classes/scenes
   - snake_case for variables/functions
   - UPPER_CASE for constants
   - Private functions: `_function_name`

2. **Comments**
   - Document all public functions
   - Explain complex logic
   - Use `# TODO:` for planned features
   - Add header comments to files

3. **Signals**
   - Use signals for loose coupling
   - Document signal parameters
   - Disconnect when not needed

4. **Error Handling**
   - Check for null before accessing nodes
   - Validate user input
   - Log errors to console
   - Show user-friendly error messages

### Adding New Screens

1. Create scene in appropriate folder
2. Attach controller script
3. Use theme: `theme = ExtResource("edugame_theme")`
4. Connect to GameManager for navigation
5. Handle session checks if needed

### Making API Calls

1. Call appropriate APIManager function
2. Connect to `request_completed` signal
3. Show loading indicator
4. Handle both success and error cases
5. Update UI based on response
6. Cache data if appropriate

### Testing Locally

1. Run in Godot editor (F5)
2. Check console for errors
3. Test all input validation
4. Test API calls with mock data
5. Test offline scenarios
6. Test on actual device when possible

## Next Development Steps

### Phase 1: Subject Management
1. Create SubjectsScreen.tscn
2. Load subjects from API on menu click
3. Display subject buttons dynamically
4. Navigate to assignments on click

### Phase 2: Assignment System
1. Create AssignmentScreen.tscn
2. Load assignments for selected subject
3. Display assignment list
4. Allow selection to start game

### Phase 3: Gameplay
1. Create GameplayScreen.tscn
2. Display questions one at a time
3. Implement turn-based mechanics
4. Handle answer submission
5. Show results and score

### Phase 4: Additional Features
1. Join class UI
2. Leaderboard display
3. Settings screen
4. Profile screen
5. Achievement system

### Phase 5: Polish
1. Add animations
2. Sound effects
3. Better error dialogs
4. Loading animations
5. Tutorial/help system

## Known Limitations

1. **No Assets**: Project uses default Godot UI elements
2. **No Gameplay**: Core gameplay not yet implemented
3. **Simple Theme**: Basic StyleBox-based theme
4. **No Animations**: Static UI elements
5. **No Audio**: No sound effects or music
6. **Mock Confirmations**: Logout confirmation always returns true

## Testing Checklist

### Authentication Testing
- [ ] Valid login succeeds
- [ ] Invalid credentials show error
- [ ] Empty fields show validation error
- [ ] Invalid email format rejected
- [ ] Password confirmation works
- [ ] Session persists after restart
- [ ] Logout clears session

### API Testing
- [ ] Login endpoint responds correctly
- [ ] Registration endpoint responds correctly
- [ ] Network errors handled gracefully
- [ ] Timeout handled properly
- [ ] Response parsing works
- [ ] Error messages displayed

### UI Testing
- [ ] Theme applied correctly
- [ ] Buttons are touch-friendly
- [ ] Text is readable
- [ ] Layout works on different screen sizes
- [ ] ScrollContainer works properly
- [ ] Loading indicators show during API calls

### Mobile Testing
- [ ] Runs on Android device
- [ ] Portrait mode enforced
- [ ] Touch input works
- [ ] Performance is acceptable
- [ ] Screen sizes handled properly
- [ ] Soft keyboard doesn't break layout

## Performance Considerations

1. **Single HTTPRequest**: Shared across all API calls
2. **Caching**: Reduces redundant API calls
3. **Mobile Renderer**: Optimized for mobile devices
4. **Minimal Autoloads**: Only 3 singleton scripts
5. **Signal Cleanup**: Disconnect when not needed

## Security Notes

1. **Device ID**: Unique per device, not personally identifiable
2. **Password Handling**: Sent over HTTPS, not stored locally
3. **Session Data**: Stored in ConfigFile (local only)
4. **API Communication**: HTTPS only
5. **No Sensitive Data**: No credit cards or personal info stored

## Troubleshooting

### Common Issues

**Issue**: API requests fail
- Check internet connection
- Verify API URL is correct
- Check console for detailed errors
- Test API with curl/Postman

**Issue**: Scenes won't load
- Check scene file paths are correct
- Verify scenes are saved properly
- Check for script errors in console

**Issue**: Theme not applied
- Verify theme resource path
- Check theme is loaded in scene
- Ensure StyleBoxes are configured

**Issue**: Can't export to Android
- Install Android build template
- Configure Android SDK path
- Check export preset settings
- Verify permissions are set

## Support and Resources

- **Godot Docs**: https://docs.godotengine.org
- **GDScript Guide**: https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/
- **API Documentation**: See API_INTEGRATION.md
- **Setup Guide**: See SETUP.md

## Version History

### v1.0 (Current)
- Initial Godot 4 project structure
- Authentication system complete
- API integration complete
- Manager singletons implemented
- UI theme created
- Documentation added
- Android export preset configured

### Planned for v1.1
- Subject management UI
- Assignment loading
- Basic gameplay implementation
- Leaderboard display

### Planned for v2.0
- Full gameplay system
- Animations and polish
- Audio system
- Achievement system
- Tutorial mode

---

**Last Updated**: 2025-01-04
**Godot Version**: 4.2+
**Target Platform**: Android (Primary), iOS (Secondary)
**Backend API**: https://capstoneproject-jq2h.onrender.com
