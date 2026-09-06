using TicTacToe.Api.Services;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevCorsPolicy = "AngularDevCors";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// In-memory storage per the problem statement; singleton so state
// persists across requests for the lifetime of the process.
builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();
builder.Services.AddSingleton<IGameLogic, GameLogic>();
builder.Services.AddScoped<IGameService, GameService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(AngularDevCorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory-based integration tests, if added later.
public partial class Program { }
