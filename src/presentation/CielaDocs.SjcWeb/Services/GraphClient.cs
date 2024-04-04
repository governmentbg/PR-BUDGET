using Microsoft.Graph;
using Microsoft.Identity.Client;

using System;
using System.Threading.Tasks;

namespace CielaDocs.SjcWeb.Services
{
    public class GraphClient
    {
        private GraphServiceClient serviceClient;
        public GraphServiceClient ServiceClient
        {
            get
            {
                if (serviceClient != null)
                {
                    return serviceClient;
                }

                serviceClient = new GraphServiceClient(AuthenticationProvider.AuthProvider);
                return serviceClient;
            }
        }

        public GraphClientAuthProvider AuthenticationProvider { get; }
        public GraphClient(GraphClientAuthProvider authProvider)
        {
            AuthenticationProvider = authProvider;
        }
        public async Task<object> GetGraphUser(string upn = "me")
        {
            User user;
            try
            {
                if (upn.Equals("me"))
                {
                    user = await ServiceClient.Me.Request().GetAsync();
                }
                else
                {
                    user = await ServiceClient.Users[upn].Request().GetAsync();
                }
            }

            catch (MsalUiRequiredException ex)
            {
                AuthenticationProvider.Token.ReplyForbiddenWithWwwAuthenticateHeader(AuthenticationProvider.Scopes, ex);
                throw ex;
            }
            catch (Exception)
            {
                throw;
            }

            return new
            {
                name = user?.DisplayName,
                upn = user?.UserPrincipalName

            };
        }
    }
}
