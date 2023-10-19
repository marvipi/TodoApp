namespace TodoApp.Website.Models.Todos;

public class TodoViewModel
{
	public IEnumerable<TodoModel> Todos { get; set; }
    public int Page { get; set; }
    public int Rows { get; set; }

    public TodoViewModel(IEnumerable<TodoModel> todos)
    {
        Todos = todos;
    }
}
