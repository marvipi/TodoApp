namespace TodoApp.Website.Models.Todos;

public record TodoPutRequest(TodoState TodoState, DateTime ModificationDate);