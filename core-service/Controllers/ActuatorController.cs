using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent ;

namespace CoreWorkshop.Api.Controllers;

[ApiController]
[Route("")]
public class ActuatorController : ControllerBase
{
    [HttpGet(Name = "Health")]
    [Route("/actuator/health")]
    public ActuatorStatus GetHealth()
    {
        return new ActuatorStatus{status = "UP"};
    }
}
