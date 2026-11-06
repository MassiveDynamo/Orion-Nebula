using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Data
{
    public partial class OrionDbContext : DbContext
    {
        public static readonly string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Voyager;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30";

        public OrionDbContext(DbContextOptions<OrionDbContext> options) : base(options) { }


        // public DbSet<EDLog> EDLog { get; set; }

        public DbSet<EDSystemName> EDSystemName { get; set; }

        // FSDJump event entries
        public DbSet<Data.Models.FSDJump> FSDJump { get; set; }

        // FSS All Bodies Found event entries
        public DbSet<Data.Models.FSSAllBodiesFound> FSSAllBodiesFound { get; set; }

        // Supercruise Exit event entries
        public DbSet<Data.Models.SupercruiseExit> SupercruiseExit { get; set; }

        // Prospected Asteroid event entries (mining/resource gathering)
        public DbSet<Data.Models.ProspectedAsteroid> ProspectedAsteroid { get; set; }

        // Mining Refined event entries
        public DbSet<Data.Models.MiningRefined> MiningRefined { get; set; }

        public DbSet<Data.Models.StarSystem> StarSystem { get; set; }

        public OrionDbContext() : base()
        {
            SavingChanges += (sender, e) =>
            {
                var context = sender as OrionDbContext;
                if (context == null) return;
                foreach (var entry in context.ChangeTracker.Entries())
                {
                    if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                    {
                        Console.WriteLine(entry);
                    }
                }
            };
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {

            // Add DbContext that uses MySql MariaDB 11.8.1
            services.AddDbContext<OrionDbContext>(options =>
            {
                options.UseSqlServer(ConnectionString)
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging();
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Only configure SQL Server if no provider has been configured yet
            // This allows tests to inject their own database provider (e.g., SQLite)
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fluent API configuration for EDSystemName
            modelBuilder.Entity<EDSystemName>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");
            });

            // Fluent API configuration for StarSystem
            modelBuilder.Entity<StarSystem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");
                entity.Property(e => e.X).HasColumnType("float");
                entity.Property(e => e.Y).HasColumnType("float");
                entity.Property(e => e.Z).HasColumnType("float");
                entity.Property(e => e.Population).HasColumnType("bigint");
                entity.Property(e => e.BodyCount).HasColumnType("int");
                entity.Property(e => e.Date).HasColumnType("datetime2");
                entity.HasIndex(e => e.Name).HasDatabaseName("IX_StarSystem_Name");
            });

            // Fluent API configuration for FSDJump
            modelBuilder.Entity<FSDJump>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Timestamp)
                    .HasColumnType("datetime2");

                entity.Property(e => e.Event)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                entity.Property(e => e.StarSystem)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                entity.Property(e => e.SystemAddress)
                    .HasColumnType("bigint");

                entity.Property(e => e.StarPosX).HasColumnType("float");
                entity.Property(e => e.StarPosY).HasColumnType("float");
                entity.Property(e => e.StarPosZ).HasColumnType("float");

                entity.Property(e => e.JumpDist).HasColumnType("float");
                entity.Property(e => e.FuelUsed).HasColumnType("float");
                entity.Property(e => e.FuelLevel).HasColumnType("float");

                entity.Property(e => e.StarClass)
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                entity.Property(e => e.Conflicts)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.Faction)
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                entity.Property(e => e.FactionState)
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                entity.Property(e => e.Powers)
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                entity.Property(e => e.PowerplayState)
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                entity.Property(e => e.ReserveLevel).HasColumnType("int");

                entity.Property(e => e.NeedsPermit).HasColumnType("bit");

                entity.Property(e => e.Population).HasColumnType("bigint");

                entity.HasIndex(e => e.StarSystem).HasDatabaseName("IX_FSDJump_StarSystem");
                entity.HasIndex(e => e.SystemAddress).HasDatabaseName("IX_FSDJump_SystemAddress");
            });

            // Fluent API configuration for FSSAllBodiesFound
            modelBuilder.Entity<FSSAllBodiesFound>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Timestamp)
                    .HasColumnType("datetime2");

                entity.Property(e => e.Event)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                entity.Property(e => e.SystemName)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                entity.Property(e => e.SystemAddress)
                    .HasColumnType("bigint");

                entity.Property(e => e.BodyCount).HasColumnType("int");

                entity.Property(e => e.Bodies).HasColumnType("nvarchar(max)");

                entity.Property(e => e.ScanTime).HasColumnType("float");

                entity.HasIndex(e => e.SystemName).HasDatabaseName("IX_FSSAllBodiesFound_SystemName");
                entity.HasIndex(e => e.SystemAddress).HasDatabaseName("IX_FSSAllBodiesFound_SystemAddress");
            });

            // Fluent API configuration for SupercruiseExit
            modelBuilder.Entity<SupercruiseExit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp)
                    .HasColumnType("datetime2");
                entity.Property(e => e.Event)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");
                entity.Property(e => e.StarSystem)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");
                entity.Property(e => e.SystemAddress)
                    .HasColumnType("bigint");
                entity.Property(e => e.Body)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");
                entity.Property(e => e.BodyID).HasColumnType("int");
                entity.Property(e => e.BodyType)
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");
                entity.Property(e => e.Taxi).HasColumnType("bit");
                entity.Property(e => e.Multicrew).HasColumnType("bit");
                entity.HasIndex(e => e.StarSystem).HasDatabaseName("IX_SupercruiseExit_StarSystem");
                entity.HasIndex(e => e.SystemAddress).HasDatabaseName("IX_SupercruiseExit_SystemAddress");
            });

            // Fluent API configuration for ProspectedAsteroid
            modelBuilder.Entity<ProspectedAsteroid>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp)
                    .HasColumnType("datetime2");
                entity.Property(e => e.Event)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");
                entity.Property(e => e.Materials).HasColumnType("nvarchar(max)");
            });

            // Fluent API configuration for MiningRefined
            modelBuilder.Entity<MiningRefined>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp)
                    .HasColumnType("datetime2");
                entity.Property(e => e.Event)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
