using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Регистрируем фабрику подключений
builder.Services.AddSingleton<IDbConnectionFactory, WebDbConnectionFactory>();

// ✅ Регистрируем DBConnection как Singleton
builder.Services.AddSingleton<DBConnection>(sp =>
{
    var factory = sp.GetRequiredService<IDbConnectionFactory>();
    return new DBConnection(factory);
});

builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.Run();