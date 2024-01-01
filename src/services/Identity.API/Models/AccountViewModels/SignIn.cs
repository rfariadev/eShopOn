using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models.AccountViewModels {
  public class SignIn {
    [Required]
    [EmailAddress]
    [StringLength(70, MinimumLength = 6)]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [StringLength(12, MinimumLength = 6)]
    public string Password { get; set; }
  }
}
