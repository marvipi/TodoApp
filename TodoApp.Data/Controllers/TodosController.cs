using Flunt.Notifications;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Data.Domain.Todos;
using TodoApp.Data.Extensions;
using TodoApp.Data.Infra.Repositories;
using TodoApp.Data.Services;

namespace TodoApp.Data.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TodosController : ControllerBase
{
    readonly TodoRequestValidator _validator;
    readonly ITodoEntityRepository _repository;

    public TodosController(TodoRequestValidator validator, ITodoEntityRepository repository)
    {
        _validator = validator;
        _repository = repository;
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(TodoPostRequest request)
    {
        var isRequestValid = _validator.Validate(request);
        if (!isRequestValid)
        {
            return GetValidationProblems(_validator.Notifications);
        }
        else
        {
            var newTodo = new TodoEntity(request.Description, request.DueDate.Value);
            await _repository.AddAsync(newTodo);
            return CreatedAtAction(nameof(GetAsync), new { id = newTodo.Id }, new { id = newTodo.Id });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateAsync(Guid id, TodoPutRequest request)
    {
        var isRequestValid = _validator.Validate(request);
        if (!isRequestValid)
        {
            return GetValidationProblems(_validator.Notifications);
        }

        var todo = await _repository.GetAsync(id);
        if (todo is null)
        {
            return NotFound();
        }

        try
        {
            todo.Update(request.TodoState.Value, request.ModificationDate.Value);
        }
        catch (NotSupportedException e)
        {
            return GetValidationProblems(e);
        }

        await _repository.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [ActionName(nameof(GetAsync))]
    public async Task<ActionResult<TodoGetResponse>> GetAsync(Guid id)
    {
        var todo = await _repository.GetAsync(id);
        if (todo is null)
        {
            return NotFound();
        }
        else
        {
            var response = new TodoGetResponse(todo.Id, todo.Description, todo.TodoState, todo.CurrentStateDate);
            return Ok(response);
        }
    }

    [HttpGet("{todoState}")]
    public async Task<ActionResult<IEnumerable<TodoGetResponse>>> GetAllAsync(TodoState todoState)
    {
        var todos = await _repository.GetAllAsync(todoState);

        if (!todos.Any())
        {
            return NoContent();
        }
        else
        {
            var response = todos
                .OrderBy(todo => todo.CurrentStateDate)
                .Select(todo => new TodoGetResponse(todo.Id, todo.Description, todo.TodoState, todo.CurrentStateDate))
                .ToList();
            return Ok(response);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> RemoveAsync(Guid id)
    {
        var todoToRemove = await _repository.GetAsync(id);
        if (todoToRemove is null)
        {
            return NotFound();
        }
        else
        {
            await _repository.RemoveAsync(todoToRemove);
            return NoContent();
        }
    }

    ActionResult GetValidationProblems(IReadOnlyCollection<Notification> notifications)
    {
        var errors = notifications.ConvertToErrorsDictionary();
        var problemDetails = new ValidationProblemDetails(errors);
        return ValidationProblem(problemDetails);
    }

    ActionResult GetValidationProblems(NotSupportedException e)
    {
        var errors = e.ConvertToErrorsDictionary();
        var validationProblems = new ValidationProblemDetails(errors);
        return Conflict(validationProblems);
    }
}
