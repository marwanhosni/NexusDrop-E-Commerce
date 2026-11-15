using ECommerce.Application.Interfaces;
using ECommerce.Core.Entities;
using ECommerce.Web.Extensions;
using ECommerce.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IEmailSenderService _emailSender;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ICartService cartService,
        IOrderService orderService,
        IEmailSenderService emailSender,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _cartService = cartService;
        _orderService = orderService;
        _emailSender = emailSender;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            // Merge guest cart if any
            var guestCartId = HttpContext.GetOrCreateCartId();
            await _cartService.MergeCartAsync(guestCartId, user.Id);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt. Check your email and password.");
        return View(model);
    }

    // ==================== GOOGLE EXTERNAL AUTHENTICATION ====================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            _logger.LogError("External authentication error from provider: {Error}", remoteError);
            TempData["ErrorMessage"] = $"External authentication provider error: {remoteError}";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("Failed to load external login info from provider.");
            TempData["ErrorMessage"] = "Could not authenticate with external provider. Please try again.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        // Sign in with external provider if already linked
        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
        if (signInResult.Succeeded)
        {
            var existingUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (existingUser != null)
            {
                var guestCartId = HttpContext.GetOrCreateCartId();
                await _cartService.MergeCartAsync(guestCartId, existingUser.Id);
            }

            _logger.LogInformation("User logged in via {Provider}.", info.LoginProvider);
            return RedirectToLocal(returnUrl);
        }

        // If user does not exist with external link, check by email
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "Email address was not provided by the external identity provider.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Auto-provision user account from Google profile
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0];
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to create user account from Google profile.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            await _userManager.AddToRoleAsync(user, "Customer");
        }

        // Link the Google login to the user
        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (addLoginResult.Succeeded || await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey) != null)
        {
            await _signInManager.SignInAsync(user, isPersistent: true);

            var guestCartId = HttpContext.GetOrCreateCartId();
            await _cartService.MergeCartAsync(guestCartId, user.Id);

            TempData["SuccessMessage"] = $"Welcome {user.FullName}! Authenticated via {info.ProviderDisplayName ?? "Google"}.";
            return RedirectToLocal(returnUrl);
        }

        TempData["ErrorMessage"] = "Failed to associate external login with account.";
        return RedirectToAction(nameof(Login), new { returnUrl });
    }

    // ==================== FORGOT & RESET PASSWORD FLOW ====================

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        string? resetLink = null;

        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            resetLink = Url.Action(nameof(ResetPassword), "Account", new { token, email = model.Email }, Request.Scheme);

            await _emailSender.SendPasswordResetEmailAsync(model.Email, resetLink ?? string.Empty);
            _logger.LogInformation("Password reset token generated for {Email}: {Link}", model.Email, resetLink);
        }

        // Always display confirmation to prevent account enumeration
        ViewBag.ResetLink = resetLink;
        ViewBag.TargetEmail = model.Email;
        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string? token = null, string? email = null)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "Invalid password reset token or email address.";
            return RedirectToAction(nameof(Login));
        }

        var model = new ResetPasswordViewModel
        {
            Token = token,
            Email = email
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Do not reveal that the user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Your access key (password) has been reset successfully. Please authenticate.";
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    // ==================== REGISTER & PROFILE ====================

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Customer");
            await _signInManager.SignInAsync(user, isPersistent: true);

            // Merge guest cart
            var guestCartId = HttpContext.GetOrCreateCartId();
            await _cartService.MergeCartAsync(guestCartId, user.Id);

            TempData["SuccessMessage"] = "Welcome to Marwan Hosny Store! Your cyber account is active.";
            return RedirectToLocal(model.ReturnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["SuccessMessage"] = "You have been logged out securely.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var orders = await _orderService.GetOrdersByUserAsync(user.Id);

        var vm = new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            StreetAddress = user.StreetAddress,
            City = user.City,
            State = user.State,
            PostalCode = user.PostalCode,
            Country = user.Country,
            CreatedAt = user.CreatedAt,
            Orders = orders
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FullName = model.FullName;
        user.StreetAddress = model.StreetAddress;
        user.City = model.City;
        user.State = model.State;
        user.PostalCode = model.PostalCode;
        user.Country = model.Country;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Profile coordinates updated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update profile coordinates.";
        }

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> OrderDetails(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();

        // Ensure user can only view their own order unless Admin
        var isAdmin = User.IsInRole("Admin");
        if (order.UserId != user.Id && !isAdmin)
        {
            return Forbid();
        }

        return View(order);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
