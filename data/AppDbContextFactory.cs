//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;

//namespace TodoLite.Data
//{
//    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
//    {
//        public AppDbContext CreateDbContext(string[] args)
//        {
//            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

//            // Migration sırasında kullanılacak connection string
//            optionsBuilder.UseSqlServer("Server=localhost;Database=TodoLiteDb;Trusted_Connection=True;TrustServerCertificate=True;");

//            return new AppDbContext(optionsBuilder.Options);
//        }
//    }
//}
