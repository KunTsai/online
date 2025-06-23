using Microsoft.EntityFrameworkCore;
using FormSystem_API2_.Models.DB;

namespace FormSystem_API2_.Models.DB
{
    public class SurveyDbContext : DbContext
    {
        public SurveyDbContext(DbContextOptions<SurveyDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Survey> Surveys { get; set; }  // 關聯到 Survey 表

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Survey>(entity =>
            {
                entity.ToTable("Survey");  // 使用正確的表名 'Survey'
                entity.HasKey(e => e.SurveyID);
                entity.Property(e => e.SurveyID).IsRequired().HasMaxLength(6);
                entity.Property(e => e.SurveyName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.SurveyMode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SurveyLastModified).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Surveys)
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
