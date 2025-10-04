# Setup Guide - EduGame Development Environment

This guide will help you set up your development environment for the EduGame project.

## System Requirements

### Minimum Requirements
- **OS**: Windows 10/11, macOS 10.13+, or Linux (Ubuntu 18.04+)
- **RAM**: 4 GB minimum, 8 GB recommended
- **Storage**: 500 MB for Godot + project files
- **GPU**: OpenGL 3.3 / OpenGL ES 3.0 compatible

### Recommended Requirements
- **RAM**: 8 GB or more
- **Storage**: 2 GB or more (for exports and assets)
- **Internet**: Stable connection for API testing

## Installing Godot Engine

### Option 1: Official Website (Recommended)

1. Visit [godotengine.org](https://godotengine.org/download)
2. Download Godot 4.2 or later (Standard version)
3. Extract the downloaded archive
4. Run the Godot executable

### Option 2: Steam

1. Open Steam
2. Search for "Godot Engine"
3. Install the free Godot Engine application

### Option 3: Package Manager

**Linux (Ubuntu/Debian):**
```bash
# Add Godot PPA
sudo add-apt-repository ppa:godotengine/godot
sudo apt update
sudo apt install godot

# Or use Flatpak
flatpak install flathub org.godotengine.Godot
```

**macOS (Homebrew):**
```bash
brew install godot
```

**Windows (Chocolatey):**
```powershell
choco install godot
```

## Setting Up the Project

### 1. Clone the Repository

```bash
git clone https://github.com/aldrinivanmiole-cell/NEWGE.git
cd NEWGE/godot-project
```

### 2. Import Project in Godot

1. Launch Godot Engine
2. Click "Import" button
3. Navigate to the `godot-project` folder
4. Select the `project.godot` file
5. Click "Import & Edit"

### 3. Verify Project Setup

After importing, verify:
- [ ] Project opens without errors
- [ ] Autoload scripts are loaded (APIManager, GameManager, DataManager)
- [ ] Main scene is set to `res://scenes/auth/LoginScreen.tscn`
- [ ] Display settings are 1080x1920 portrait mode

## Development Tools

### Recommended Extensions/Plugins

1. **GDScript Toolkit** (for linting)
   ```bash
   pip install gdtoolkit
   ```

2. **VS Code with Godot Extension** (optional)
   - Install VS Code
   - Install "godot-tools" extension
   - Configure Godot to use VS Code as external editor

### Version Control Setup

```bash
# Initialize git if not already done
git init

# Add Godot-specific .gitignore
cat > .gitignore << EOF
.godot/
.import/
export.cfg
export_presets.cfg
*.translation
.mono/
data_*/
mono_crash.*.json
EOF
```

## Testing the API Connection

### 1. Check Internet Connection

Ensure you can access: `https://capstoneproject-jq2h.onrender.com`

### 2. Test Registration

1. Run the project (F5)
2. Click "REGISTER"
3. Fill in the form with test data:
   - First Name: Test
   - Last Name: Student
   - Email: test@example.com
   - Password: test123
   - Confirm Password: test123
4. Click "SUBMIT"
5. Check console for API response

### 3. Test Login

1. Go to Login screen
2. Enter credentials from registration
3. Click "LOGIN"
4. Should navigate to Main Menu on success

## Mobile Development Setup

### Android Export

#### Prerequisites
- Android SDK
- Java JDK 11 or 17
- Android Debug Bridge (ADB)

#### Steps

1. **Install Android Build Template**
   - In Godot: Project -> Install Android Build Template

2. **Configure Android SDK**
   - Editor -> Editor Settings -> Export -> Android
   - Set Android SDK path
   - Set Debug Keystore path

3. **Export Settings**
   - Project -> Export
   - Add Android export preset
   - Configure permissions:
     - ✓ Internet
     - ✓ Access Network State
   - Set minimum SDK: API 21 (Android 5.0)

4. **Test on Device**
   ```bash
   # Enable USB debugging on your device
   # Connect device via USB
   adb devices  # Verify connection
   
   # In Godot, click "Remote Debug" and run
   ```

### iOS Export (macOS only)

1. Install Xcode from App Store
2. Install Xcode Command Line Tools
3. Configure iOS export preset in Godot
4. Sign with Apple Developer account

## Troubleshooting

### Issue: Project won't import

**Solution:**
- Ensure you're using Godot 4.2 or later
- Check that `project.godot` exists in the folder
- Try creating a new project and copying files over

### Issue: API requests fail

**Solution:**
- Check internet connection
- Verify API URL is correct
- Check console for detailed error messages
- Test API endpoint in browser or Postman

### Issue: Black screen on mobile

**Solution:**
- Check renderer is set to "Mobile" in project settings
- Verify OpenGL ES 3.0 support on device
- Check device orientation settings

### Issue: Scripts not found

**Solution:**
- Verify autoload paths in Project Settings -> Autoload
- Check script paths in scene files
- Reimport project (Project -> Reload Current Project)

## Development Workflow

### 1. Daily Setup
```bash
# Pull latest changes
git pull origin main

# Open project in Godot
godot project.godot
```

### 2. Making Changes
- Create a new branch for features
- Test frequently using F5
- Check console for errors
- Commit often with clear messages

### 3. Testing Changes
- Test on desktop
- Test API integration
- Test on mobile device (if available)
- Check console logs

### 4. Committing Changes
```bash
git add .
git commit -m "Description of changes"
git push origin feature-branch
```

## Additional Resources

- [Godot Documentation](https://docs.godotengine.org/en/stable/)
- [GDScript Language Guide](https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/index.html)
- [Godot Community](https://godotengine.org/community)
- [Backend API Documentation](API_INTEGRATION.md)

## Getting Help

If you encounter issues:
1. Check the console output for errors
2. Review the [README.md](README.md)
3. Check [API_INTEGRATION.md](API_INTEGRATION.md) for API issues
4. Consult Godot documentation
5. Contact the development team

## Next Steps

After setup:
1. ✅ Run the project and test login/register
2. ✅ Test API connection
3. ✅ Explore the codebase
4. 📖 Read the API integration documentation
5. 🎮 Start developing new features!
