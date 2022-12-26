using BackupManager.Worker;
using BackupManager.Worker.Jobs;
using BackupManager.Worker.Services;
using Quartz;
using System.Text;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("appsettings.json");
    })
    .ConfigureServices((hostCtx, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<IFileLoggingService, FileLoggingService>();

        services.Configure<BackupSettings>(
            hostCtx.Configuration.GetSection("BackupSettings")
        );

        services.AddQuartz(q =>
        {
            // handy when part of cluster or you want to otherwise identify multiple schedulers
            q.SchedulerId = "BackupScheduler-Core";

            // we take this from appsettings.json, just show it's possible
            // q.SchedulerName = "Quartz ASP.NET Core Sample Scheduler";

            // as of 3.3.2 this also injects scoped services (like EF DbContext) without problems
            q.UseMicrosoftDependencyInjectionJobFactory();

            // or for scoped service support like EF Core DbContext
            // q.UseMicrosoftDependencyInjectionScopedJobFactory();

            // these are the defaults
            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 10;
            });

            // quickest way to create a job with single trigger is to use ScheduleJob
            // (requires version 3.2)
            q.ScheduleJob<TakeBackup>(trigger => trigger
                .WithIdentity("Combined Configuration Trigger")
                .WithSimpleSchedule(schedule => schedule.WithIntervalInHours(1).RepeatForever())
                .WithDescription("Hourly backup of replica database via 'Mariabackup' tool.")
            );
        });

        // Quartz.Extensions.Hosting allows you to fire background service that handles scheduler lifecycle
        services.AddQuartzHostedService(options =>
        {
            // when shutting down we want jobs to complete gracefully
            options.WaitForJobsToComplete = true;
        });

    })
    .Build();

await host.RunAsync();
