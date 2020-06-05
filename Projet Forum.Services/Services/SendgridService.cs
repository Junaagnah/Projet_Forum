using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using SendGrid;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Helpers;
using Projet_Forum.Services.Models;
using System.Net.Mail;

namespace Projet_Forum.Services.Services
{
    /// <summary>
    /// Service permettant de gérer toutes les actions liées à Sendgrid (envoi de mail, etc)
    /// </summary>
    public class SendgridService : ISendgridService
    {
        private readonly AppSettings _appSettings;
        private readonly EmailAddress _from = new EmailAddress("projetforum@junaagnah.com", "Projet Forum");

        public SendgridService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Permet de créer un SendGridMessage pour l'email de validation
        /// </summary>
        /// <param name="email"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="isTest"></param>
        /// <returns>SendGridMessage</returns>
        private SendGridMessage PopulateValidationEmail(string email, string userId, string token, bool isTest = false)
        {
            // Encodage du token pour le passage par url
            token = HttpUtility.UrlEncode(token);

            SendGridMessage msg = new SendGridMessage
            {
                From = _from,
                Subject = "Projet Forum - E-mail de validation",
                HtmlContent = "<p>Bienvenue !<br>" +
                "Vous pouvez dès à présent valider votre compte en cliquant <a href='" + _appSettings.ApplicationUrl + "/confirmEmail?id=" + userId + "&token=" + token + "'>ici</a><br><br>" +
                "Ce lien expirera dans 24h.</p>",
                MailSettings = new MailSettings
                {
                    SandboxMode = new SandboxMode
                    {
                        Enable = isTest
                    }
                }
            };
            msg.AddTo(email);

            return msg;
        }

        /// <summary>
        /// Permet de créer un SendGridMessage pour l'email de récupération de mot de passe
        /// </summary>
        /// <param name="email"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="isTest"></param>
        /// <returns>SendGridMessage</returns>
        private SendGridMessage PopulateRecoveryEmail(string email, string userId, string token, bool isTest = false)
        {
            token = HttpUtility.UrlEncode(token);

            SendGridMessage msg = new SendGridMessage
            {
                From = _from,
                Subject = "Projet Forum - Récupération de mot de passe",
                HtmlContent = "<p>Bonjour,<br>" +
                "Afin de récupérer l'accès à votre compte et modifier votre mot de passe, merci de cliquer sur <a href='" + _appSettings.ApplicationUrl + "/recoverPassword?id=" + userId + "&token=" + token + "'>ce lien.</a><br><br>" +
                "Ce lien expirera dans 24h.</p>",
                MailSettings = new MailSettings
                {
                    SandboxMode = new SandboxMode
                    {
                        Enable = isTest
                    }
                }
            };
            msg.AddTo(email);

            return msg;
        }

        /// <summary>
        /// Permet de créer un SendGridMessage pour l'envoi de mails de contact
        /// </summary>
        /// <param name="email"></param>
        /// <param name="message"></param>
        /// <param name="isTest"></param>
        /// <returns>SendGridMessage</returns>
        private SendGridMessage PopulateContactEmail(MailAddress email, string message, bool isTest = false)
        {
            SendGridMessage msg = new SendGridMessage
            {
                From = _from,
                Subject = $"Contact de la part de - {email.Address}",
                PlainTextContent = message,
                MailSettings = new MailSettings
                {
                    SandboxMode = new SandboxMode
                    {
                        Enable = isTest
                    }
                }
            };
            msg.AddTo(new EmailAddress(_appSettings.ContactEmail));

            return msg;
        }

        /// <summary>
        /// Permet d'envoyer un mail via SendGrid
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>Boolean</returns>
        private async Task<bool> SendMessage(SendGridMessage msg)
        {
            int i = 0;
            var apiKey = _appSettings.SendgridApiKey;
            var client = new SendGridClient(apiKey);
            var response = await client.SendEmailAsync(msg);

            // Si échec, on réessaie deux fois et sinon on abandonne
            while ((response.StatusCode != HttpStatusCode.Accepted && response.StatusCode != HttpStatusCode.OK) && i < 2)
            {
                response = await client.SendEmailAsync(msg);
                i++;
            }

            if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode != HttpStatusCode.OK)
                return true;

            return false;
        }

        /// <summary>
        /// Fonction appelée par les services pour envoyer un mail de validation de compte
        /// </summary>
        /// <param name="email"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="isTest"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> SendValidationEmail(string email, string userId, string token, bool isTest = false)
        {
            bool result = false;

            if (!String.IsNullOrWhiteSpace(email) && !String.IsNullOrWhiteSpace(userId) && !String.IsNullOrWhiteSpace(token))
            {
                SendGridMessage msg = PopulateValidationEmail(email, userId, token, isTest);

                result = await SendMessage(msg);
            }

            return result;
        }

        /// <summary>
        /// Fonction appelée par les services pour envoyer un mail de récupération de mot de passe
        /// </summary>
        /// <param name="email"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="isTest"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> SendRecoveryEmail(string email, string userId, string token, bool isTest = false)
        {
            bool result = false;

            if (!String.IsNullOrWhiteSpace(email) && !String.IsNullOrWhiteSpace(userId) && !String.IsNullOrWhiteSpace(token))
            {
                SendGridMessage msg = PopulateRecoveryEmail(email, userId, token, isTest);

                result = await SendMessage(msg);
            }

            return result;
        }

        /// <summary>
        /// Fonction appelée par un controller pour envoyer un mail de contact
        /// </summary>
        /// <param name="email"></param>
        /// <param name="message"></param>
        /// <param name="isTest"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> SendContactEmail(string email, string message, bool isTest = false)
        {
            bool result = false;
            MyResponse response = new MyResponse();

            if (!String.IsNullOrWhiteSpace(email) && !String.IsNullOrWhiteSpace(message))
            {
                // Checking if email address is valid
                try
                {
                    var validAddress = new MailAddress(email);

                    SendGridMessage msg = PopulateContactEmail(validAddress, message, isTest);

                    result = await SendMessage(msg);

                    if (!result) response.Messages.Add("EmailNotSent");
                }
                catch(FormatException)
                {
                    result = false;
                    response.Messages.Add("InvalidEmailFormat");
                }
            }
            response.Succeeded = result;

            return response;
        }
    }
}
