using MongoDB.Driver;
using solDocs.Data;
using solDocs.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();

builder.Services.AddCorsConfiguration();
builder.Services.AddProjectServices();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthenticationConfig(builder.Configuration);
builder.Services.AddMongoDbConfiguration(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var database = services.GetRequiredService<IMongoDatabase>();
    await MongoDbInitializer.ConfigureIndexesAsync(database);
    await MongoDbInitializer.SeedAdminUserAsync(services);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowNextJsApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();