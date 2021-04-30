using Microsoft.EntityFrameworkCore;
using System;

namespace APMOkSvc.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            Database.EnsureCreated();
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
