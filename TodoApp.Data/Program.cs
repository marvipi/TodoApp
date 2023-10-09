using TodoApp.Data.Infra.Repositories;
using TodoApp.Data.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RouteOptions>(routeOptions =>
{
    routeOptions.LowercaseUrls = true;
    routeOptions.LowercaseQueryStrings = true;
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;
builder.Services.AddSqlServer<ApplicationDbContext>(connectionString: configuration.GetConnectionString("TodoAppDb"));
builder.Services.AddScoped<ITodoEntityRepository, TodoEntitySqlServer>();
builder.Services.AddScoped<TodoRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
