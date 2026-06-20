using Microsoft.Extensions.Options;

namespace FlightPlanApi.Authentication
{
    public class UserService : IUserService
    {
        private readonly AdminCredentials _credentials;

        public UserService(IOptions<AdminCredentials> credentials)
        {
            _credentials = credentials.Value;
        }

        public Task<User> Authenticate(string username, string password)
        {
            if (username != _credentials.Username || password != _credentials.Password)
            {
                return Task.FromResult<User>(null);
            }

            var user = new User
            {
                Username = username,
                Id = Guid.NewGuid().ToString("N")
            };

            return Task.FromResult(user);
        }
    }
}