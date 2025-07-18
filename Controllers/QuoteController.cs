using Microsoft.AspNetCore.Mvc;
using SweepoServer.Models;
using SweepoServer.Services;
using System.ComponentModel.DataAnnotations;

namespace SweepoServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuoteController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<QuoteController> _logger;

        public QuoteController(IEmailService emailService, ILogger<QuoteController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<QuoteResponse>> SubmitQuote([FromBody] QuoteRequest request)
        {
            try
            {
                // Validate the request
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.LogWarning("Invalid quote request: {Errors}", string.Join(", ", errors));
                    
                    return BadRequest(new QuoteResponse
                    {
                        Success = false,
                        Message = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                // Generate a unique request ID
                var requestId = Guid.NewGuid().ToString("N")[..8];
                
                _logger.LogInformation("Processing quote request {RequestId} from {Name} ({Email})", 
                    requestId, request.Name, request.Email);

                // Send email
                var emailSent = await _emailService.SendQuoteRequestEmailAsync(request);

                if (emailSent)
                {
                    _logger.LogInformation("Quote request {RequestId} processed successfully", requestId);
                    
                    return Ok(new QuoteResponse
                    {
                        Success = true,
                        Message = "Quote request submitted successfully! We'll contact you within 24 hours.",
                        RequestId = requestId
                    });
                }
                else
                {
                    _logger.LogError("Failed to send email for quote request {RequestId}", requestId);
                    
                    return StatusCode(500, new QuoteResponse
                    {
                        Success = false,
                        Message = "Failed to process quote request. Please try again later.",
                        RequestId = requestId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing quote request from {Email}", request?.Email);
                
                return StatusCode(500, new QuoteResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later."
                });
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
