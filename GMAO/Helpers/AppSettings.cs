namespace GMAO.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; } = string.Empty;
        public int TokenExpirationMinutes { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
    }
}
