using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrientaTFG.Shared.Infrastructure.Model;

namespace OrientaTFG.Shared.Infrastructure.DBContext;

public class OrientaTFGContext : DbContext
{
    #region Db Sets

    public DbSet<Administrator> Administrators { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<MainTask> MainTasks { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<SubTask> SubTasks { get; set; }
    public DbSet<SubTaskStatus> SubTaskStatus { get; set; }
    public DbSet<TFG> TFGs { get; set; }
    public DbSet<Tutor> Tutors { get; set; }
    public DbSet<StudentAlertConfiguration> StudentsAlertConfigurations { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Message> Messages { get; set; }
    #endregion

    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrientaTFGContext"/> class
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <exception cref="ArgumentNullException">If any of the parameters is not provided</exception>
    public OrientaTFGContext(IConfiguration configuration)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
    }

    public OrientaTFGContext(DbContextOptions<OrientaTFGContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Overrides OnConfiguring
    /// </summary>
    /// <param name="optionsBuilder">The DbContextOptions builder</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (configuration != null)
        {
            optionsBuilder.UseSqlServer(this.configuration["DBConnectionString"]);
        }
    }

}
