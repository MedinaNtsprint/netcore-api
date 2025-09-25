using System;
using System.ComponentModel.DataAnnotations;

namespace APICore.Common.DTO.Request
{
    public record SignUpRequest(
        [property: Required] string FullName,
        [property: Required][property: MinLength(6)] string Password,
        [property: Compare("Password", ErrorMessage = "The password and confirmation password do not match.")] string ConfirmationPassword,
        [property: Required][property: EmailAddress] string Email,
        string Phone
    );
}