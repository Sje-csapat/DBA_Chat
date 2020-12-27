using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace masodik
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            contentRoot = env.ContentRootPath;
            Elerhato.TestDBConnection(contentRoot);
            setUsersToOffline();
        }

        public void setUsersToOffline()
        {
            SQLiteConnection dbconn = Elerhato.Dbconn;
            int conn_status = 0;
            if (dbconn != null && dbconn.State == ConnectionState.Closed)
            {
                conn_status = 1;
                dbconn.Open();
            }
            using var cmd = new SQLiteCommand(dbconn);
            cmd.CommandText = "UPDATE users SET status = @status, updated_at = @updated_at";
            cmd.Parameters.AddWithValue("@status", 0);
            cmd.Parameters.AddWithValue("@updated_at", (int)DateTimeOffset.Now.ToUnixTimeSeconds());
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            if (Convert.ToBoolean(conn_status))
            {
                dbconn.Close();
            }
        }

        public IConfiguration Configuration { get; }
        public string contentRoot;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(600);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddMemoryCache();
            services.AddControllers();
            services.AddMvc();
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

            app.UseStaticFiles();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllers();
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
