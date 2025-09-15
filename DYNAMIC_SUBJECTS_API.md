# Dynamic Subject Loading API Documentation

## Overview
This document describes the API changes needed to make subjects dynamic in the Unity app. Currently, the app only loads one subject per class code, but students need to see ALL subjects they have access to from ALL their joined classes.

## Problem Description
- Students can join classes using class codes in the web app
- The Unity app should show ALL subjects the student has access to dynamically
- Currently, the app only shows static subjects or one subject per class code

## Required API Endpoints

### POST `/student/subjects`

**Purpose**: Get all subjects available to a logged-in student from all their joined classes.

**Request Body**:
```json
{
    "student_id": 123
}
```

**Response (Success - 200)**:
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

### POST `/student/assignments`

**Purpose**: Get all assignments created by teachers for a specific subject that the student has access to.

**Request Body**:
```json
{
    "student_id": 123,
    "subject": "Mathematics"
}
```

**Response (Success - 200)**:
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
        },
        {
            "assignment_id": 2,
            "title": "Advanced Calculus",
            "description": "Derivatives and integrals",
            "created_by": "Teacher Johnson",
            "due_date": "2025-09-25",
            "questions": [...]
        }
    ]
}
```

**Response (No assignments - 200)**:
```json
{
    "assignments": []
}
```

**Response (Error - 404)**:
```json
{
    "error": "Student not found"
}
```

**Response (Error - 400)**:
```json
{
    "error": "Subject not found or student not enrolled"
}
```

## Implementation Logic (Web App Side)

1. Receive student_id from request
2. Find all classes the student has joined
3. For each class, get all subjects available in that class
4. Return unique list of subjects with their gameplay types
5. Handle cases where student hasn't joined any classes

## Unity App Changes Made

### New Features Added:
1. **Dynamic Subject Loading**: App now calls `/student/subjects` to get all available subjects
2. **Assignment Loading**: When student clicks a subject, app calls `/student/assignments` to get teacher-created assignments
3. **Automatic Refresh**: After joining a new class, subjects are automatically refreshed
4. **Manual Refresh Button**: Students can manually refresh their subjects using a refresh button
5. **Fallback System**: If server is unavailable, app falls back to local PlayerPrefs data

### How It Works:
1. When student logs in, app calls `/student/subjects` to load all available subjects
2. Subjects are displayed dynamically in the UI (not hardcoded)
3. When student clicks a subject, app calls `/student/assignments` for that specific subject
4. If assignments are found, they're loaded into the gameplay scene
5. If no assignments found, student gets practice mode with default questions
6. When student joins a new class, subjects list is automatically updated
7. Students can manually refresh using the refresh button

### Assignment Flow:
1. **Student clicks subject** → `LoadSubjectScene(subjectName)` is called
2. **App saves subject info** → Stores current subject and gameplay type in PlayerPrefs
3. **App calls assignments API** → `/student/assignments` with student_id and subject name
4. **Server returns assignments** → Teacher-created assignments for that subject
5. **App saves assignment data** → Stores assignments in PlayerPrefs for gameplay scene
6. **Gameplay scene loads** → Can access both subject info and assignment data
7. **Student completes assignments** → Results can be submitted back to teacher

### Code Changes:
- Added `LoadAssignmentsForSubject()` coroutine for fetching assignments
- Modified `LoadSubjectScene()` to handle assignment loading
- Added `LoadGameplayScene()` method for final scene transition
- Updated subject button click behavior to directly load assignments
- Added PlayerPrefs storage for current subject and assignment data

## Testing Instructions

### For Web App Developers:
1. Implement the `/student/subjects` endpoint as described above
2. Implement the `/student/assignments` endpoint for teacher-created assignments
3. Test with students who have joined multiple classes
4. Test with teachers who have created assignments for different subjects
5. Verify that all subjects from all classes are returned
6. Verify that assignments are properly filtered by subject and student access
7. Test error cases (student not found, no classes joined, no assignments)

### For Unity Testing:
1. Set a student ID in PlayerPrefs: `PlayerPrefs.SetInt("StudentID", your_test_id)`
2. Enable classroom mode in ClassCodeGate component
3. Set correct server URL in ClassCodeGate component
4. Test joining classes and see if subjects appear dynamically
5. **Test clicking subjects** and see if assignments load
6. Test the refresh subjects button
7. Verify that assignment data is properly passed to gameplay scenes

### Expected Behavior:
1. **Join Class**: Student enters classcode → Subject appears in UI
2. **Click Subject**: Student clicks subject → App loads teacher assignments 
3. **Play Assignment**: Gameplay scene loads with teacher's questions
4. **Complete Assignment**: Student can submit results back to teacher

## Benefits
- **Dynamic**: No more hardcoded subjects - subjects come from actual joined classes
- **Real-time**: Subjects update automatically when joining new classes
- **Scalable**: Supports unlimited number of subjects and classes
- **User-friendly**: Students see exactly what they have access to
- **Robust**: Falls back to local data if server is unavailable

## API Endpoint Already Working
- `POST /student/join-class` - Already implemented and working for joining individual classes
- The new endpoint complements this by fetching ALL subjects at once