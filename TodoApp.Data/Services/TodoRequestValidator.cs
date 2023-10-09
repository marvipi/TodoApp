using Flunt.Notifications;
using Flunt.Validations;
using TodoApp.Data.Domain.Todos;
using TodoApp.Data.Infra.Repositories;

namespace TodoApp.Data.Services;

public class TodoRequestValidator : Notifiable<Notification>
{
    public const int MIN_PAGE_VAL = 1;
    public const int MIN_ROWS_VAL = 1;
    public const int MAX_ROWS_VAL = 50;
    public const int DEFAULT_ROWS_VAL = 25;

    public bool Validate(TodoPostRequest request)
    {
        ValidateNullProperties(request);

        if (!IsValid)
        {
            return false;
        }
        else
        {
            ValidateDescription(request.Description);
            return IsValid;
        }
    }

    public bool Validate(TodoPutRequest request)
    {
        ValidateNullProperties(request);

        if (!IsValid)
        {
            return false;
        }
        else
        {
            ValidateDescription(request.Description);
            return IsValid;
        }
    }

    public (int validPage, int validRows) Validate(int? page, int? rows)
    {
        var validPage = page is null or < MIN_PAGE_VAL
            ? MIN_ROWS_VAL
            : page.Value;

        var validRows = rows switch
        {
            null => DEFAULT_ROWS_VAL,
            < MIN_ROWS_VAL => DEFAULT_ROWS_VAL,
            > MAX_ROWS_VAL => MAX_ROWS_VAL,
            _ => rows.Value
        };

        return (validPage, validRows);
    }

    void ValidateNullProperties(TodoPostRequest request)
    {
        var nullPropertiesContract = new Contract<TodoPostRequest>()
            .IsNotNull(request.Description, "Description", "Missing required parameter: Description")
            .IsNotNull(request.DueDate, "DueDate", "Missing required parameter: DueDate");

        AddNotifications(nullPropertiesContract);
    }

    void ValidateNullProperties(TodoPutRequest request)
    {
        var nullPropertiesContract = new Contract<TodoPutRequest>()
            .IsNotNull(request.Description, "Description", "Missing required parameter: Description")
            .IsNotNull(request.TodoState, "TodoState", "Missing required parameter: TodoState")
            .IsNotNull(request.ModificationDate, "ModificationDate", "Missing required parameter: ModificationDate");

        AddNotifications(nullPropertiesContract);
    }

    void ValidateDescription(string description)
    {
        var maxLength = ApplicationDbContext.DESCRIPTION_MAX_LENGTH;

        var descriptionContract = new Contract<TodoPostRequest>()
            .IsNotNullOrWhiteSpace(description, "Description", "Description cannot be blank")
            .IsNotNullOrEmpty(description, "Description", "Description cannot be blank")
            .IsTrue(description.All(c => c != '\0'), "Description", "Description cannot be blank")
            .IsLowerOrEqualsThan(description.Length,
                                maxLength, 
                                "Description", 
                                $"Description must contain at most {maxLength} characters");

        AddNotifications(descriptionContract);
    }
}
