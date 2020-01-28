using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using WebApp.TokenStorage;

namespace WebApp.Helpers
{
    public static class GraphHelper
    {
        private static string appId = ConfigurationManager.AppSettings["ida:AppId"];
        private static string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string graphScopes = ConfigurationManager.AppSettings["ida:AppScopes"];


        private static GraphServiceClient GetAuthenticatedClient()
        {
            return new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        var idClient = ConfidentialClientApplicationBuilder.Create(appId)
                            .WithRedirectUri(redirectUri)
                            .WithClientSecret(appSecret)
                            .Build();

                        var tokenStore = new SessionTokenStore(idClient.UserTokenCache,
                                HttpContext.Current, ClaimsPrincipal.Current);

                        var accounts = await idClient.GetAccountsAsync().ConfigureAwait(true);

                        // By calling this here, the token can be refreshed
                        // if it's expired right before the Graph call is made
                        var scopes = graphScopes.Split(' ');
                        var result = await idClient.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                            .ExecuteAsync().ConfigureAwait(false);

                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    }));
        }

        public static async Task<User> GetUserDetailsAsync(string accessToken)
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);
                    }));

            return await graphClient.Me.Request().GetAsync().ConfigureAwait(false);
        }

        public static async Task<IEnumerable<User>> GetUsersAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var users = await graphClient.Users.Request().GetAsync().ConfigureAwait(false);
            return users.CurrentPage;
        }

        public static async Task<User> CreateUserAsync()
        {
            var graphClient = GetAuthenticatedClient();

            var user = new User
            {
                AccountEnabled = true,
                DisplayName = "TestUserByGraph",
                MailNickname = "TestUserByGraph",
                UserPrincipalName = "testuserbygraph@dotnetsoft.co.kr",
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = true,
                    Password = "password1!"
                }
            };

            User savedUser = await graphClient.Users
                .Request()
                .AddAsync(user).ConfigureAwait(false);

            return savedUser;
        }
    }
}