using TodoApp.Data.Domain.Todos;

namespace TodoApp.Data.Infra.Repositories;

public interface ITodoEntityRepository
{
    Task AddAsync(TodoEntity todo);
    Task<TodoEntity?> GetAsync(Guid id);
    Task<IEnumerable<TodoEntity>> GetAllAsync(TodoState todoState);
    Task RemoveAsync(TodoEntity todo);
    Task SaveChangesAsync();
}
