using Microsoft.EntityFrameworkCore;
using Todo.Domain.Entities;

namespace Todo.Infrastructure.Persistence;

public sealed class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    public DbSet<TodoTask> Tasks => Set<TodoTask>();
    public DbSet<TaskList> Lists => Set<TaskList>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TaskList
        modelBuilder.Entity<TaskList>(b =>
        {
            b.ToTable("Lists");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(80);
        });

        // TodoTask
        modelBuilder.Entity<TodoTask>(b =>
        {
            b.ToTable("Tasks");
            b.HasKey(x => x.Id);

            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.IsCompleted).IsRequired();
            b.Property(x => x.Deadline);

            b.HasOne<TaskList>()
                .WithMany()
                .HasForeignKey(x => x.ListId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.ListId);
        });
    }
}
