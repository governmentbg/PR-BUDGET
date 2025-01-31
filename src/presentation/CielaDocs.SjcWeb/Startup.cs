﻿using CielaDocs.Application;
using CielaDocs.Data;
using CielaDocs.Data.Contexts;
using CielaDocs.Models;
using CielaDocs.Shared;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Extensions;
using CielaDocs.SjcWeb.Helper;
using CielaDocs.SjcWeb.Models;
using CielaDocs.SjcWeb.Services;

using DevExpress.AspNetCore;
using DevExpress.Web.Office;

using gRpcFileTransfer;

using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using ITfoxtec.Identity.Saml2.Util;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;

using Newtonsoft.Json.Serialization;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CielaDocs.SjcWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration,IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Env = environment;
        }
        public IWebHostEnvironment Env { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddSession();
            services.AddApplication(Configuration);
            services.AddInfrastructureData(Configuration);
            services.AddInfrastructureShared(Configuration);
            services.AddTransient<IBufferedFileUploadService, BufferedFileUploadLocalService>();
            services.AddDbContext<ApplicationDbContext>(options =>
                     options.UseSqlServer(connectionString));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                  .AddRoles<IdentityRole>()
                 .AddEntityFrameworkStores<ApplicationDbContext>()
             .AddDefaultTokenProviders();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
         

            services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Events.OnRedirectToAccessDenied = new Func<Microsoft.AspNetCore.Authentication.RedirectContext<CookieAuthenticationOptions>, Task>(context =>
                {
                    context.Response.Redirect("./Account/AccessDenied/");
                    return context.Response.CompleteAsync();
                });

            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddDevExpressControls(options => {
                options.AddSpreadsheet(
                    spreadsheetOptions => {
                        spreadsheetOptions
                            .AddHibernation(hibernationOptions => {
                                hibernationOptions.StoragePath = System.IO.Path.Combine(Env.WebRootPath, "App_Data", "HibernationStorage");
                                hibernationOptions.Timeout = TimeSpan.FromMinutes(20);
                                hibernationOptions.DocumentsDisposeTimeout = TimeSpan.FromDays(1);
                                hibernationOptions.AllDocumentsOnApplicationEnd = true;
                            })
                            .AddAutoSaving(
                                (IDocumentInfo documentInfo, DocumentSavingEventArgs e) => {
                                    e.Handled = true;
                                }
                            );
                    }
                );
            });

            services.AddMvc().AddRazorPagesOptions(o => {
                o.Conventions.ConfigureFilter(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());
            });
            services.AddHttpContextAccessor();
            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();

            }).AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);


            var keysFolder = Path.Combine(Env.ContentRootPath, "temp-keys");
            services.AddDataProtection()
                .SetApplicationName("CielaDocs.SjcWeb")
                .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
                .SetDefaultKeyLifetime(TimeSpan.FromDays(14));

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "TOKEN-CookieName";
                options.HeaderName = "XSRF-TOKEN";
                options.FormFieldName = "TOKEN-FieldName";
                options.SuppressXFrameOptionsHeader = false;
            });





            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireClaim("Admin"));
                options.AddPolicy("CourtUserOnly", policy => policy.RequireClaim("CourtUser"));

            });
            services.AddSession(options =>
            {
                options.Cookie.Name = ".SjcBudget.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(20);//default
                options.Cookie.IsEssential = false;
            });
            services.BindConfig<Saml2Configuration>(Configuration, "Saml2", (serviceProvider, saml2Configuration) =>
            { 
                 //saml2Configuration.SigningCertificate = CertificateUtil.Load(Env.MapToPhysicalFilePath(Configuration["Saml2:SigningCertificateFile"]), Configuration["Saml2:SigningCertificatePassword"], X509KeyStorageFlags.DefaultKeySet | X509KeyStorageFlags.PersistKeySet);
                //Alternatively load the certificate by thumbprint from the machines Certificate Store.
                saml2Configuration.SigningCertificate = CertificateUtil.Load(StoreName.My, StoreLocation.LocalMachine, X509FindType.FindByThumbprint, Configuration["Saml2:SigningCertificateThumbprint"]);

                //saml2Configuration.SignatureValidationCertificates.Add(CertificateUtil.Load(AppEnvironment.MapToPhysicalFilePath(Configuration["Saml2:SignatureValidationCertificateFile"])));
                saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);

                var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
                var entityDescriptor = new EntityDescriptor();
                entityDescriptor.ReadIdPSsoDescriptorFromUrlAsync(httpClientFactory, new Uri(Configuration["Saml2:IdPMetadata"])).GetAwaiter().GetResult();
                if (entityDescriptor.IdPSsoDescriptor != null)
                {
                    saml2Configuration.AllowedIssuer = entityDescriptor.EntityId;
                    saml2Configuration.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;

                    //saml2Configuration.SingleLogoutDestination = entityDescriptor?.IdPSsoDescriptor?.SingleLogoutServices?.First()?.Location;
                    foreach (var signingCertificate in entityDescriptor.IdPSsoDescriptor.SigningCertificates)
                    {
                        if (signingCertificate.IsValidLocalTime())
                        {
                            saml2Configuration.SignatureValidationCertificates.Add(signingCertificate);
                        }
                    }
                    if (saml2Configuration.SignatureValidationCertificates.Count <= 0)
                    {
                        //throw new Exception("The IdP signing certificates has expired.");
                    }
                    if (entityDescriptor.IdPSsoDescriptor.WantAuthnRequestsSigned.HasValue)
                    {
                        saml2Configuration.SignAuthnRequest = entityDescriptor.IdPSsoDescriptor.WantAuthnRequestsSigned.Value;
                    }
                    //by me
                    saml2Configuration.SignAuthnRequest = true;
                }
                else
                {
                    throw new Exception("IdPSsoDescriptor not loaded from metadata.");
                }

                return saml2Configuration;
            });
            //services.AddSaml2(slidingExpiration: true);
            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Home/HandleError/{0}");
                //app.UseExceptionHandler("/Error");
                //app.UseStatusCodePagesWithRedirects("/Error/{0}");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCookiePolicy();
            app.UseHttpsRedirection();
            app.UseSession();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors("ClientPermission");
            app.UseSaml2();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });
            string? appMode = GlobalConfig.GetValue("ApplicationMode:AppMode");
            if (appMode.ToLower() == "demo")
            {
                app.UsePathBase("/pbDemo");
            }
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                name: "Admin",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

               endpoints.MapControllerRoute(
               name: "CourtUser",
               pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

    }
}
