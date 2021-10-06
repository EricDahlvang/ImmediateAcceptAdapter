using ImmediateAcceptBot.BackgroundQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestImmediateAcceptAdapter.CustomAction;

namespace TestImmediateAcceptAdapter
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddBotRuntime(Configuration);

            services.AddSingleton<DeclarativeType>(new DeclarativeType<SleepAction>(SleepAction.Kind));

            // Activity specific BackgroundService for processing athenticated activities.
            services.AddHostedService<HostedActivityService>();
            services.AddSingleton<IActivityTaskQueue, ActivityTaskQueue>();

            // ImmediateAcceptAdapter, uses the ActivityTaskQueue 
            services.AddSingleton<ImmediateAcceptAdapter>();
            services.AddSingleton<IBotFrameworkHttpAdapter>(sp => sp.GetRequiredService<ImmediateAcceptAdapter>());

            // Needed for SkillsHttpClient which depends on BotAdapter
            services.AddSingleton<BotAdapter>(sp => sp.GetRequiredService<ImmediateAcceptAdapter>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();

            // Set up custom content types - associating file extension to MIME type.
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".lu"] = "application/vnd.microsoft.lu";
            provider.Mappings[".qna"] = "application/vnd.microsoft.qna";

            // Expose static files in manifests folder for skill scenarios.
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });
            app.UseWebSockets();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}