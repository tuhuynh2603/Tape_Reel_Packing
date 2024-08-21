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
            //builder.AddFilter(DbLoggerCategory.Database.Name, LogLevel.Information);

        });
        private const string _connectionString = "server=localhost;user=root;database=db1;port=3306;password=gacon05637";
        //private const string _connectionString = "server=localhost;user=root;database=mvvmlogindb;port=3306;password=gacon05637";
        public DbSet<RectanglesModel> rectanglesModel { get; set; }
        //public DbSet<CategoryTeachParameter> userlogins { set; get; }
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

            modelBuilder.Entity<RectanglesModel>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Configure the columns
                entity.Property(e => e.left).IsRequired();
                entity.Property(e => e.top).IsRequired();
                entity.Property(e => e.Width).IsRequired();
                entity.Property(e => e.Height).IsRequired();
                entity.Property(e => e.Angle).IsRequired();
            });
        }

        private string SerializeRectangle(RectanglesModel rect)
        {
            // Implement serialization logic (e.g., JSON)
            return JsonConvert.SerializeObject(rect);
        }

        private RectanglesModel DeserializeRectangle(string json)
        {
            // Implement deserialization logic (e.g., JSON)
            return JsonConvert.DeserializeObject<RectanglesModel>(json);
        }

    }
}
