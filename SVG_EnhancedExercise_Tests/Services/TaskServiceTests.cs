using Xunit;
using Moq;
using FluentAssertions;
using TaskManagementAPI.Models;
using TaskManagementAPI.Services;
using Microsoft.Extensions.Logging;

namespace SVG_EnhancedExercise_Tests.Services
{
    public class TaskServiceTests
    {
        private readonly TaskService _taskService;
        private readonly Mock<ILogger<TaskService>> _mockLogger;

        public TaskServiceTests()
        {
            // Mock the logger to avoid dealing with actual logs during testing
            _mockLogger = new Mock<ILogger<TaskService>>();
            _taskService = new TaskService(_mockLogger.Object);
        }

        [Fact]
        public void AddTask_Should_Add_ValidTask()
        {
            // Arrange
            var task = new TaskManagementAPI.Models.Task
            {
                Id = 1,
                Title = "Test Task",
                DueDate = DateTime.Now.AddDays(5),
                Dependencies = new List<int>()
            };

            // Act
            _taskService.AddTask(task);

            // Assert
            var result = _taskService.GetTask(task.Id);
            result.Should().NotBeNull();
            result.Title.Should().Be("Test Task");
            result.DueDate.Should().BeCloseTo(DateTime.Now.AddDays(5), TimeSpan.FromSeconds(1));  // Ensure DueDate is correct
        }

        [Fact]
        public void ValidateDependencies_Should_ReturnOne_When_CircularDependencyExists()
        {
            // Arrange
            var taskA = new TaskManagementAPI.Models.Task { Id = 1, Title = "Task A", Dependencies = new List<int> { 2 } };  // Task A depends on Task B
            var taskB = new TaskManagementAPI.Models.Task { Id = 2, Title = "Task B", Dependencies = new List<int> { 1 } };  // Task B depends on Task A

            // Add the tasks to the service
            _taskService.AddTask(taskA);
            _taskService.AddTask(taskB);

            // Act
            var result = _taskService.ValidateDependencies(taskA);  // Check for circular dependencies starting from Task A

            // Assert
            Assert.Equal(DependencyValidationResult.CircularDependency, result);  // Circular dependency detected, so validation should return 1
        }

        [Fact]
        public void ValidateDependencies_Should_ReturnNegativeOne_When_MissingDependencyExists()
        {
            // Arrange
            var taskC = new TaskManagementAPI.Models.Task { Id = 3, Title = "Task C", Dependencies = new List<int> { 14 } };  // Task C depends on Task D, which does not exist

            // Add only Task C to the service, but not Task D
            _taskService.AddTask(taskC);

            // Act
            var result = _taskService.ValidateDependencies(taskC);  // Task D (ID: 4) is missing

            // Assert
            Assert.Equal(DependencyValidationResult.MissingTask, result);  // Missing dependency, so validation should return -1
  
        }

        [Fact]
        public void CompleteTask_Should_Fail_If_Dependencies_Not_Completed()
        {
            // Arrange
            var task1 = new TaskManagementAPI.Models.Task { Id = 1, Title = "Task 1", Dependencies = new List<int> { 2 } };
            var task2 = new TaskManagementAPI.Models.Task { Id = 2, Title = "Task 2", IsCompleted = false };

            _taskService.AddTask(task1);
            _taskService.AddTask(task2);

            // Act
            var canComplete = _taskService.CompleteTask(1);

            // Assert
            canComplete.Should().BeFalse();  // Task 1 cannot be completed because Task 2 is not completed
        }

        [Fact]
        public void CompleteTask_Should_Succeed_If_All_Dependencies_Completed()
        {
            // Arrange
            var task1 = new TaskManagementAPI.Models.Task { Id = 1, Title = "Task 1", Dependencies = new List<int> { 2 } };
            var task2 = new TaskManagementAPI.Models.Task { Id = 2, Title = "Task 2", IsCompleted = true };

            _taskService.AddTask(task1);
            _taskService.AddTask(task2);

            // Act
            var canComplete = _taskService.CompleteTask(1);

            // Assert
            canComplete.Should().BeTrue();  // Task 1 can be completed because Task 2 is completed
        }

        [Fact]
        public void CompleteTask_Should_HandleConcurrency_When_SimultaneousAttemptsToCompleteTasks()
        {
            // Arrange
            var taskA = new TaskManagementAPI.Models.Task { Id = 1, Title = "Task A", Dependencies = new List<int> { 2 }, IsCompleted = false };  // Task A depends on Task B
            var taskB = new TaskManagementAPI.Models.Task { Id = 2, Title = "Task B", IsCompleted = false };  // Task B has no dependencies

            _taskService.AddTask(taskA);
            _taskService.AddTask(taskB);

            // Act
            Parallel.ForEach(new[] { taskA, taskB }, task =>
            {
                var result = _taskService.CompleteTask(task.Id);  // Simulate concurrent task completion
                if (task.Id == 1)
                {
                    // Task A should not be completed until Task B is completed
                    Assert.False(result);
                }
                else if (task.Id == 2)
                {
                    // Task B can be completed independently
                    Assert.True(result);
                }
            });
        }

    }
}
