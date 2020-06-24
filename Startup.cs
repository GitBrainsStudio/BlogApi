using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitBrainsBlogApi.Handlers;
using GitBrainsBlogApi.Models;
using GitBrainsBlogApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GitBrainsBlogApi
{
    public class Startup
    {
        public static string SQLiteConnectionString { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //���� �������� JObject �� ����� � ������ ����������� http-post
            services.AddControllers().AddNewtonsoftJson();

            //��������� ���������� ���� ��������� � ������
            services.AddMvc(options =>
                {
                    options.Filters.Add(typeof(ExceptionHandler));
                });

            //�������� ������ ����������� � �� �� ����� ������������ appSettings
            SQLiteConnectionString = Configuration.GetConnectionString("DefaultConnection");

            //��������� jwt �������������
            var key = Encoding.ASCII.GetBytes("superSecretKey@345");
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            //��������� ��������� ��� ��������� � ����� ����� ������� ��������������� �����
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<Account>();

            //��������� ����������� ��� ������ � ������������� �����������
            services.AddSingleton<PostRepository>();
            services.AddSingleton<RoleRepository>();
            services.AddSingleton<TagRepository>();
            services.AddSingleton<PostTagRepository>();
            services.AddSingleton<CategoryRepository>();
            services.AddSingleton<CategoryPostRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //��������� ���� �������� ������� � ����������
            app.UseCors(corsPolicyBuilder =>
              corsPolicyBuilder.WithOrigins("http://localhost:4200")
             .AllowAnyMethod()
             .AllowAnyHeader()
           );

            
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            app.UseDefaultFiles();
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
