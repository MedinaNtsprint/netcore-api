using APICore.Common.DTO.Response;
using APICore.API.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace APICore.API.Controllers
{
    public class DiagnosticsController : Controller
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly HealthCheckOptions _healthCheckOptions;
        private readonly IAppMapper _mapper;

        public DiagnosticsController(HealthCheckService healthCheckService, IAppMapper mapper, HealthCheckOptions healthCheckOptions = null)
        {
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));

            _healthCheckOptions = healthCheckOptions ?? new HealthCheckOptions();
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Api health check that allows us to see the status of the external and infrastructure services.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("health-check")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        public async Task<IActionResult> HealthCheckActionResult()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            var list = new List<HealthCheckResponse>();
            foreach (var item in report.Entries)
            {
                var healthCheckItem = _mapper.Map(item.Value);
                // HealthCheckResponse is a positional record; use 'with' to create a copy with ServiceName set
                healthCheckItem = healthCheckItem with { ServiceName = item.Key };
                list.Add(healthCheckItem);
            }

            return report.Status == HealthStatus.Healthy ? Ok(list) : StatusCode((int)HttpStatusCode.ServiceUnavailable, list);
        }
    }
}