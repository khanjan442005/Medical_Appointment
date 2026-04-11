using Doctor_Appointment_System.Models;
using Doctor_Appointment_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doctor_Appointment_System.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IDemoPortalService _portalService;

        public AuthController(IDemoPortalService portalService)
        {
            _portalService = portalService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var redirect = RedirectAuthenticatedUser();
            if (redirect is not null)
            {
                return redirect;
            }

            return View(new LoginInputModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var (success, errorMessage) = await _portalService.TryLoginAsync(input.Role, input.Email, input.Password);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(input);
            }

            return input.Role switch
            {
                "Patient" => RedirectToAction("Dashboard", "Patient"),
                "Doctor" => RedirectToAction("Dashboard", "Doctor"),
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _portalService.LogoutAsync();
            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Register()
        {
            var redirect = RedirectAuthenticatedUser();
            if (redirect is not null)
            {
                return redirect;
            }

            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordInputModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            if (!_portalService.TryBeginPasswordReset(input.Email, out var normalizedEmail, out var errorMessage))
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(input);
            }

            TempData["Success"] = "Email verified successfully. You can now set a new password.";
            return RedirectToAction(nameof(ResetPassword), new { email = normalizedEmail });
        }

        [HttpGet]
        public IActionResult ResetPassword(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            return View(new ResetPasswordInputModel
            {
                Email = email
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            if (!_portalService.ResetPassword(input.Email, input.Password, out var errorMessage))
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(input);
            }

            TempData["Success"] = "Password updated successfully. You can now sign in.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            ModelState.AddModelError(string.Empty, "You do not have permission to access that page.");
            return View(nameof(Login), new LoginInputModel());
        }

        private IActionResult? RedirectAuthenticatedUser()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            if (User.IsInRole("Patient"))
            {
                return RedirectToAction("Dashboard", "Patient");
            }

            if (User.IsInRole("Doctor"))
            {
                return RedirectToAction("Dashboard", "Doctor");
            }

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
