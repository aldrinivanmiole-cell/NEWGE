extends Control

# LoginController - Handles login form validation and API integration

@onready var email_input: LineEdit = $BackgroundPanel/WoodenFrame/VBoxContainer/EmailInput
@onready var password_input: LineEdit = $BackgroundPanel/WoodenFrame/VBoxContainer/PasswordInput
@onready var login_button: Button = $BackgroundPanel/WoodenFrame/VBoxContainer/LoginButton
@onready var register_button: Button = $BackgroundPanel/WoodenFrame/VBoxContainer/RegisterButton
@onready var status_label: Label = $BackgroundPanel/WoodenFrame/VBoxContainer/StatusLabel
@onready var loading_panel: Panel = $LoadingPanel
@onready var loading_label: Label = $LoadingPanel/LoadingLabel

var is_loading: bool = false

func _ready():
	# Hide loading panel initially
	if loading_panel:
		loading_panel.visible = false
	
	# Clear status label
	if status_label:
		status_label.text = ""
	
	# Connect button signals
	if login_button:
		login_button.pressed.connect(_on_login_pressed)
	if register_button:
		register_button.pressed.connect(_on_register_pressed)
	
	# Connect API Manager signal
	APIManager.request_completed.connect(_on_api_response)
	
	# Set password input to secret mode
	if password_input:
		password_input.secret = true

func _on_login_pressed():
	if is_loading:
		return
	
	# Get input values
	var email = email_input.text.strip_edges()
	var password = password_input.text
	
	# Validate inputs
	if not _validate_login_form(email, password):
		return
	
	# Show loading state
	_set_loading(true, "Logging in...")
	
	# Get device ID
	var device_id = GameManager.get_device_id()
	
	# Make API request
	APIManager.student_login(email, password, device_id)

func _validate_login_form(email: String, password: String) -> bool:
	# Check if email is empty
	if email.is_empty():
		_show_error("Please enter your email")
		return false
	
	# Check if email format is valid
	if not _is_valid_email(email):
		_show_error("Please enter a valid email address")
		return false
	
	# Check if password is empty
	if password.is_empty():
		_show_error("Please enter your password")
		return false
	
	return true

func _is_valid_email(email: String) -> bool:
	# Simple email validation
	var regex = RegEx.new()
	regex.compile("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")
	return regex.search(email) != null

func _on_register_pressed():
	# Navigate to register screen
	GameManager.goto_register()

func _on_api_response(success: bool, data: Dictionary, error_message: String):
	if not is_loading:
		return
	
	_set_loading(false)
	
	if success:
		# Login successful
		_handle_login_success(data)
	else:
		# Login failed
		_show_error(error_message)

func _handle_login_success(data: Dictionary):
	# Extract student data
	var student_data = {}
	
	# Check for different response formats
	if data.has("student_id"):
		student_data["student_id"] = data.get("student_id", 0)
	if data.has("student_name"):
		student_data["student_name"] = data.get("student_name", "")
	elif data.has("name"):
		student_data["student_name"] = data.get("name", "")
	if data.has("email"):
		student_data["email"] = data.get("email", "")
	if data.has("total_points"):
		student_data["total_points"] = data.get("total_points", 0)
	
	# Validate we have required data
	if not student_data.has("student_id") or student_data["student_id"] == 0:
		_show_error("Invalid login response from server")
		return
	
	# Start game session
	GameManager.start_session(student_data)
	
	# Show success message
	_show_success("Login successful! Welcome " + student_data.get("student_name", "Student") + "!")
	
	# Wait a bit before transitioning
	await get_tree().create_timer(1.0).timeout
	
	# Navigate to main menu
	GameManager.goto_main_menu()

func _set_loading(loading: bool, message: String = ""):
	is_loading = loading
	
	if loading_panel:
		loading_panel.visible = loading
	
	if loading_label and not message.is_empty():
		loading_label.text = message
	
	# Disable inputs and buttons during loading
	if email_input:
		email_input.editable = not loading
	if password_input:
		password_input.editable = not loading
	if login_button:
		login_button.disabled = loading
	if register_button:
		register_button.disabled = loading

func _show_error(message: String):
	if status_label:
		status_label.text = message
		status_label.add_theme_color_override("font_color", Color.RED)

func _show_success(message: String):
	if status_label:
		status_label.text = message
		status_label.add_theme_color_override("font_color", Color.GREEN)
