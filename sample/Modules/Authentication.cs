using Auth0.Nancy.SelfHost;
using Nancy;
using System.Configuration;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;

namespace auth0_nancyfx_sample
{
    public class Authentication
        : NancyModule
    {
        public Authentication()
        {
            Get["/login"] = o =>
            {
                if (this.SessionIsAuthenticated())
                    return Response.AsRedirect("securepage");

                var apiClient = new AuthenticationApiClient(ConfigurationManager.AppSettings["auth0:domain"]);
                var authorizationUri = apiClient.BuildAuthorizationUrl()
                    .WithClient(ConfigurationManager.AppSettings["auth0:ClientId"])
                    .WithRedirectUrl(ConfigurationManager.AppSettings["auth0:CallbackUrl"])
                    .WithResponseType(AuthorizationResponseType.Code)
                    .WithScope("openid profile")
                    .Build();

                return Response.AsRedirect(authorizationUri.ToString());
            };

            Get["/login-callback"] = o => this
                .AuthenticateThisSession()
                .ThenRedirectTo("securepage");

            Get["/logout"] = o => this
                .RemoveAuthenticationFromThisSession()
                .ThenRedirectTo("index");
        }
    }
}