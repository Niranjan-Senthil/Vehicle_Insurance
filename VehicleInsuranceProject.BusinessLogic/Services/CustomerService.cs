// VehicleInsuranceProject.BusinessLogic.Services/CustomerService.cs
using Microsoft.AspNetCore.Identity;
using VehicleInsuranceProject.Repository.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using VehicleInsuranceProject.Repository.Data; // Import ApplicationDbContext namespace
using Microsoft.EntityFrameworkCore; // For .FirstOrDefaultAsync and .ToListAsync

namespace VehicleInsuranceProject.BusinessLogic.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        // REMOVE THIS LINE: private readonly SignInManager<IdentityUser> _signInManager;

        public CustomerService(UserManager<IdentityUser> userManager,
                               ApplicationDbContext context
                               // REMOVE THIS PARAMETER: SignInManager<IdentityUser> signInManager
                               )
        {
            _userManager = userManager;
            _context = context;
            // REMOVE THIS LINE: _signInManager = signInManager;
        }

        public async Task<IdentityResult> CreateCustomerAsync(Customer customer, string password)
        {
            var user = new IdentityUser { UserName = customer.Email, Email = customer.Email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                customer.IdentityUserId = user.Id;
                customer.IsActive = true; // Set IsActive to true for new customer
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }
            return result;
        }

        public async Task<Customer> GetCustomerByIdAsync(string identityUserId)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.IdentityUserId == identityUserId);
        }

        public async Task UpdateCustomerAsync(Customer updatedCustomer)
        {
            if (updatedCustomer.CustomerId <= 0)
            {
                throw new ArgumentException("A valid CustomerId is required for update.");
            }

            var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == updatedCustomer.CustomerId);
            if (existingCustomer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {updatedCustomer.CustomerId} not found for update.");
            }

            // Basic validation for required fields
            if (string.IsNullOrWhiteSpace(updatedCustomer.Name))
                throw new ArgumentException("Name cannot be empty.");
            if (string.IsNullOrWhiteSpace(updatedCustomer.Email))
                throw new ArgumentException("Email cannot be empty.");

            // Update only the mutable profile fields
            existingCustomer.Name = updatedCustomer.Name;
            existingCustomer.Email = updatedCustomer.Email;
            existingCustomer.Phone = updatedCustomer.Phone;
            existingCustomer.Address = updatedCustomer.Address;

            // DO NOT update existingCustomer.IsActive here. It's controlled by dedicated methods.

            _context.Customers.Update(existingCustomer);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task AddCustomerProfileAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }



        // Admin Methods

        // --- NEW: GetCustomerByCustomerIdAsync ---
        public async Task<Customer?> GetCustomerByCustomerIdAsync(int customerId)
        {
            return await _context.Customers
                                 .Include(c => c.IdentityUser) // Include IdentityUser for full data
                                 .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        // --- NEW: DeactivateCustomerAsync ---
        public async Task DeactivateCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {customerId} not found for deactivation.");
            }

            if (!customer.IsActive)
            {
                throw new InvalidOperationException($"Customer with ID {customerId} is already inactive.");
            }

            customer.IsActive = false;
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            // Also lock out the associated IdentityUser to prevent login
            var identityUser = await _userManager.FindByIdAsync(customer.IdentityUserId);
            if (identityUser != null)
            {
                await _userManager.SetLockoutEndDateAsync(identityUser, DateTimeOffset.UtcNow.AddYears(100)); // Lock out indefinitely
            }
        }

        // --- NEW: ActivateCustomerAsync ---
        public async Task ActivateCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {customerId} not found for activation.");
            }

            if (customer.IsActive)
            {
                throw new InvalidOperationException($"Customer with ID {customerId} is already active.");
            }

            customer.IsActive = true;
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            // Also unlock the associated IdentityUser
            var identityUser = await _userManager.FindByIdAsync(customer.IdentityUserId);
            if (identityUser != null)
            {
                await _userManager.SetLockoutEndDateAsync(identityUser, null); // Remove lockout
            }
        }

        // --- MODIFIED: GetAllCustomersAsync (renamed for clarity and fetches IdentityUser) ---
        // Your old GetAllCustomersAsync() now becomes GetAllCustomersForAdminAsync()
        public async Task<IEnumerable<Customer>> GetAllCustomersForAdminAsync()
        {
            return await _context.Customers
                                 .Include(c => c.IdentityUser) // Include IdentityUser for admin display
                                 .ToListAsync();
        }

        // --- NEW: SearchCustomersAsync ---
        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            var query = _context.Customers.Include(c => c.IdentityUser).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(searchTerm) ||
                                         c.Email.ToLower().Contains(searchTerm) ||
                                         c.CustomerId.ToString().Contains(searchTerm));
            }

            return await query.ToListAsync();
        }

        // --- REMOVED: Your old DeleteCustomerAsync and AddCustomerProfileAsync are implicitly replaced/handled ---
        // If you had a separate AddCustomerProfileAsync, its functionality is now incorporated into CreateCustomerAsync.
        // If you still need a hard DeleteCustomer, consider its implications on related data.
        // For typical admin use, Deactivate is usually preferred over hard delete for customers.
    


}
}