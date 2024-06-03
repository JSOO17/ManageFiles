using ManageFiles.Config;
using ManageFiles.Interfaces;
using ManageFiles.Listeners;
using ManageFiles.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<ServiceBusOptions>(builder.Configuration.GetSection("AzureServiceBus"));
builder.Services.Configure<AzureBlobOptions>(builder.Configuration.GetSection("AzureBStorage"));
builder.Services.AddTransient<IFilesRepository, AzureBlobStorageRepository>();
builder.Services.AddTransient<IEventManage, AzureEventGridClient>();
builder.Services.AddHostedService<ServiceBusListener>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
