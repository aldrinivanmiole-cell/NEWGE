using System;

[System.Serializable]
public class WebAppQuestion
{
    public string question = "";
    public string[] options = Array.Empty<string>();
    public int correct_answer = 0; // index-based
    public string question_type = "multiple_choice";
}

[System.Serializable]
public class WebAppAssignment
{
    public string title = "";
    public string subject = "";
    public string assignment_type = "multiple_choice";
    public WebAppQuestion[] questions = Array.Empty<WebAppQuestion>();
}

// API Response classes matching your Flask API
[System.Serializable]
public class AssignmentsResponse
{
    public Assignment[] assignments = Array.Empty<Assignment>();
}

[System.Serializable]
public class Assignment
{
    public int assignment_id;
    public string title = "";
    public string description = "";
    public string subject = "";
    public string created_by = "";
    public string due_date = "";
    public AssignmentQuestion[] questions = Array.Empty<AssignmentQuestion>();
}

[System.Serializable]
public class AssignmentQuestion
{
    public int question_id;
    public string question_text = "";
    public string question_type = "";
    public string[] options = Array.Empty<string>();
    public string correct_answer = "";
}