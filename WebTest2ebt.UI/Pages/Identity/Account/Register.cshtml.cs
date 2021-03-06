using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WebTest2ebt.BusinessLogicLayer.Managers;
using WebTest2ebt.BusinessLogicLayer.Models;
using WebTest2ebt.BusinessLogicLayer.Validation;
using WebTest2ebt.DataAccessLayer.Models.Identity;
using WebTest2ebt.UI.AppConfiguration;

namespace WebTest2ebt.UI.Pages.Identity.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityBuyer> _signInManager;
        private readonly UserManager<IdentityBuyer> _userManager;
        private readonly CartManager _cartManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<IdentityBuyer> userManager,
            SignInManager<IdentityBuyer> signInManager,
            ILogger<RegisterModel> logger,
            CartManager cartManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _cartManager = cartManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]            
            [Display(Name = "FirstName")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "SecondName")]
            public string SecondName { get; set; }

            [Required]
            [Display(Name = "Address")]

            public string Address { get; set; }
            [Required]
            [RegularExpression(RegexCollection.PhoneRegex)]
            [Display(Name = "PhoneNumber")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "PassportNumber")]
            public string PassportNumber { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

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
            returnUrl = returnUrl ?? Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new IdentityBuyer { UserName = Input.Email, Email = Input.Email,
                                               FirstName = Input.FirstName, SecondName = Input.SecondName, 
                                               Address = Input.Address, PhoneNumber = Input.PhoneNumber,
                                               PassportNumber = Input.PassportNumber};

                var result = await _userManager.CreateAsync(user, Input.Password);

                var cart = new Cart { IdentityBuyerId = user.Id };

                await _cartManager.Add(cart);

                var a = await _cartManager.GetAll();

                

                user.Id = cart.IdentityBuyerId;

                await _userManager.UpdateAsync(user);

                await _userManager.AddToRoleAsync(user, RolesNames.User);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
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
    }
}
