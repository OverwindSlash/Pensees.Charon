using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pensees.Charon.Users.Dto
{
    public class GetPermissionsDto
    {
        [Required]
        public int? TenantId { get; set; }

        [Required]
        public long UserId { get; set; }
    }
}
