using Microsoft.EntityFrameworkCore;
using TodoApp.Data.Domain.Todos;

namespace TodoApp.Data.Infra.Repositories;

public class ApplicationDbContext : DbContext
{
    public const int DESCRIPTION_MAX_LENGTH = 100;

    const int TODOSTATE_MAX_LENGTH = 10;

    public DbSet<TodoEntity> Todos { get; init; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        SetupTodoEntity(modelBuilder);
    }

    static void SetupTodoEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoEntity>()
                    .Property(te => te.Description)
                    .HasMaxLength(DESCRIPTION_MAX_LENGTH)
                    .IsUnicode()
                    .IsRequired();

        modelBuilder.Entity<TodoEntity>()
            .Property(te => te.DueDate)
            .IsRequired();

        modelBuilder.Entity<TodoEntity>()
            .Property(te => te.TodoState)
            .HasConversion<string>()
            .HasMaxLength(TODOSTATE_MAX_LENGTH)
            .IsRequired();

        modelBuilder.Entity<TodoEntity>()
            .Property(te => te.CompletionDate)
            .IsRequired(false);

        modelBuilder.Entity<TodoEntity>()
            .Property(te => te.CancellationDate)
            .IsRequired(false);

        modelBuilder.Entity<TodoEntity>()
            .Ignore(te => te.CurrentStateDate);
    }
}
