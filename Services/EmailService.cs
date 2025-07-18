using MailKit.Net.Smtp;
using MimeKit;
using SweepoServer.Models;

namespace SweepoServer.Services
{
    public interface IEmailService
    {
        Task<bool> SendQuoteRequestEmailAsync(QuoteRequest request, string requestId);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ITemplateService _templateService;
        private readonly ILogger<EmailService> _logger;

        public EmailService(EmailConfiguration emailConfig, ITemplateService templateService, ILogger<EmailService> logger)
        {
            _emailConfig = emailConfig;
            _templateService = templateService;
            _logger = logger;
        }

        public async Task<bool> SendQuoteRequestEmailAsync(QuoteRequest request, string requestId)
        {
            _logger.LogInformation("[{RequestId}] === EMAIL SENDING STARTED ===", requestId);
            _logger.LogInformation("[{RequestId}] Step 1: Validating email configuration", requestId);
            
            try
            {
                // Enhanced configuration validation and logging
                _logger.LogDebug("[{RequestId}] SMTP Server: {Server}:{Port}", requestId, _emailConfig.SmtpServer, _emailConfig.SmtpPort);
                _logger.LogDebug("[{RequestId}] SSL Enabled: {EnableSsl}", requestId, _emailConfig.EnableSsl);
                _logger.LogDebug("[{RequestId}] From Email: {FromEmail}", requestId, _emailConfig.FromEmail);
                _logger.LogDebug("[{RequestId}] Username: {Username}", requestId, _emailConfig.SmtpUsername);
                _logger.LogDebug("[{RequestId}] Password Configured: {PasswordConfigured}", requestId, !string.IsNullOrEmpty(_emailConfig.SmtpPassword));
                _logger.LogDebug("[{RequestId}] Password Length: {PasswordLength}", requestId, _emailConfig.SmtpPassword?.Length ?? 0);
                _logger.LogDebug("[{RequestId}] Recipients Count: {RecipientCount}", requestId, _emailConfig.RecipientEmails.Count);
                
                // Log recipients for debugging
                for (int i = 0; i < _emailConfig.RecipientEmails.Count; i++)
                {
                    _logger.LogDebug("[{RequestId}] Recipient {Index}: {Email}", requestId, i + 1, _emailConfig.RecipientEmails[i]);
                }
                
                // Validate critical configuration with detailed error messages
                if (string.IsNullOrEmpty(_emailConfig.SmtpServer))
                {
                    _logger.LogError("[{RequestId}] ❌ SMTP Server is not configured", requestId);
                    return false;
                }
                
                if (string.IsNullOrEmpty(_emailConfig.SmtpUsername))
                {
                    _logger.LogError("[{RequestId}] ❌ SMTP Username is not configured", requestId);
                    return false;
                }
                
                if (string.IsNullOrEmpty(_emailConfig.SmtpPassword))
                {
                    _logger.LogError("[{RequestId}] ❌ SMTP Password is not configured", requestId);
                    _logger.LogError("[{RequestId}] Check SWEEPO_FROM_EMAIL_PASSWORD environment variable or appsettings.json", requestId);
                    return false;
                }
                
                if (_emailConfig.RecipientEmails.Count == 0)
                {
                    _logger.LogError("[{RequestId}] ❌ No recipient emails configured", requestId);
                    return false;
                }
                
                _logger.LogInformation("[{RequestId}] Step 1: ✅ Email configuration validated successfully", requestId);
                
                _logger.LogInformation("[{RequestId}] Step 2: Generating email content using templates", requestId);
                
                // Generate email content with detailed logging
                string htmlContent = "";
                string textContent = "";
                
                try
                {
                    _logger.LogDebug("[{RequestId}] Step 2.1: Processing HTML template", requestId);
                    htmlContent = await _templateService.GenerateHtmlEmailAsync(request, requestId);
                    _logger.LogDebug("[{RequestId}] Step 2.1: ✅ HTML template processed - Length: {Length}", requestId, htmlContent.Length);
                    
                    _logger.LogDebug("[{RequestId}] Step 2.2: Processing text template", requestId);
                    textContent = await _templateService.GenerateTextEmailAsync(request, requestId);
                    _logger.LogDebug("[{RequestId}] Step 2.2: ✅ Text template processed - Length: {Length}", requestId, textContent.Length);
                }
                catch (Exception templateEx)
                {
                    _logger.LogError(templateEx, "[{RequestId}] ❌ Template processing failed", requestId);
                    _logger.LogError("[{RequestId}] Template Exception: {ExceptionType} - {Message}", requestId, templateEx.GetType().Name, templateEx.Message);
                    return false;
                }
                
                _logger.LogInformation("[{RequestId}] Step 2: ✅ Email content generated - HTML: {HtmlLength} chars, Text: {TextLength} chars", 
                    requestId, htmlContent.Length, textContent.Length);
                
                _logger.LogInformation("[{CorrelationId}] Step 3: Creating email message", requestId);
                
                // Create the email message with detailed logging
                MimeMessage message;
                try
                {
                    message = new MimeMessage();
                    _logger.LogDebug("[{CorrelationId}] Step 3.1: MimeMessage created", requestId);
                    
                    message.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromEmail));
                    _logger.LogDebug("[{CorrelationId}] Step 3.2: From address added: {FromName} <{FromEmail}>", 
                        requestId, _emailConfig.FromName, _emailConfig.FromEmail);
                    
                    // Add recipients with detailed logging
                    _logger.LogDebug("[{CorrelationId}] Step 3.3: Adding recipients", requestId);
                    foreach (var recipient in _emailConfig.RecipientEmails)
                    {
                        message.To.Add(new MailboxAddress("", recipient));
                        _logger.LogDebug("[{CorrelationId}] Step 3.3: Added recipient: {Recipient}", requestId, recipient);
                    }
                    
                    message.Subject = _emailConfig.Subject;
                    _logger.LogDebug("[{CorrelationId}] Step 3.4: Subject set: {Subject}", requestId, message.Subject);
                    
                    // Create multipart message
                    _logger.LogDebug("[{CorrelationId}] Step 3.5: Creating multipart message body", requestId);
                    var multipart = new Multipart("alternative");
                    multipart.Add(new TextPart("plain") { Text = textContent });
                    multipart.Add(new TextPart("html") { Text = htmlContent });
                    message.Body = multipart;
                    _logger.LogDebug("[{CorrelationId}] Step 3.5: Multipart body created with plain and HTML parts", requestId);
                }
                catch (Exception messageEx)
                {
                    _logger.LogError(messageEx, "[{CorrelationId}] ❌ Email message creation failed", requestId);
                    _logger.LogError("[{CorrelationId}] Message Exception: {ExceptionType} - {Message}", requestId, messageEx.GetType().Name, messageEx.Message);
                    return false;
                }
                
