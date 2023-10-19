namespace TodoApp.Website.Models.Todos;

public class TodoModel
{
	public Guid Id { get; init; }
	public string Description { get; set; }
	public TodoState TodoState { get; set; }
	public DateTime CurrentStateDate { get; set; }

    public TodoModel(Guid id, string description, TodoState todoState, DateTime currentStateDate)
    {
		Id = id;
		Description = description;
		TodoState = todoState;
		CurrentStateDate = currentStateDate;
    }
}
