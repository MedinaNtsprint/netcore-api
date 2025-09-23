namespace APICore.Common.DTO.Response
{
    public record HealthCheckResponse(int ServiceStatus, string ServiceName, string Description, string Exception, string Duration);
}