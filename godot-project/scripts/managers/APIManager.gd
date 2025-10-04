extends Node

# APIManager - Handles all HTTP requests to the backend API
# Base URL for the backend API
const BASE_URL = "https://capstoneproject-jq2h.onrender.com"

# Request completed signal
signal request_completed(success: bool, data: Dictionary, error_message: String)

# HTTPRequest node for making API calls
var http_request: HTTPRequest

func _ready():
	# Create HTTPRequest node
	http_request = HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(_on_request_completed)

func _on_request_completed(result: int, response_code: int, headers: PackedStringArray, body: PackedByteArray):
	var response_text = body.get_string_from_utf8()
	
	print("API Response Code: ", response_code)
	print("API Response Body: ", response_text)
	
	if result != HTTPRequest.RESULT_SUCCESS:
		request_completed.emit(false, {}, "Connection failed: " + str(result))
		return
	
	if response_code < 200 or response_code >= 300:
		var error_msg = "HTTP Error " + str(response_code)
		if response_text:
			error_msg += ": " + response_text
		request_completed.emit(false, {}, error_msg)
		return
	
	# Parse JSON response
	var json = JSON.new()
	var parse_result = json.parse(response_text)
	
	if parse_result != OK:
		request_completed.emit(false, {}, "Failed to parse JSON response")
		return
	
	var data = json.data
	if typeof(data) != TYPE_DICTIONARY:
		data = {"response": data}
	
	request_completed.emit(true, data, "")

# Make a generic API request
func make_request(endpoint: String, method: int, data: Dictionary = {}, custom_headers: PackedStringArray = []):
	var url = BASE_URL + endpoint
	var headers = ["Content-Type: application/json", "Accept: application/json"]
	
	# Add custom headers
	for header in custom_headers:
		headers.append(header)
	
	var json_string = ""
	if not data.is_empty():
		json_string = JSON.stringify(data)
	
	print("API Request: ", method, " ", url)
	print("API Request Data: ", json_string)
	
	var error = http_request.request(url, headers, method, json_string)
	
	if error != OK:
		request_completed.emit(false, {}, "Failed to send request: " + str(error))

# Student login
func student_login(email: String, password: String, device_id: String):
	var data = {
		"email": email,
		"password": password,
		"device_id": device_id
	}
	make_request("/api/student/login", HTTPClient.METHOD_POST, data)

# Student registration (simple register - no class enrollment)
func student_register(first_name: String, last_name: String, email: String, password: String, device_id: String, grade_level: String = "Grade 1"):
	var data = {
		"name": first_name + " " + last_name,
		"email": email,
		"password": password,
		"device_id": device_id,
		"grade_level": grade_level,
		"avatar_url": ""
	}
	make_request("/api/student/simple-register", HTTPClient.METHOD_POST, data)

# Get subjects for a student
func get_subjects(student_id: int):
	var data = {
		"student_id": student_id
	}
	make_request("/api/student/subjects", HTTPClient.METHOD_POST, data)

# Get assignments for a student and subject
func get_assignments(student_id: int, subject: String):
	var data = {
		"student_id": student_id,
		"subject": subject
	}
	make_request("/api/student/assignments", HTTPClient.METHOD_POST, data)

# Submit answer for an assignment
func submit_answer(assignment_id: int, answers: Array):
	var data = {
		"answers": answers
	}
	make_request("/api/submit/" + str(assignment_id), HTTPClient.METHOD_POST, data)

# Get leaderboard
func get_leaderboard(class_code: String):
	make_request("/api/leaderboard/" + class_code, HTTPClient.METHOD_GET)

# Join a class
func join_class(student_id: int, class_code: String):
	var data = {
		"student_id": student_id,
		"class_code": class_code
	}
	make_request("/api/student/join-class", HTTPClient.METHOD_POST, data)
