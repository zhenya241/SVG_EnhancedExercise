# Task Management API

A simple RESTful API for managing tasks with dependencies, built using ASP.NET Core (.NET 8.0).

## Features

- Create tasks with dependencies
- Mark tasks as complete only if all dependent tasks are completed
- Thread-safe in-memory storage for tasks using `ConcurrentDictionary`
- API validation for task creation and updates
- Logging implemented with `ILogger`

## API Endpoints

- `POST /tasks` - Create a new task
- `PUT /tasks/{id}/complete` - Mark a task as complete (if all dependencies are complete)
- More CRUD endpoints coming soon...

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

Logging is enabled using the built-in ILogger. You can expand logging by integrating Serilog or another logging provider.
