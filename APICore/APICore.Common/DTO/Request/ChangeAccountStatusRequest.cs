using System.ComponentModel.DataAnnotations;

namespace APICore.Common.DTO.Request
{
    public record ChangeAccountStatusRequest([property: Required] string Identity, [property: Required] bool Active);
}