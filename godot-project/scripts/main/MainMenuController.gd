extends Control

# MainMenuController - Handles main menu navigation and display

@onready var welcome_label: Label = $BackgroundPanel/VBoxContainer/WelcomeLabel
@onready var subjects_button: Button = $BackgroundPanel/VBoxContainer/SubjectsButton
@onready var classes_button: Button = $BackgroundPanel/VBoxContainer/ClassesButton
@onready var leaderboard_button: Button = $BackgroundPanel/VBoxContainer/LeaderboardButton
@onready var settings_button: Button = $BackgroundPanel/VBoxContainer/SettingsButton
@onready var logout_button: Button = $BackgroundPanel/VBoxContainer/LogoutButton

func _ready():
	# Update welcome message with student name
	if welcome_label and GameManager.is_logged_in():
		welcome_label.text = "Welcome, %s!" % GameManager.student_name
	
	# Connect button signals
	if subjects_button:
		subjects_button.pressed.connect(_on_subjects_pressed)
	if classes_button:
		classes_button.pressed.connect(_on_classes_pressed)
	if leaderboard_button:
		leaderboard_button.pressed.connect(_on_leaderboard_pressed)
	if settings_button:
		settings_button.pressed.connect(_on_settings_pressed)
	if logout_button:
		logout_button.pressed.connect(_on_logout_pressed)
	
	# Check if user is logged in
	if not GameManager.is_logged_in():
		# Redirect to login if not logged in
		GameManager.goto_login()

func _on_subjects_pressed():
	# TODO: Navigate to subjects screen
	print("Subjects button pressed")
	_show_message("Subjects feature coming soon!")

func _on_classes_pressed():
	# TODO: Navigate to join class screen
	print("Classes button pressed")
	_show_message("Join Class feature coming soon!")

func _on_leaderboard_pressed():
	# TODO: Navigate to leaderboard screen
	print("Leaderboard button pressed")
	_show_message("Leaderboard feature coming soon!")

func _on_settings_pressed():
	# TODO: Navigate to settings screen
	print("Settings button pressed")
	_show_message("Settings feature coming soon!")

func _on_logout_pressed():
	# Confirm logout
	var confirm = await _show_confirmation("Are you sure you want to logout?")
	if confirm:
		GameManager.end_session()
		GameManager.goto_login()

func _show_message(message: String):
	# Simple message display (you can enhance this with a proper dialog)
	print("Message: ", message)
	# TODO: Show proper dialog/toast notification

func _show_confirmation(message: String) -> bool:
	# Simple confirmation (you can enhance this with a proper dialog)
	print("Confirmation: ", message)
	# For now, just return true
	# TODO: Show proper confirmation dialog
	return true
