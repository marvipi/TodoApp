namespace TodoApp.Data.Domain.Todos;

public record TodoPostRequest(string? Description, DateTime? DueDate);