using ChessApp.Server.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<ChessService>();
builder.Services.AddHttpClient<AiService>();


builder.Services.AddCors(opt =>
    opt.AddPolicy("ReactDev", p =>
        p.WithOrigins("http://localhost:5173", "https://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod()));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ReactDev");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
