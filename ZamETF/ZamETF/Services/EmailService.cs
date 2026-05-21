using MailKit.Net.Smtp;
using MimeKit;

namespace ZamETF.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task PošaljiEmail(string primalacEmail, string primalacIme,
            string naslov, byte[] pdfBytes, string pdfNaziv)
        {
            var settings = _config.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                settings["SenderName"],
                settings["SenderEmail"]));
            message.To.Add(new MailboxAddress(primalacIme, primalacEmail));
            message.Subject = naslov;

            var builder = new BodyBuilder();
            builder.TextBody = $"Poštovani {primalacIme},\n\nU prilogu se nalazi traženi dokument.\n\nS poštovanjem,\nStudentska služba ETF";
            builder.Attachments.Add(pdfNaziv, pdfBytes,
                new ContentType("application", "pdf"));

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(settings["SmtpServer"],
                int.Parse(settings["SmtpPort"]), false);
            await client.AuthenticateAsync(
                settings["SenderEmail"],
                settings["SenderPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}