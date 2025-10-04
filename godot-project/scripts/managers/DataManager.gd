extends Node

# DataManager - Handles local data persistence using ConfigFile

var config: ConfigFile
const CONFIG_FILE_PATH = "user://edugame_data.cfg"

func _ready():
	config = ConfigFile.new()
	# Load existing config if it exists
	var err = config.load(CONFIG_FILE_PATH)
	if err != OK:
		print("No existing config file, creating new one")

# Save session data
func save_session(session_data: Dictionary):
	config.set_value("session", "student_id", session_data.get("student_id", 0))
	config.set_value("session", "student_name", session_data.get("student_name", ""))
	config.set_value("session", "student_email", session_data.get("student_email", ""))
	config.set_value("session", "total_points", session_data.get("total_points", 0))
	config.set_value("session", "last_login", Time.get_unix_time_from_system())
	_save_config()

# Load session data
func load_session() -> Dictionary:
	if not config.has_section("session"):
		return {}
	
	return {
		"student_id": config.get_value("session", "student_id", 0),
		"student_name": config.get_value("session", "student_name", ""),
		"student_email": config.get_value("session", "student_email", ""),
		"total_points": config.get_value("session", "total_points", 0),
		"last_login": config.get_value("session", "last_login", 0)
	}

# Clear session data
func clear_session():
	if config.has_section("session"):
		config.erase_section("session")
		_save_config()

# Save device ID
func save_device_id(device_id: String):
	config.set_value("device", "device_id", device_id)
	_save_config()

# Load device ID
func load_device_id() -> String:
	return config.get_value("device", "device_id", "")

# Save settings
func save_setting(key: String, value):
	config.set_value("settings", key, value)
	_save_config()

# Load setting
func load_setting(key: String, default_value = null):
	return config.get_value("settings", key, default_value)

# Cache subject data
func cache_subjects(subjects: Array):
	config.set_value("cache", "subjects", subjects)
	config.set_value("cache", "subjects_timestamp", Time.get_unix_time_from_system())
	_save_config()

# Load cached subjects
func load_cached_subjects() -> Array:
	if not config.has_section_key("cache", "subjects"):
		return []
	
	var timestamp = config.get_value("cache", "subjects_timestamp", 0)
	var current_time = Time.get_unix_time_from_system()
	
	# Cache expires after 1 hour
	if current_time - timestamp > 3600:
		return []
	
	return config.get_value("cache", "subjects", [])

# Cache assignments
func cache_assignments(subject: String, assignments: Array):
	config.set_value("assignments", subject, assignments)
	config.set_value("assignments", subject + "_timestamp", Time.get_unix_time_from_system())
	_save_config()

# Load cached assignments
func load_cached_assignments(subject: String) -> Array:
	if not config.has_section_key("assignments", subject):
		return []
	
	var timestamp = config.get_value("assignments", subject + "_timestamp", 0)
	var current_time = Time.get_unix_time_from_system()
	
	# Cache expires after 1 hour
	if current_time - timestamp > 3600:
		return []
	
	return config.get_value("assignments", subject, [])

# Save configuration to file
func _save_config():
	var err = config.save(CONFIG_FILE_PATH)
	if err != OK:
		print("Error saving config file: ", err)
	else:
		print("Config saved successfully")

# Clear all cached data
func clear_cache():
	if config.has_section("cache"):
		config.erase_section("cache")
	if config.has_section("assignments"):
		config.erase_section("assignments")
	_save_config()

# Get all settings
func get_all_settings() -> Dictionary:
	var settings = {}
	if config.has_section("settings"):
		for key in config.get_section_keys("settings"):
			settings[key] = config.get_value("settings", key)
	return settings
