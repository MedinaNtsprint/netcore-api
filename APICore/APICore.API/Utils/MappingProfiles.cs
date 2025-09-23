using APICore.Common.DTO.Response;
using APICore.Data.Entities;
using AutoMapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;

namespace APICore.API.Utils
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Map User -> UserResponse using positional record constructor
            CreateMap<User, UserResponse>()
                .ConstructUsing(source => new UserResponse(
                    source.Id,
                    source.BirthDate,
                    source.FullName,
                    source.Identity,
                    (int)source.Gender,
                    source.Gender.ToString(),
                    source.Email,
                    source.Phone,
                    (int)source.Status,
                    source.Status.ToString(),
                    source.Avatar,
                    source.AvatarMimeType
                ));

            // Map HealthReportEntry -> HealthCheckResponse using constructor; ServiceName is set later in controller
            CreateMap<HealthReportEntry, HealthCheckResponse>()
                .ConstructUsing(source => new HealthCheckResponse(
                    (int)(source.Status == HealthStatus.Healthy
                        ? HttpStatusCode.OK
                        : (source.Status == HealthStatus.Degraded ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable)),
                    string.Empty,
                    source.Description,
                    source.Exception == null ? string.Empty : source.Exception.Message,
                    source.Duration.TotalSeconds.ToString()
                ));

            // Setting maps by name automatically to positional record
            CreateMap<Setting, SettingResponse>();

            // Map Log -> LogResponse using constructor
            CreateMap<Log, LogResponse>()
                .ConstructUsing(source => new LogResponse(
                    source.Id,
                    source.EventType.ToString(),
                    source.LogType.ToString(),
                    source.CreatedAt,
                    source.UserId,
                    source.Description
                ));
        }
    }
}