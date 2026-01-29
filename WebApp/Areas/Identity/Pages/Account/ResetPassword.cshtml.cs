using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ResetPasswordModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match")]
            public string ConfirmPassword { get; set; }
        }

        public IActionResult OnGet(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToPage("./Login");

            Input = new InputModel { Email = email };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return Page();
            }

            // Remove old password
            await _userManager.RemovePasswordAsync(user);

            // Set new password
            var result = await _userManager.AddPasswordAsync(user, Input.Password);

            if (result.Succeeded)
                return RedirectToPage("./Login");

            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);

            return Page();
        }
    }
}
