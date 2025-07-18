using MailKit.Net.Smtp;
using MimeKit;
using SweepoServer.Models;
using System.Text;

namespace SweepoServer.Services
{
    public interface IEmailService
    {
        Task<bool> SendQuoteRequestEmailAsync(QuoteRequest request);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ILogger<EmailService> _logger;

        public EmailService(EmailConfiguration emailConfig, ILogger<EmailService> logger)
        {
            _emailConfig = emailConfig;
            _logger = logger;
        }

        public async Task<bool> SendQuoteRequestEmailAsync(QuoteRequest request)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromEmail));
                
                foreach (var recipient in _emailConfig.RecipientEmails)
                {
                    message.To.Add(new MailboxAddress("", recipient));
                }

                message.Subject = _emailConfig.Subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = GenerateEmailTemplate(request),
                    TextBody = GenerateTextEmailTemplate(request)
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.SmtpPort, _emailConfig.EnableSsl);
                
                if (!string.IsNullOrEmpty(_emailConfig.SmtpUsername))
                {
                    await client.AuthenticateAsync(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Quote request email sent successfully for {Name} ({Email})", request.Name, request.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send quote request email for {Name} ({Email})", request.Name, request.Email);
                return false;
            }
        }

        private string GenerateEmailTemplate(QuoteRequest request)
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
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>New Quote Request</h1>
        </div>
        <div class='content'>
            <div class='field'>
                <span class='label'>Name:</span>
                <span class='value'>{request.Name}</span>
            </div>
            <div class='field'>
                <span class='label'>Email:</span>
                <span class='value'>{request.Email}</span>
            </div>
            <div class='field'>
                <span class='label'>Phone:</span>
                <span class='value'>{request.Phone}</span>
            </div>
            <div class='field'>
                <span class='label'>Service:</span>
                <span class='value'>{serviceDisplayName}</span>
            </div>
            {(string.IsNullOrEmpty(request.Address) ? "" : $@"
            <div class='field'>
                <span class='label'>Address:</span>
                <span class='value'>{request.Address}</span>
            </div>")}
            {(string.IsNullOrEmpty(request.Message) ? "" : $@"
            <div class='field'>
                <span class='label'>Message:</span>
                <span class='value'>{request.Message}</span>
            </div>")}
            <div class='field'>
                <span class='label'>Submitted:</span>
                <span class='value'>{request.Timestamp:yyyy-MM-dd HH:mm:ss} UTC</span>
            </div>
            <div class='field'>
                <span class='label'>Source:</span>
                <span class='value'>{request.Source}</span>
            </div>
        </div>
        <div class='footer'>
            <p>This quote request was submitted through the Sweepo website.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateTextEmailTemplate(QuoteRequest request)
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
            sb.AppendLine($"Source: {request.Source}");
            sb.AppendLine();
            sb.AppendLine("This quote request was submitted through the Sweepo website.");
            
            return sb.ToString();
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
    }
}
