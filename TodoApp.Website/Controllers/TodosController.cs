using Microsoft.AspNetCore.Mvc;
using TodoApp.Website.Models.Todos;
using TodoApp.Website.Services;

namespace TodoApp.Website.Controllers;

public class TodosController : Controller
{
    readonly ITodoDataService _todoDataRetriver;

    public TodosController(ITodoDataService todoDataRetriver)
    {
        _todoDataRetriver = todoDataRetriver;
    }

    public async Task<IActionResult> Index()
    {
        return await GetViewModelAsync(TodoState.Due);
    }

    public async Task<IActionResult> Done()
    {
        return await GetViewModelAsync(TodoState.Done);
    }

    public async Task<IActionResult> Cancelled()
    {
        return await GetViewModelAsync(TodoState.Cancelled);
    }

    async Task<IActionResult> GetViewModelAsync(TodoState todoState)
    {
        var viewModel = await _todoDataRetriver.GetAllAsync(todoState);
        return View(viewModel);
    }

    [Route("[controller]/removeasync")]
    [HttpPost]
    public async Task<IActionResult> RemoveAsync(Guid id, TodoState todoState)
    {
        await _todoDataRetriver.RemoveAsync(id);
        return RedirectByTodoState(todoState);
    }

    [Route("[controller]/updateasync")]
    [HttpPost]
    public async Task<IActionResult> UpdateAsync(Guid id, TodoState oldState, TodoState newState, DateTime modificationDate)
    {
        var request = new TodoPutRequest(newState, modificationDate);
        await _todoDataRetriver.UpdateAsync(id, request);
        return RedirectByTodoState(oldState);
    }

    [Route("[controller]/createasync")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(string description, DateTime dueDate)
    {
        var todo = new TodoValidationModel(description, dueDate);

        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index");
        }

        var request = new TodoPostRequest(description, dueDate);
        await _todoDataRetriver.CreateAsync(request);
        return RedirectToAction("Index");
    }

    IActionResult RedirectByTodoState(TodoState todoState)
    {
        var action = todoState switch
        {
            TodoState.Due => "Index",
            TodoState.Done => "Done",
            TodoState.Cancelled => "Cancelled",
            _ => "Index",
        };
        return RedirectToAction(action);
    }
}
