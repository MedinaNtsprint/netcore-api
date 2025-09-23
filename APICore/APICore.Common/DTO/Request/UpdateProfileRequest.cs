using System;
using System.ComponentModel.DataAnnotations;

namespace APICore.Common.DTO.Request
{
    public record UpdateProfileRequest(
        [property: Required] string FullName,
        [property: Required][property: Range(0, 1)] int Gender,
        DateTime? Birthday,
        string Phone
    )
    {
        public UpdateProfileRequest() : this(default!, default, default, default!) { }
    };
}