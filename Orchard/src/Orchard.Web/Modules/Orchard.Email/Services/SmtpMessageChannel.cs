using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Logging;
using Orchard.Email.Models;
using System.IO;

namespace Orchard.Email.Services {
    public class SmtpMessageChannel : Component, ISmtpChannel, IDisposable {
        private readonly SmtpSettingsPart _smtpSettings;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly Lazy<SmtpClient> _smtpClientField;
        public static readonly string MessageType = "Email";

        public SmtpMessageChannel(
            IOrchardServices orchardServices,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay) {

            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;

            _smtpSettings = orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            _smtpClientField = new Lazy<SmtpClient>(CreateSmtpClient);
        }

        public void Dispose() {
            if (!_smtpClientField.IsValueCreated) {
                return;
            }

            _smtpClientField.Value.Dispose();
        }

        public void Process(IDictionary<string, object> parameters) {
            if (!_smtpSettings.IsValid()) {
                return;
            }

            var emailMessage = new EmailMessage {
                Body = Read(parameters, "Body"),
                Subject = Read(parameters, "Subject"),
                Recipients = Read(parameters, "Recipients"),
                ReplyTo = Read(parameters, "ReplyTo"),
                FromAddress = Read(parameters, "FromAddress"),
                FromName = Read(parameters, "FromName"),
                Bcc = Read(parameters, "Bcc"),
                Cc = Read(parameters, "CC"),
                Attachments = (IEnumerable<string>)(
                    parameters.ContainsKey("Attachments")
                        ? parameters["Attachments"]
                        : new List<string>()
                )
            };

            if (string.IsNullOrWhiteSpace(emailMessage.Recipients)) {
                Logger.Error("Email message doesn't have any recipient");
                return;
            }

            var mailMessage = CreteMailMessage(parameters, emailMessage);

            try {
                _smtpClientField.Value.Send(mailMessage);
            }
            catch (Exception e) {
                Logger.Error(e, "Could not send email");
            }
        }

        private MailMessage CreteMailMessage(IDictionary<string, object> parameters, EmailMessage emailMessage) {

            // Apply default Body alteration for SmtpChannel.
            var template = _shapeFactory.Create("Template_Smtp_Wrapper", Arguments.From(new {
                Content = new MvcHtmlString(emailMessage.Body)
            }));

            var mailMessage = new MailMessage {
                Subject = emailMessage.Subject,
                Body = _shapeDisplay.Display(template),
                IsBodyHtml = true
            };

            if (parameters.ContainsKey("Message")) {
                // A full message object is provided by the sender.
                var oldMessage = mailMessage;
                mailMessage = (MailMessage)parameters["Message"];

                if (string.IsNullOrWhiteSpace(mailMessage.Subject))
                    mailMessage.Subject = oldMessage.Subject;

                if (string.IsNullOrWhiteSpace(mailMessage.Body)) {
                    mailMessage.Body = oldMessage.Body;
                    mailMessage.IsBodyHtml = oldMessage.IsBodyHtml;
                }
            }

            foreach (var recipient in ParseRecipients(emailMessage.Recipients)) {
                mailMessage.To.Add(new MailAddress(recipient));
            }

            if (!string.IsNullOrWhiteSpace(emailMessage.Cc)) {
                foreach (var recipient in ParseRecipients(emailMessage.Cc)) {
                    mailMessage.CC.Add(new MailAddress(recipient));
                }
            }

            if (!string.IsNullOrWhiteSpace(emailMessage.Bcc)) {
                foreach (var recipient in ParseRecipients(emailMessage.Bcc)) {
                    mailMessage.Bcc.Add(new MailAddress(recipient));
                }
            }

            var senderAddress =
                !string.IsNullOrWhiteSpace(emailMessage.FromAddress) ? emailMessage.FromAddress :
                !string.IsNullOrWhiteSpace(_smtpSettings.FromAddress) ? _smtpSettings.FromAddress :
                // Take 'From' address from site settings or web.config.
                ((SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp")).From;

            var senderName = !string.IsNullOrWhiteSpace(emailMessage.FromName)
                ? emailMessage.FromName
                : _smtpSettings.FromName;

            if (senderAddress != null && senderName != null) {
                mailMessage.From = new MailAddress(senderAddress, senderName);
            } else if (senderAddress != null && senderName == null) {
                mailMessage.From = new MailAddress(senderAddress);
            } else if (senderAddress == null && senderName == null) {
                throw new InvalidOperationException("No sender email address");
            }

            var replyTo =
                !string.IsNullOrWhiteSpace(emailMessage.ReplyTo) ? ParseRecipients(emailMessage.ReplyTo) :
                !string.IsNullOrWhiteSpace(_smtpSettings.ReplyTo) ? new[] { _smtpSettings.ReplyTo } :
                Array.Empty<string>();

            foreach (var recipient in replyTo) {
                mailMessage.ReplyToList.Add(new MailAddress(recipient));
            }

            foreach (var attachmentPath in emailMessage.Attachments) {
                if (File.Exists(attachmentPath)) {
                    mailMessage.Attachments.Add(new Attachment(attachmentPath));
                }
                else {
                    throw new FileNotFoundException(T("One or more attachments not found.").Text);
                }
            }

            if (parameters.ContainsKey("NotifyReadEmail")) {
                if (parameters["NotifyReadEmail"] is bool) {
                    if ((bool)(parameters["NotifyReadEmail"])) {
                        mailMessage.Headers.Add("Disposition-Notification-To", mailMessage.From.ToString());
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(_smtpSettings.ListUnsubscribe)) {
                mailMessage.Headers.Add("List-Unsubscribe", _smtpSettings.ListUnsubscribe);
            }

            return mailMessage;
        }

        private SmtpClient CreateSmtpClient() {
            // If no properties are set in the dashboard, use the web.config value.
            if (string.IsNullOrWhiteSpace(_smtpSettings.Host)) {
                return new SmtpClient();
            }

            var smtpClient = new SmtpClient {
                UseDefaultCredentials = _smtpSettings.RequireCredentials && _smtpSettings.UseDefaultCredentials
            };

            if (!smtpClient.UseDefaultCredentials && !string.IsNullOrWhiteSpace(_smtpSettings.UserName)) {
                smtpClient.Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password);
            }

            if (_smtpSettings.Host != null) {
                smtpClient.Host = _smtpSettings.Host;
            }

            smtpClient.Port = _smtpSettings.Port;
            smtpClient.EnableSsl = _smtpSettings.EnableSsl;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            return smtpClient;
        }

        private string Read(IDictionary<string, object> dictionary, string key) =>
            dictionary.ContainsKey(key) ? dictionary[key] as string : null;

        private IEnumerable<string> ParseRecipients(string recipients) =>
            recipients.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
