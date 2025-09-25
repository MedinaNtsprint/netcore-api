using System;
using System.Collections.Generic;
using System.Linq;
using APICore.Common.DTO.Response;
using APICore.Data.Entities;
using APICore.Data.Entities.Enums;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Riok.Mapperly.Abstractions;

namespace APICore.API.Mappers
{
    [Mapper]
    public partial class AppMapper : IAppMapper
    {
        // Mapperly will generate this mapping automatically based on property names
        public partial SettingResponse Map(Setting setting);

        // Manual mapping for User - complex conversion with enum values
        public UserResponse Map(User user)
        {
            if (user == null) return null;
            return new UserResponse(
                user.Id,
                user.FullName,
                user.Identity,
                user.Email,
                user.Phone,
                (int)user.Status,
                user.Status.ToString(),
                user.Avatar,
                user.AvatarMimeType
            );
        }

        // Manual mapping for Log - enum to string conversion
        public LogResponse Map(Log log)
        {
            if (log == null) return null;
            return new LogResponse(
                log.Id,
                log.EventType.ToString(),
                log.LogType.ToString(),
                log.CreatedAt,
                log.UserId,
                log.Description
            );
        }

        // Manual mapping for collection
        public IEnumerable<LogResponse> Map(IEnumerable<Log> logs)
        {
            if (logs == null) return Enumerable.Empty<LogResponse>();
            return logs.Select(Map).ToList();
        }

        // Manual mapping for HealthReportEntry - complex status calculation
        public HealthCheckResponse Map(HealthReportEntry entry)
        {
            var statusCode = entry.Status == HealthStatus.Healthy
                ? 200
                : (entry.Status == HealthStatus.Degraded ? 200 : 503);

            return new HealthCheckResponse(
                statusCode,
                string.Empty, // ServiceName set by caller
                entry.Description,
                entry.Exception?.Message ?? string.Empty,
                entry.Duration.TotalSeconds.ToString()
            );
        }
    }
}