using Microsoft.EntityFrameworkCore;

namespace FormSystem_API2_.Models.DB
{
    /// <summary>
    /// SurveyAnswer 的資料庫上下文
    /// </summary>
    public class SurveyAnswerDbContext : DbContext
    {
        public SurveyAnswerDbContext(DbContextOptions<SurveyAnswerDbContext> options)
            : base(options)
        {
        }

        public DbSet<SurveyAnswer> SurveyAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SurveyAnswer>(entity =>
            {
                entity.ToTable("SurveyAnswer");

                // 設置複合主鍵
                entity.HasKey(e => new { e.AnswerID, e.SurveyContentQuestion });

                // 設置欄位長度和必填屬性
                entity.Property(e => e.AnswerID)
                    .IsRequired()
                    .HasMaxLength(6);

                entity.Property(e => e.SurveyID)
                    .IsRequired()
                    .HasMaxLength(6);

                entity.Property(e => e.SurveyContentQuestion)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.SurveyContentUnit)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Answer)
                    .HasMaxLength(255);

                // 設置默認值和日期格式
                entity.Property(e => e.AnswerWhen)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("DATEADD(HOUR, 8, GETUTCDATE())");
            });
        }
    }
}
