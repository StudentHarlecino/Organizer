using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net;

namespace Organizer
{
    // Сервис для отправки email-уведомлений через SMTP-сервер
    public class EmailService
    {
        // Настройки SMTP-сервера
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        // Конструктор с параметрами подключения к SMTP
        public EmailService(string smtpServer, int smtpPort, string smtpUsername, string smtpPassword, bool enableSsl = true)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUsername = smtpUsername;
            _smtpPassword = smtpPassword;
        }

        // Основной метод отправки email
        public void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                // Создание MIME-сообщения
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Органайзер задач", _smtpUsername));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;
                message.Date = DateTimeOffset.Now.DateTime;

                // Формирование тела письма (текст + HTML-версия)
                var bodyBuilder = new BodyBuilder
                {
                    TextBody = body,
                    HtmlBody = $@"
                    <html>
                        <body>
                            <div style='font-family: Arial, sans-serif; font-size: 14px;'>
                                {WebUtility.HtmlEncode(body).Replace("\n", "<br/>")}
                            </div>
                            <p style='color: #888; font-size: 12px; margin-top: 20px;'>
                                Это автоматическое уведомление. Пожалуйста, не отвечайте на это письмо.
                            </p>
                        </body>
                    </html>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                // Добавление служебных заголовков
                message.Headers.Add("X-Mailer", "Organizer/1.0");
                message.Headers.Add("X-Priority", "3");
                message.Headers.Add("X-Auto-Response-Suppress", "All");

                // Отправка через SMTP-клиент
                using (var client = new SmtpClient())
                {
                    // Настройки подключения
                    client.Timeout = 60000;
                    client.CheckCertificateRevocation = false;

                    // Установка соединения и аутентификация
                    client.Connect(_smtpServer, _smtpPort, SecureSocketOptions.SslOnConnect);
                    client.Authenticate(_smtpUsername, _smtpPassword);

                    // Отправка сообщения
                    client.Send(message);
                    Console.WriteLine($"Письмо успешно отправлено на {toEmail}");

                    // Искусственная задержка для избежания спам-фильтров
                    Thread.Sleep(1000);
                }
            }
            // Обработка ошибок SMTP
            catch (SmtpCommandException ex) when (ex.StatusCode == SmtpStatusCode.MailboxBusy)
            {
                // Повторная попытка после задержки при временной ошибке сервера
                Thread.Sleep(10000);
                Console.WriteLine($"Временная ошибка сервера: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // Логирование общей ошибки отправки
                Console.WriteLine($"Ошибка отправки: {ex.GetType().Name}: {ex.Message}");
                throw new ApplicationException("Не удалось отправить письмо", ex);
            }
        }
    }
}