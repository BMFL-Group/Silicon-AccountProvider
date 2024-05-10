using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Silicon_AccountProvider.Models;
using System.Text;


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
                // verify code using VerificationProvider, d�r man fick koden ska �ven generera kod 
                // �ndra true till adressen till VerifactionProvider 
                // detta �r bara en simulering 


                // denna fungerar inte, m�ste ha r�tt adress till verificationprovidern
                // �ndra detta till service bus
                try
                {
                    using var http = new HttpClient();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(vr), Encoding.UTF8, "application/json");
                    // skickar ingenstanns nu
                    //var response = await http.PostAsync("http://verificationprovider.silicon.azurewebsite.net/api/verify", content);

                    // true �r bara en simulering f�r att man ska kunna testa
                    if (true)
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
                catch (Exception ex)
                {
                    _logger.LogError($"http.PostAsync :: {ex.Message}");
                }
              
            }
        }

        return new UnauthorizedResult();
    }
}