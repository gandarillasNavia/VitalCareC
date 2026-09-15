using System.Net;
using System.Net.Mail;
using MSUsuarios.App.Interfaces;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.App.Servicios
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IConfiguration configuration)
        {
            _smtpSettings = new SmtpSettings
            {
                Host = ObtenerValorConfiguracion(configuration, "SMTP_HOST", "SmtpSettings:Host")
                    ?? "smtp.example.com",
                Port = int.TryParse(
                    ObtenerValorConfiguracion(configuration, "SMTP_PORT", "SmtpSettings:Port"),
                    out int port) ? port : 587,
                RemitenteNombre = ObtenerValorConfiguracion(
                        configuration,
                        "SMTP_REMITENTE_NOMBRE",
                        "SmtpSettings:RemitenteNombre"
                    )
                    ?? "VitalCare",
                RemitenteEmail = ObtenerValorConfiguracion(
                        configuration,
                        "SMTP_REMITENTE_EMAIL",
                        "SmtpSettings:RemitenteEmail"
                    )
                    ?? "no-reply@example.com",
                Password = ObtenerValorConfiguracion(configuration, "SMTP_PASSWORD", "SmtpSettings:Password")
                    ?? string.Empty,
                UseSsl = bool.TryParse(
                    ObtenerValorConfiguracion(configuration, "SMTP_USE_SSL", "SmtpSettings:UseSsl"),
                    out bool ssl) ? ssl : true
            };
        }

        private static string? ObtenerValorConfiguracion(
            IConfiguration configuration,
            string variableEntornoPrincipal,
            string claveConfiguracion)
        {
            string variableEntornoCompatibilidad = claveConfiguracion.Replace(':', '_');
            variableEntornoCompatibilidad = variableEntornoCompatibilidad.Replace("_", "__");

            return Environment.GetEnvironmentVariable(variableEntornoPrincipal)
                ?? Environment.GetEnvironmentVariable(variableEntornoCompatibilidad)
                ?? configuration[claveConfiguracion];
        }

        public Result EnviarCorreoActivacionCuenta(
            string emailDestino,
            string nombres,
            string userName,
            string passwordTemporal,
            string tokenActivacion)
        {
            emailDestino = emailDestino?.Trim() ?? string.Empty;
            nombres = nombres?.Trim() ?? string.Empty;
            userName = userName?.Trim() ?? string.Empty;
            passwordTemporal = passwordTemporal?.Trim() ?? string.Empty;
            tokenActivacion = tokenActivacion?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(emailDestino))
                return Result.Fail("El correo destino es obligatorio.");

            if (string.IsNullOrWhiteSpace(userName))
                return Result.Fail("El nombre de usuario es obligatorio para el correo.");

            if (string.IsNullOrWhiteSpace(passwordTemporal))
                return Result.Fail("La contraseña temporal es obligatoria para el correo.");

            if (string.IsNullOrWhiteSpace(tokenActivacion))
                return Result.Fail("El token de activación es obligatorio para el correo.");

            try
            {
                string asunto = "Activación de cuenta - Farmacia VitalCare";
                string cuerpoHtml = ConstruirHtmlActivacionCuenta(
                    nombres,
                    userName,
                    passwordTemporal,
                    tokenActivacion
                );

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(
                    _smtpSettings.RemitenteEmail,
                    _smtpSettings.RemitenteNombre
                );
                message.To.Add(emailDestino);
                message.Subject = asunto;
                message.Body = cuerpoHtml;
                message.IsBodyHtml = true;

                using SmtpClient client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(
                        _smtpSettings.RemitenteEmail,
                        _smtpSettings.Password
                    )
                };
                client.EnableSsl = _smtpSettings.UseSsl;
                client.Timeout = 10000; // 10 segundos timeout

                client.Send(message);

                return Result.Ok();
            }
            catch (SmtpException smtpEx)
            {
                return Result.Fail($"Error SMTP: No se pudo conectar con el servidor de correo. {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"No se pudo enviar el correo electronico. Detalle: {ex.Message}");
            }
        }



        private string ConstruirHtmlActivacionCuenta(
            string nombres,
            string userName,
            string passwordTemporal,
            string tokenActivacion)
        {
            string saludo = string.IsNullOrWhiteSpace(nombres) ? "Estimado usuario" : $"Estimado/a {nombres}";
            string urlActivacion = $"http://localhost:5081/Auth/ActivarCuenta?token={Uri.EscapeDataString(tokenActivacion)}";

            return $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
        </head>
        <body style='margin:0; padding:0; background-color:#f0f7f5; font-family:Arial, sans-serif; color:#1f2937;'>

        <table width='100%' cellpadding='0' cellspacing='0' style='padding:30px 0;'>
        <tr>
        <td align='center'>

        <table width='600' style='background:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 4px 18px rgba(0,0,0,0.08);'>

        <tr>
        <td style='background:linear-gradient(135deg, #1f7a63, #14532d); padding:30px; text-align:center;'>
            <h1 style='margin:0; color:white;'>Activación de cuenta</h1>
            <p style='margin:8px 0 0; color:#d1fae5;'>Farmacia VitalCare</p>
        </td>
        </tr>

        <tr>
        <td style='padding:40px;'>

        <p style='font-size:16px;'>{saludo},</p>

        <p style='color:#4b5563; line-height:1.6;'>
        Tu cuenta ha sido registrada correctamente en Farmacia VitalCare. Para completar tu registro y poder acceder al sistema, debes activar tu cuenta.
        </p>

        <p style='color:#4b5563; line-height:1.6;'>
        <strong>Tus credenciales temporales:</strong>
        </p>

        <table width='100%' style='margin:20px 0; background:#ecfdf5; border:1px solid #bbf7d0; border-radius:10px;'>
        <tr>
        <td style='padding:20px;'>

        <p><strong>Usuario:</strong>
        <span style='color:#065f46;'>{userName}</span></p>

        <p><strong>Contraseña temporal:</strong>
        <span style='color:#b91c1c; font-weight:bold;'>{passwordTemporal}</span></p>

        </td>
        </tr>
        </table>

        <p style='color:#4b5563; margin-top:30px;'>
        <strong>Importante:</strong> Por favor, activa tu cuenta haciendo clic en el botón de abajo. Luego podrás establecer una nueva contraseña segura.
        </p>

        <div style='text-align:center; margin:30px 0;'>
            <a href='{urlActivacion}' style='display:inline-block; background-color:#1f7a63; color:white; padding:14px 40px; text-decoration:none; border-radius:6px; font-weight:bold; font-size:16px;'>
                Activar mi cuenta
            </a>
        </div>

        <p style='color:#6b7280; font-size:14px; margin-top:30px;'>
        Si el botón anterior no funciona, copia y pega este enlace en tu navegador:<br>
        <span style='word-break:break-all; color:#4b5563;'>{urlActivacion}</span>
        </p>

        <p style='color:#4b5563; margin-top:20px;'>
        Este enlace expirará en 24 horas. Si no activas tu cuenta dentro de ese tiempo, deberás solicitar un nuevo enlace de activación.
        </p>

        <p style='margin-top:30px;'>Atentamente,<br><strong>Farmacia VitalCare</strong></p>

        </td>
        </tr>

        <tr>
        <td style='padding:20px; text-align:center; background:#ecfdf5; font-size:12px; color:#6b7280;'>
        Mensaje automático - no responder
        </td>
        </tr>

        </table>

        </td>
        </tr>
        </table>

        </body>
        </html>";
        }


    }
}
