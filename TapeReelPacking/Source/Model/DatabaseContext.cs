using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapeReelPacking.Source.Define;

namespace TapeReelPacking.Source.Model

{
    public class DatabaseContext : DbContext
    {

        public static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddFilter(DbLoggerCategory.Query.Name, LogLevel.Information);
            builder.AddConsole();

        });
        private const string _connectionString = "server=localhost;user=root;database=db1;port=3306;password=gacon05637";
        public DbSet<RectanglesModel> rectanglesModel { get; set; }
        public DbSet<CategoryTeachParameter> categoryTeachParametersModel { get; set; }
        public DbSet<CategoryVisionParameter> categoryVisionParametersModel { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionssBuilder)
        {
            base.OnConfiguring(optionssBuilder);
            optionssBuilder.UseLoggerFactory(loggerFactory);
            optionssBuilder.UseMySQL(_connectionString);
            //optionssBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CategoryVisionParameter>(
                entity =>
                {
                    entity.HasKey(e => new { e.cameraID, e.areaID});
                    entity.HasOne(r => r.categoryTeachParameter)
                        .WithMany(c => c.categoryVisionParameter)
                        .HasForeignKey(r => r.cameraID)
                        .OnDelete(DeleteBehavior.NoAction);
                }
                );

            //modelBuilder.Entity<CategoryVisionParameter>()
            //.HasOne(r => r.categoryTeachParameter)
            //.WithMany(c => c.categoryVisionParameter)
            //.HasForeignKey(r => r.cameraID)
            //.OnDelete(DeleteBehavior.NoAction);
            //modelBuilder.Entity<RectanglesModel>(entity =>
            //{
            //    entity.HasKey(e => e.Id);

            //    // Configure the columns
            //    entity.Property(e => e.left).IsRequired();
            //    entity.Property(e => e.top).IsRequired();
            //    entity.Property(e => e.Width).IsRequired();
            //    entity.Property(e => e.Height).IsRequired();
            //    entity.Property(e => e.Angle).IsRequired();
            //});
        }

    }
}
