using Microsoft.AspNetCore.Authentication;
using FlightPlanApi.Authentication;
using FlightPlanApi.Configuration;
using FlightPlanApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SwaggerConfiguration.Configure);
builder.Services.AddCors();
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
    ("BasicAuthentication", null);
builder.Services.AddScoped<IDatabaseAdapter, MongoDbDatabase>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.Configure<AdminCredentials>(builder.Configuration.GetSection("AdminCredentials"));
builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection("MongoDb"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint($"/swagger/{SwaggerConfiguration.DocName}/swagger.json", "Flight Plan API"));
}
app.UseCors(config =>
{
    config
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
});

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
