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
            var correlationId = Guid.NewGuid().ToString("N")[..8];
            
            _logger.LogInformation("[{CorrelationId}] === QUOTE REQUEST STARTED ===", correlationId);
            _logger.LogInformation("[{CorrelationId}] Received quote request from IP: {RemoteIp}", 
                correlationId, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            
            try
            {
                // Log incoming request details
                _logger.LogInformation("[{CorrelationId}] Step 1: Validating incoming request", correlationId);
                _logger.LogDebug("[{CorrelationId}] Request payload: Name={Name}, Email={Email}, Phone={Phone}, Service={Service}, Address={Address}, MessageLength={MessageLength}", 
                    correlationId, request?.Name ?? "null", request?.Email ?? "null", request?.Phone ?? "null", 
                    request?.Service ?? "null", request?.Address ?? "null", request?.Message?.Length ?? 0);
                
                // Validate the request
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.LogWarning("[{CorrelationId}] Step 1: VALIDATION FAILED - Errors: {Errors}", 
                        correlationId, string.Join(", ", errors));
                    _logger.LogInformation("[{CorrelationId}] === QUOTE REQUEST ENDED (VALIDATION ERROR) ===", correlationId);
                    
                    return BadRequest(new QuoteResponse
                    {
                        Success = false,
                        Message = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }
                
                _logger.LogInformation("[{CorrelationId}] Step 1: ✅ Request validation passed", correlationId);

                // Generate a unique request ID
                _logger.LogInformation("[{CorrelationId}] Step 2: Generating unique request ID", correlationId);
                var requestId = Guid.NewGuid().ToString("N")[..8];
                _logger.LogInformation("[{CorrelationId}] Step 2: ✅ Generated request ID: {RequestId}", correlationId, requestId);
                
                _logger.LogInformation("[{CorrelationId}] Step 3: Processing quote request {RequestId} from {Name} ({Email}) for service: {Service}", 
                    correlationId, requestId, request.Name, request.Email, request.Service);

                // Send email
                _logger.LogInformation("[{CorrelationId}] Step 4: Initiating email sending process", correlationId);
                var emailSent = await _emailService.SendQuoteRequestEmailAsync(request, requestId);
                
                if (emailSent)
                {
                    _logger.LogInformation("[{CorrelationId}] Step 4: ✅ Email sent successfully", correlationId);
                    _logger.LogInformation("[{CorrelationId}] Step 5: Preparing success response", correlationId);
                    
                    var successResponse = new QuoteResponse
                    {
                        Success = true,
                        Message = "Quote request submitted successfully! We'll contact you within 24 hours.",
                        RequestId = requestId
                    };
                    
                    _logger.LogInformation("[{CorrelationId}] Step 5: ✅ Quote request {RequestId} processed successfully", correlationId, requestId);
                    _logger.LogInformation("[{CorrelationId}] === QUOTE REQUEST ENDED (SUCCESS) ===", correlationId);
                    
                    return Ok(successResponse);
                }
                else
                {
                    _logger.LogError("[{CorrelationId}] Step 4: ❌ Email sending failed for request {RequestId}", correlationId, requestId);
                    _logger.LogInformation("[{CorrelationId}] Step 5: Preparing error response", correlationId);
                    
                    var errorResponse = new QuoteResponse
                    {
                        Success = false,
                        Message = "Failed to process quote request. Please try again later.",
                        RequestId = requestId
                    };
                    
                    _logger.LogInformation("[{CorrelationId}] === QUOTE REQUEST ENDED (EMAIL FAILURE) ===", correlationId);
                    
                    return StatusCode(500, errorResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{CorrelationId}] ❌ UNEXPECTED ERROR processing quote request from {Email}", 
                    correlationId, request?.Email ?? "Unknown");
                _logger.LogError("[{CorrelationId}] Exception details: {ExceptionType} - {Message}", 
                    correlationId, ex.GetType().Name, ex.Message);
                _logger.LogInformation("[{CorrelationId}] === QUOTE REQUEST ENDED (EXCEPTION) ===", correlationId);
                
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
