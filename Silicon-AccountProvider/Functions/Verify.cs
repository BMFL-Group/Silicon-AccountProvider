using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Silicon_AccountProvider.Models;


namespace Silicon_AccountProvider.Functions;

public class Verify
{
    private readonly ILogger<Verify> _logger;
    private readonly UserManager<UserAccount> _userManager;

    public Verify(ILogger<Verify> logger, UserManager<UserAccount> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    [Function("Verify")]
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
            // urr = user registration request 
            VerificationRequest vr = null!;

            try
            {
                vr = JsonConvert.DeserializeObject<VerificationRequest>(body)!;
            }
            catch (Exception ex)
            {
                _logger.LogError($"JsonConvert.DeserializeObject<VerificationRequest> :: {ex.Message}");
            }

            if (vr != null && !string.IsNullOrEmpty(vr.Email) && !string.IsNullOrEmpty(vr.VerificationCode))
            {
                // verify code using VerificationProvider, där man fick koden ska även generera kod 
                // ändra true till adressen till VerifactionProvider 
                // detta är bara en simulering 
                var isVerified = true;

                if (isVerified)
                {
                    var userAccount = await _userManager.FindByEmailAsync(vr.Email);
                    if (userAccount != null)
                    {
                        userAccount.EmailConfirmed = true;
                        await _userManager.UpdateAsync(userAccount);

                        if (await _userManager.IsEmailConfirmedAsync(userAccount))
                        {
                            return new OkResult();
                        }
                    }
                }
            }
        }

        return new UnauthorizedResult();
    }
}