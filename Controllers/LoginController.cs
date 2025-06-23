using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using FormSystem_API2_.Models.DB;
using Microsoft.AspNetCore.Identity.Data;

namespace FormSystem_API2_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly UserManagementContext _context;

        public LoginController(UserManagementContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid request");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                return Unauthorized(new { success = false, message = "帳號或密碼錯誤" });
            }

            // 返回成功時增加 UserID 欄位
            return Ok(new { success = true, name = user.Name, userId = user.UserID });  // 修改為 userId 小寫

        }
    }
}
