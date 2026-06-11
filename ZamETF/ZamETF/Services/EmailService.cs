using MailKit.Net.Smtp;
using MailKit.Security;
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

        // Slanje emaila S PDF prilogom (za funk. 7 — dokumenti studentske službe)
        public async Task PošaljiEmail(string primalacEmail, string primalacIme,
            string naslov, byte[] pdfBytes, string pdfNaziv)
        {
            var settings = _config.GetSection("EmailSettings");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings["SenderName"], settings["SenderEmail"]));
            message.To.Add(new MailboxAddress(primalacIme, primalacEmail));
            message.Subject = naslov;

            var builder = new BodyBuilder();
            builder.TextBody = $"Poštovani {primalacIme},\n\nU prilogu se nalazi traženi dokument.\n\nS poštovanjem,\nStudentska služba ETF";
            builder.Attachments.Add(pdfNaziv, pdfBytes, new ContentType("application", "pdf"));
            message.Body = builder.ToMessageBody();

            await SendAsync(message, settings);
        }

        // Slanje emaila BEZ priloga (za funk. 5 — notifikacije)
        public async Task PošaljiEmail(string primalacEmail, string primalacIme,
            string naslov, string poruka)
        {
            var settings = _config.GetSection("EmailSettings");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings["SenderName"], settings["SenderEmail"]));
            message.To.Add(new MailboxAddress(primalacIme, primalacEmail));
            message.Subject = naslov;

            var builder = new BodyBuilder();
            builder.TextBody = $"Poštovani {primalacIme},\n\n{poruka}\n\nS poštovanjem,\nETF Sarajevo";
            message.Body = builder.ToMessageBody();

            await SendAsync(message, settings);
        }

        // Bulk slanje — jedna SMTP konekcija za sve primatelje
        public async Task PošaljiBulkEmail(
            IEnumerable<(string Email, string Ime)> primatelji,
            string naslov, string poruka)
        {
            var settings = _config.GetSection("EmailSettings");

            var lista = primatelji
                .Where(p => !string.IsNullOrWhiteSpace(p.Email))
                .ToList();

            if (!lista.Any()) return;

            using var client = new SmtpClient();
            await client.ConnectAsync(settings["SmtpServer"],
                int.Parse(settings["SmtpPort"]), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(settings["SenderEmail"], settings["SenderPassword"]);

            foreach (var p in lista)
            {
                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(settings["SenderName"], settings["SenderEmail"]));
                    message.To.Add(new MailboxAddress(p.Ime, p.Email));
                    message.Subject = naslov;
                    message.Body = new BodyBuilder
                    {
                        TextBody = $"Poštovani {p.Ime},\n\n{poruka}\n\nS poštovanjem,\nETF Sarajevo"
                    }.ToMessageBody();

                    await client.SendAsync(message);
                }
                catch { /* jedna neuspješna ne blokira ostale */ }
            }

            await client.DisconnectAsync(true);
        }

        // Zajednička SMTP logika za pojedinačno slanje
        private async Task SendAsync(MimeMessage message, IConfigurationSection settings)
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(settings["SmtpServer"],
                int.Parse(settings["SmtpPort"]), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(settings["SenderEmail"], settings["SenderPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
