namespace TodoApp.Data.Domain.Todos;

public record TodoGetResponse(Guid Id, string Description, TodoState TodoState, DateTime Date);
