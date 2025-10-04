# Contributing to EduGame

Thank you for your interest in contributing to the EduGame project! This guide will help you get started.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Submitting Changes](#submitting-changes)
- [Project Structure](#project-structure)

## Code of Conduct

This is an educational project for elementary students. Please:
- Keep code and comments appropriate
- Be respectful in discussions
- Help others learn
- Focus on educational value

## Getting Started

### 1. Set Up Your Environment

See [SETUP.md](SETUP.md) for detailed instructions. Quick version:

```bash
# Clone the repository
git clone https://github.com/aldrinivanmiole-cell/NEWGE.git
cd NEWGE/godot-project

# Open in Godot 4.2+
# Press F5 to run
```

### 2. Create a Branch

Always create a new branch for your work:

```bash
git checkout -b feature/my-new-feature
# or
git checkout -b fix/bug-description
```

Branch naming conventions:
- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation changes
- `refactor/` - Code refactoring
- `test/` - Test additions

### 3. Understand the Architecture

Read [PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md) to understand:
- Manager pattern (singletons)
- Scene structure
- API integration
- Design system

## Development Workflow

### 1. Make Changes

```bash
# Edit files in Godot editor
# Test frequently with F5
# Check console for errors
```

### 2. Test Your Changes

- Run the game (F5)
- Test all affected features
- Check console for warnings/errors
- Test on mobile if possible

### 3. Commit Changes

```bash
git add .
git commit -m "Add descriptive message"
```

Commit message format:
```
Add login validation for empty fields

- Check if email is empty before login
- Show error message for empty password
- Add unit tests for validation
```

### 4. Push and Create PR

```bash
git push origin feature/my-new-feature
# Then create Pull Request on GitHub
```

## Coding Standards

### GDScript Style

#### Naming Conventions

```gdscript
# Variables and functions: snake_case
var student_name: String
func get_student_data():

# Constants: UPPER_CASE
const MAX_STUDENTS = 100
const API_URL = "https://example.com"

# Classes and scenes: PascalCase
class_name StudentData
# LoginScreen.tscn

# Private functions: _snake_case
func _on_button_pressed():
func _validate_input():

# Signals: past tense
signal login_completed
signal data_loaded
```

#### Type Hints

Always use type hints:

```gdscript
# Good
var name: String = "Student"
var age: int = 10
func calculate_score(points: int) -> int:

# Avoid
var name = "Student"
var age = 10
func calculate_score(points):
```

#### Documentation

Document all public functions:

```gdscript
# Good
## Validates email format using regex
## Returns true if email is valid, false otherwise
func validate_email(email: String) -> bool:
    var regex = RegEx.new()
    regex.compile("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")
    return regex.search(email) != null

# Also good (single line for simple functions)
## Returns the student's full name
func get_full_name() -> String:
    return first_name + " " + last_name
```

#### Error Handling

Always check for null and handle errors:

```gdscript
# Good
func update_label():
    if status_label:
        status_label.text = "Updated"
    else:
        push_error("status_label is null")

func load_data():
    var data = DataManager.load_session()
    if data.is_empty():
        print("No saved session found")
        return
    
    # Use data...
```

#### Signal Usage

```gdscript
# Define signals at top of script
signal request_completed(success: bool, data: Dictionary)

# Emit with proper parameters
request_completed.emit(true, {"id": 123})

# Connect in _ready()
func _ready():
    APIManager.request_completed.connect(_on_api_response)

# Disconnect when not needed
func _exit_tree():
    APIManager.request_completed.disconnect(_on_api_response)
```

### Scene Organization

#### Node Hierarchy

```
Control (Root)
├── BackgroundPanel (Panel)
│   └── ContentContainer (VBoxContainer)
│       ├── Title (Label)
│       ├── Input1 (LineEdit)
│       ├── Input2 (LineEdit)
│       └── Button (Button)
└── LoadingPanel (Panel)
    └── LoadingLabel (Label)
```

#### Node Names

- Use PascalCase for node names
- Be descriptive: `EmailInput` not `LineEdit1`
- Group related nodes: `ButtonContainer`, `InputGroup`

#### Custom Minimum Sizes

Always set minimum sizes for touch-friendly UI:

```gdscript
# In scene or code
custom_minimum_size = Vector2(0, 80)  # Buttons
custom_minimum_size = Vector2(0, 80)  # Input fields
```

### File Organization

#### Script Headers

```gdscript
extends Control

# LoginController - Handles login form validation and API integration
# 
# This controller manages the login screen, validates user input,
# communicates with the APIManager, and handles navigation on success.

# Node references
@onready var email_input: LineEdit = $VBox/EmailInput
@onready var password_input: LineEdit = $VBox/PasswordInput

# State variables
var is_loading: bool = false

# Constants
const MIN_PASSWORD_LENGTH = 6
```

## Testing Guidelines

### Manual Testing Checklist

Before submitting:

- [ ] Run project (F5) - no errors
- [ ] Test happy path (normal usage)
- [ ] Test error cases (invalid input)
- [ ] Test edge cases (empty strings, special chars)
- [ ] Check console for warnings
- [ ] Test on mobile if possible

### Test User Inputs

Always test with:
- ✅ Valid data
- ❌ Empty fields
- ❌ Invalid format (email)
- ❌ Special characters
- ❌ Very long strings
- ❌ Mismatched passwords

### API Testing

Test API calls with:
- ✅ Valid credentials
- ❌ Invalid credentials
- ❌ Network timeout
- ❌ Server error (500)
- ❌ Invalid JSON response

## Submitting Changes

### Before Submitting

1. **Test thoroughly**
   - All features work
   - No console errors
   - No visual glitches

2. **Review your code**
   - Remove debug prints
   - Check formatting
   - Verify type hints
   - Update documentation

3. **Update documentation**
   - Add comments
   - Update README if needed
   - Document new features

### Pull Request Guidelines

#### PR Title Format

```
[Type] Brief description

Examples:
[Feature] Add subject selection screen
[Fix] Fix login validation bug
[Docs] Update API integration guide
[Refactor] Simplify APIManager code
```

#### PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] New feature
- [ ] Bug fix
- [ ] Documentation update
- [ ] Refactoring
- [ ] Performance improvement

## Changes Made
- Added subject selection screen
- Updated APIManager to cache subjects
- Added loading indicator

## Testing
- [x] Tested locally
- [x] No console errors
- [x] Validated input handling
- [ ] Tested on mobile

## Screenshots
(If UI changes)

## Related Issues
Closes #123
```

### Code Review Process

1. Maintainer reviews code
2. Provides feedback if needed
3. You make changes
4. Maintainer approves
5. Code is merged

## Project Structure

### Adding New Features

#### 1. New Scene

```bash
# Create scene file
scenes/features/NewFeature.tscn

# Create controller
scripts/features/NewFeatureController.gd

# Attach script to scene root
# Add to theme if needed
```

#### 2. New Manager Function

```gdscript
# In appropriate manager
func new_feature_function(param: Type) -> ReturnType:
    ## Documentation
    # Implementation
    pass
```

#### 3. New API Endpoint

```gdscript
# In APIManager.gd
func new_api_call(param: Type):
    var data = {
        "param": param
    }
    make_request("/api/new-endpoint", HTTPClient.METHOD_POST, data)
```

### Directory Structure

```
godot-project/
├── scenes/
│   ├── auth/              # Authentication screens
│   ├── main/              # Main menu and navigation
│   ├── game/              # Gameplay screens
│   └── ui/                # Reusable UI components
├── scripts/
│   ├── auth/              # Auth controllers
│   ├── main/              # Main menu logic
│   ├── game/              # Gameplay logic
│   ├── managers/          # Singleton managers
│   └── ui/                # UI component scripts
├── assets/
│   ├── textures/          # Images
│   ├── fonts/             # Fonts
│   ├── audio/             # Sounds
│   └── themes/            # UI themes
└── documentation/         # Additional docs (optional)
```

## Common Tasks

### Adding a New Screen

1. Create scene: `scenes/category/ScreenName.tscn`
2. Create controller: `scripts/category/ScreenNameController.gd`
3. Add theme: `theme = preload("res://assets/themes/edugame_theme.tres")`
4. Add navigation in GameManager
5. Update documentation

### Adding an API Endpoint

1. Add function to APIManager.gd
2. Document parameters and response
3. Test with curl/Postman
4. Add error handling
5. Update API_INTEGRATION.md

### Modifying UI Theme

1. Open `assets/themes/edugame_theme.tres`
2. Modify StyleBoxes as needed
3. Test on all screens
4. Document changes

## Getting Help

- **Code Questions**: Check PROJECT_OVERVIEW.md
- **Setup Issues**: Read SETUP.md
- **API Issues**: See API_INTEGRATION.md
- **Godot Help**: https://docs.godotengine.org

## Recognition

Contributors will be recognized in the project README!

## Questions?

Feel free to:
- Open an issue for questions
- Ask for clarification on PRs
- Suggest improvements to this guide

---

Thank you for contributing to EduGame! Your work helps students learn! 🎓
