namespace Silicon_AccountProvider.Models;


public class SignInRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool IsPresistent { get; set; }
}
