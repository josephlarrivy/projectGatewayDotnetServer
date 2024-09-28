using MimeKit;
using MailKit.Net.Smtp;


namespace DotnetServer.Services
{
    public class EmailSender
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _fromEmail;
        public string _frontendUrl;

        public EmailSender(string smtpServer, int port, string username, string password, string fromEmail, string frontendUrl)
        {
            _smtpServer = smtpServer;
            _port = port;
            _username = username;
            _password = password;
            _fromEmail = fromEmail;
            _frontendUrl = frontendUrl;
        }
        public void SendEmail(string recipientEmail, string code)
        {
            // Create the email message
            var emailMessage = new MimeMessage();
            string link = $"{_frontendUrl}/login?code={code}";
            emailMessage.From.Add(new MimeKit.MailboxAddress("", _fromEmail));
            emailMessage.To.Add(new MimeKit.MailboxAddress("", recipientEmail));
            emailMessage.Subject = "Temporary Login Code";

            var htmlBody = string.Format(@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .first {{ font-size: 20px; }}
                        .second {{ font-size: 15px; }}
                        .third {{ font-size: 15px; }}
                        .button {{
                            display: inline-block;
                            padding: 10px 20px;
                            font-size: 16px;
                            color: black;
                            background-color: #ffffff;
                            border: 1px solid black;
                            text-align: center;
                            text-decoration: none;
                            border-radius: 5px;
                        }}
                        a {{
                            color: black !important;
                            text-decoration: none !important;
                        }}
                    </style>
                </head>
                <body>
                    <p class=""first"">Your temporary login code is <b>{0}</b></p>
                    <p class=""second"">You are receiving this email because you have asked to sign-in with a temporary login code.</p>
                    <p class=""third"">This code will expire in 5 minutes.</p>
                    <a href=""{1}"" class=""button"">Click Here to Sign In</a>
                </body>
                </html>", code, link
            );

            emailMessage.Body = new TextPart("html")
            {
                Text = htmlBody
            };

            // Send the email
            using (var client = new SmtpClient())
            {
                client.Connect(_smtpServer, _port, true);
                client.Authenticate(_username, _password);
                client.Send(emailMessage);
                client.Disconnect(true);
            }
        }
    }
}