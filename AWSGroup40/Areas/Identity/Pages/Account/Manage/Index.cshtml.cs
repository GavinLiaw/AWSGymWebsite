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
    public class IndexModel : PageModel
    {
        private readonly UserManager<AWSGymWebsiteUser> _userManager;
        private readonly SignInManager<AWSGymWebsiteUser> _signInManager;

        public IndexModel(
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
        [Required]
        public string Userfname { get; set; }
        [Required]
        public string Userlname { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public string ContactNumber { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime UserDob { get; set; }
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
            public string Userfname { get; set; }
            public string Userlname { get; set; }
            public string Gender { get; set; }
            public string ContactNumber { get; set; }
            public DateTime UserDob { get; set; }

        }

        private async Task LoadAsync(AWSGymWebsiteUser user)
        {
            //Get user detail
            user = await _userManager.GetUserAsync(User);
            //Retrive detail
            var ufname = user.Userfname;
            var ulname = user.Userlname;
            var ugender = user.Gender;
            var ucontact = user.ContactNumber;
            var udob = user.UserDob;

            Userfname = ufname;
            Userlname = ulname;
            Gender = ugender;
            ContactNumber = ucontact;
            UserDob = udob;

            Input = new InputModel
            {
                Userfname = ufname,
                Userlname = ulname,
                Gender = ugender,
                ContactNumber = ucontact,
                UserDob = udob,
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

            //Update Profile
            //Update Business Profile Here
            user.ContactNumber = Input.ContactNumber;
            user.Userfname = Input.Userfname;
            user.Userlname= Input.Userlname;
            user.Gender = Input.Gender;
            user.UserDob = Input.UserDob;
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
