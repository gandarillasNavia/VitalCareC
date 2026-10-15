using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;
using MSUsuarios.App.Servicios;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.Tests.App.Servicios
{
    [Collection("SmtpEnv")]
    public class EmailServiceTests : IDisposable
    {
        private static readonly string[] VariablesEntorno =
        {
            "SMTP_HOST", "SMTP_PORT", "SMTP_REMITENTE_NOMBRE",
            "SMTP_REMITENTE_EMAIL", "SMTP_PASSWORD", "SMTP_USE_SSL",
            "SmtpSettings__Host", "SmtpSettings__Port", "SmtpSettings__RemitenteNombre",
            "SmtpSettings__RemitenteEmail", "SmtpSettings__Password", "SmtpSettings__UseSsl",
            "VC_TEST_PRINCIPAL", "VcTest__Clave"
        };

        private readonly Dictionary<string, string?> _entornoOriginal = new();

        public EmailServiceTests()
        {
            foreach (string nombre in VariablesEntorno)
            {
                _entornoOriginal[nombre] = Environment.GetEnvironmentVariable(nombre);
                Environment.SetEnvironmentVariable(nombre, null);
            }
        }

        public void Dispose()
        {
            foreach (var par in _entornoOriginal)
                Environment.SetEnvironmentVariable(par.Key, par.Value);
        }

        private static IConfiguration CrearConfig(Dictionary<string, string>? valores = null)
        {
            IConfiguration config = Substitute.For<IConfiguration>();
            config[Arg.Any<string>()].Returns((string?)null);

            if (valores != null)
            {
                foreach (var par in valores)
                    config[par.Key].Returns(par.Value);
            }

            return config;
        }

        private static EmailService CrearServicio(Dictionary<string, string>? valores = null) =>
            new EmailService(CrearConfig(valores));

        private static SmtpSettings ObtenerSettings(EmailService servicio)
        {
            FieldInfo campo = typeof(EmailService).GetField(
                "_smtpSettings", BindingFlags.NonPublic | BindingFlags.Instance)!;
            return (SmtpSettings)campo.GetValue(servicio)!;
        }

        private static void AssertSettings(
            EmailService servicio, string host, int port, string nombre,
            string email, string password, bool useSsl)
        {
            SmtpSettings s = ObtenerSettings(servicio);
            Assert.Equal(host, s.Host);
            Assert.Equal(port, s.Port);
            Assert.Equal(nombre, s.RemitenteNombre);
            Assert.Equal(email, s.RemitenteEmail);
            Assert.Equal(password, s.Password);
            Assert.Equal(useSsl, s.UseSsl);
        }

        private static int ObtenerPuertoLibre()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int puerto = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return puerto;
        }

        private static Dictionary<string, string> ConfigSmtp(int puerto) => new()
        {
            ["SmtpSettings:Host"] = "127.0.0.1",
            ["SmtpSettings:Port"] = puerto.ToString(),
            ["SmtpSettings:UseSsl"] = "false",
            ["SmtpSettings:RemitenteEmail"] = "no-reply@vitalcare.test",
            ["SmtpSettings:Password"] = "secreto"
        };


        [Fact] 
        public void Constructor_SinConfiguracion_UsaValoresPorDefecto()
        {
            EmailService sut = CrearServicio();

            AssertSettings(sut, "smtp.example.com", 587, "VitalCare",
                "no-reply@example.com", string.Empty, true);
        }

        [Fact]
        public void Constructor_HostConfigurado_UsaHostConfigurado()
        {
            EmailService sut = CrearServicio(new() { ["SmtpSettings:Host"] = "mail.test" });

            AssertSettings(sut, "mail.test", 587, "VitalCare",
                "no-reply@example.com", string.Empty, true);
        }

        [Fact] 
        public void Constructor_PortValido_UsaPortConfigurado()
        {
            EmailService sut = CrearServicio(new() { ["SmtpSettings:Port"] = "2525" });

            AssertSettings(sut, "smtp.example.com", 2525, "VitalCare",
                "no-reply@example.com", string.Empty, true);
        }

        [Fact]
        public void Constructor_RemitenteNombreConfigurado_UsaNombre()
        {
            EmailService sut = CrearServicio(new() { ["SmtpSettings:RemitenteNombre"] = "Farmacia" });

            AssertSettings(sut, "smtp.example.com", 587, "Farmacia",
                "no-reply@example.com", string.Empty, true);
        }

        [Fact]
        public void Constructor_RemitenteEmailConfigurado_UsaEmail()
        {
            EmailService sut = CrearServicio(new() { ["SmtpSettings:RemitenteEmail"] = "a@b.com" });

            AssertSettings(sut, "smtp.example.com", 587, "VitalCare",
                "a@b.com", string.Empty, true);
        }

        [Fact] 
        public void Constructor_PasswordConfigurado_UsaPassword()
        {
            EmailService sut = CrearServicio(new() { ["SmtpSettings:Password"] = "secreto" });

            AssertSettings(sut, "smtp.example.com", 587, "VitalCare",
                "no-reply@example.com", "secreto", true);
        }

        [Fact]
        public void Constructor_UseSslTrue_UseSslEsTrue()
        {
            EmailService sut = CrearServicio(new() { ["SmtpSettings:UseSsl"] = "true" });

            Assert.True(ObtenerSettings(sut).UseSsl);
        }

        [Fact]
        public void Constructor_UseSslFalse_UseSslEsFalse()
        {
            EmailService sut = CrearServicio(new() { ["SmtpSettings:UseSsl"] = "false" });

            Assert.False(ObtenerSettings(sut).UseSsl);
        }

        private static string? InvocarObtenerValor(IConfiguration config)
        {
            MethodInfo mi = typeof(EmailService).GetMethod(
                "ObtenerValorConfiguracion", BindingFlags.NonPublic | BindingFlags.Static)!;
            return (string?)mi.Invoke(null, new object[] { config, "VC_TEST_PRINCIPAL", "VcTest:Clave" });
        }

        [Fact] 
        public void ObtenerValor_VariablePrincipal_RetornaPrincipal()
        {
            Environment.SetEnvironmentVariable("VC_TEST_PRINCIPAL", "principal");
            Environment.SetEnvironmentVariable("VcTest__Clave", "compat");

            Assert.Equal("principal", InvocarObtenerValor(CrearConfig()));
        }

        [Fact]
        public void ObtenerValor_SoloVariableCompatibilidad_RetornaCompat()
        {
            Environment.SetEnvironmentVariable("VcTest__Clave", "compat");

            Assert.Equal("compat", InvocarObtenerValor(CrearConfig()));
        }

        [Fact]
        public void ObtenerValor_SinVariables_RetornaConfiguracion()
        {
            IConfiguration config = CrearConfig(new() { ["VcTest:Clave"] = "cfg" });

            Assert.Equal("cfg", InvocarObtenerValor(config));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void EnviarCorreo_EmailDestinoVacio_RetornaFail(string? email)
        {
            Result resultado = CrearServicio().EnviarCorreoActivacionCuenta(
                email!, "Juan", "jperez", "Temp123!", "token");

            Assert.False(resultado.IsSuccess);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void EnviarCorreo_UserNameVacio_RetornaFail(string? userName)
        {
            Result resultado = CrearServicio().EnviarCorreoActivacionCuenta(
                "destino@mail.com", "Juan", userName!, "Temp123!", "token");

            Assert.False(resultado.IsSuccess);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void EnviarCorreo_PasswordTemporalVacia_RetornaFail(string? password)
        {
            Result resultado = CrearServicio().EnviarCorreoActivacionCuenta(
                "destino@mail.com", "Juan", "jperez", password!, "token");

            Assert.False(resultado.IsSuccess);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void EnviarCorreo_TokenVacio_RetornaFail(string? token)
        {
            Result resultado = CrearServicio().EnviarCorreoActivacionCuenta(
                "destino@mail.com", null!, "jperez", "Temp123!", token!);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void EnviarCorreo_ServidorDisponible_RetornaOk()
        {
            using FakeSmtpServer servidor = new FakeSmtpServer();
            EmailService sut = CrearServicio(ConfigSmtp(servidor.Puerto));

            Result resultado = sut.EnviarCorreoActivacionCuenta(
                "destino@mail.com", "  Juan  ", "jperez", "Temp123!", "token-abc");

            Assert.True(resultado.IsSuccess);
            Assert.Contains("destino@mail.com", servidor.MensajeRecibido);
        }

        [Fact]
        public void EnviarCorreo_ServidorNoDisponible_RetornaFailSmtp()
        {
            EmailService sut = CrearServicio(ConfigSmtp(ObtenerPuertoLibre()));

            Result resultado = sut.EnviarCorreoActivacionCuenta(
                "destino@mail.com", "Juan", "jperez", "Temp123!", "token-abc");

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void EnviarCorreo_EmailDestinoInvalido_RetornaFailGenerico()
        {
            Result resultado = CrearServicio().EnviarCorreoActivacionCuenta(
                "no-es-un-correo", "Juan", "jperez", "Temp123!", "token-abc");

            Assert.False(resultado.IsSuccess);
        }

        private static string InvocarConstruirHtml(string nombres, string token)
        {
            MethodInfo mi = typeof(EmailService).GetMethod(
                "ConstruirHtmlActivacionCuenta", BindingFlags.NonPublic | BindingFlags.Static)!;
            return (string)mi.Invoke(null, new object[] { nombres, "jperez", "Temp123!", token })!;
        }

        [Fact]
        public void ConstruirHtml_SinNombres_UsaSaludoGenerico()
        {
            string html = InvocarConstruirHtml(string.Empty, "token");

            Assert.Contains("Estimado usuario", html);
        }

        [Fact] 
        public void ConstruirHtml_ConNombres_SaludaYEscapaToken()
        {
            string html = InvocarConstruirHtml("Juan", "a b&c");

            Assert.Contains("Estimado/a Juan", html);
            Assert.Contains("token=a%20b%26c", html);
        }

        private sealed class FakeSmtpServer : IDisposable
        {
            private readonly TcpListener _listener;
            private readonly Task _atencion;
            private volatile string _mensaje = string.Empty;

            public int Puerto { get; }
            public string MensajeRecibido => _mensaje;

            public FakeSmtpServer()
            {
                _listener = new TcpListener(IPAddress.Loopback, 0);
                _listener.Start();
                Puerto = ((IPEndPoint)_listener.LocalEndpoint).Port;
                _atencion = Task.Run(AtenderAsync);
            }

            private async Task AtenderAsync()
            {
                try
                {
                    using TcpClient cliente = await _listener.AcceptTcpClientAsync();
                    using NetworkStream flujo = cliente.GetStream();
                    using StreamReader lector = new StreamReader(flujo, Encoding.ASCII);
                    using StreamWriter escritor = new StreamWriter(flujo, Encoding.ASCII)
                    {
                        NewLine = "\r\n",
                        AutoFlush = true
                    };

                    await escritor.WriteLineAsync("220 localhost ESMTP");

                    bool enDatos = false;
                    StringBuilder datos = new StringBuilder();
                    string? linea;

                    while ((linea = await lector.ReadLineAsync()) != null)
                    {
                        if (enDatos)
                        {
                            if (linea == ".")
                            {
                                enDatos = false;
                                _mensaje = datos.ToString();
                                await escritor.WriteLineAsync("250 OK");
                            }
                            else
                            {
                                datos.AppendLine(linea);
                            }
                            continue;
                        }

                        string comando = linea.ToUpperInvariant();
                        if (comando.StartsWith("EHLO") || comando.StartsWith("HELO"))
                            await escritor.WriteLineAsync("250 localhost");
                        else if (comando.StartsWith("DATA"))
                        {
                            enDatos = true;
                            await escritor.WriteLineAsync("354 Fin con <CRLF>.<CRLF>");
                        }
                        else if (comando.StartsWith("QUIT"))
                        {
                            await escritor.WriteLineAsync("221 Bye");
                            break;
                        }
                        else
                            await escritor.WriteLineAsync("250 OK");
                    }
                }
                catch
                {
                }
            }

            public void Dispose()
            {
                _listener.Stop();
                _atencion.Wait(TimeSpan.FromSeconds(2));
            }
        }
    }
}