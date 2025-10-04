extends Node

# GameManager - Handles game state, scene transitions, and session management

# Current student session data
var student_id: int = 0
var student_name: String = ""
var student_email: String = ""
var total_points: int = 0
var current_class: String = ""
var current_subject: String = ""
var current_assignment: Dictionary = {}

# Scene paths
const SCENE_LOGIN = "res://scenes/auth/LoginScreen.tscn"
const SCENE_REGISTER = "res://scenes/auth/RegisterScreen.tscn"
const SCENE_MAIN_MENU = "res://scenes/main/MainMenu.tscn"

# Signals
signal session_started(student_data: Dictionary)
signal session_ended()
signal scene_changing(new_scene: String)
signal points_updated(new_points: int)

func _ready():
	# Check for existing session on startup
	load_session()

# Start a new student session
func start_session(student_data: Dictionary):
	if student_data.has("student_id"):
		student_id = student_data.get("student_id", 0)
	if student_data.has("student_name"):
		student_name = student_data.get("student_name", "")
	if student_data.has("name"):
		student_name = student_data.get("name", "")
	if student_data.has("email"):
		student_email = student_data.get("email", "")
	if student_data.has("total_points"):
		total_points = student_data.get("total_points", 0)
	
	# Save session
	save_session()
	
	session_started.emit(student_data)
	print("Session started for: ", student_name, " (ID: ", student_id, ")")

# End the current session
func end_session():
	student_id = 0
	student_name = ""
	student_email = ""
	total_points = 0
	current_class = ""
	current_subject = ""
	current_assignment = {}
	
	# Clear saved session
	DataManager.clear_session()
	
	session_ended.emit()
	print("Session ended")

# Save current session to local storage
func save_session():
	var session_data = {
		"student_id": student_id,
		"student_name": student_name,
		"student_email": student_email,
		"total_points": total_points
	}
	DataManager.save_session(session_data)

# Load session from local storage
func load_session():
	var session_data = DataManager.load_session()
	if not session_data.is_empty():
		student_id = session_data.get("student_id", 0)
		student_name = session_data.get("student_name", "")
		student_email = session_data.get("student_email", "")
		total_points = session_data.get("total_points", 0)
		print("Session loaded for: ", student_name)

# Check if user is logged in
func is_logged_in() -> bool:
	return student_id > 0

# Change scene
func change_scene(scene_path: String):
	scene_changing.emit(scene_path)
	get_tree().change_scene_to_file(scene_path)

# Navigate to login screen
func goto_login():
	change_scene(SCENE_LOGIN)

# Navigate to register screen
func goto_register():
	change_scene(SCENE_REGISTER)

# Navigate to main menu
func goto_main_menu():
	change_scene(SCENE_MAIN_MENU)

# Update student points
func update_points(points: int):
	total_points = points
	save_session()
	points_updated.emit(total_points)

# Add points to student
func add_points(points: int):
	update_points(total_points + points)

# Set current assignment
func set_current_assignment(assignment: Dictionary):
	current_assignment = assignment

# Get current assignment
func get_current_assignment() -> Dictionary:
	return current_assignment

# Generate unique device ID
func get_device_id() -> String:
	# Try to get a saved device ID first
	var device_id = DataManager.load_device_id()
	if device_id.is_empty():
		# Generate a new one
		device_id = _generate_device_id()
		DataManager.save_device_id(device_id)
	return device_id

# Generate a unique device ID
func _generate_device_id() -> String:
	# Use OS and time to generate a unique ID
	var time = Time.get_unix_time_from_system()
	var random = randi()
	var os_name = OS.get_name()
	return "%s_%d_%d" % [os_name, time, random]
