using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FormSystem_API2_.Models.DB;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// 設置日誌
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 註冊 UserManagementContext、SurveyDbContext、SurveyContentDbContext 和 SurveyAnswerDbContext
builder.Services.AddDbContext<UserManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddDbContext<SurveyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddDbContext<SurveyContentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 新增 SurveyAnswerDbContext 註冊
builder.Services.AddDbContext<SurveyAnswerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// 執行數據庫遷移
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SurveyDbContext>();
    dbContext.Database.Migrate();

    var userManagementContext = scope.ServiceProvider.GetRequiredService<UserManagementContext>();
    userManagementContext.Database.Migrate();

    var surveyContentContext = scope.ServiceProvider.GetRequiredService<SurveyContentDbContext>();
    surveyContentContext.Database.Migrate();

    // 新增 SurveyAnswerDbContext 的遷移
    var surveyAnswerContext = scope.ServiceProvider.GetRequiredService<SurveyAnswerDbContext>();
    surveyAnswerContext.Database.Migrate();
}

// 開發模式下啟用開發者例外頁面及 Swagger
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FormSystem API v1"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
