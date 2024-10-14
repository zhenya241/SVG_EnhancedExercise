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
        _taskService = taskService;
        _logger = logger;
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
}
