# Task Management API

A simple RESTful API for managing tasks with dependencies, built using ASP.NET Core (.NET 8.0).

## Features


- **Create tasks** with dependencies.
- **Complete tasks** only if all dependent tasks are completed.
- **Full CRUD support**: Create, Read, Update, and Delete tasks.
- **Circular dependency detection** to avoid infinite loops between tasks.
- **Concurrency-safe operations** using `ConcurrentDictionary`.
- **Input validation** for task creation and updates (ID, title, dependencies).
- **Logging** implemented using `ILogger`.

## API Endpoints

1. **Create a New Task**
   - **POST** `/tasks`
   - Request body (example):
     ```json
     {
       "id": 1,
       "title": "Task A",
       "dueDate": "2024-01-01T00:00:00Z",
       "dependencies": [2]
     }
     ```
   - Validations:
     - Task ID must be a **positive integer**.
     - Title must be **non-empty** and have a **maximum of 100 characters**.
     - A task cannot depend on itself or have circular dependencies.

2. **Retrieve a Task by ID**
   - **GET** `/tasks/{id}`
   - Retrieves the task with the given ID along with its dependencies.

3. **Update an Existing Task**
   - **PUT** `/tasks/{id}`
   - Request body (example):
     ```json
     {
       "title": "Updated Task A",
       "dueDate": "2024-02-01T00:00:00Z",
       "dependencies": [3]
     }
     ```
   - Updates the task's title, due date, and dependencies.

4. **Mark a Task as Complete**
   - **PUT** `/tasks/{id}/complete`
   - Marks a task as complete, but only if all dependent tasks are already completed.

5. **Delete a Task**
   - **DELETE** `/tasks/{id}`
   - Deletes the task with the given ID.


## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/zhenya241/SVG_EnhancedExercise.git
2. Navigate to the project directory:
   ```bash
   cd SVG_EnhancedExercise
3. Run the project:
   ```bash
   dotnet run
4. Access the Swagger UI at `https://localhost:5001/swagger/index.html` to interact with the API.

## Logging

-The API uses built-in logging with ILogger for tracking task creation, updates, and error handling (e.g., circular dependencies).
-For more advanced logging (like file-based logging, structured logs, etc.), you can integrate Serilog or any other logging provider of your choice.

## Circular Dependency Detection

The API automatically checks for circular dependencies between tasks.

Example:
Task A depends on Task B, and Task B depends on Task A.
The API will return a 400 Bad Request if such a situation occurs, preventing circular dependencies.

## Unit Testing
The project includes comprehensive unit tests for key functionality:

-ircular dependency detection: Ensures that tasks with circular dependencies are properly detected and rejected.
-Concurrency handling: Simulates multiple threads trying to complete tasks simultaneously to ensure thread safety.
