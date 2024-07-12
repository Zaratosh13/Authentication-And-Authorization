namespace Authentication_And_Authorization.Models.DTO
{
    public class TokenResponse
    {
        public string? TokenString { get; set; }

        public DateTime ValidTo { get; set; }
    }
}
