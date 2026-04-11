using Doctor_Appointment_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doctor_Appointment_System.Controllers.Api;

[ApiController]
[Route("api/doctors")]
public sealed class DoctorsApiController : ControllerBase
{
    private readonly IDemoPortalService _portalService;

    public DoctorsApiController(IDemoPortalService portalService)
    {
        _portalService = portalService;
    }

    [HttpGet]
    public IActionResult GetDoctors() => Ok(_portalService.GetVerifiedDoctors());

    [HttpGet("{doctorId:int}")]
    public IActionResult GetDoctor(int doctorId)
    {
        var doctor = _portalService.GetDoctor(doctorId);
        return doctor is null ? NotFound() : Ok(doctor);
    }

    [HttpGet("{doctorId:int}/availability")]
    public IActionResult GetAvailability(int doctorId) => Ok(_portalService.GetAvailabilitySlots(doctorId));

    [Authorize(Roles = "Doctor,Admin")]
    [HttpGet("{doctorId:int}/appointments")]
    public IActionResult GetAppointments(int doctorId)
    {
        if (User.IsInRole("Doctor") && doctorId != _portalService.GetCurrentDoctor().Id)
        {
            return Forbid();
        }

        return Ok(_portalService.GetAppointmentsForDoctor(doctorId));
    }
}
