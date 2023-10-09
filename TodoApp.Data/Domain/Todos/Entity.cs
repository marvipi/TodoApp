namespace TodoApp.Data.Domain.Todos;

public abstract class Entity
{
    public Guid Id { get; init; }

    public Entity()
    {
        Id = Guid.NewGuid();
    }
}
