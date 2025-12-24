using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Data.Entities;

public enum UserType
{
    Customer = 0,
    Admin = 1,
    Supplier = 2
}

public class ApplicationUser : IdentityUser
{
    public UserType UserType { get; set; } = UserType.Customer;
}
