using System;

namespace APICore.Common.DTO.Response
{
    public record UserResponse(
        int Id,
        string FullName,
        string Identity,
        string Email,
        string Phone,
        int StatusId,
        string Status,
        string Avatar,
        string AvatarMimeType
    );
}