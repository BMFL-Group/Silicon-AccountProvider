using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Silicon_AccountProvider.Models;


namespace Silicon_AccountProvider.Functions;


public class SignIn(ILogger<SignIn> logger, SignInManager<UserAccount> signInManager, UserManager<UserAccount> userManager)

{
    private readonly ILogger<SignIn> _logger = logger;
    private readonly SignInManager<UserAccount> _signInManager = signInManager;
    private readonly UserManager<UserAccount> _userManager = userManager;


    [Function("SignIn")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string body = null!;

        try
        {
            body = await new StreamReader(req.Body).ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"StreamReader :: {ex.Message}");
        }

        if (body != null)
        {
            // sir = SignInRequest
            SignInRequest sir = null!;

            try
            {
                sir = JsonConvert.DeserializeObject<SignInRequest>(body)!;
            }
            catch (Exception ex)
            {
                _logger.LogError($"JsonConvert.DeserializeObject<UserLoginRequest> :: {ex.Message}");
            }

            if (sir != null && !string.IsNullOrEmpty(sir.Email) && !string.IsNullOrEmpty(sir.Password))
            {
                try
                {
                    var userAccount = await _userManager.FindByEmailAsync(sir.Email);
                    var result = await _signInManager.CheckPasswordSignInAsync(userAccount!, sir.Password, false);

                    if (result.Succeeded)
                    {
                        // Get accesstoken from TokenProvider 
                        return new OkObjectResult("accesstoken");
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"_signInManager.PasswordSignInAsync :: {ex.Message}");
                }

                return new UnauthorizedResult();
            }
        }
        return new BadRequestResult();
    }
}
