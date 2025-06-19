using System.Data;
using System.Text;
using dotsend.Utility;

namespace dotsend
{
    class Program
    {
        private readonly SqlDataAccess dataAccess = new();

        public static async Task Main(string[] args)
        {
            var program = new Program();
            await program.SendQueuedNotifications();
        }

        public async Task SendQueuedNotifications()
        {
            Console.WriteLine("Starting dotsend...");
            StringBuilder builder = new();
            List<Message> unsentMessages = GetUnsentMessages();

            if (unsentMessages.Any())
            {
                foreach (var message in unsentMessages)
                {
                    string response = await SendEmail(message);
                    builder.Append($"UPDATE DIY_MESSAGING SET PROCESSED = 1, RESPONSE = '{response}', Sent = GETDATE() WHERE ID = {message.Id}; ");
                }
                UpdateMessageQueueWithResponse(builder.ToString());
            }
        }

        private List<Message> GetUnsentMessages()
        {
            List<Message> messages = new();
            string unsentMessagesQuery = "SELECT * FROM DIY_MESSAGING WHERE PROCESSED = 0";

            try
            {
                Console.WriteLine($"Retrieving unsent messages");
                DataTable table = dataAccess.ExecuteQuery(unsentMessagesQuery, null);
                Console.WriteLine($"{table.Rows.Count} rows returned");

                foreach (DataRow row in table.Rows)
                {
                    messages.Add(new Message
                    {
                        Id = row.Field<int?>("Id"),
                        Sender = row.Field<string?>("Sender"),
                        Recipients = row.Field<string?>("Recipients"),
                        Cc = row.Field<string?>("Cc"),
                        Subject = row.Field<string?>("Subject"),
                        HtmlBody = row.Field<string?>("Body"),
                    });
                }

                return messages;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return messages;
            }
        }

        private static async Task<string> SendEmail(Message message)
        {
            Notification dto = new()
            {
                To = message.Recipients,
                Cc = message.Cc,
                Subject = message.Subject,
                HtmlBody = message.HtmlBody
            };

            try
            {
                var response = await EmailService.SendEmailAsync(dto);
                Console.WriteLine($"Notification email sent for message Id: {message.Id}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification email failed for message Id: {message.Id} : {ex.Message}");
                return ex.Message;
            }
        }

        private void UpdateMessageQueueWithResponse(string compoundQuery)
        {
            try
            {
                Console.WriteLine($"Updating message queue");
                dataAccess.ExecuteQuery(compoundQuery, null);
                Console.WriteLine("Messages updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class Message
    {
        public int? Id { get; set; }
        public string? Sender { get; set; }
        public string? Recipients { get; set; }
        public string? Cc { get; set; }
        public string? Subject { get; set; }
        public string? HtmlBody { get; set; }
        public bool? Processed { get; set; }
        public DateTime? Received { get; set; }
        public DateTime? Sent { get; set; }
        public string? Response { get; set; }
    }
}
