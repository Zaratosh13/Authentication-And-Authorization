namespace Authentication_And_Authorization.Models.DTO
{
    public class RefreshTokenRequest
    {
        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }
    }
}