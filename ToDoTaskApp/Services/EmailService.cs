using Azure;
using Azure.Communication.Email;

namespace TodoApp.Services
{
    public class EmailService
    {
        private readonly EmailClient _emailClient;
        private readonly string _sender;

        public EmailService(IConfiguration config)
        {
            var connectionString = config["AzureCommunication:ConnectionString"];
            _sender = config["AzureCommunication:SenderEmail"];

            _emailClient = new EmailClient(connectionString);
        }

        public async Task<bool> SendEmailAsync(string receiver, string subject, string body)
        {
            try
            {
                var message = new EmailMessage(
                    _sender,
                    receiver,
                    new EmailContent(subject)
                    {
                        PlainText = body,
                        Html = $"<p>{body}</p>"
                    });

                EmailSendOperation operation = await _emailClient.SendAsync(WaitUntil.Completed, message);

                return operation.HasCompleted && operation.Value.Status == EmailSendStatus.Succeeded;
            }
            catch
            {
                return false;
            }
        }
    }
}