                _logger.LogInformation("[{CorrelationId}] Step 3: ✅ Email message created - Subject: {Subject}, Recipients: {RecipientCount}", 
                    requestId, message.Subject, message.To.Count);
                
                _logger.LogInformation("[{CorrelationId}] Step 4: Connecting to SMTP server {Server}:{Port}", 
                    requestId, _emailConfig.SmtpServer, _emailConfig.SmtpPort);
                
                // Send the email with detailed connection logging
                using var client = new SmtpClient();
                
                try
                {
                    _logger.LogDebug("[{CorrelationId}] Step 4.1: Creating SMTP client", requestId);
                    _logger.LogDebug("[{CorrelationId}] Step 4.2: Attempting connection to {Server}:{Port} with SSL={EnableSsl}", 
                        requestId, _emailConfig.SmtpServer, _emailConfig.SmtpPort, _emailConfig.EnableSsl);
                    
                    // Use STARTTLS for Gmail on port 587
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    _logger.LogInformation("[{CorrelationId}] Step 4: ✅ Connected to SMTP server successfully", requestId);
                    _logger.LogDebug("[{CorrelationId}] SMTP Connection Info: IsConnected={IsConnected}, IsSecure={IsSecure}, IsAuthenticated={IsAuthenticated}", 
                        requestId, client.IsConnected, client.IsSecure, client.IsAuthenticated);
                }
                catch (Exception connectEx)
                {
                    _logger.LogError(connectEx, "[{CorrelationId}] ❌ SMTP connection failed", requestId);
                    _logger.LogError("[{CorrelationId}] Connection Exception: {ExceptionType} - {Message}", requestId, connectEx.GetType().Name, connectEx.Message);
                    _logger.LogError("[{CorrelationId}] Connection Details: Server={Server}, Port={Port}, SSL={EnableSsl}", 
                        requestId, _emailConfig.SmtpServer, _emailConfig.SmtpPort, _emailConfig.EnableSsl);
                    return false;
                }
                
