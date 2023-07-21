// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AWSGymWebsite.Areas.Identity.Data;
using AWSGymWebsite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AWSGymWebsite.Areas.Identity.Pages.Account.Manage
{
    public class BusinessModel : PageModel
    {
        private readonly UserManager<AWSGymWebsiteUser> _userManager;
        private readonly SignInManager<AWSGymWebsiteUser> _signInManager;

        public BusinessModel(
            UserManager<AWSGymWebsiteUser> userManager,
        SignInManager<AWSGymWebsiteUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string BusinessEmail { get; set; }

        public string BusinessContactNumber { get; set; }
        public string BusinessSSM { get; set; }
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public string BusinessEmail { get; set; }
            public string BusinessContactNumber { get; set; }
            public string BusinessSSM { get; set; }
        }

        private async Task LoadAsync(AWSGymWebsiteUser user)
        {
            //Get user detail
            user = await _userManager.GetUserAsync(User);
            //Retrive detail
            var bemail = user.BusinessEmail;
            var bcontact = user.BusinessContactNumber;
            var bssm = user.BusinessSSM;

            BusinessEmail = bemail;
            BusinessContactNumber = bcontact;
            BusinessSSM = bssm;

            Input = new InputModel
            {
                BusinessEmail = bemail,
                BusinessContactNumber = bcontact,
                BusinessSSM = bssm

            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            //Update Business Profile Here

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your Business Info has been updated";
            return RedirectToPage();
        }
    }
}
