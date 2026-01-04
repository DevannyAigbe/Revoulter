namespace Revoulter.Core.Models
{
    public class SendCodeRequest
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

    public class VerifyCodeRequest
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Code { get; set; }
    }
}
