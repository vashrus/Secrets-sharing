namespace WebApplication1.Services
{
    public interface IUserService
    {
        string GenerateJwtToken(int userId);
    }
}