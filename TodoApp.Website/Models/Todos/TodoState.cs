using System.Text.Json.Serialization;

namespace TodoApp.Website.Models.Todos;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TodoState
{
    Due = 10,
    Done = 20,
    Cancelled = 30,
}