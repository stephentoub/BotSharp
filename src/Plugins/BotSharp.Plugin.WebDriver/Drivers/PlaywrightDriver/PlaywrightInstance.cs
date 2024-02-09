using System.IO;

namespace BotSharp.Plugin.WebDriver.Drivers.PlaywrightDriver;

public class PlaywrightInstance : IDisposable
{
    IPlaywright _playwright;
    IBrowserContext _context;

    public IBrowserContext Context => _context;
    public IPage Page
    {
        get
        {
            if (_context == null)
            {
                InitInstance().Wait();
            }
            return _context.Pages.LastOrDefault();
        }
    }

    public async Task InitInstance()
    {
        if (_playwright == null)
        {
            _playwright = await Playwright.CreateAsync();
        }

        if (_context == null)
        {
            string tempFolderPath = $"{Path.GetTempPath()}\\playwright\\{Guid.NewGuid()}";
            _context = await _playwright.Chromium.LaunchPersistentContextAsync(tempFolderPath, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = true,
                Channel = "chrome",
                IgnoreDefaultArgs = new[]
                {
                    "--disable-infobars"
                },
                Args = new[]
                {
                    "--disable-infobars",
                    // "--start-maximized"
                }
            });

            _context.Page += async (sender, e) =>
            {
                e.Close += async (sender, e) =>
                {
                    Serilog.Log.Information($"Page is closed: {e.Url}");
                };
                Serilog.Log.Information($"New page is created: {e.Url}");
                await e.SetViewportSizeAsync(1280, 800);
            };

            _context.Close += async (sender, e) =>
            {
                Serilog.Log.Warning($"Playwright browser context is closed");
                _context = null;
            };
        }
    }

    public void Dispose()
    {
        _playwright.Dispose();
    }
}
