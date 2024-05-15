using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using Azure;
using EmailProvider.Models;
using Newtonsoft.Json;
using EmailProvider.Functions;
using Microsoft.Extensions.Logging;
namespace EmailProvider.Services;

public class EmailService(ILogger<EmailSender> logger, EmailClient emailClient) : IEmailService
{
    private readonly ILogger<EmailSender> _logger = logger;
    private readonly EmailClient _emailClient = emailClient;

    public EmailRequest UnpackEmailRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var emailRequest = JsonConvert.DeserializeObject<EmailRequest>(message.Body.ToString());
            if (emailRequest != null)
                return emailRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.UnpackEmailRequest :: {ex.Message}");
        }
        return null!;
    }

    public bool SendEmail(EmailRequest emailRequest)
    {
        try
        {
            var result = _emailClient.Send(

            WaitUntil.Completed,
            senderAddress: Environment.GetEnvironmentVariable("senderAddress"),
            recipientAddress: emailRequest.To,
            subject: emailRequest.Subject,
            htmlContent: emailRequest.HtmlContent,
            plainTextContent: emailRequest.TextContent);

            if (result.HasCompleted)
                return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.SendEmail :: {ex.Message}");
        }
        return false;
    }
}
