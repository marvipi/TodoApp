using Newtonsoft.Json;
using TodoApp.Website.Models.Todos;

namespace TodoApp.Website.Services;

public class TodoDataService : ITodoDataService
{
    readonly HttpClient _client;
    readonly Uri _apiUri;

    public TodoDataService(HttpClient client, IConfiguration configuration)
    {
        _client = client;
        _apiUri = new Uri(configuration["BackendUrl"]);
    }

    public async Task CreateAsync(TodoPostRequest request)
    {
        var response = await _client.PostAsJsonAsync(_apiUri, request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<TodoModel?> GetAsync(Guid id)
    {
        var requestUri = _apiUri.AbsoluteUri + $"/{id}";
        try
        {
            var responseBody = await _client.GetStringAsync(requestUri);
            var todo = JsonConvert.DeserializeObject<TodoModel>(responseBody);
            return todo;
        }
        catch
        {
            return null;
        }
    }

    public async Task<TodoViewModel?> GetAllAsync(TodoState todoState)
    {
        var requestUri = _apiUri.AbsoluteUri + $"/{todoState}";
        try
        {
            var responseBody = await _client.GetStringAsync(requestUri);

            var todos = responseBody is null || responseBody == string.Empty
                ? new List<TodoModel>()
                : JsonConvert.DeserializeObject<List<TodoModel>>(responseBody);

            var viewModel = new TodoViewModel(todos);
            return viewModel;
        }
        catch
        {
            return null;
        }
    }

    public async Task RemoveAsync(Guid id)
    {
        var requestUri = _apiUri.AbsoluteUri + $"/{id}";
        await _client.DeleteAsync(requestUri);
    }

    public async Task UpdateAsync(Guid id, TodoPutRequest request)
    {
        var requestUri = _apiUri + $"/{id}";
        var response = await _client.PutAsJsonAsync(requestUri, request);
        response.EnsureSuccessStatusCode();
    }
}
