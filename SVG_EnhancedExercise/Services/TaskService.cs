using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TaskManagementAPI.Models;  

namespace TaskManagementAPI.Services
{
   
    // Service to manage tasks, handle task creation, completion, and dependencies
    public class TaskService
    {
        // Thread-safe dictionary to store tasks, keyed by their ID
        private readonly ConcurrentDictionary<int, TaskManagementAPI.Models.Task> _tasks = new ConcurrentDictionary<int, TaskManagementAPI.Models.Task>();
        private readonly ILogger<TaskService> _logger;

        // Constructor to inject logger into the service
        public TaskService(ILogger<TaskService> logger)
        {
            _logger = logger;
        }

        // Fetch a task by ID. Returns null if the task doesn't exist.
        public TaskManagementAPI.Models.Task GetTask(int id)
        {
            _logger.LogInformation("Fetching task with ID {Id}", id);

            // Check if the task exists, return it if found, otherwise log a warning
            if (!_tasks.ContainsKey(id))
            {
                _logger.LogWarning("Task with ID {Id} was not found.", id);
                return null;  // Return null to indicate the task was not found
            }

            return _tasks[id];  // Return the found task
        }

        // Add a new task to the dictionary
        public bool AddTask(TaskManagementAPI.Models.Task task)
        {
            // Check if a task with the same ID already exists
            if (_tasks.ContainsKey(task.Id))
            {
                _logger.LogWarning("Task with ID {TaskId} already exists. Cannot add duplicate.", task.Id);
                return false;  // Indicate failure to add due to duplicate ID
            }

            // Try to add the task to the dictionary
            var result = _tasks.TryAdd(task.Id, task);

            if (result)
            {
                _logger.LogInformation("Task with ID {TaskId} added successfully.", task.Id);
            }
            else
            {
                _logger.LogError("Failed to add task with ID {TaskId}.", task.Id);
            }

            return result;  // Return true if successfully added, false otherwise
        }

        // Check if a task can be marked as complete
        public bool CanCompleteTask(int taskId)
        {
            _logger.LogInformation("Checking if task with ID {Id} can be completed", taskId);
            var task = GetTask(taskId);

            // Task doesn't exist or is already completed
            if (task == null || task.IsCompleted)
            {
                _logger.LogWarning("Task with ID {Id} cannot be completed. Either it does not exist or it is already completed.", taskId);
                return false;
            }

            // Check if all dependent tasks are completed
            bool canComplete = task.Dependencies.All(depId =>
                _tasks.TryGetValue(depId, out var depTask) && depTask.IsCompleted);

            // Log a warning if dependencies are not completed
            if (!canComplete)
            {
                _logger.LogWarning("Task with ID {Id} cannot be completed because dependent tasks are not finished.", taskId);
            }

            return canComplete;
        }

        // Mark a task as complete if all dependencies are satisfied
        public bool CompleteTask(int taskId)
        {
            _logger.LogInformation("Attempting to complete task with ID {Id}", taskId);

            // Check if the task can be completed
            if (CanCompleteTask(taskId))
            {
                var task = GetTask(taskId);
                task!.IsCompleted = true;
                _logger.LogInformation("Task with ID {Id} marked as completed.", taskId);
                return true;
            }

            _logger.LogWarning("Task with ID {Id} could not be completed.", taskId);
            return false;
        }

        public IEnumerable<TaskManagementAPI.Models.Task> GetAllTasks()
        {
            return _tasks.Values.ToList();  // Return all tasks in the dictionary
        }

        public bool DeleteTask(int id)
        {
            return _tasks.TryRemove(id, out _);  // Remove the task from the dictionary
        }

        // Check if there's a circular dependency using Depth-First Search (DFS)
        private int HasCircularDependency(int taskId, HashSet<int> visited, TaskManagementAPI.Models.Task currentTask)
        {
            // If the task is already in the visited set, a circular dependency is detected
            if (visited.Contains(taskId))
            {
                _logger.LogWarning("Circular dependency detected while checking task ID {TaskId}.", taskId);
                return 1;  // Circular dependency detected (flag: 1)
            }

            // Use the currentTask object if taskId matches its ID, otherwise get the task from the dictionary
            TaskManagementAPI.Models.Task task = (taskId == currentTask.Id) ? currentTask : GetTask(taskId);

            // If the task does not exist (missing dependency), return -1 to indicate missing task
            if (task == null)
            {
                return -1;  // Missing task detected (flag: -1)
            }

            // Mark the task as visited
            visited.Add(taskId);

            // Recursively check each dependency
            foreach (var depId in task.Dependencies)
            {
                int result = HasCircularDependency(depId, visited, currentTask);
                if (result != 0)  // If a circular dependency or missing dependency is found
                {
                    return result;  // Return the result (1 for circular, -1 for missing)
                }
            }

            // Backtrack: Remove the task from the visited set
            visited.Remove(taskId);

            return 0;  // No issues found (flag: 0)
        }

        // Validates that a task's dependencies do not form a circular dependency
        public int ValidateDependencies(TaskManagementAPI.Models.Task task)
        {
            // If the task has no dependencies, there's nothing to validate
            if (task.Dependencies == null || !task.Dependencies.Any())
            {
                _logger.LogInformation("Task ID {TaskId} has no dependencies to validate.", task.Id);
                return 0;  // No dependencies, so no issues
            }

            // Track visited tasks to avoid redundant checks
            var visited = new HashSet<int>();

            // Check for circular dependencies or missing tasks
            return HasCircularDependency(task.Id, visited, task);  // Returns 0 (no issues), 1 (circular), or -1 (missing)

        }



    }
}
