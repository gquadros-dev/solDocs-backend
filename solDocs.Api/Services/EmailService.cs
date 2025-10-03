using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string code)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_config["SmtpSettings:SenderName"], _config["SmtpSettings:SenderEmail"]));
        email.To.Add(new MailboxAddress(toEmail, toEmail));
        email.Subject = "Seu Código de Recuperação de Senha";

        email.Body = new TextPart("html")
        {
            Text = $"<p>Olá,</p><p>Seu código para redefinir a senha é: <strong>{code}</strong></p><p>Este código expira em 5 minutos.</p>"
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_config["SmtpSettings:Server"], int.Parse(_config["SmtpSettings:Port"]), SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}