namespace HealthConnect.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<ProfileDashboardViewModel> GetDashboardAsync(string userId);
        Task<UserProfileViewModel> GetUserProfileAsync(string userId);
        Task<IdentityResult> UpdateUserProfileAsync(string userId, UserProfileViewModel updatedUser);
        Task<byte[]> GetProfilePhotoAsync(string userId);
    }
}
