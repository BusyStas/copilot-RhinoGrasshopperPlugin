# Development guide

## Prerequisites

- Rhino 8 for Windows or macOS (needed to run and debug the plugin)
- .NET SDK 7 or newer
- An IDE: Visual Studio 2022 (Windows), JetBrains Rider, or VS Code with the C# extension
- An Anthropic API key

## One-time setup

1. Clone the repository.
2. Set your API key as an environment variable and restart your IDE and Rhino so they pick it up:
   - Windows (PowerShell): `setx ANTHROPIC_API_KEY "your-key-here"`
   - macOS or Linux (zsh): add `export ANTHROPIC_API_KEY="your-key-here"` to `~/.zshrc`
3. Install the Rhino plugin templates (useful for scaffolding and reference): `dotnet new install Rhino.Templates`

The key is never stored in the repository.

## Build

```
dotnet build
```

The RhinoCommon package comes from NuGet, so building does not require Rhino to be running.

## Run and debug

1. Build the plugin.
2. In Rhino, run the `PlugInManager` command and install the built `.rhp` file.
3. Run the copilot's panel command to open the chat panel.
4. To debug, attach your IDE's debugger to the Rhino process.

Exact project paths and the panel command name will be documented here once the first project is added.

## Tests

```
dotnet test
```

The default test run needs no Rhino and makes no network calls. Tests that need a live Rhino document or the real Claude API are opt-in, because they require a Rhino install and cost money.

## Packaging

Plugins are distributed through Rhino's Package Manager using Yak:

1. Build the release configuration.
2. Create the package with `yak build` from the output folder (needs a `manifest.yml`).
3. Publish with `yak push`.

## Troubleshooting

- **Panel does not appear:** confirm the plugin is loaded in `PlugInManager` and that the build targets the .NET version your Rhino 8 uses.
- **Authentication errors:** confirm `ANTHROPIC_API_KEY` is set in the environment Rhino was launched from. Restart Rhino after setting it.
- **Changes do not show up:** Rhino locks loaded plugin files. Close Rhino before rebuilding, or use Rhino 8's plugin reload where available.
