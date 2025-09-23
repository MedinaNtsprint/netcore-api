namespace APICore.Common.DTO.Request
{
    public record ChangePasswordRequest(string OldPassword, string NewPassword, string ConfirmPassword);
}