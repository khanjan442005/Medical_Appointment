using Doctor_Appointment_System.Models;
using Doctor_Appointment_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doctor_Appointment_System.Controllers.Api;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsApiController : ControllerBase
{
    private readonly IDemoPortalService _portalService;

    public AppointmentsApiController(IDemoPortalService portalService)
    {
        _portalService = portalService;
    }

    [Authorize]
    [HttpGet("{appointmentId:int}")]
    public IActionResult GetAppointment(int appointmentId)
    {
        var appointment = _portalService.GetAppointment(appointmentId);
        if (appointment is null)
        {
            return NotFound();
        }

        if (User.IsInRole("Admin"))
        {
            return Ok(appointment);
        }

        if (User.IsInRole("Patient") && appointment.PatientId == _portalService.GetCurrentPatient().Id)
        {
            return Ok(appointment);
        }

        if (User.IsInRole("Doctor") && appointment.DoctorId == _portalService.GetCurrentDoctor().Id)
        {
            return Ok(appointment);
        }

        return Forbid();
    }

    [Authorize(Roles = "Patient")]
    [HttpPost]
    public IActionResult CreateAppointment([FromBody] CreateAppointmentApiRequest request)
    {
        if (!DateOnly.TryParse(request.AppointmentDate, out var appointmentDate))
        {
            return BadRequest(new { message = "Invalid appointment date." });
        }

        try
        {
            var appointment = _portalService.CreatePendingAppointment(_portalService.GetCurrentPatient().Id, request.DoctorId, appointmentDate, request.TimeSlot);
            return CreatedAtAction(nameof(GetAppointment), new { appointmentId = appointment.Id }, appointment);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize(Roles = "Patient,Admin")]
    [HttpPost("{appointmentId:int}/payment")]
    public IActionResult CompletePayment(int appointmentId, [FromBody] CompletePaymentApiRequest request)
    {
        var appointment = _portalService.GetAppointment(appointmentId);
        if (appointment is null)
        {
            return NotFound(new { message = "Appointment not found." });
        }

        if (User.IsInRole("Patient") && appointment.PatientId != _portalService.GetCurrentPatient().Id)
        {
            return Forbid();
        }

        var success = _portalService.CompleteAppointmentPayment(appointmentId, request.PaymentMethod);
        return success
            ? Ok(new { message = "Payment completed successfully.", appointmentId })
            : NotFound(new { message = "Appointment not found." });
    }

    public sealed class CreateAppointmentApiRequest
    {
        public int DoctorId { get; set; }
        public string AppointmentDate { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
    }

    public sealed class CompletePaymentApiRequest
    {
        public string PaymentMethod { get; set; } = "UPI";
    }
}
