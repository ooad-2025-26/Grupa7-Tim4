using SendGrid;
using SendGrid.Helpers.Mail;

namespace ZamETF.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // Slanje emaila S PDF prilogom (funk. 7)
        public async Task PošaljiEmail(string primalacEmail, string primalacIme,
            string naslov, byte[] pdfBytes, string pdfNaziv)
        {
            var settings = _config.GetSection("EmailSettings");
            var client = new SendGridClient(settings["SendGridApiKey"]);

            var msg = new SendGridMessage
            {
                From = new EmailAddress(settings["SenderEmail"], settings["SenderName"]),
                Subject = naslov,
                PlainTextContent = $"Poštovani {primalacIme},\n\nU prilogu se nalazi traženi dokument.\n\nS poštovanjem,\nStudentska služba ETF"
            };
            msg.AddTo(new EmailAddress(primalacEmail, primalacIme));
            msg.AddAttachment(pdfNaziv, Convert.ToBase64String(pdfBytes), "application/pdf");

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("SendGrid greška: {Status} {Body}", response.StatusCode, body);
                throw new Exception($"SendGrid greška: {response.StatusCode}");
            }
        }

        // Slanje emaila BEZ priloga (funk. 5 — jedna notifikacija)
        public async Task PošaljiEmail(string primalacEmail, string primalacIme,
            string naslov, string poruka)
        {
            var settings = _config.GetSection("EmailSettings");
            var client = new SendGridClient(settings["SendGridApiKey"]);

            var msg = new SendGridMessage
            {
                From = new EmailAddress(settings["SenderEmail"], settings["SenderName"]),
                Subject = naslov,
                PlainTextContent = $"Poštovani {primalacIme},\n\n{poruka}\n\nS poštovanjem,\nETF Sarajevo"
            };
            msg.AddTo(new EmailAddress(primalacEmail, primalacIme));

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("SendGrid greška: {Status} {Body}", response.StatusCode, body);
                throw new Exception($"SendGrid greška: {response.StatusCode}");
            }
        }

        // Bulk slanje — za admin notifikacije
        public async Task PošaljiBulkEmail(
            IEnumerable<(string Email, string Ime)> primatelji,
            string naslov, string poruka)
        {
            var settings = _config.GetSection("EmailSettings");
            var client = new SendGridClient(settings["SendGridApiKey"]);
            var from = new EmailAddress(settings["SenderEmail"], settings["SenderName"]);

            var lista = primatelji
                .Where(p => !string.IsNullOrWhiteSpace(p.Email))
                .ToList();

            if (!lista.Any()) return;

            // SendGrid podržava do 1000 primatelja u jednom pozivu
            var msg = new SendGridMessage
            {
                From = from,
                Subject = naslov
            };

            foreach (var p in lista)
            {
                msg.AddTo(new EmailAddress(p.Email, p.Ime));
            }

            // Personalizovani tekst nije moguć u bulk modu — koristimo generički
            msg.PlainTextContent = $"{poruka}\n\nS poštovanjem,\nETF Sarajevo";

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("SendGrid bulk greška: {Status} {Body}", response.StatusCode, body);
                throw new Exception($"SendGrid greška: {response.StatusCode}");
            }

            _logger.LogInformation("Bulk email poslan na {Count} primatelja", lista.Count);
        }
    }
}
