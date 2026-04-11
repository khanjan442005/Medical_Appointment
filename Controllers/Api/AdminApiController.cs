using Doctor_Appointment_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doctor_Appointment_System.Controllers.Api;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public sealed class AdminApiController : ControllerBase
{
    private readonly IDemoPortalService _portalService;

    public AdminApiController(IDemoPortalService portalService)
    {
        _portalService = portalService;
    }

    [HttpGet("overview")]
    public IActionResult GetOverview() => Ok(_portalService.GetPortalMetrics());

    [HttpGet("doctor-requests")]
    public IActionResult GetDoctorRequests() => Ok(_portalService.GetAllDoctorRequests());

    [HttpPost("doctor-requests/{requestId:int}/approve")]
    public IActionResult ApproveDoctor(int requestId)
    {
        return _portalService.ApproveDoctorRequest(requestId)
            ? Ok(new { message = "Doctor request approved.", requestId })
            : NotFound(new { message = "Doctor request not found.", requestId });
    }

    [HttpPost("doctor-requests/{requestId:int}/reject")]
    public IActionResult RejectDoctor(int requestId)
    {
        return _portalService.RejectDoctorRequest(requestId)
            ? Ok(new { message = "Doctor request rejected.", requestId })
            : NotFound(new { message = "Doctor request not found.", requestId });
    }
}