                try
                {
                    _logger.LogInformation("[{CorrelationId}] Step 5: Authenticating with username: {Username}", 
                        requestId, _emailConfig.SmtpUsername);
                    _logger.LogDebug("[{CorrelationId}] Step 5.1: Attempting SMTP authentication", requestId);
                    
                    await client.AuthenticateAsync(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
                    _logger.LogInformation("[{CorrelationId}] Step 5: ✅ SMTP authentication successful", requestId);
                    _logger.LogDebug("[{CorrelationId}] Authentication Status: IsAuthenticated={IsAuthenticated}", 
                        requestId, client.IsAuthenticated);
                }
                catch (Exception authEx)
                {
                    _logger.LogError(authEx, "[{CorrelationId}] ❌ SMTP authentication failed", requestId);
                    _logger.LogError("[{CorrelationId}] Authentication Exception: {ExceptionType} - {Message}", requestId, authEx.GetType().Name, authEx.Message);
                    _logger.LogError("[{CorrelationId}] Authentication Details: Username={Username}, PasswordLength={PasswordLength}", 
                        requestId, _emailConfig.SmtpUsername, _emailConfig.SmtpPassword?.Length ?? 0);
                    _logger.LogError("[{CorrelationId}] Hint: Check if Gmail App Password is correct and 2FA is enabled", requestId);
                    return false;
                }
                
                try
                {
                    _logger.LogInformation("[{CorrelationId}] Step 6: Sending email message", requestId);
                    _logger.LogDebug("[{CorrelationId}] Step 6.1: Calling SendAsync", requestId);
                    
                    await client.SendAsync(message);
                    _logger.LogDebug("[{CorrelationId}] Step 6.2: Email sent, disconnecting", requestId);
                    
                    await client.DisconnectAsync(true);
                    _logger.LogDebug("[{CorrelationId}] Step 6.3: Disconnected from SMTP server", requestId);
                }
                catch (Exception sendEx)
                {
                    _logger.LogError(sendEx, "[{CorrelationId}] ❌ Email sending failed", requestId);
                    _logger.LogError("[{CorrelationId}] Send Exception: {ExceptionType} - {Message}", requestId, sendEx.GetType().Name, sendEx.Message);
                    _logger.LogError("[{CorrelationId}] Message Details: Subject={Subject}, Recipients={RecipientCount}", 
                        requestId, message.Subject, message.To.Count);
                    return false;
                }
                
                _logger.LogInformation("[{CorrelationId}] Step 6: ✅ Email sent successfully to {RecipientCount} recipients", 
                    requestId, _emailConfig.RecipientEmails.Count);
                _logger.LogInformation("[{CorrelationId}] === EMAIL SENDING COMPLETED SUCCESSFULLY ===", requestId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{CorrelationId}] ❌ UNEXPECTED EMAIL SENDING ERROR for RequestId: {RequestId}", requestId, requestId);
                _logger.LogError("[{CorrelationId}] Unexpected Exception Type: {ExceptionType}", requestId, ex.GetType().Name);
                _logger.LogError("[{CorrelationId}] Unexpected Exception Message: {Message}", requestId, ex.Message);
                _logger.LogError("[{CorrelationId}] Stack Trace: {StackTrace}", requestId, ex.StackTrace);
                _logger.LogInformation("[{CorrelationId}] === EMAIL SENDING ENDED (UNEXPECTED FAILURE) ===", requestId);
                _logger.LogError(ex, "[{RequestId}] ❌ FAILED to send quote request email for {Name} ({Email})", 
                    requestId, request.Name, request.Email);
                _logger.LogError("[{RequestId}] Exception details: {ExceptionType} - {Message}", 
                    requestId, ex.GetType().Name, ex.Message);
                
                if (ex.InnerException != null)
                {
                    _logger.LogError("[{RequestId}] Inner exception: {InnerExceptionType} - {InnerMessage}", 
                        requestId, ex.InnerException.GetType().Name, ex.InnerException.Message);
                }
                
                _logger.LogInformation("[{RequestId}] === EMAIL SENDING COMPLETED (FAILED) ===", requestId);
                return false;
            }
        }
    }
}
