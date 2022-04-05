using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Server
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
            // needed to load configuration from appsettings.json
            services.AddOptions();

            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();

            //load general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            //load ip rules from appsettings.json
            //services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

            // inject counter and rules stores
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
                       
            // https://github.com/aspnet/Hosting/issues/793
            // the IHttpContextAccessor service is not registered by default.
            // the clientId/clientIp resolvers use it.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // Add SignalR
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region Security

            app.UseHsts(options => options.MaxAge(365).IncludeSubdomains().Preload());
            app.UseXXssProtection(opts => opts.EnabledWithBlockMode());
            app.UseXfo(opts => opts.SameOrigin());
            app.UseCsp(opts => opts.BlockAllMixedContent()
                                   .StyleSources(s => s.Self())
                                   .StyleSources(s => s.Self())
                                   .FontSources(s => s.Self())
                                   .FormActions(s => s.Self())
                                   .FrameAncestors(s => s.Self())
                                   .ImageSources(s => s.Self())
                                   .ScriptSources(s => s.Self()));
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
                await next();
            });
            
            #endregion

            app.UseRouting();

            app.UseAuthorization();
            
            app.UseIpRateLimiting();                      

            app.UseEndpoints(endpoints =>
            {
                // Route SignalR
                endpoints.MapHub<MyHub>("/chat");

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Server SignalR");
                });
            });            
        }
    }
}
