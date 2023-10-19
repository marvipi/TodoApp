namespace TodoApp.Data.Domain.Todos;

public class TodoEntity : Entity
{
    public string Description { get; init; }
    public DateTime CurrentStateDate
    {
        get
        {
            return TodoState switch
            {
                TodoState.Due => DueDate,
                TodoState.Done => CompletionDate.Value,
                TodoState.Cancelled => CancellationDate.Value
            };
        }
        private set
        {
            switch (TodoState)
            {
                case TodoState.Due:
                    DueDate = value;
                    break;
                case TodoState.Done:
                    CompletionDate = value;
                    break;
                case TodoState.Cancelled:
                    CancellationDate = value;
                    break;
            };
        }
    }
    public TodoState TodoState { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? CompletionDate { get; private set; }
    public DateTime? CancellationDate { get; private set; }

    public TodoEntity(string description, DateTime dueDate)
    {
        Description = description;
        DueDate = dueDate;
        TodoState = TodoState.Due;
    }

    public void Update(TodoState newState, DateTime modificationDate)
    {
        switch (TodoState, newState)
        {
            case (TodoState.Done, TodoState.Due):
            case (TodoState.Cancelled, TodoState.Due):
                TodoState = newState;
                break;
            case (TodoState.Due, TodoState.Done):
            case (TodoState.Due, TodoState.Cancelled):
                TodoState = newState;
                CurrentStateDate = modificationDate;
                break;
            case (TodoState.Done, TodoState.Cancelled):
                throw new System.NotSupportedException("Cannot cancel a completed todo");
            case (TodoState.Cancelled, TodoState.Done):
                throw new System.NotSupportedException("Cannot complete a cancelled todo");
        }
    }
}
