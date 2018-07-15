using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MVC_Exhibition.Models
{
    public class ExpoUser
    {
        public Guid Id { get; set; }

        [Required]
        public string UserName { get; set; }
        [Required]
        public string UserEmail { get; set; }
        [Required]
        public string UserPhoneNumber { get; set; }
        [Required]
        public string UserCompanyName { get; set; }

        public int UserCountryId { get; set; }
        //public Country UserCountry { get; set; }

        public int UserCityId { get; set; }
        //public City UserCity { get; set; }
        [Required]
        public string StoAddress { get; set; }
        public string KindOfActivity { get; set; }
        public string WorkPosition { get; set; }
        public DateTime? DateOfRegistration { get; set; }
        public DateTime? DateOfVisiting { get; set; }

        public async Task<string> SendMessageAsync(string barCodeFileName)
        {
            try
            {
                MailAddress from = new MailAddress("csharp.sdp.162@gmail.com", "Супер пупер поддержка");
                MailAddress to = new MailAddress(UserEmail);
                MailMessage m = new MailMessage(from, to)
                {
                    Subject = "О , мой Господь , вы были зарегестрированы!",
                    Body = $"Здравствуйте! {UserName}\n" +
                           $"Ваш уникальный id - {Id}\n" +
                           $"Дата регистрации - {DateOfRegistration}\n" +
                           $"Ваш номер - {UserPhoneNumber}\n" +
                           $"В приложении смотрите вам отправленный код"
                };
                m.Attachments.Add(new Attachment(barCodeFileName));

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("csharp.sdp.162@gmail.com", "sdp123456789"),
                    EnableSsl = true
                };
                await smtp.SendMailAsync(m);
                return "Сообщение на почту было отправлено";
            }
            catch (Exception e)
            {
                return e.ToString();
            }

        }

        public async Task<string> SendMessageAsync(string subject, string message, string fromName)
        {
            try
            {
                MailAddress from = new MailAddress("csharp.sdp.162@gmail.com", fromName);
                MailAddress to = new MailAddress(UserEmail);
                MailMessage m = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = message
                };
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("csharp.sdp.162@gmail.com", "sdp123456789"),
                    EnableSsl = true
                };
                await smtp.SendMailAsync(m);
                return "Сообщение на почту было отправлено";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}