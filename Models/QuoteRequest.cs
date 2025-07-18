using System.ComponentModel.DataAnnotations;

namespace SweepoServer.Models
{
    public class QuoteRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Service { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string Source { get; set; } = "website";
    }

    public class QuoteResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RequestId { get; set; }
    }
}
