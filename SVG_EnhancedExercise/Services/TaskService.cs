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
                throw new KeyNotFoundException($"Task with ID {id} was not found.");
            }

            return _tasks[id];  // Return the found task
        }

        // Add a new task to the dictionary
        public void AddTask(TaskManagementAPI.Models.Task task)
        {
            _tasks.TryAdd(task.Id, task);
            _logger.LogInformation("Added new task with ID {Id} and Title: {Title}", task.Id, task.Title);
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
        private bool HasCircularDependency(int taskId, HashSet<int> visited)
        {
            if (visited.Contains(taskId))
            {
                return true;  // Circular dependency detected
            }

            // Mark the current task as visited
            var task = GetTask(taskId);
            if (task == null)
            {
                return false;  // No task found, no circular dependency
            }

            visited.Add(taskId);
            foreach (var depId in task.Dependencies)
            {
                if (HasCircularDependency(depId, visited)) // Recursively check dependencies
                {
                    return true;  // Found a circular dependency
                }
            }

            // Backtrack: Remove the task from visited set
            visited.Remove(taskId);
            return false;
        }

        // Validates that a task's dependencies do not form a circular dependency
        public bool ValidateDependencies(TaskManagementAPI.Models.Task task)
        {
            var visited = new HashSet<int>();
            return !HasCircularDependency(task.Id, visited);  // Returns false if a circular dependency is found
        }



    }
}
