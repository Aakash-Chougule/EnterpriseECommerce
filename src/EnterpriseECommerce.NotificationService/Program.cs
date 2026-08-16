using EnterpriseECommerce.NotificationService;
using EnterpriseECommerce.NotificationService.Services;

var builder =
    Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IEmailService, EmailService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();