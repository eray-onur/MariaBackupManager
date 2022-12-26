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

            // Hourly backup scheduling
            q.ScheduleJob<TakeBackup>(trigger => trigger
                .WithIdentity("Hourly Backup")
                .UsingJobData("frequency", Convert.ToInt16(BackupFrequency.Hourly))
                .WithSimpleSchedule(schedule => schedule.WithIntervalInHours(1).RepeatForever())
                .WithDescription("Hourly backup of replica database via 'Mariabackup' tool.")
                .StartNow()
            );

            // Daily backup scheduling
            q.ScheduleJob<TakeBackup>(trigger => trigger
                .WithIdentity("Daily Backup")
                .UsingJobData("frequency", Convert.ToInt16(BackupFrequency.Daily))
                .WithSimpleSchedule(schedule => schedule.WithIntervalInHours(24).RepeatForever())
                .WithDescription("Daily backup of replica database via 'Mariabackup' tool.")
                .StartNow()
            );

            // Weekly backup scheduling
            q.ScheduleJob<TakeBackup>(trigger => trigger
                .WithIdentity("Weekly Backup")
                .UsingJobData("frequency", Convert.ToInt16(BackupFrequency.Weekly))
                .WithSimpleSchedule(schedule => schedule.WithIntervalInHours(168).RepeatForever())
                .WithDescription("Weekly backup of replica database via 'Mariabackup' tool.")
                .StartNow()
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
