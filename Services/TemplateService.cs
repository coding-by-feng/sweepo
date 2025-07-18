using SweepoServer.Models;
using System.Text;

namespace SweepoServer.Services
{
    public interface ITemplateService
    {
        Task<string> GenerateHtmlEmailAsync(QuoteRequest request, string requestId);
        Task<string> GenerateTextEmailAsync(QuoteRequest request, string requestId);
    }

    public class TemplateService : ITemplateService
    {
        private readonly ILogger<TemplateService> _logger;
        private readonly IWebHostEnvironment _environment;

        public TemplateService(ILogger<TemplateService> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async Task<string> GenerateHtmlEmailAsync(QuoteRequest request, string requestId)
        {
            _logger.LogInformation("[{RequestId}] Template Step 1: Starting HTML template generation", requestId);
            
            try
            {
                _logger.LogDebug("[{RequestId}] Template Step 1.1: Resolving HTML template path", requestId);
                var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "QuoteRequestEmail.html");
                _logger.LogDebug("[{RequestId}] Template path: {TemplatePath}", requestId, templatePath);
                
                _logger.LogDebug("[{RequestId}] Template Step 1.2: Checking template file existence", requestId);
                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning("[{RequestId}] HTML template file not found at {TemplatePath}", requestId, templatePath);
                    _logger.LogInformation("[{RequestId}] Template Step 1.2: Using fallback HTML template", requestId);
                    return GenerateFallbackHtmlTemplate(request, requestId);
                }
                
                _logger.LogDebug("[{RequestId}] Template Step 1.3: Reading HTML template file", requestId);
                var template = await File.ReadAllTextAsync(templatePath);
                _logger.LogDebug("[{RequestId}] Template Step 1.3: ✅ HTML template loaded ({Length} characters)", requestId, template.Length);
                
                _logger.LogInformation("[{RequestId}] Template Step 1.4: Processing template placeholders", requestId);
                var result = ReplaceTemplatePlaceholders(template, request, requestId);
                _logger.LogInformation("[{RequestId}] Template Step 1.4: ✅ HTML template generation completed ({Length} characters)", requestId, result.Length);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{RequestId}] ❌ Failed to generate HTML email template", requestId);
                _logger.LogError("[{RequestId}] Exception details: {ExceptionType} - {Message}", requestId, ex.GetType().Name, ex.Message);
                _logger.LogInformation("[{RequestId}] Template Step 1.5: Using fallback HTML template due to error", requestId);
                return GenerateFallbackHtmlTemplate(request, requestId);
            }
        }

        public async Task<string> GenerateTextEmailAsync(QuoteRequest request, string requestId)
        {
            _logger.LogInformation("[{RequestId}] Template Step 2: Starting text template generation", requestId);
            
            try
            {
                _logger.LogDebug("[{RequestId}] Template Step 2.1: Resolving text template path", requestId);
                var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "QuoteRequestEmail.txt");
                _logger.LogDebug("[{RequestId}] Template path: {TemplatePath}", requestId, templatePath);
                
                _logger.LogDebug("[{RequestId}] Template Step 2.2: Checking template file existence", requestId);
                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning("[{RequestId}] Text template file not found at {TemplatePath}", requestId, templatePath);
                    _logger.LogInformation("[{RequestId}] Template Step 2.2: Using fallback text template", requestId);
                    return GenerateFallbackTextTemplate(request, requestId);
                }
                
                _logger.LogDebug("[{RequestId}] Template Step 2.3: Reading text template file", requestId);
                var template = await File.ReadAllTextAsync(templatePath);
                _logger.LogDebug("[{RequestId}] Template Step 2.3: ✅ Text template loaded ({Length} characters)", requestId, template.Length);
                
                _logger.LogInformation("[{RequestId}] Template Step 2.4: Processing template placeholders", requestId);
                var result = ReplaceTemplatePlaceholders(template, request, requestId);
                _logger.LogInformation("[{RequestId}] Template Step 2.4: ✅ Text template generation completed ({Length} characters)", requestId, result.Length);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{RequestId}] ❌ Failed to generate text email template", requestId);
                _logger.LogError("[{RequestId}] Exception details: {ExceptionType} - {Message}", requestId, ex.GetType().Name, ex.Message);
                _logger.LogInformation("[{RequestId}] Template Step 2.5: Using fallback text template due to error", requestId);
                return GenerateFallbackTextTemplate(request, requestId);
            }
        }

        private string ReplaceTemplatePlaceholders(string template, QuoteRequest request, string requestId)
        {
            _logger.LogDebug("[{RequestId}] Template processing: Starting placeholder replacement", requestId);
            
            var serviceDisplayName = GetServiceDisplayName(request.Service);
            _logger.LogDebug("[{RequestId}] Service display name: {ServiceType} -> {ServiceDisplayName}", 
                requestId, request.Service, serviceDisplayName);
            
            var replacements = new Dictionary<string, string>
            {
                {"{{CustomerName}}", request.Name},
                {"{{CustomerEmail}}", request.Email},
                {"{{CustomerPhone}}", request.Phone},
                {"{{ServiceType}}", serviceDisplayName},
                {"{{CustomerAddress}}", request.Address ?? "Not provided"},
                {"{{CustomerMessage}}", request.Message ?? "No additional details provided"},
                {"{{SubmissionTime}}", request.Timestamp.ToString("yyyy-MM-dd HH:mm:ss UTC")},
                {"{{Source}}", request.Source},
                {"{{RequestId}}", requestId}
            };

            _logger.LogDebug("[{RequestId}] Template processing: Applying {ReplacementCount} placeholder replacements", 
                requestId, replacements.Count);
            
            var result = template;
            foreach (var replacement in replacements)
            {
                var beforeLength = result.Length;
                result = result.Replace(replacement.Key, replacement.Value);
                var afterLength = result.Length;
                var changeCount = (beforeLength - afterLength + replacement.Value.Length - replacement.Key.Length);
                
                if (changeCount != 0)
                {
                    _logger.LogDebug("[{RequestId}] Replaced {Placeholder} with '{Value}' (length change: {ChangeCount})", 
                        requestId, replacement.Key, replacement.Value, changeCount);
                }
            }

            _logger.LogDebug("[{RequestId}] Template processing: Handling conditional sections", requestId);
            // Handle conditional sections
            result = HandleConditionalSections(result, request);
            
            _logger.LogDebug("[{RequestId}] Template processing: ✅ Placeholder replacement completed", requestId);
            return result;
        }

        private string HandleConditionalSections(string template, QuoteRequest request)
        {
            // Handle {{#if CustomerAddress}} sections
            if (string.IsNullOrEmpty(request.Address))
            {
                template = RemoveConditionalSection(template, "{{#if CustomerAddress}}", "{{/if}}");
            }
            else
            {
                template = template.Replace("{{#if CustomerAddress}}", "").Replace("{{/if}}", "");
            }

            // Handle {{#if CustomerMessage}} sections
            if (string.IsNullOrEmpty(request.Message))
            {
                template = RemoveConditionalSection(template, "{{#if CustomerMessage}}", "{{/if}}");
            }
            else
            {
                template = template.Replace("{{#if CustomerMessage}}", "").Replace("{{/if}}", "");
            }

            return template;
        }

        private string RemoveConditionalSection(string template, string startTag, string endTag)
        {
            var startIndex = template.IndexOf(startTag);
            if (startIndex == -1) return template;

            var endIndex = template.IndexOf(endTag, startIndex);
            if (endIndex == -1) return template;

            return template.Remove(startIndex, endIndex - startIndex + endTag.Length);
        }

        private string GetServiceDisplayName(string service)
        {
            return service switch
            {
                "home-cleaning" => "Home Cleaning",
                "commercial-cleaning" => "Commercial Cleaning",
                "pest-control" => "Pest Control",
                "garbage-removal" => "Garbage Removal",
                "lawn-garden" => "Lawn & Garden",
                "car-valet" => "Car Valet",
                _ => service
            };
        }

        private string GenerateFallbackHtmlTemplate(QuoteRequest request, string requestId)
        {
            var serviceDisplayName = GetServiceDisplayName(request.Service);
            
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>New Quote Request</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .field {{ margin-bottom: 15px; }}
        .label {{ font-weight: bold; color: #555; }}
        .value {{ margin-left: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>New Quote Request</h1>
        </div>
        <div class='content'>
            <div class='field'><span class='label'>Name:</span><span class='value'>{request.Name}</span></div>
            <div class='field'><span class='label'>Email:</span><span class='value'>{request.Email}</span></div>
            <div class='field'><span class='label'>Phone:</span><span class='value'>{request.Phone}</span></div>
            <div class='field'><span class='label'>Service:</span><span class='value'>{serviceDisplayName}</span></div>
            {(string.IsNullOrEmpty(request.Address) ? "" : $"<div class='field'><span class='label'>Address:</span><span class='value'>{request.Address}</span></div>")}
            {(string.IsNullOrEmpty(request.Message) ? "" : $"<div class='field'><span class='label'>Message:</span><span class='value'>{request.Message}</span></div>")}
            <div class='field'><span class='label'>Submitted:</span><span class='value'>{request.Timestamp:yyyy-MM-dd HH:mm:ss} UTC</span></div>
            <div class='field'><span class='label'>Request ID:</span><span class='value'>{requestId}</span></div>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateFallbackTextTemplate(QuoteRequest request, string requestId)
        {
            var serviceDisplayName = GetServiceDisplayName(request.Service);
            var sb = new StringBuilder();
            
            sb.AppendLine("NEW QUOTE REQUEST");
            sb.AppendLine("==================");
            sb.AppendLine();
            sb.AppendLine($"Name: {request.Name}");
            sb.AppendLine($"Email: {request.Email}");
            sb.AppendLine($"Phone: {request.Phone}");
            sb.AppendLine($"Service: {serviceDisplayName}");
            
            if (!string.IsNullOrEmpty(request.Address))
                sb.AppendLine($"Address: {request.Address}");
            
            if (!string.IsNullOrEmpty(request.Message))
            {
                sb.AppendLine($"Message:");
                sb.AppendLine(request.Message);
            }
            
            sb.AppendLine($"Submitted: {request.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine($"Request ID: {requestId}");
            
            return sb.ToString();
        }
    }
}
