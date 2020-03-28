using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pensees.Charon.Users.Dto
{
    public class SmsResetPasswordDto
    {
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string AutoCode { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
