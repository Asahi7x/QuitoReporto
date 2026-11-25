using System.Net;
using System.Net.Mail;

namespace QuitoReporto.Api.Services
{
    public interface IEmailService
    {
        Task EnviarAsync(string destinatario, string asunto, string cuerpo);
    }

    /// <summary>
    /// Envía correos por SMTP. Si no hay servidor configurado (sección "Smtp" vacía),
    /// solo escribe el mensaje en el log: útil en desarrollo para ver el código de recuperación.
    /// </summary>
    public class SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger) : IEmailService
    {
        public async Task EnviarAsync(string destinatario, string asunto, string cuerpo)
        {
            var host = config["Smtp:Host"];
            if (string.IsNullOrWhiteSpace(host))
            {
                logger.LogWarning("SMTP no configurado. Correo para {Destinatario} | {Asunto}: {Cuerpo}", destinatario, asunto, cuerpo);
                return;
            }

            using var cliente = new SmtpClient(host, config.GetValue("Smtp:Puerto", 587))
            {
                EnableSsl = config.GetValue("Smtp:Ssl", true),
                Credentials = new NetworkCredential(config["Smtp:Usuario"], config["Smtp:Clave"])
            };
            var remitente = config["Smtp:Remitente"] ?? config["Smtp:Usuario"] ?? "no-reply@quitoreporto.ec";
            using var mensaje = new MailMessage(remitente, destinatario, asunto, cuerpo);
            await cliente.SendMailAsync(mensaje);
        }
    }
}
