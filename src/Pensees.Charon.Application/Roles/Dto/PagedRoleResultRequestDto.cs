using Abp.Application.Services.Dto;

namespace Pensees.Charon.Roles.Dto
{
    public class PagedRoleResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}

