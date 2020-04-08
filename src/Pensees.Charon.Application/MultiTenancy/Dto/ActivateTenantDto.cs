using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pensees.Charon.MultiTenancy.Dto
{
    public class ActivateTenantDto
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
