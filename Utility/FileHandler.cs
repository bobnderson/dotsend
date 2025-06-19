using Microsoft.Extensions.Configuration;
namespace dotsend.Utility
{
    public class FileHandler
    {
        public static string GetAppSettingsConfig(string desiredConfig)
        {

            IConfiguration configuration = new ConfigurationBuilder()
                 .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                 .AddJsonFile("appsettings.json")
                 .Build();

            return desiredConfig switch
            {
                "SmtpServer" => configuration.GetSection("SmtpClient")["SmtpServer"],
                "Port" => configuration.GetSection("SmtpClient")["Port"],
                "From" => configuration.GetSection("SmtpClient")["From"],
                "FromDisplayName" => configuration.GetSection("SmtpClient")["FromDisplayName"],
                "UserName" => configuration.GetSection("SmtpClient")["UserName"],
                "Harsh" => configuration.GetSection("SmtpClient")["Harsh"],
                "EnableSsl" => configuration.GetSection("SmtpClient")["EnableSsl"],
                "SendNotification" => configuration.GetSection("SmtpClient")["SendNotification"],

                "domain" => configuration.GetSection("env")["Domain"],
                "ldap" => configuration.GetSection("env")["Ldap"],

                "MessagingInterval" => configuration.GetSection("BackgroundServices")["MessagingInterval"],
                "MessagingIntervalLastRun" => configuration.GetSection("BackgroundServices")["MessagingIntervalLastRun"],
                "reportUrl" => configuration.GetSection("Reporting")["BaseUrl"],
                _ => "",
            };
        }

        public class FileObject
        {
            public byte[]? FileArray { get; set; }
            public string? FileName { get; set; }
            public string? ContentType { get; set; }
        }

        public static string[] FONTS =
        {
        Path.Combine(GetAppSettingsConfig("util"), "Helvetica.ttf"),
            Path.Combine(GetAppSettingsConfig("util"), "Futura.ttf"),
            Path.Combine(GetAppSettingsConfig("util"), "Futura-Bold.ttf")
        };

    }
}


