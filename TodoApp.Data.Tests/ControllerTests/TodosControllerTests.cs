using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoApp.Data.Controllers;
using TodoApp.Data.Domain.Todos;
using TodoApp.Data.Infra.Repositories;
using TodoApp.Data.Services;

namespace TodoApp.Data.Tests.ControllerTests;

[TestFixture]
internal class TodosControllerTests
{
    [Test]
    public async Task CreateAsync_InvalidRequest_Returns400BadRequestAlongWithErrorMessages()
    {
        var request = new TodoPostRequest(null, null);
        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        var controller = new TodosController(validator, repository.Object);

        var actionResult = await controller.CreateAsync(request);
        var result = actionResult as BadRequestObjectResult;
        var validationProblemDetails = result?.Value as ValidationProblemDetails;
        var errors = validationProblemDetails?.Errors;

        Assert.Multiple(() =>
        {
            Assert.That(actionResult, Is.TypeOf<BadRequestObjectResult>());
            Assert.That(errors, Is.Not.Null);
            Assert.That(errors, Contains.Key("Error"));
            Assert.That(errors["Error"], Is.Not.Empty);
        });
    }

    [Test]
    public async Task CreateAsync_ValidRequest_ReturnsResult201CreatedAtAction()
    {
        var request = new TodoPostRequest("Take out the trash", new DateTime(2023, 10, 5));
        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        var controller = new TodosController(validator, repository.Object);

        var actionResult = await controller.CreateAsync(request);

        Assert.That(actionResult, Is.TypeOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task CreateAsync_ValidRequest_AddsANewTodoEntityToTheDatabase()
    {
        var request = new TodoPostRequest("Mock db", new DateTime(2023, 10, 7));
        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        var newTodo = new TodoEntity(request.Description, request.DueDate.Value);
        repository
            .Setup(r => r.AddAsync(newTodo));
        repository
            .Setup(r => r.GetAsync(newTodo.Id))
            .ReturnsAsync(newTodo);
        var controller = new TodosController(validator, repository.Object);


        var actionResult = await controller.CreateAsync(request);
        var todoInRepository = await repository.Object.GetAsync(newTodo.Id);


        Assert.Multiple(() =>
        {
            Assert.That(todoInRepository?.Id, Is.EqualTo(newTodo.Id));
            Assert.That(todoInRepository?.Description, Is.EqualTo(newTodo.Description));
            Assert.That(todoInRepository?.DueDate, Is.EqualTo(newTodo.DueDate));
            Assert.That(todoInRepository?.TodoState, Is.EqualTo(newTodo.TodoState));
        });
    }

    [Test]
    public async Task GetAsync_IdNotInTheDatabase_Returns404NotFound()
    {
        var queryId = Guid.Empty;
        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        var controller = new TodosController(validator, repository.Object);

        var actionResult = await controller.GetAsync(queryId);
        var result = actionResult.Result;

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetAsync_IdIsInDatabase_ReturnsOkAlongWithATodoGetResponse()
    {
        var todoPostRequest = new TodoPostRequest("Test GetAsync method", new DateTime(2023, 10, 7, 9, 50, 24));
        var newTodo = new TodoEntity(todoPostRequest.Description, todoPostRequest.DueDate.Value);
        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        repository
            .Setup(r => r.AddAsync(newTodo));
        repository
            .Setup(r => r.GetAsync(newTodo.Id))
            .ReturnsAsync(newTodo);
        var controller = new TodosController(validator, repository.Object);


        await controller.CreateAsync(todoPostRequest);
        var actionResult = await controller.GetAsync(newTodo.Id);
        var result = actionResult.Result as OkObjectResult;
        var response = result?.Value as TodoGetResponse;


        Assert.Multiple(() =>
        {
            Assert.That(actionResult.Result, Is.TypeOf<OkObjectResult>());
            Assert.That(response?.Id, Is.EqualTo(newTodo.Id));
            Assert.That(response?.Description, Is.EqualTo(newTodo.Description));
            Assert.That(response?.CurrentStateDate, Is.EqualTo(newTodo.DueDate));
            Assert.That(response?.TodoState, Is.EqualTo(newTodo.TodoState));
        });
    }

    [Test]
    public async Task GetAllAsync_NoTodosInRepository_ReturnsNoContent()
    {
        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        repository
            .Setup(r => r.GetAllAsync(It.IsAny<TodoState>()))
            .ReturnsAsync(new List<TodoEntity>());
        var controller = new TodosController(validator, repository.Object);

        var actionResult = await controller.GetAllAsync(TodoState.Due);
        var result = actionResult.Result;

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    static readonly IEnumerable<TodoEntity> dueTodos = CreateTodos();
    static readonly IEnumerable<TodoEntity> doneTodos = CreateDoneTodos();
    static readonly IEnumerable<TodoEntity> cancelledTodos = CreateCancelledTodos();
    static readonly IEnumerable<TodoEntity> allStatesTodos = dueTodos.Concat(doneTodos).Concat(cancelledTodos);
    static readonly (IEnumerable<TodoEntity>, TodoState)[] getPagedSortedTestParams =
    {
        (dueTodos, TodoState.Due),
        (doneTodos, TodoState.Done),
        (cancelledTodos, TodoState.Cancelled),

        (dueTodos, TodoState.Due),
        (doneTodos, TodoState.Done),
        (cancelledTodos, TodoState.Cancelled),

        (dueTodos, TodoState.Due),
        (doneTodos, TodoState.Done),
        (cancelledTodos, TodoState.Cancelled),

        (allStatesTodos, TodoState.Due),
        (allStatesTodos, TodoState.Done),
        (allStatesTodos, TodoState.Cancelled)
    };
    [TestCaseSource(nameof(getPagedSortedTestParams))]
    public async Task GetAllAsync_TodosInRepository_ReturnsOkWithResponsesSortedByDateInAscendingOrder(
        (IEnumerable<TodoEntity>, TodoState) testParams)
    {
        (var todosInRepository, var expectedState) = testParams;

        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        repository
            .Setup(r => r.GetAllAsync(expectedState))
            .ReturnsAsync(todosInRepository
                            .Where(todo => todo.TodoState == expectedState));
        var controller = new TodosController(validator, repository.Object);


        var actionResult = await controller.GetAllAsync(expectedState);
        var result = actionResult.Result as OkObjectResult;
        var response = result?.Value as IEnumerable<TodoGetResponse>;
        var todoStates = response?.Select(r => r.TodoState);
        var todosInRepoCount = todosInRepository.Where(t => t.TodoState == expectedState).Count();

        Assert.Multiple(() =>
        {
            Assert.That(actionResult.Result, Is.TypeOf<OkObjectResult>());
            Assert.That(todoStates.Count(), Is.EqualTo(todosInRepoCount));
            Assert.That(todoStates, Is.All.EqualTo(expectedState));
            Assert.That(response, Is.Ordered.Ascending.By("CurrentStateDate"));
        });
    }

    [Test]
    public async Task RemoveAsync_InvalidId_ReturnsNotFound()
    {
        var invalidId = Guid.Empty;
        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        repository
            .Setup(r => r.GetAsync(invalidId))
            .ReturnsAsync(null as TodoEntity);
        var controller = new TodosController(validator, repository.Object);

        var actionResult = await controller.RemoveAsync(invalidId);

        Assert.That(actionResult, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task RemoveAsync_ValidId_RemovesTodoEntityFromDb()
    {
        var newTodo = new TodoEntity("Doesn't matter", new DateTime(2023, 10, 08, 9, 30, 3));
        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        repository
            .Setup(r => r.GetAsync(newTodo.Id))
            .ReturnsAsync(newTodo);
        repository
            .Setup(r => r.RemoveAsync(newTodo));
        var controller = new TodosController(validator, repository.Object);

        var actionResult = await controller.RemoveAsync(newTodo.Id);

        Assert.That(actionResult, Is.TypeOf<NoContentResult>());
        repository.Verify(r => r.RemoveAsync(newTodo), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_InvalidRequest_ReturnsBadRequestWithNotificationErrors()
    {
        var request = new TodoPutRequest(null, null);
        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        var controller = new TodosController(validator, repository.Object);

        var actionResult = await controller.UpdateAsync(Guid.NewGuid(), request);
        var result = actionResult as BadRequestObjectResult;
        var validationProblemDetails = result?.Value as ValidationProblemDetails;
        var errors = validationProblemDetails?.Errors;

        Assert.Multiple(() =>
        {
            Assert.That(actionResult, Is.TypeOf<BadRequestObjectResult>());
            Assert.That(errors, Contains.Key("Error"));
            Assert.That(errors["Error"], Is.Not.Empty);
        });
    }

    [Test]
    public async Task UpdateAsync_IdNotInRepository_ReturnsNotFound()
    {
        var invalidId = Guid.Empty;
        var request = new TodoPutRequest(TodoState.Done, new DateTime(2023, 10, 8, 13, 16, 31));
        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        repository
            .Setup(r => r.GetAsync(invalidId))
            .ReturnsAsync(null as TodoEntity);
        var controller = new TodosController(validator, repository.Object);

        var actionResult = await controller.UpdateAsync(invalidId, request);

        Assert.That(actionResult, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateAsync_IdInRepositoryAndValidRequest_ReturnsNoContent()
    {
        var todo = new TodoEntity("Refactor the project", new DateTime(2023, 10, 7, 23, 59, 59));
        var request = new TodoPutRequest(TodoState.Cancelled, new DateTime(2023, 10, 8, 13, 18, 54));
        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        repository
            .Setup(r => r.GetAsync(todo.Id))
            .ReturnsAsync(todo);
        repository
            .Setup(r => r.SaveChangesAsync());
        var controller = new TodosController(validator, repository.Object);


        var actionResult = await controller.UpdateAsync(todo.Id, request);


        Assert.Multiple(() =>
        {
            repository.Verify(r => r.SaveChangesAsync(), Times.Once());
            Assert.That(actionResult, Is.TypeOf<NoContentResult>());
        });
    }

    [TestCase(TodoState.Done, TodoState.Cancelled)]
    [TestCase(TodoState.Cancelled, TodoState.Done)]
    public async Task UpdateAsync_InvalidTodoStateUpdate_ReturnsConflictContainingTheValidationProblems
        (TodoState currentState, TodoState invalidNewState)
    {
        var todo = new TodoEntity("Test the exception catching in the updateasync method", new DateTime(2023, 10, 9, 8, 38, 25));
        var validator = new TodoRequestValidator();
        var repository = new Mock<ITodoEntityRepository>();
        repository
            .Setup(r => r.GetAsync(todo.Id))
            .ReturnsAsync(todo);
        var controller = new TodosController(validator, repository.Object);


        todo.Update(currentState, new DateTime(2023, 10, 9, 8, 40, 51));
        var request = new TodoPutRequest(invalidNewState, new DateTime(2023, 10, 9, 8, 42, 42));
        var actionResult = await controller.UpdateAsync(todo.Id, request);
        var result = actionResult as ConflictObjectResult;
        var problemDetails = result?.Value as ValidationProblemDetails;
        var errors = problemDetails?.Errors;


        Assert.Multiple(() =>
        {
            Assert.That(actionResult, Is.TypeOf<ConflictObjectResult>());
            Assert.That(errors, Contains.Key("Error"));
            Assert.That(errors["Error"], Is.Not.Empty);
        });
    }

    static IEnumerable<TodoEntity> CreateTodos()
    {
        var now = new DateTime(2023, 10, 7, 12, 53, 37);
        var todosInRepository = new List<TodoEntity>();
        for (int i = 0; i < 50; i++)
        {
            var todo = new TodoEntity("Doesn't matter", now.AddHours(i));
            todosInRepository.Add(todo);
        }
        return todosInRepository;
    }

    static IEnumerable<TodoEntity> CreateDoneTodos()
    {
        var todos = CreateTodos().ToList();
        var completionDate = new DateTime(2023, 10, 7, 18, 57, 9);
        for (int i = 0; i < todos.Count; i++)
        {
            todos[i].Update(TodoState.Done, completionDate.AddHours(1));
        }
        return todos;
    }

    static IEnumerable<TodoEntity> CreateCancelledTodos()
    {
        var todos = CreateTodos().ToList();
        var cancellationDate = new DateTime(2023, 10, 07, 18, 47, 11);
        for (int i = 0; i < todos.Count; i++)
        {
            todos[i].Update(TodoState.Cancelled, cancellationDate.AddHours(i));
        }
        return todos;
    }
}
