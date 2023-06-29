using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Shop.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }

        public async Task Execute(string email, string subject, string body)
        {
            MailjetClient client = new MailjetClient("b424865754a2e54316d055853a5f7ad5", "1f729d69cb108e9624b5c39dd07d40e3")
            {
            };
            MailjetRequest request = new MailjetRequest
            {
                Resource = SendV31.Resource,
            }
             .Property(Send.Messages, 
             new JArray 
             { 
                 new JObject {
                    {
                        "From", new JObject 
                        {
                            {"Email", "melnyk.net.dev@protonmail.com"},
                            {"Name", "Yevhenii"}
                        }
                    }, 
                    { "To", new JArray 
                        {
                            new JObject 
                            {
                                { "Email", email }, 
                                { "Name", "DotNetMastery" }
                            }
                        }
                    }, 
                    { "Subject", subject }, 
                    { "TextPart", "My first Mailjet email" }, 
                    { "HTMLPart", body }
                 }
             });
             await client.PostAsync(request);
        }
    }
}
