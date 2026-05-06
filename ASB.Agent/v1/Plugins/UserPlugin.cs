using ASB.Services.v1.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ASB.Agent.v1.Plugins;

/// <summary>
/// Semantic Kernel plugin that exposes User management operations as tools
/// the LLM agent can call during a conversation.
/// </summary>
public class UserPlugin
{
    private readonly IUserService _userService;

    public UserPlugin(IUserService userService)
    {
        _userService = userService;
    }

    [KernelFunction("get_all_users")]
    [Description("Retrieves a list of all users in the system with their IDs, usernames, emails, and group assignments.")]
    public async Task<string> GetAllUsersAsync()
    {
        var users = await _userService.GetUsers();
        var summary = users.Select(u => $"- {u.Username} (ID: {u.Id}, Email: {u.Email})");
        return string.Join("\n", summary);
    }

    [KernelFunction("get_user_by_id")]
    [Description("Gets detailed information about a specific user by their integer ID.")]
    public async Task<string> GetUserByIdAsync(
        [Description("The integer ID of the user")] int userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return $"No user found with ID {userId}";

        return $"User: {user.Username}, Email: {user.Email}, Groups: {string.Join(", ", user.UserGroupIds ?? [])}";
    }
}
