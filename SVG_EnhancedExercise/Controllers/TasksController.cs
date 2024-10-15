using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskManagementAPI.Models; 
using TaskManagementAPI.Services;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    // Inject TaskService and ILogger to manage tasks and log operations
    private readonly TaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(TaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;      // Task service to handle business logic
        _logger = logger;                // Logger instance to log important events
    }

    // POST: /api/tasks
    // Endpoint to create a new task
    [HttpPost]
    public IActionResult CreateTask([FromBody] TaskManagementAPI.Models.Task newTask)
    {
        // Validate the Task ID field before proceeding (check for empty/negative ID)
        if (newTask.Id < 0)
        {
            _logger.LogWarning("Invalid ID for task creation. Task ID is {TaskId}.", newTask.Id);
            return BadRequest("Invalid Task ID: Task ID must be greater than 0 and not empty.");
        }

        // Validate the Title field before proceeding
        if (string.IsNullOrEmpty(newTask.Title) || newTask.Title.Length > 100)
        {
            _logger.LogWarning("Invalid title for task creation. Title length is {Length}.", newTask.Title.Length);
            return BadRequest("Invalid Title");
        }

        // Validate DueDate (it cannot be in the past)
        if (newTask.DueDate < DateTime.Now)
        {
            _logger.LogWarning("Invalid DueDate for task creation. Task ID: {Id}", newTask.Id);
            return BadRequest("Due date cannot be in the past.");
        }

        // Prevent self-dependency
        if (newTask.Dependencies.Contains(newTask.Id))
        {
            _logger.LogWarning("Task with ID {Id} cannot depend on itself.", newTask.Id);
            return BadRequest("Task cannot depend on itself.");
        }

        // Validate dependencies (check for circular or missing dependencies)
        var validationResult = _taskService.ValidateDependencies(newTask);
        switch (validationResult)
        {
            case DependencyValidationResult.CircularDependency:
                return BadRequest("Circular dependency detected.");
            case DependencyValidationResult.MissingTask:
                return BadRequest("A dependent task is missing.");
            case DependencyValidationResult.NoIssues:
                // Add the new task using the TaskService
                // Try to add the task, checking for duplicate IDs
                if (!_taskService.AddTask(newTask))
                {
                    _logger.LogWarning("Failed to create task with ID {TaskId}, ID already exists.", newTask.Id);
                    return BadRequest($"Task with ID {newTask.Id} already exists.");
                }
                _logger.LogInformation("Task with ID {Id} was created.", newTask.Id);
                return Ok(newTask);  // Return the created task as the response
            default:
                return BadRequest("Unknown error.");
        }
    }

    // PUT: /api/tasks/{id}/complete
    // Endpoint to mark a task as complete
    [HttpPut("{id}/complete")]
    public IActionResult MarkTaskComplete(int id)
    {
        // First, check if the task exists
        var task = _taskService.GetTask(id);
        if (task == null)
        {
            _logger.LogWarning("Task with ID {Id} not found.", id);
            return NotFound($"Task with ID {id} not found.");
        }

        // Check if the task can be marked as complete (e.g., dependencies are satisfied)
        if (_taskService.CompleteTask(id))
        {
            _logger.LogInformation("Task with ID {Id} was successfully completed.", id);
            return Ok();  // Task was completed successfully
        }

        // If the task exists but can't be completed due to dependencies
        _logger.LogWarning("Task with ID {Id} cannot be completed. Dependencies are incomplete.", id);
        return BadRequest("Cannot complete task. Dependencies are incomplete.");

    }

    // GET: /api/tasks
    // Retrieve all tasks
    [HttpGet]
    public IActionResult GetAllTasks()
    {
        var tasks = _taskService.GetAllTasks();
        return Ok(tasks);  // Return all tasks in the system
    }

    // GET: /api/tasks/{id}
    // Retrieve a task by ID
    [HttpGet("{id}")]
    public IActionResult GetTaskById(int id)
    {
        var task = _taskService.GetTask(id);   // Fetch task by ID using TaskService
        if (task == null)
        {
            _logger.LogWarning("Task with ID {Id} not found.", id);
            return NotFound();  // Return 404 if the task does not exist
        }
        return Ok(task);    // Return the found task
    }

    // PUT: /api/tasks/{id}
    // Update an existing task by ID
    [HttpPut("{id}")]
    public IActionResult UpdateTask(int id, [FromBody] TaskManagementAPI.Models.Task updatedTask)
    {
        var task = _taskService.GetTask(id);
        if (task == null)
        {
            _logger.LogWarning("Task with ID {Id} not found.", id);
            return NotFound();  // Return 404 if the task does not exist
        }

        // Validate DueDate
        if (updatedTask.DueDate < DateTime.Now)
        {
            _logger.LogWarning("Invalid DueDate for task update. Task ID: {Id}", updatedTask.Id);
            return BadRequest("Due date cannot be in the past.");
        }

        // Prevent self-dependency
        if (updatedTask.Dependencies.Contains(updatedTask.Id))
        {
            _logger.LogWarning("Task with ID {Id} cannot depend on itself.", updatedTask.Id);
            return BadRequest("Task cannot depend on itself.");
        }

        // Validate dependencies (check for circular or missing dependencies)
        var validationResult = _taskService.ValidateDependencies(updatedTask);
        switch (validationResult)
        {
            case DependencyValidationResult.CircularDependency:
                _logger.LogWarning("Failed to update task ID {TaskId} due to circular dependency.", updatedTask.Id);
                return BadRequest("Circular dependency detected.");
            case DependencyValidationResult.MissingTask:
                return BadRequest("A dependent task is missing.");
            case DependencyValidationResult.NoIssues:
                // Add the new task using the TaskService
                // Try to add the task, checking for duplicate IDs
                // Update the task properties (e.g., Title, DueDate, Dependencies)
                task.Title = updatedTask.Title;
                task.DueDate = updatedTask.DueDate;
                task.IsCompleted = updatedTask.IsCompleted;
                task.Dependencies = updatedTask.Dependencies;

                return Ok(task);   // Return the updated task
            default:
                return BadRequest("Unknown error.");
        }
    }

    // DELETE: /api/tasks/{id}
    // Delete a task by ID
    [HttpDelete("{id}")]
    public IActionResult DeleteTask(int id) 
    {
        if (_taskService.DeleteTask(id))
        {
            _logger.LogInformation("Task with ID {Id} deleted.", id);
            return NoContent();  // Return 204 No Content if successful
        }

        _logger.LogWarning("Task with ID {Id} not found.", id);
        return NotFound();   // Return 404 if the task does not exist
    }



}
