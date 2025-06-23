using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FormSystem_API2_.Models.DB
{
    [Index("Email", Name = "UQ__Users__A9D105349DD163E6", IsUnique = true)]
    public partial class Users
    {
        [Key]
        [StringLength(10)]
        public string UserID { get; set; }  // 將 UserID 更改為 string 類型

        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(255)]
        public string Name { get; set; } = null!;

        [StringLength(255)]
        public string PasswordHash { get; set; } = null!;

        [Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Survey> Surveys { get; set; } = new List<Survey>();
    }
}
