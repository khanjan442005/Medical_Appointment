using Doctor_Appointment_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doctor_Appointment_System.Controllers.Api;

[ApiController]
[Route("api/patients")]
public sealed class PatientsApiController : ControllerBase
{
    private readonly IDemoPortalService _portalService;

    public PatientsApiController(IDemoPortalService portalService)
    {
        _portalService = portalService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult GetPatients() => Ok(_portalService.GetPatients());

    [Authorize(Roles = "Patient,Admin")]
    [HttpGet("{patientId:int}")]
    public IActionResult GetPatient(int patientId)
    {
        if (User.IsInRole("Patient") && patientId != _portalService.GetCurrentPatient().Id)
        {
            return Forbid();
        }

        var patient = _portalService.GetPatient(patientId);
        return patient is null ? NotFound() : Ok(patient);
    }

    [Authorize(Roles = "Patient,Admin")]
    [HttpGet("{patientId:int}/appointments")]
    public IActionResult GetAppointments(int patientId)
    {
        if (User.IsInRole("Patient") && patientId != _portalService.GetCurrentPatient().Id)
        {
            return Forbid();
        }

        return Ok(_portalService.GetAppointmentsForPatient(patientId));
    }

    [Authorize(Roles = "Patient,Admin")]
    [HttpGet("{patientId:int}/notifications")]
    public IActionResult GetNotifications(int patientId)
    {
        if (User.IsInRole("Patient") && patientId != _portalService.GetCurrentPatient().Id)
        {
            return Forbid();
        }

        return Ok(_portalService.GetNotificationsForPatient(patientId));
    }
}
