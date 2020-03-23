using System.ComponentModel.DataAnnotations;

namespace Pensees.Charon.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}