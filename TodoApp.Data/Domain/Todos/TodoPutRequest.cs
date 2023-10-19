namespace TodoApp.Data.Domain.Todos;

public record TodoPutRequest(TodoState? TodoState, DateTime? ModificationDate);