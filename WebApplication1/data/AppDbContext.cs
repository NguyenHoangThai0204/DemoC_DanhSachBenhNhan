using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Nguoi> Nguois { get; set; }
        public DbSet<BenhNhan> BenhNhans { get; set; }
        public DbSet<DanToc> DanTocs { get; set; }
        public DbSet<TinhThanh> TinhThanhs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Nguoi>().ToTable("Nguoi");
            modelBuilder.Entity<BenhNhan>().ToTable("BenhNhan");
            modelBuilder.Entity<DanToc>().ToTable("DanToc");
            modelBuilder.Entity<TinhThanh>()
    .Property(tt => tt.VietTat)
    .HasColumnType("nvarchar(max)");
        }

        public bool TestConnection()
        {
            try
            {
                return this.Database.CanConnect();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
