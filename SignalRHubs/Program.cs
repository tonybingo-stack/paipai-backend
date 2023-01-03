using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using System.Text.Json.Serialization;

using Microsoft.OpenApi.Models;
using SignalRHubs.Hubs;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

//GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => new MyIdProvider());
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

//builder.Services.AddSignalR().AddAzureSignalR("Endpoint=https://paipaisignalr.service.signalr.net/;AccessKey=xvCB88J0XjYLkhO1oQ6yO9j5nGnSXWb/kysDihoDB4I=;Version=1.0;");
builder.Services.AddControllersWithViews();

//builder.Services.AddSignalR(hubOptions =>
//{
//    hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(10);
//    hubOptions.MaximumReceiveMessageSize = 65_536;
//    hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(15);
//    hubOptions.MaximumParallelInvocationsPerClient = 2;
//    hubOptions.EnableDetailedErrors = true;
//    hubOptions.StreamBufferCapacity = 15;

//    if (hubOptions?.SupportedProtocols is not null)
//    {
//        foreach (var protocol in hubOptions.SupportedProtocols)
//            Console.WriteLine($"SignalR supports {protocol} protocol.");
//    }

//}).AddAzureSignalR("Endpoint=https://paipaisignalr.service.signalr.net/;AccessKey=xvCB88J0XjYLkhO1oQ6yO9j5nGnSXWb/kysDihoDB4I=;Version=1.0;")
//.AddJsonProtocol(options =>
//{
//    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
//    options.PayloadSerializerOptions.Encoder = null;
//    options.PayloadSerializerOptions.IncludeFields = false;
//    options.PayloadSerializerOptions.IgnoreReadOnlyFields = false;
//    options.PayloadSerializerOptions.IgnoreReadOnlyProperties = false;
//    options.PayloadSerializerOptions.MaxDepth = 0;
//    options.PayloadSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
//    options.PayloadSerializerOptions.DictionaryKeyPolicy = null;
//    options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
//    options.PayloadSerializerOptions.PropertyNameCaseInsensitive = false;
//    options.PayloadSerializerOptions.DefaultBufferSize = 32_768;
//    options.PayloadSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
//    options.PayloadSerializerOptions.ReferenceHandler = null;
//    options.PayloadSerializerOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;
//    options.PayloadSerializerOptions.WriteIndented = true;

//    Console.WriteLine($"Number of default JSON converters: {options.PayloadSerializerOptions.Converters.Count}");
//});

//builder.Services.AddIdentityCore<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true);

builder.Services.AddRazorPages();

// Configure Dapper Service
builder.Services.ConfigureDbHelper(connectionStrName: "DbConnection");

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IHomeService, HomeService>();

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Add the comments in the method to swagger doc
    var xfile = $"{builder.Environment.ApplicationName}.xml";
    var xpath = Path.Combine(AppContext.BaseDirectory, xfile);
    c.IncludeXmlComments(xpath);

    // Add JWT Authorization in swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Pass in the token from auth endpoint as value: Bearer eyj..",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
    //o.Events = new JwtBearerEvents
    //{
    //    OnMessageReceived = context =>
    //    {
    //        var accessToken = context.Request.Query["access_token"];

    //        // If the request is for our hub...
    //        var path = context.HttpContext.Request.Path;
    //        if (!string.IsNullOrEmpty(accessToken) &&
    //            (path.StartsWithSegments("/chatHub")))
    //        {
    //            // Read the token out of the query string
    //            context.Token = accessToken;
    //        }
    //        return Task.CompletedTask;
    //    }
    //};
});
builder.Services.AddAuthorization();
// Authentication
builder.Services.AddSignalR()
    .AddAzureSignalR(options =>
    {
        options.ConnectionString = "Endpoint=https://paipaisignalr.service.signalr.net/;AccessKey=xvCB88J0XjYLkhO1oQ6yO9j5nGnSXWb/kysDihoDB4I=;Version=1.0;";
        options.ClaimsProvider = context => new[]
        {
            new Claim(ClaimTypes.NameIdentifier, context.Request.Query["username"])
        };
    }
);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseFileServer();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();

app.MapHub<ChatHub>("/chathub", options =>
{
    options.Transports =
                HttpTransportType.WebSockets |
                HttpTransportType.LongPolling;
    options.CloseOnAuthenticationExpiration = true;
    options.ApplicationMaxBufferSize = 65_536;
    options.TransportMaxBufferSize = 65_536;
    options.MinimumProtocolVersion = 0;
    options.TransportSendTimeout = TimeSpan.FromSeconds(10);
    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(3);
    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(10);

    Console.WriteLine($"Authorization data items: {options.AuthorizationData.Count}");
});
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapHub<ChatHub>("/chathub", options =>
//    {
//        options.Transports =
//        HttpTransportType.WebSockets |
//        HttpTransportType.LongPolling;
//    }
//    );
//}
//);

app.Run();
