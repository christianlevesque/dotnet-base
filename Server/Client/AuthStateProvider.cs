using System.Security.Claims;
using System.Threading.Tasks;
using Autoinjector;
using Microsoft.AspNetCore.Components.Authorization;

namespace Server.Client;

[Service(typeof(AuthenticationStateProvider))]
public class AuthStateProvider : AuthenticationStateProvider
{
	public override Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		var identity = new ClaimsIdentity();
		var principal = new ClaimsPrincipal(identity);
		var state = new AuthenticationState(principal);
		return Task.FromResult(state);
	}
}