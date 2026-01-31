using Ivy;
using Ivy.Auth.GitHub;

// Создаём сервер
var server = new Server();

// Включаем Hot Reload для разработки
server.UseHotReload();

// Регистрируем HttpClient для GitHub API
server.Services.AddHttpClient("GitHubAuth", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
});

// Регистрируем IConfiguration
server.Services.AddSingleton(server.Configuration);

// Настраиваем GitHub Auth Provider
server.UseAuth<GitHubAuthProvider>(c => c.UseGitHub());

// Добавляем приложения из сборки
server.AddAppsFromAssembly();

// Настраиваем Chrome (боковая панель)
server.UseChrome();

// Устанавливаем заголовок
server.SetMetaTitle("OAuth Test App");

// Запускаем сервер
await server.RunAsync();
