using System;

namespace APICore.Common.DTO.Request
{
    public record AddLogRequest(int EventType, int LogType, int UserId, string Description, string App, string Module);
}
