using FamilyCloud.Application.Services;

namespace FamilyCloud.Bootstrap;

public class Bootstrap
{
    private readonly CreateFirstAdminService _createFirstAdminService;

    public Bootstrap(CreateFirstAdminService createFirstAdminService)
    {
        _createFirstAdminService = createFirstAdminService;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Write login:");
        string login = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(login))
            return;

        Console.Write("Password:");
        string? password = ReadPassword();
        Console.WriteLine();

        if (string.IsNullOrWhiteSpace(password))
            return;

        bool created = await _createFirstAdminService.CreateAsync(login, password, cancellationToken);

        Console.WriteLine(created
            ? "Administrator created successfully."
            : "Administrator already exists.");
    }

    private string? ReadPassword()
    {
        string password = string.Empty;
        while (true)
        {
            var keyInfo = Console.ReadKey(true);
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    return password;

                case ConsoleKey.Escape:
                    return null;

                case ConsoleKey.Backspace:
                    if (password.Length > 0)
                        password = password[..^1];
                    break;

                default:
                    if (!char.IsControl(keyInfo.KeyChar))
                        password += keyInfo.KeyChar;
                    break;
            }
        }
    }
}