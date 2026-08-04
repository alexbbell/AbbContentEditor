namespace AbbContentEditor.Models.Account
{
    public class ForgottenPasswordRequestModel
    {
        public string Email { get; set; } = string.Empty;
        public string TurnstileToken { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestModel
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class TurnstileVerifyResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("error-codes")]
        public string[] ErrorCodes { get; set; } = Array.Empty<string>();
    }
}
