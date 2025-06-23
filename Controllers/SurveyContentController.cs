using Microsoft.AspNetCore.Mvc;
using FormSystem_API2_.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormSystem_API2_.Models;
using Microsoft.EntityFrameworkCore;

namespace FormSystem_API2_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyContentController : ControllerBase
    {
        private readonly SurveyContentDbContext _context;

        public SurveyContentController(SurveyContentDbContext context)
        {
            _context = context;
        }

        [HttpPost("SaveSurvey")]
        public async Task<IActionResult> SaveSurvey([FromBody] List<SurveyContentRequest> surveyContentRequests)
        {
            if (surveyContentRequests == null || surveyContentRequests.Count == 0)
            {
                return BadRequest("問卷資料不可為空");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 查詢當前最大版本號
                var surveyId = surveyContentRequests.First().SurveyId; // 假設所有資料屬於同一問卷
                var unitName = surveyContentRequests.First().SurveyContentUnit;

                var maxVersion = await _context.SurveyContent
                    .Where(sc => sc.SurveyId == surveyId && sc.SurveyContentUnit == unitName)
                    .MaxAsync(sc => (int?)sc.VersionNumber) ?? 0; // 如果無資料，版本號從 0 開始

                var newVersion = maxVersion + 1;

                foreach (var request in surveyContentRequests)
                {
                    string contentId = string.IsNullOrEmpty(request.SurveyContentID) ||
                                       _context.SurveyContent.Any(sc => sc.SurveyContentID == request.SurveyContentID)
                        ? GenerateUniqueContentId()
                        : request.SurveyContentID;

                    var surveyContent = new SurveyContent
                    {
                        SurveyId = request.SurveyId,
                        SurveyContentID = contentId,
                        SurveyContentUnit = request.SurveyContentUnit,
                        SurveyContentQuestion = request.SurveyContentQuestion,
                        SurveyContentType = request.SurveyContentType,
                        SurveyContentOption = request.SurveyContentOption,
                        Order = request.Order,
                        QuestionOrder = request.QuestionOrder,
                        OptionOrder = request.OptionOrder,
                        VersionNumber = newVersion // 設置版本號
                    };

                    _context.SurveyContent.Add(surveyContent);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "問卷已儲存", version = newVersion });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"存儲過程中出現錯誤: {ex.Message}");
            }
        }

        // 取得問卷內容 API
        [HttpGet("GetSurveyContent/{surveyId}")]
        public async Task<IActionResult> GetSurveyContent(string surveyId)
        {
            var surveyContents = await _context.SurveyContent
                .Where(sc => sc.SurveyId == surveyId)
                .OrderBy(sc => sc.Order)
                .ThenBy(sc => sc.QuestionOrder)
                .ThenBy(sc => sc.OptionOrder)
                .ToListAsync();

            if (surveyContents == null || !surveyContents.Any())
            {
                return NotFound(new { success = false, message = "未找到該問卷的內容" });
            }

            return Ok(surveyContents); // 返回問卷內容
        }

        // 新增 API：取得最新版本的問卷內容
        [HttpGet("GetLatestSurveyContent/{surveyId}")]
        public async Task<IActionResult> GetLatestSurveyContent(string surveyId)
        {
            try
            {
                // 找到該 SurveyID 的最大版本號
                var maxVersion = await _context.SurveyContent
                    .Where(sc => sc.SurveyId == surveyId)
                    .MaxAsync(sc => (int?)sc.VersionNumber) ?? 0;

                if (maxVersion == 0)
                {
                    return NotFound(new { success = false, message = "未找到任何問卷內容" });
                }

                // 篩選出最新版本號的資料
                var latestSurveyContent = await _context.SurveyContent
                    .Where(sc => sc.SurveyId == surveyId && sc.VersionNumber == maxVersion)
                    .OrderBy(sc => sc.Order)            // 按照單元順序排序
                    .ThenBy(sc => sc.QuestionOrder)     // 按照題目順序排序
                    .ThenBy(sc => sc.OptionOrder)       // 按照選項順序排序
                    .ToListAsync();

                if (!latestSurveyContent.Any())
                {
                    return NotFound(new { success = false, message = "未找到最新版本的問卷內容" });
                }

                return Ok(latestSurveyContent); // 返回最新版本的問卷內容
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"取得問卷內容時出現錯誤: {ex.Message}");
            }
        }

        private string GenerateUniqueContentId()
        {
            string newId;

            do
            {
                newId = new Random().Next(1, 1000000).ToString("D6");
            }
            while (_context.SurveyContent.AsNoTracking().Any(sc => sc.SurveyContentID == newId)); // 減少鎖定的數據範圍

            return newId;
        }
    }
}
