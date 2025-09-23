using System;

namespace APICore.Common.DTO.Response
{
    public record UserResponse(
        int Id,
        DateTime BirthDate,
        string FullName,
        string Identity,
        int GenderId,
        string Gender,
        string Email,
        string Phone,
        int StatusId,
        string Status,
        string Avatar,
        string AvatarMimeType
    );
}