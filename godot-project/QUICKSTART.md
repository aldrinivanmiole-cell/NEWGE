# Quick Start Guide - EduGame

Get up and running with the Godot 4 EduGame project in 5 minutes!

## Prerequisites

Download and install:
- **Godot Engine 4.2+** from [godotengine.org](https://godotengine.org/download)

That's it! No additional dependencies needed.

## Step 1: Get the Project

```bash
# Clone the repository
git clone https://github.com/aldrinivanmiole-cell/NEWGE.git

# Navigate to Godot project
cd NEWGE/godot-project
```

## Step 2: Open in Godot

1. Launch Godot Engine
2. Click **"Import"** button
3. Click **"Browse"** and select the `godot-project` folder
4. Select `project.godot` file
5. Click **"Import & Edit"**

The project will open in Godot editor.

## Step 3: Run the Project

Press **F5** or click the **Play** button (▶️) in the top-right corner.

The game will start at the Login screen!

## Step 4: Test Registration

1. Click **"REGISTER"** button
2. Fill in the form:
   - First Name: `Test`
   - Last Name: `Student`
   - Email: `test@example.com`
   - Password: `test123`
   - Confirm Password: `test123`
3. Click **"SUBMIT"**

If successful, you'll see a success message and be redirected to login.

## Step 5: Test Login

1. Enter your credentials:
   - Email: `test@example.com`
   - Password: `test123`
2. Click **"LOGIN"**

If successful, you'll be taken to the Main Menu!

## What's Next?

### Explore the Code

- **Managers**: Check `scripts/managers/` for core systems
- **Controllers**: Look at `scripts/auth/` for authentication logic
- **Scenes**: Open `scenes/auth/` to see the UI

### Read the Documentation

- **README.md**: Project overview
- **SETUP.md**: Detailed setup instructions
- **API_INTEGRATION.md**: Backend API documentation
- **PROJECT_OVERVIEW.md**: Architecture and design

### Start Developing

1. Create a new branch: `git checkout -b feature/my-feature`
2. Make your changes
3. Test thoroughly
4. Commit and push: `git commit -m "Add my feature"`

## Common Issues

### "Can't open project"
- Make sure you selected the `project.godot` file
- Try Godot 4.2 or later

### "API connection failed"
- Check your internet connection
- The backend server might be slow (first request takes time)
- Check console for error details

### "Scene won't load"
- Check console for script errors
- Verify all script paths are correct
- Try Project → Reload Current Project

## Tips

### Debugging
- Press **F8** to pause/resume game
- Press **F9** to step through code
- Use `print()` statements liberally
- Check Output tab for console messages

### Editor Navigation
- **F5**: Run project
- **F6**: Run current scene
- **Ctrl+S**: Save scene/script
- **Ctrl+D**: Duplicate node
- **Ctrl+K**: Search nodes

### Testing on Mobile
1. Connect Android device via USB
2. Enable USB debugging on device
3. Click Remote Debug button (📱)
4. Select your device

## Project Structure Quick Reference

```
godot-project/
├── scenes/               # Game scenes (.tscn files)
│   ├── auth/            # Login & Register
│   └── main/            # Main menu
├── scripts/             # GDScript code (.gd files)
│   ├── auth/            # Auth controllers
│   ├── managers/        # Singleton managers
│   └── main/            # Main menu controller
├── assets/              # Resources
│   └── themes/          # UI theme
└── project.godot        # Project config
```

## Quick Commands

```bash
# Run Godot from command line
godot project.godot

# Run project directly
godot --path godot-project

# Export to Android
godot --export "Android" builds/game.apk
```

## Need Help?

1. Check the **Output** tab in Godot for errors
2. Read **SETUP.md** for detailed instructions
3. Review **PROJECT_OVERVIEW.md** for architecture
4. Check **API_INTEGRATION.md** for API issues
5. Look at existing code for examples

## Development Workflow

1. **Make changes** in Godot editor
2. **Test** by pressing F5
3. **Check console** for errors
4. **Debug** issues
5. **Repeat** until working
6. **Commit** your changes

## Testing Checklist

- [ ] Project opens without errors
- [ ] Login screen displays correctly
- [ ] Registration form validates input
- [ ] API calls succeed (check console)
- [ ] Navigation works (login → main menu)
- [ ] Theme is applied to UI

## Resources

- **Godot Docs**: https://docs.godotengine.org
- **GDScript Basics**: https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/
- **UI Tutorial**: https://docs.godotengine.org/en/stable/tutorials/ui/

## Success!

If you can:
1. ✅ Open the project in Godot
2. ✅ Run the game (F5)
3. ✅ See the Login screen
4. ✅ Register a new account
5. ✅ Login successfully

**Congratulations!** You're ready to start developing! 🎉

---

**Next Steps**: Read [PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md) to understand the architecture, then start adding features!
