using System;
using System.Configuration;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Nancy;
using Nancy.Security;

namespace Auth0.Nancy.SelfHost
{
    public static class ModuleExtensions
    {
        private static readonly AuthenticationApiClient Auth0Client = new AuthenticationApiClient(new Uri("https://" + ConfigurationManager.AppSettings["auth0:Domain"]));

        public static void RequiresAuthentication(this NancyModule module)
        {
            module.Before.AddItemToEndOfPipeline(Auth0Authentication.AuthenticateSession);
        }

        public static IResponseFormatter AuthenticateThisSession(this NancyModule module)
        {
            var code = (string) module.Request.Query["code"];

            var token = Auth0Client.ExchangeCodeForAccessTokenAsync(new ExchangeCodeRequest
            {
                ClientId = ConfigurationManager.AppSettings["auth0:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["auth0:ClientSecret"],
                RedirectUri = ConfigurationManager.AppSettings["auth0:CallbackUrl"],
                AuthorizationCode = code
 
             }).ConfigureAwait(false).GetAwaiter().GetResult();

            var userInfo = Auth0Client.GetUserInfoAsync(token.AccessToken).ConfigureAwait(false).GetAwaiter().GetResult();

            var user = new Auth0User
            {
                AccessToken = token.AccessToken,
                UserToken = token.IdToken,
                UserId = userInfo.UserId,
                Name = userInfo.FullName,
                Nickname = userInfo.NickName,
                GravatarUrl = userInfo.Picture,
                Email = userInfo.Email,
                UserMetadata = userInfo.UserMetadata,
                AppMetadata = userInfo.AppMetadata
            };

            Auth0Authentication.CreateAuthenticationSessionFor(user, module.Context.Request.Session);

            return module.Response;
        }

        public static IResponseFormatter RemoveAuthenticationFromThisSession(this NancyModule module)
        {
            var userInstance = module.Context.CurrentUser.ToUserModel();
            Auth0Authentication.RemoveAuthenticationFor(userInstance, module.Session);

            return module.Response;
        }

        public static bool SessionIsAuthenticated(this NancyModule module)
        {
            return module.Context.CurrentUser.IsAuthenticated();
        }

        public static Response ThenRedirectTo(this IResponseFormatter response, string viewName)
        {
            return response.AsRedirect(viewName);
        }
    }
}