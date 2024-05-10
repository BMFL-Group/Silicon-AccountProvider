using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Silicon_AccountProvider.Models;
using System.Text;


namespace Silicon_AccountProvider.Functions

{
    public class SignUp(ILogger<SignUp> logger, UserManager<UserAccount> userManager)

    {

        private readonly ILogger<SignUp> _logger = logger;
        private readonly UserManager<UserAccount> _userManager = userManager;

        [Function("SignUp")]

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
                UserRegistrationRequest urr = null!;

                try
                {
                    urr = JsonConvert.DeserializeObject<UserRegistrationRequest>(body)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"JsonConvert.DeserializeObject<UserRegistrationRequest> :: {ex.Message}");
                }

                if (urr != null && !string.IsNullOrEmpty(urr.Email) && !string.IsNullOrEmpty(urr.Password))
                {
                    if (!await _userManager.Users.AnyAsync(x => x.Email == urr.Email))
                    {
                        var userAccount = new UserAccount
                        {
                            FirstName = urr.FirstName,
                            LastName = urr.LastName,
                            Email = urr.Email,
                            UserName = urr.Email
                        };

                        try
                        {
                            var result = await _userManager.CreateAsync(userAccount, urr.Password);
                            if (result.Succeeded)
                            {
                                // send verifactionCode h�r! 

                                // denna fungerar inte, m�ste ha r�tt adress till verificationprovidern
                                // �ndra detta till service bus
                               try
                                {
                                    using var http = new HttpClient();
                                    StringContent content = new StringContent(JsonConvert.SerializeObject(new { Email = userAccount.Email }), Encoding.UTF8, "application/json");
                                    var response = await http.PostAsync("http://verificationprovider.silicon.azurewebsite.net/api/generate", content);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"http.PostAsync :: {ex.Message}");
                                }

                                return new OkResult();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"_userManager.CreateAsync :: {ex.Message}");
                        }
                    }
                    else
                    {
                        return new ConflictResult();
                    }
                }
            }
            return new BadRequestResult();
        }
    }
}
