using System.ComponentModel.DataAnnotations;

namespace Certify.Web.Models;

public class ProfileViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "CPR")]
    public string? CPR { get; set; }
}
