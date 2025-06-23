using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using FormSystem_API2_.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace FormSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly UserManagementContext _context;

        public RegisterController(UserManagementContext context)
        {
            _context = context;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { success = false, message = "所有欄位均為必填項。" });
            }

            try
            {
                // 生成随机的 UserID
                var userId = GenerateUserId();

                // 哈希用户密码
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // 创建新的用户对象
                var user = new Users
                {
                    UserID = userId, // 设置生成的 UserID
                    Name = request.Name,
                    PasswordHash = hashedPassword,
                    CreatedDate = DateTime.UtcNow.AddHours(8), 
                    Email = request.Email
                };

                // 将新用户添加到数据库
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "註冊成功" });
            }
            catch (Exception ex)
            {
                // 记录详细的异常信息
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { success = false, message = "服務器錯誤，請稍後再試" });
            }
        }

        // 生成唯一的 UserID
        private string GenerateUserId()
        {
            string newId;
            do
            {
                // 隨機生成 3 位數字的 UserID
                newId = new Random().Next(0, 1000).ToString("D3");
            }
            // 檢查資料庫中是否存在相同的 UserID
            while (_context.Users.AsNoTracking().Any(u => u.UserID == newId));

            return newId;
        }
    }

    public class RegisterRequest
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
