// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using AWSGymWebsite.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using AWSGymWebsite.Models;

namespace AWSGymWebsite.Areas.Identity.Pages.Account
{
    public class GymOwnerRegisterModel : PageModel
    {
        private readonly SignInManager<AWSGymWebsiteUser> _signInManager;
        private readonly UserManager<AWSGymWebsiteUser> _userManager;
        private readonly IUserStore<AWSGymWebsiteUser> _userStore;
        private readonly IUserEmailStore<AWSGymWebsiteUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> roleManager;


        public GymOwnerRegisterModel(
            UserManager<AWSGymWebsiteUser> userManager,
            IUserStore<AWSGymWebsiteUser> userStore,
            SignInManager<AWSGymWebsiteUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<AWSGymWebsiteUser>)GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            this.roleManager = roleManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Email is Required")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }


            [Required(ErrorMessage = "First Name is Required")]
            [RegularExpression("[a-zA-Z]{1,99}", ErrorMessage = "Alphabet only")]
            [Display(Name = "First Name")]
            public string Userfname { get; set; }

            [Required(ErrorMessage = "Last Name is Required")]
            [RegularExpression("[a-zA-Z]{1,99}", ErrorMessage = "Alphabet only")]
            [Display(Name = "Last Name")]
            public string Userlname { get; set; }

            [Required(ErrorMessage = "Business SSM Number is Required")]
            [RegularExpression("[0-9]{4}[0-9]{2}[0-9]{6}", ErrorMessage = "12 digit SSM Register Number")]
            [Display(Name = "Business SSM Number")]
            public string BusinessSSM { get; set; }

            [Required(ErrorMessage = "Business Email is Required")]
            [EmailAddress]
            [Display(Name = "Business Email")]
            public string BusinessEmail { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
         
             ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
 
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new AWSGymWebsiteUser
                {
                    Email = Input.Email,
                    Userfname = Input.Userfname,
                    Userlname = Input.Userlname,
                    BusinessSSM = Input.BusinessSSM,
                    BusinessEmail= Input.BusinessEmail,
                    EmailConfirmed = true,
                    role = "GymOwner"
                };

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    bool roleresult = await roleManager.RoleExistsAsync("Viewer");
                    if (!roleresult)
                    {
                        await roleManager.CreateAsync(new IdentityRole("Viewer"));
                    }
                    roleresult = await roleManager.RoleExistsAsync("GymOwner");
                    if (!roleresult)
                    {
                        await roleManager.CreateAsync(new IdentityRole("GymOwner"));
                    }
                    //Register user as viewer
                    await _userManager.AddToRoleAsync(user, "GymOwner");

                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private AWSGymWebsiteUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AWSGymWebsiteUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AWSGymWebsiteUser)}'. " +
                    $"Ensure that '{nameof(AWSGymWebsiteUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<AWSGymWebsiteUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AWSGymWebsiteUser>)_userStore;
        }
    }
}
