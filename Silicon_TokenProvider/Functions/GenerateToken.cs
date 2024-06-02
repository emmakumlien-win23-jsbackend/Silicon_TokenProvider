using Infrastructure.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Silicon_TokenProvider.Functions
{
    public class GenerateToken
    {
        private readonly ILogger<GenerateToken> _logger;
        private readonly ITokenService _tokenService;

        public GenerateToken(ILogger<GenerateToken> logger, ITokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
        }

        [Function("GenerateToken")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "token/generate")] HttpRequest req)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var tokenRequest = JsonConvert.DeserializeObject<TokenRequest>(body);



            if (tokenRequest == null || tokenRequest.UserId == null || tokenRequest.Email == null)
            {
                return new BadRequestObjectResult(new { Error = "Please provide an valid userid and email address" });
            }
            try
            {
                RefreshTokenResult refreshTokenResult = null!;
                AccessTokenResult accessTokenResult = null!;


                using var ctsTimeOut = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctsTimeOut.Token, req.HttpContext.RequestAborted);

                req.HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);

                
                

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    refreshTokenResult = await _tokenService.GenerateRefreshTokenAsync(tokenRequest.UserId, cts.Token);

                    accessTokenResult = _tokenService.GenerateAccessToken(tokenRequest, refreshTokenResult.Token);

                    if (refreshTokenResult.Token != null && refreshTokenResult.cookieOptions != null)
                    {
                        req.HttpContext.Response.Cookies.Append("refreshToken", refreshTokenResult.Token, refreshTokenResult.cookieOptions);
                    }

                    if (accessTokenResult != null && accessTokenResult.Token != null && refreshTokenResult.Token != null)
                    {

                        return new OkObjectResult(new { AccessToken = accessTokenResult.Token, RefreshToken = refreshTokenResult.Token });

                    }
                }

            }
            catch { }
            return new UnauthorizedObjectResult(new { Error = "Could not create token." });

        }
    }
}


