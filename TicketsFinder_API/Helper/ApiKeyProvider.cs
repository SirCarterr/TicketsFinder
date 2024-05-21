using AspNetCore.Authentication.ApiKey;
using System.Security.Claims;

namespace TicketsFinder_API.Helper
{
    public class ApiKeyProvider : IApiKeyProvider
    {
        public async Task<IApiKey> ProvideAsync(string key)
        {
            return new ApiKey("37ac4560-7e1a-44b5-8ec5-f4dfe5f14bfe", "TicketsNotifier_Bot", new List<Claim>()
            {
                new(ClaimTypes.Name, "tickets_finder_bot")
            });
        }

        public class ApiKey : IApiKey
        {
            public ApiKey(string key, string owner, List<Claim> claims = null)
            {
                Key = key;
                OwnerName = owner;
                Claims = claims ?? new List<Claim>();
            }

            public string Key { get; }
            public string OwnerName { get; }
            public IReadOnlyCollection<Claim> Claims { get; }
        }
    }
}
