using System;

namespace APICore.Common.DTO.Response
{
    public record LogResponse(int Id, string EventType, string LogType, DateTime CreatedAt, int UserId, string Description);
}
