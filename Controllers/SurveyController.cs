using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormSystem_API2_.Models.DB;
using FormSystem_API2_.Models;
using Microsoft.Extensions.Logging;

namespace FormSystem_API2_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly SurveyDbContext _context;
        private readonly SurveyContentDbContext _contentContext;
        private readonly SurveyAnswerDbContext _answerContext;
        private readonly ILogger<SurveyController> _logger;

        public SurveyController(SurveyDbContext context, SurveyContentDbContext contentContext, SurveyAnswerDbContext answerContext, ILogger<SurveyController> logger)
        {
            _context = context;
            _contentContext = contentContext;
            _answerContext = answerContext;
            _logger = logger;
        }

        // 新增問卷
        [HttpPost("addSurvey")]
        public async Task<IActionResult> AddSurvey([FromBody] SurveyRequest surveyRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data");
            }

            string surveyId;
            do
            {
                // 生成隨機的 6 位數字 ID
                surveyId = new Random().Next(100000, 999999).ToString("D6");
            }
            // 檢查資料表中是否已有此 ID
            while (await _context.Surveys.AsNoTracking().AnyAsync(s => s.SurveyID == surveyId));

            var newSurvey = new Survey
            {
                SurveyID = surveyId,
                SurveyName = surveyRequest.SurveyName,
                SurveyMode = surveyRequest.SurveyMode,
                SurveyLastModified = DateTime.UtcNow.AddHours(8),
                UserID = surveyRequest.UserID
            };

            _context.Surveys.Add(newSurvey);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, surveyID = newSurvey.SurveyID });
        }

        // 根據 UserID 獲取該使用者的所有問卷
        [HttpGet("getSurveysByUser/{userId}")]
        public async Task<IActionResult> GetSurveysByUser(string userId)
        {
            _logger.LogInformation($"Searching surveys for UserID: {userId}");

            var surveys = await _context.Surveys
                .Where(s => s.UserID.Trim().ToLower() == userId.Trim().ToLower())
                .Select(s => new
                {
                    SurveyID = s.SurveyID,
                    SurveyName = s.SurveyName,
                    SurveyMode = s.SurveyMode,
                    SurveyLastModified = s.SurveyLastModified
                })
                .ToListAsync();

            if (surveys == null || !surveys.Any())
            {
                return NotFound(new { success = false, message = "No surveys found for this user" });
            }

            return Ok(new
            {
                success = true,
                surveys = surveys.Select(s => new
                {
                    surveyID = s.SurveyID,
                    surveyName = s.SurveyName,
                    surveyMode = s.SurveyMode,
                    surveyLastModified = s.SurveyLastModified
                })
            });
        }

        [HttpDelete("deleteAllSurveyData/{surveyId}")]
        public async Task<IActionResult> DeleteAllSurveyData(string surveyId)
        {
            try
            {
                // 刪除 SurveyAnswer 表中的相關記錄
                await _answerContext.Database.ExecuteSqlRawAsync("DELETE FROM SurveyAnswer WHERE SurveyID = {0}", surveyId);

                // 刪除 SurveyContent 表中的相關記錄
                await _contentContext.Database.ExecuteSqlRawAsync("DELETE FROM SurveyContent WHERE SurveyId = {0}", surveyId);

                // 刪除 Survey 表中的問卷記錄
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Survey WHERE SurveyID = {0}", surveyId);

                return Ok(new { success = true, message = "問卷及相關資料已成功刪除" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"刪除問卷資料時出現錯誤: {ex.Message}");
                return StatusCode(500, new { success = false, message = "刪除問卷時發生錯誤" });
            }
        }
    }
}
