using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS cho FE
builder.Services.AddCors(opt => {
    opt.AddPolicy("fe", p => p.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Swagger cho debug
app.UseSwagger();
app.UseSwaggerUI();

// CORS
app.UseCors("fe");

// Proxy
app.MapReverseProxy();

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "gateway" }));

app.Run();
