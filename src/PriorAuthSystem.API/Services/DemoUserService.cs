using PriorAuthSystem.API.Models;

namespace PriorAuthSystem.API.Services;

public class DemoUserService
{
    public DemoUser GetUserByRole(string role) => role switch
    {
        "Admin" => new DemoUser("Admin", "Dr. Sarah Chen", "admin-001"),
        "Reviewer" => new DemoUser("Reviewer", "Marcus Williams", "reviewer-001"),
        _ => new DemoUser("Provider", "Dr. James Okafor", "provider-001")
    };
}
