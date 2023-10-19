using System.ComponentModel.DataAnnotations;

namespace TodoApp.Website.Models.Todos;

public class TodoValidationModel
{
    [Required(ErrorMessage = "A descrição é obrigatória")]
    [RegularExpression(pattern: @".*\S+.*", ErrorMessage = "A descrição é obrigatória")]
    [MinLength(1, ErrorMessage = "A descrição deve ter pelo menos um caractere")]
    [MaxLength(100, ErrorMessage = "A descrição deve ter no máximo 100 caracteres")]
    public string Description { get; init; }

    [Required(ErrorMessage = "O prazo de entrega é obrigatório")]
    public DateTime DueDate { get; init; }

    public TodoValidationModel(string description, DateTime dueDate)
    {
        Description = description;
        DueDate = dueDate;
    }
}
