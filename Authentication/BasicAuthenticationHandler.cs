namespace FlightPlanApi.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserService _userService;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserService userService)
            : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            User? user;

            try
            {
                var authHeaderValue = Request.Headers["Authorization"].FirstOrDefault();

                if (string.IsNullOrEmpty(authHeaderValue))
                {
                    return AuthenticateResult.Fail("Missing Authorization header.");
                }

                var authHeader = AuthenticationHeaderValue.Parse(authHeaderValue);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? string.Empty);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);

                if (credentials.Length < 2)
                {
                    return AuthenticateResult.Fail("Invalid authorization header format.");
                }

                var username = credentials[0];
                var password = credentials[1];
                user = await _userService.Authenticate(username, password);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Authorization failed.");
                return AuthenticateResult.Fail("Authorization failed.");
            }

            if (user == null)
            {
                return AuthenticateResult.Fail("Invalid Credentials");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}