using System.ComponentModel.DataAnnotations;

namespace Pensees.Charon.Models
{
    public class GetTenantAdminToken
    {
        [Required]
        public int TenantId { get; set; }
    }
}
