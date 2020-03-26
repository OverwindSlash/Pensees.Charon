using System.ComponentModel.DataAnnotations;

namespace Pensees.Charon.Users.Dto
{
    public class GetPermissionsDto
    {
        public int? TenantId { get; set; }

        [Required]
        public long UserId { get; set; }
    }
}
