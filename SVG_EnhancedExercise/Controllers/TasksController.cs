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

        // Validate for circular dependencies
        if (!_taskService.ValidateDependencies(newTask))
        {
            _logger.LogWarning("Circular dependency detected for task ID {Id}.", newTask.Id);
            return BadRequest("Task has a circular dependency.");
        }

        // Add the new task using the TaskService
        _taskService.AddTask(newTask);
        _logger.LogInformation("Task with ID {Id} was created.", newTask.Id);
        return Ok(newTask);     // Return the created task as the response
    }

    // PUT: /api/tasks/{id}/complete
    // Endpoint to mark a task as complete
    [HttpPut("{id}/complete")]
    public IActionResult MarkTaskComplete(int id)
    {
        _logger.LogInformation("Received request to complete task with ID {Id}.", id);

        // Check if the task can be marked as complete
        if (_taskService.CompleteTask(id)) { 
            _logger.LogInformation("Task with ID {Id} was successfully completed.", id);
            return Ok();
         }

        // Return a BadRequest if the task cannot be completed
        _logger.LogWarning("Failed to complete task with ID {Id}.", id);
        return BadRequest("Cannot complete task, dependencies are incomplete or task does not exist.");
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

        // Validate for circular dependencies
        if (!_taskService.ValidateDependencies(updatedTask))
        {
            _logger.LogWarning("Circular dependency detected for task ID {Id}.", updatedTask.Id);
            return BadRequest("Task has a circular dependency.");
        }

        // Update the task properties (e.g., Title, DueDate, Dependencies)
        task.Title = updatedTask.Title;
        task.DueDate = updatedTask.DueDate;
        task.Dependencies = updatedTask.Dependencies;

        return Ok(task);   // Return the updated task
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
