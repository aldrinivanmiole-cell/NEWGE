extends Control

# RegisterController - Handles registration form validation and API integration

@onready var first_name_input: LineEdit = $BackgroundPanel/WoodenFrame/ScrollContainer/VBoxContainer/FirstNameInput
@onready var last_name_input: LineEdit = $BackgroundPanel/WoodenFrame/ScrollContainer/VBoxContainer/LastNameInput
@onready var email_input: LineEdit = $BackgroundPanel/WoodenFrame/ScrollContainer/VBoxContainer/EmailInput
@onready var password_input: LineEdit = $BackgroundPanel/WoodenFrame/ScrollContainer/VBoxContainer/PasswordInput
@onready var confirm_password_input: LineEdit = $BackgroundPanel/WoodenFrame/ScrollContainer/VBoxContainer/ConfirmPasswordInput
@onready var submit_button: Button = $BackgroundPanel/WoodenFrame/ScrollContainer/VBoxContainer/SubmitButton
@onready var back_button: Button = $BackgroundPanel/WoodenFrame/ScrollContainer/VBoxContainer/BackButton
@onready var status_label: Label = $BackgroundPanel/WoodenFrame/ScrollContainer/VBoxContainer/StatusLabel
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
	if submit_button:
		submit_button.pressed.connect(_on_submit_pressed)
	if back_button:
		back_button.pressed.connect(_on_back_pressed)
	
	# Connect API Manager signal
	APIManager.request_completed.connect(_on_api_response)
	
	# Set password inputs to secret mode
	if password_input:
		password_input.secret = true
	if confirm_password_input:
		confirm_password_input.secret = true

func _on_submit_pressed():
	if is_loading:
		return
	
	# Get input values
	var first_name = first_name_input.text.strip_edges()
	var last_name = last_name_input.text.strip_edges()
	var email = email_input.text.strip_edges()
	var password = password_input.text
	var confirm_password = confirm_password_input.text
	
	# Validate inputs
	if not _validate_registration_form(first_name, last_name, email, password, confirm_password):
		return
	
	# Show loading state
	_set_loading(true, "Creating account...")
	
	# Get device ID
	var device_id = GameManager.get_device_id()
	
	# Make API request
	APIManager.student_register(first_name, last_name, email, password, device_id)

func _validate_registration_form(first_name: String, last_name: String, email: String, password: String, confirm_password: String) -> bool:
	# Check if first name is empty
	if first_name.is_empty():
		_show_error("Please enter your first name")
		return false
	
	# Check if last name is empty
	if last_name.is_empty():
		_show_error("Please enter your last name")
		return false
	
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
		_show_error("Please enter a password")
		return false
	
	# Check password length
	if password.length() < 6:
		_show_error("Password must be at least 6 characters")
		return false
	
	# Check if passwords match
	if password != confirm_password:
		_show_error("Passwords do not match")
		return false
	
	return true

func _is_valid_email(email: String) -> bool:
	# Simple email validation
	var regex = RegEx.new()
	regex.compile("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")
	return regex.search(email) != null

func _on_back_pressed():
	# Navigate back to login screen
	GameManager.goto_login()

func _on_api_response(success: bool, data: Dictionary, error_message: String):
	if not is_loading:
		return
	
	_set_loading(false)
	
	if success:
		# Registration successful
		_handle_registration_success(data)
	else:
		# Registration failed
		_show_error(error_message)

func _handle_registration_success(data: Dictionary):
	# Show success message
	var student_name = data.get("student_name", "Student")
	_show_success("Registration successful! Welcome " + student_name + "! Please log in.")
	
	# Clear form
	_clear_form()
	
	# Wait a bit before transitioning
	await get_tree().create_timer(2.0).timeout
	
	# Navigate to login screen
	GameManager.goto_login()

func _clear_form():
	if first_name_input:
		first_name_input.text = ""
	if last_name_input:
		last_name_input.text = ""
	if email_input:
		email_input.text = ""
	if password_input:
		password_input.text = ""
	if confirm_password_input:
		confirm_password_input.text = ""

func _set_loading(loading: bool, message: String = ""):
	is_loading = loading
	
	if loading_panel:
		loading_panel.visible = loading
	
	if loading_label and not message.is_empty():
		loading_label.text = message
	
	# Disable inputs and buttons during loading
	if first_name_input:
		first_name_input.editable = not loading
	if last_name_input:
		last_name_input.editable = not loading
	if email_input:
		email_input.editable = not loading
	if password_input:
		password_input.editable = not loading
	if confirm_password_input:
		confirm_password_input.editable = not loading
	if submit_button:
		submit_button.disabled = loading
	if back_button:
		back_button.disabled = loading

func _show_error(message: String):
	if status_label:
		status_label.text = message
		status_label.add_theme_color_override("font_color", Color.RED)

func _show_success(message: String):
	if status_label:
		status_label.text = message
		status_label.add_theme_color_override("font_color", Color.GREEN)
