# API Integration Documentation

This document provides detailed information about integrating with the backend API at `https://capstoneproject-jq2h.onrender.com`.

## Table of Contents

- [Overview](#overview)
- [Authentication](#authentication)
- [API Endpoints](#api-endpoints)
- [Request/Response Format](#requestresponse-format)
- [Error Handling](#error-handling)
- [Usage Examples](#usage-examples)

## Overview

The EduGame application communicates with a FastAPI backend server that manages:
- Student authentication
- Classroom enrollment
- Subject assignments
- Progress tracking
- Leaderboards

**Base URL**: `https://capstoneproject-jq2h.onrender.com`

**Protocol**: HTTPS

**Content-Type**: `application/json`

## Authentication

### Device ID

Each device generates a unique device ID for tracking sessions:

```gdscript
var device_id = GameManager.get_device_id()
```

The device ID is generated on first run and stored locally.

### Session Management

After successful login, the following data is stored locally:
- `student_id` - Unique student identifier
- `student_name` - Student's full name
- `student_email` - Student's email address
- `total_points` - Student's accumulated points

## API Endpoints

### 1. Student Login

**Endpoint**: `POST /api/student/login`

**Description**: Authenticate a student with email and password.

**Request Body**:
```json
{
  "email": "student@example.com",
  "password": "password123",
  "device_id": "unique_device_id"
}
```

**Success Response (200)**:
```json
{
  "student_id": 123,
  "student_name": "John Doe",
  "email": "student@example.com",
  "total_points": 500
}
```

**Error Response (401)**:
```json
{
  "error": "Invalid credentials"
}
```

**Usage**:
```gdscript
APIManager.student_login(email, password, device_id)
APIManager.request_completed.connect(_on_login_response)
```

---

### 2. Student Registration

**Endpoint**: `POST /api/student/simple-register`

**Description**: Register a new student account without class enrollment.

**Request Body**:
```json
{
  "name": "John Doe",
  "email": "student@example.com",
  "password": "password123",
  "device_id": "unique_device_id",
  "grade_level": "Grade 1",
  "avatar_url": ""
}
```

**Success Response (200)**:
```json
{
  "status": "success",
  "student_id": 123,
  "student_name": "John Doe",
  "total_points": 0
}
```

**Error Response (400)**:
```json
{
  "error": "Email already exists"
}
```

**Usage**:
```gdscript
APIManager.student_register(first_name, last_name, email, password, device_id, "Grade 1")
APIManager.request_completed.connect(_on_register_response)
```

---

### 3. Get Student Subjects

**Endpoint**: `POST /api/student/subjects`

**Description**: Retrieve all subjects available to the student from all joined classes.

**Request Body**:
```json
{
  "student_id": 123
}
```

**Success Response (200)**:
```json
{
  "subjects": [
    {
      "subject_name": "Mathematics",
      "gameplay_type": "MultipleChoice"
    },
    {
      "subject_name": "Science",
      "gameplay_type": "Enumeration"
    },
    {
      "subject_name": "English",
      "gameplay_type": "FillInBlank"
    },
    {
      "subject_name": "History",
      "gameplay_type": "YesNo"
    }
  ]
}
```

**Error Response (404)**:
```json
{
  "error": "Student not found"
}
```

**Usage**:
```gdscript
APIManager.get_subjects(student_id)
APIManager.request_completed.connect(_on_subjects_response)
```

---

### 4. Get Subject Assignments

**Endpoint**: `POST /api/student/assignments`

**Description**: Get all teacher-created assignments for a specific subject.

**Request Body**:
```json
{
  "student_id": 123,
  "subject": "Mathematics"
}
```

**Success Response (200)**:
```json
{
  "assignments": [
    {
      "assignment_id": 1,
      "title": "Basic Algebra Quiz",
      "description": "Introduction to algebraic expressions",
      "created_by": "Teacher Smith",
      "due_date": "2025-09-20",
      "questions": [
        {
          "question_id": 1,
          "question_text": "What is 2 + 2?",
          "question_type": "MultipleChoice",
          "options": ["3", "4", "5", "6"],
          "correct_answer": "4"
        },
        {
          "question_id": 2,
          "question_text": "Solve for x: 2x + 5 = 11",
          "question_type": "FillInBlank",
          "correct_answer": "3"
        }
      ]
    }
  ]
}
```

**No Assignments Response (200)**:
```json
{
  "assignments": []
}
```

**Usage**:
```gdscript
APIManager.get_assignments(student_id, subject_name)
APIManager.request_completed.connect(_on_assignments_response)
```

---

### 5. Submit Assignment Answers

**Endpoint**: `POST /api/submit/{assignment_id}`

**Description**: Submit student's answers for an assignment.

**Request Body**:
```json
{
  "answers": [
    {
      "question_id": 1,
      "student_answer": "4"
    },
    {
      "question_id": 2,
      "student_answer": "3"
    }
  ]
}
```

**Success Response (200)**:
```json
{
  "score": 100,
  "correct_answers": 2,
  "total_questions": 2,
  "points_earned": 50
}
```

**Usage**:
```gdscript
var answers = [
  {"question_id": 1, "student_answer": "4"},
  {"question_id": 2, "student_answer": "3"}
]
APIManager.submit_answer(assignment_id, answers)
APIManager.request_completed.connect(_on_submit_response)
```

---

### 6. Get Leaderboard

**Endpoint**: `GET /api/leaderboard/{class_code}`

**Description**: Retrieve the leaderboard for a specific class.

**Success Response (200)**:
```json
{
  "class_code": "ABC123",
  "class_name": "Grade 5 Mathematics",
  "leaderboard": [
    {
      "rank": 1,
      "student_name": "Alice Johnson",
      "total_points": 1500
    },
    {
      "rank": 2,
      "student_name": "Bob Smith",
      "total_points": 1200
    }
  ]
}
```

**Usage**:
```gdscript
APIManager.get_leaderboard(class_code)
APIManager.request_completed.connect(_on_leaderboard_response)
```

---

### 7. Join Class

**Endpoint**: `POST /api/student/join-class`

**Description**: Enroll a student in a class using a class code.

**Request Body**:
```json
{
  "student_id": 123,
  "class_code": "ABC123"
}
```

**Success Response (200)**:
```json
{
  "status": "success",
  "subject": "Mathematics",
  "gameplay_type": "MultipleChoice",
  "message": "Successfully joined class"
}
```

**Error Response (404)**:
```json
{
  "error": "Class not found"
}
```

**Usage**:
```gdscript
APIManager.join_class(student_id, class_code)
APIManager.request_completed.connect(_on_join_class_response)
```

## Request/Response Format

### Headers

All requests should include:
```
Content-Type: application/json
Accept: application/json
```

### Response Status Codes

- `200` - Success
- `400` - Bad Request (invalid data)
- `401` - Unauthorized (invalid credentials)
- `404` - Not Found (resource doesn't exist)
- `500` - Server Error

## Error Handling

### In APIManager

The APIManager automatically handles HTTP errors and connection issues:

```gdscript
func _on_api_response(success: bool, data: Dictionary, error_message: String):
    if success:
        # Handle successful response
        print("Success: ", data)
    else:
        # Handle error
        print("Error: ", error_message)
        _show_error_dialog(error_message)
```

### Common Error Scenarios

1. **Network Timeout**
   - Error: "Connection failed"
   - Solution: Check internet connection, retry request

2. **Invalid Credentials**
   - Error: "Invalid credentials"
   - Solution: Verify email/password are correct

3. **Student Not Found**
   - Error: "Student not found"
   - Solution: Ensure student is registered and logged in

4. **Class Not Found**
   - Error: "Class not found"
   - Solution: Verify class code is correct

5. **Email Already Exists**
   - Error: "Email already exists"
   - Solution: Use different email or try logging in

## Usage Examples

### Complete Login Flow

```gdscript
extends Control

func _ready():
    APIManager.request_completed.connect(_on_api_response)

func _on_login_button_pressed():
    var email = email_input.text
    var password = password_input.text
    var device_id = GameManager.get_device_id()
    
    # Show loading
    loading_panel.visible = true
    
    # Make API call
    APIManager.student_login(email, password, device_id)

func _on_api_response(success: bool, data: Dictionary, error_message: String):
    loading_panel.visible = false
    
    if success:
        # Start session
        GameManager.start_session(data)
        # Navigate to main menu
        GameManager.goto_main_menu()
    else:
        # Show error
        error_label.text = error_message
```

### Fetching and Displaying Subjects

```gdscript
extends Control

var subjects: Array = []

func _ready():
    APIManager.request_completed.connect(_on_subjects_loaded)
    load_subjects()

func load_subjects():
    var student_id = GameManager.student_id
    APIManager.get_subjects(student_id)

func _on_subjects_loaded(success: bool, data: Dictionary, error_message: String):
    if success and data.has("subjects"):
        subjects = data["subjects"]
        _display_subjects()
    else:
        print("Error loading subjects: ", error_message)

func _display_subjects():
    for subject in subjects:
        var button = Button.new()
        button.text = subject["subject_name"]
        button.pressed.connect(_on_subject_selected.bind(subject))
        subject_container.add_child(button)

func _on_subject_selected(subject: Dictionary):
    # Load assignments for this subject
    APIManager.get_assignments(GameManager.student_id, subject["subject_name"])
```

### Offline Mode with Caching

```gdscript
func load_subjects_with_cache():
    # Try to load from cache first
    var cached_subjects = DataManager.load_cached_subjects()
    if not cached_subjects.is_empty():
        _display_subjects(cached_subjects)
    
    # Always try to fetch fresh data
    APIManager.get_subjects(GameManager.student_id)

func _on_subjects_loaded(success: bool, data: Dictionary, error_message: String):
    if success and data.has("subjects"):
        var subjects = data["subjects"]
        # Cache for offline use
        DataManager.cache_subjects(subjects)
        _display_subjects(subjects)
```

## Testing API Endpoints

### Using curl

```bash
# Test login
curl -X POST https://capstoneproject-jq2h.onrender.com/api/student/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"test123","device_id":"test_device"}'

# Test registration
curl -X POST https://capstoneproject-jq2h.onrender.com/api/student/simple-register \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Student","email":"test@example.com","password":"test123","device_id":"test_device","grade_level":"Grade 1","avatar_url":""}'
```

### Using Postman

1. Create a new collection "EduGame API"
2. Add requests for each endpoint
3. Set base URL as environment variable
4. Test each endpoint with sample data

## Best Practices

1. **Always handle both success and error cases**
2. **Cache data locally for offline access**
3. **Show loading indicators during API calls**
4. **Validate input before making requests**
5. **Log API responses for debugging**
6. **Use timeouts to prevent hanging**
7. **Disconnect signal handlers when not needed**

## Troubleshooting

### API Not Responding

1. Check if server is online: `https://capstoneproject-jq2h.onrender.com`
2. Verify internet connection
3. Check console for detailed error messages
4. Try endpoint in browser or Postman

### Invalid Response Format

1. Check API endpoint is correct
2. Verify request body format
3. Log full response in console
4. Check for API version changes

### Timeout Issues

1. Increase timeout in HTTPRequest
2. Check server status
3. Test with smaller requests
4. Implement retry logic

## Support

For API-related issues:
1. Check console output for errors
2. Review request/response in debug mode
3. Test endpoint with curl/Postman
4. Contact backend team if server-side issue
