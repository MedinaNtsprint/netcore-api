using System.Collections.Generic;
using APICore.Common.DTO.Response;
using APICore.Data.Entities;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace APICore.API.Mappers
{
    public interface IAppMapper
    {
        UserResponse Map(User user);
        IEnumerable<LogResponse> Map(IEnumerable<Log> logs);
        LogResponse Map(Log log);
        SettingResponse Map(Setting setting);
        HealthCheckResponse Map(HealthReportEntry entry);
    }
}