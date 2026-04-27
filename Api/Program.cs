var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Api.Controllers.UsersController).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
