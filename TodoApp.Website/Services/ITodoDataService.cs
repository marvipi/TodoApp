using TodoApp.Website.Models.Todos;

namespace TodoApp.Website.Services;

public interface ITodoDataService
{
	Task CreateAsync(TodoPostRequest request);
	Task UpdateAsync(Guid id, TodoPutRequest request);
	Task<TodoModel?> GetAsync(Guid id);
	Task<TodoViewModel?> GetAllAsync(TodoState todoState);
	Task RemoveAsync(Guid id);
}
