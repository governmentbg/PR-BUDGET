using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting.WindowsServices;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using CielaDocs.DocStoreService.DataAccess;
using CielaDocs.DocStoreService.Repository;
using CielaDocs.DocStoreService.Services;
using Microsoft.AspNetCore.Http;



WebApplicationOptions options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
};

WebApplicationBuilder builder = WebApplication.CreateBuilder(options);


// Set WindowsServiceLifetime
builder.Host.UseWindowsService();
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<FileUploadService>().EnableGrpcWeb(); 
app.MapGrpcReflectionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();


