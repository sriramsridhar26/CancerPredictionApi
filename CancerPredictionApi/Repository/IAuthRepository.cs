using CancerPredictionApi.Model;
using System.Runtime.CompilerServices;

namespace CancerPredictionApi.Repository
{
    public interface IAuthRepository
    {
        Task<bool> Add(User user);
        Task<User> UserGetById(string emailId);
        Task<bool> UserExists(string emailId);
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt );
        string CreateToken(User user);
    }
}
