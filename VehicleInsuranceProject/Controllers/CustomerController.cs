using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using VehicleInsuranceProject.BusinessLogic.Services;
using VehicleInsuranceProject.Repository.Models;
using VehicleInsuranceProject.ViewModels;
using System.Threading.Tasks;
using System.Linq;

namespace VehicleInsuranceProject.Controllers
{
    [Authorize] // Require authentication for most actions in this controller
    public class CustomerController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ICustomerService _customerService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IVehicleService _vehicleService;

        public CustomerController(
            ICustomerService customerService,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IVehicleService vehicleService)
        {
            _customerService = customerService;
            _signInManager = signInManager;
            _userManager = userManager;
            _vehicleService = vehicleService;
        }

        // GET: Customer/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Customer/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(CustomerLoginViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Name); // Assuming Name is username/email

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: model.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        user = await _userManager.FindByNameAsync(model.Name);

                        if (user != null)
                        {
                            if (await _userManager.IsInRoleAsync(user, "Admin"))
                            {
                                return RedirectToAction("AdminDashboard", "Admin");
                            }
                            else if (await _userManager.IsInRoleAsync(user, "Customer"))
                            {
                                TempData["WelcomeMessage"] = $"Welcome back, {user.UserName}!"; // Use UserName from IdentityUser
                                return RedirectToAction("CustomerDashboard");
                            }
                            return LocalRedirect(returnUrl);
                        }
                    }
                    else if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Account locked out.");
                    }
                    else if (result.IsNotAllowed)
                    {
                        ModelState.AddModelError(string.Empty, "Login not allowed (e.g., email not confirmed).");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your username/email and password.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt. User not found.");
                }
            }
            return View(model);
        }

        // GET: Customer/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Customer/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(CustomerRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = new Customer
                {
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address
                };

                var result = await _customerService.CreateCustomerAsync(customer, model.Password);

                if (result.Succeeded)
                {
                    var createdUser = await _userManager.FindByEmailAsync(model.Email);
                    if (createdUser != null)
                    {
                        await _signInManager.SignInAsync(createdUser, isPersistent: false);
                        TempData["WelcomeMessage"] = $"Registration successful! Welcome, {createdUser.UserName}!";
                        return RedirectToAction("CustomerDashboard");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // GET: Customer/CustomerDashboard
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CustomerDashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Customer");
            }

            var customer = await _customerService.GetCustomerByIdAsync(currentUser.Id);

            if (customer == null)
            {
                TempData["ErrorMessage"] = "Your customer profile is incomplete. Please update your details to proceed.";
                return RedirectToAction("UpdateProfile", "Customer");
            }

            // Vehicles are fetched based on the CustomerId linked to the IdentityUser
            customer.Vehicles = _vehicleService.GetVehicleByCustomerId(customer.CustomerId).ToList();

            return View(customer);
        }

        // GET: Customer/Details (View Customer Profile)
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Details()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var customer = await _customerService.GetCustomerByIdAsync(user.Id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Your customer profile was not found. Please update it.";
                return RedirectToAction("UpdateProfile");
            }

            var model = new CustomerDetailsViewModel
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address
            };
            return View(model);
        }

        // GET: Customer/UpdateProfile (Edit Customer Profile)
        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var customer = await _customerService.GetCustomerByIdAsync(user.Id);

            if (customer != null)
            {
                // Profile exists, load existing data
                var model = new CustomerUpdateViewModel
                {
                    CustomerId = customer.CustomerId,
                    Name = customer.Name,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Address = customer.Address
                };
                return View(model);
            }
            else
            {
                // Profile does NOT exist, initialize empty ViewModel for creation
                var newCustomerModel = new CustomerUpdateViewModel
                {
                    Email = user.Email, // Pre-fill email from IdentityUser
                    Name = user.UserName // Pre-fill name from IdentityUser, if available (often null for email-based login)
                };
                return View(newCustomerModel);
            }
        }

        // POST: Customer/UpdateProfile
        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(CustomerUpdateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var customer = await _customerService.GetCustomerByIdAsync(user.Id);

                if (customer == null)
                {
                    // If profile doesn't exist, create it.
                    var newCustomer = new Customer
                    {
                        IdentityUserId = user.Id, // Link to the existing IdentityUser's ID
                        Name = model.Name,
                        Email = model.Email,
                        Phone = model.Phone,
                        Address = model.Address
                    };
                    await _customerService.AddCustomerProfileAsync(newCustomer);
                    TempData["SuccessMessage"] = "Profile created successfully!";

                }
                else
                {
                    // Update existing profile
                    customer.Name = model.Name;
                    customer.Email = model.Email;
                    customer.Phone = model.Phone;
                    customer.Address = model.Address;
                    await _customerService.UpdateCustomerAsync(customer);
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                }

                return RedirectToAction("Details");
            }
            return View(model);
        }

        // POST: Customer/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}