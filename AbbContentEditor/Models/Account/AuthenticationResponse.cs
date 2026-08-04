namespace AbbContentEditor.Models.Account
{


    public class AuthenticationResponse
    {
        public UserDto User { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
