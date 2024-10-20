
using Hangfire;
using HangfireBasicAuthenticationFilter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using SurveyBasket;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);


builder.Host.UseSerilog((context, configuration) =>
configuration.ReadFrom.Configuration(context.Configuration)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    DashboardTitle = "Survey Basket Dashboard",
    Authorization =
    [
        new HangfireCustomBasicAuthenticationFilter
        {
            User = builder.Configuration.GetSection("HangFireSettings:Username").Value,
            Pass =  builder.Configuration.GetSection("HangFireSettings:Password").Value
        }
    ],
    //  IsReadOnlyFunc = (DashboardContext context) => true
});

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using var scope = scopeFactory.CreateScope();
var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
RecurringJob.AddOrUpdate("SendNewPollsNotification", () => notificationService.SendNewPollsNotification(null), Cron.Daily());
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler();
app.UseRateLimiter();
app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.Run();
