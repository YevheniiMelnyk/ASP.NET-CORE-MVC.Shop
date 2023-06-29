﻿using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Shop.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public MailJetSettings _mailJetSettings;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }

        public async Task Execute(string email, string subject, string body)
        {
            _mailJetSettings = _configuration.GetSection("MailJet").Get<MailJetSettings>();

            MailjetClient client = new MailjetClient(_mailJetSettings.ApiKey, _mailJetSettings.SecretKey)
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
