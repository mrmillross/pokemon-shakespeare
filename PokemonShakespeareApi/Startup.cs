using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PokemonShakespeareApi.Controllers;

namespace PokemonShakespeareApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath).AddJsonFile("appsettings.json");
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<PokemonConfig>(Configuration.GetSection("PokemonConfig"));
            services.Configure<ShakespeareConfig>(Configuration.GetSection("ShakespeareConfig"));
            services.AddSingleton<ITranslateIntoShakespeare, ShakespeareTranslationService>();
            services.AddSingleton<IGetPokemonDetails, PokemonGetterService>();
            services.AddHttpClient();
            services.AddControllers().AddJsonOptions(jsonOps =>
            {
                jsonOps.JsonSerializerOptions.WriteIndented = true;
                jsonOps.JsonSerializerOptions.AllowTrailingCommas = false;
                jsonOps.JsonSerializerOptions.IgnoreNullValues = true;
            });
            services.AddMvcCore();
            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
