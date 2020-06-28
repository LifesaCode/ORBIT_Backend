#nullable disable warnings

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Orbit.Models;

namespace Orbit.Data
{
    public class OrbitDbContext : DbContext
    {
        public OrbitDbContext(DbContextOptions<OrbitDbContext> options) : base(options)
        {
        }

        public DbSet<UrineSystemData> UrineProcessorData { get; set; }

        public DbSet<WaterProcessorData> WaterProcessorData { get; set; }

        public DbSet<WasteWaterStorageTankData> WasteWaterStorageTankData { get; set; }

        public DbSet<AtmosphereData> AtmosphereData { get; set; }

        public DbSet<OxygenGenerator> OxygenGeneratorData { get; set; }

        public DbSet<CarbonDioxideRemediation> CarbonDioxideRemoverData { get; set; }

        public DbSet<PowerSystemData> PowerSystemData { get; set; }


        public DbSet<InternalCoolantLoopData> InternalCoolantLoopData { get; set; }

        public DbSet<ExternalCoolantLoopData> ExternalCoolantLoopData { get; set; }

        public DbSet<WaterGeneratorData> WaterGeneratorData { get; set; }

        private void Seed<T>() where T : class, ISeedableModel, new()
        {
            var data = new T();

            data.SeedData();
            this.Set<T>().Add(data);
        }

        public void InsertSeedData()
        {
            var seedMethodGeneric = this.GetType()
                .GetMethod(nameof(Seed), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);

            foreach (var type in this.GetType().Assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ISeedableModel))))
            {
                var seedMethod = seedMethodGeneric?.MakeGenericMethod(type);
                if (seedMethod == null)
                {
                    throw new InvalidOperationException($"Could not find Seed Method for type {type}");
                }
                seedMethod?.Invoke(this, null);
            }
            
            this.SaveChanges();
        }

        private static void MapModelCommonProps<T>(EntityTypeBuilder<T> e) where T : class, IModel
        {
            e.Property<Guid>("Id").ValueGeneratedOnAdd();
            e.HasKey("Id");

            e.Property(p => p.ReportDateTime).ValueGeneratedOnAdd();
            e.HasAlternateKey(p => p.ReportDateTime);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new ArgumentNullException(nameof(modelBuilder));

            // Do this for all of these so we don't need to worry about defining the ids in code
            modelBuilder.Entity<UrineSystemData>(MapModelCommonProps);
            modelBuilder.Entity<WaterProcessorData>(MapModelCommonProps);
            modelBuilder.Entity<WasteWaterStorageTankData>(MapModelCommonProps);
            modelBuilder.Entity<AtmosphereData>(MapModelCommonProps);
            modelBuilder.Entity<CarbonDioxideRemediation>(MapModelCommonProps);
            modelBuilder.Entity<OxygenGenerator>(MapModelCommonProps);
            modelBuilder.Entity<PowerSystemData>(MapModelCommonProps);
            modelBuilder.Entity<ExternalCoolantLoopData>(MapModelCommonProps);
            modelBuilder.Entity<InternalCoolantLoopData>(MapModelCommonProps);
            modelBuilder.Entity<WaterGeneratorData>(MapModelCommonProps);
        }
    }
}
