using APMOkSvc.Code;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace APMOkSvc.Data
{
    public class DataContext : DbContext
    {
        private readonly IHostEnvironment _environment;
        private readonly IConnectionStringsConfiguration _connectionStringsConfiguration;

        public DataContext(DbContextOptions<DataContext> options, IHostEnvironment environment, IConnectionStringsConfiguration connectionStringsConfiguration)
            : base(options)
        {
            _environment = environment;
            _connectionStringsConfiguration = connectionStringsConfiguration;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var DataContext = _connectionStringsConfiguration.DataContext.Value;
            if (string.IsNullOrEmpty(DataContext))
                throw new Exception("DataContext is not set and must not be empty");

            var connectionOptions = new SqliteConnectionStringBuilder(DataContext);
            var dbFile = connectionOptions.DataSource;
            var dbPath = Path.GetDirectoryName(Path.GetFullPath(dbFile));
            if (string.IsNullOrEmpty(dbPath))
                throw new Exception("DataContext path must not be empty");

            Directory.CreateDirectory(dbPath);

            optionsBuilder.UseSqlite(_connectionStringsConfiguration.DataContext.Value);
            optionsBuilder.EnableSensitiveDataLogging(_environment.IsDevelopment());
            //optionsBuilder.EnableDetailedErrors(_environment.IsDevelopment());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new ArgumentNullException(nameof(modelBuilder));

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ConfigData>().Property(p => p.Id).ValueGeneratedOnAdd();
        }

        public DbSet<ConfigData> ConfigDataSet { get; set; }
    }
}
