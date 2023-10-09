namespace TodoApp.Data.Domain.Todos;

public record TodoPutRequest(string? Description, TodoState? TodoState, DateTime? ModificationDate);