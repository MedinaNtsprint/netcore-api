using System;
using System.ComponentModel.DataAnnotations;

namespace APICore.Common.DTO.Request
{
    public record UpdateProfileRequest(
        [property: Required] string FullName,
        string Phone
    );
}