# Development guide

## Repository layout

```
RhinoCopilot.sln
Directory.Build.props
src/
  RhinoCopilot.Core/            Rhino-independent library (netstandard2.0)
    Agent/                      Agent loop, conversation, options, results
    Claude/                     Claude Messages API client and response types
    Model/                      IModelAdapter and the small DTOs tools return
    Tools/                      Tool implementations and DefaultTools registry
  RhinoCopilot.Plugin/          The Rhino 8 plugin (net7.0-windows, net7.0, net48)
    CopilotPlugIn.cs            Plugin entry point, registers the panel
    CopilotCommand.cs           The "StasCopilot" command that opens the panel
    CopilotPanel.cs             Eto chat panel
    RhinoHostContext.cs         UI-thread marshalling and undo records
    RhinoModelAdapter.cs        IModelAdapter implemented with RhinoCommon
tests/
  RhinoCopilot.Core.Tests/      xunit tests, no Rhino and no network needed
```

The plugin is a thin shell. The agent loop, tool definitions and Claude client live in Core so they can be tested without Rhino.

## Prerequisites

- Rhino 8 for Windows or macOS (to run and debug the plugin)
- .NET SDK 8 or newer (the tests target net8.0; the SDK also builds the older targets)
- An IDE: Visual Studio 2022, JetBrains Rider, or VS Code with the C# extension
- An Anthropic API key

## One-time setup

1. Clone the repository.
2. Set your API key as an environment variable, then restart your IDE and Rhino so they pick it up:
   - Windows (PowerShell): `setx ANTHROPIC_API_KEY "your-key-here"`
   - macOS (zsh): add `export ANTHROPIC_API_KEY="your-key-here"` to `~/.zshrc`
3. Optional: set `RHINO_COPILOT_MODEL` to use a different Claude model. The default is `claude-sonnet-5-5`.

The key is never stored in the repository and is never logged.

## Build

```
dotnet build RhinoCopilot.sln
```

The plugin project builds on Windows. On macOS or Linux the `net7.0-windows` target cannot build, so build only the library and tests there:

```
dotnet build src/RhinoCopilot.Core
dotnet test tests/RhinoCopilot.Core.Tests
```

Building the plugin produces `StasCopilot.rhp` under `src/RhinoCopilot.Plugin/bin/<Configuration>/<framework>/`. Rhino 8 on Windows uses the `net7.0-windows` output by default.

## Tests

```
dotnet test
```

The default test run needs no Rhino and makes no network calls. The Claude client is tested against a stub HTTP handler, and the agent loop against a scripted fake client.

## Run in Rhino

1. Build the plugin.
2. In Rhino, run the `PlugInManager` command, choose Install, and select `StasCopilot.rhp` from the build output folder.
3. Run the `StasCopilot` command. The chat panel opens docked in Rhino.
4. Type a request such as "What units is this document in?" or "Make a box 10 by 20 by 5 on a layer called Test."
5. A whole request is one undo step: run `Undo` once to revert everything the copilot did.

To debug, attach your IDE's debugger to the Rhino process (`Rhino.exe` on Windows).

## Adding a tool

1. If the tool needs new document access, add a method to `IModelAdapter` and implement it in `RhinoModelAdapter` and `FakeModelAdapter`.
2. Add a class in `src/RhinoCopilot.Core/Tools/` that derives from `ToolBase`. Give it a clear description, a JSON input schema, and an `Execute` that returns a small result.
3. Register it in `DefaultTools.CreateRegistry`.
4. Add tests in `ToolTests.cs`. The existing schema test checks the name, description and schema shape automatically.
5. If the tool deletes, overwrites, or runs generated code, override `RequiresConfirmation` to return true.

## Packaging

Plugins are distributed through Rhino's Package Manager using Yak. This is not set up yet. When it is, the steps will be to build the release configuration, add a `manifest.yml`, run `yak build`, and publish with `yak push`.

## Troubleshooting

- **Copilot command not found:** confirm the plugin is loaded in `PlugInManager`. Rhino reports load errors there.
- **"Set the ANTHROPIC_API_KEY environment variable":** the variable must be visible to the process that launched Rhino. Set it, then fully restart Rhino.
- **HTTP 401 from the Claude API:** the key is wrong or revoked.
- **HTTP 404 or "model not found":** set `RHINO_COPILOT_MODEL` to a model your account can use.
- **Rebuild fails because a file is locked:** Rhino locks loaded plugin files. Close Rhino before rebuilding.
- **Build error about `System.Drawing` or `Eto` on a target framework:** the RhinoCommon package normally brings these in. If one target complains, add the missing package reference to `RhinoCopilot.Plugin.csproj` for that target.
