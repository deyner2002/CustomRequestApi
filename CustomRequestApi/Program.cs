using CustomRequestApi.Blob;
using CustomRequestApi.DTOs;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


IConfiguration config = new ConfigurationBuilder()
.AddAzureAppConfiguration(options =>
{
    options.Connect(builder.Configuration.GetValue<string>("AppConfigsAzure"))
           .ConfigureKeyVault(kv =>
           {
               kv.SetCredential(new DefaultAzureCredential());
           });
})
.Build();

builder.Services.Configure<ConnectionStrings>(config.GetSection("customRequest:sqlconnection"));

builder.Services.Configure<ConnectionStringsBlob>(config.GetSection("customRequest:blobStorage"));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyCors", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

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
