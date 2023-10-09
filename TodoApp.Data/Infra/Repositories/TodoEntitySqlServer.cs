using Microsoft.EntityFrameworkCore;
using TodoApp.Data.Domain.Todos;

namespace TodoApp.Data.Infra.Repositories;

public class TodoEntitySqlServer : ITodoEntityRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TodoEntitySqlServer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(TodoEntity todo)
    {
        await _dbContext.Todos.AddAsync(todo);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<TodoEntity?> GetAsync(Guid id)
    {
        return await _dbContext.Todos.FindAsync(id);
    }

    public async Task<IEnumerable<TodoEntity>> GetPagedAsync(int page, int rows, TodoState todoState)
    {
        return await _dbContext.Todos
            .Where(todo => todo.TodoState == todoState)
            .Skip((page - 1) * rows)
            .Take(rows)
            .ToListAsync();
    }

    public async Task RemoveAsync(TodoEntity todo)
    {
        _dbContext.Todos.Remove(todo);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveAllAsync()
    {
        var todos = await _dbContext.Todos.ToListAsync();
        todos.ForEach(todo => _dbContext.Remove(todo));
        await _dbContext.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
