# Instructions for AI coding agents

This file is the single source of truth for any AI agent working in this repository. `CLAUDE.md` imports it, so Claude Code and other agents read the same rules.

## Project in one paragraph

Stas Copilot is a RhinoCommon plugin for Rhino 8 that adds a docked chat panel. The panel runs an agent loop against the Claude API using tool use. The tools are C# functions that read and modify the active Rhino document, and later the active Grasshopper definition. See `README.md` for the overview and `docs/ARCHITECTURE.md` for the design.

## Stack

- C#, .NET (multi-target as the Rhino 8 plugin template does), RhinoCommon, Eto for UI
- Grasshopper SDK for the Grasshopper layer (later phase)
- Claude Messages API with tool use, called over HTTPS
- Distribution through Rhino's Package Manager (Yak)

## Rules

1. **Threading.** The Rhino document is not thread-safe. Every tool that touches the document must run on the UI thread. Network calls must never block the UI thread.
2. **Undo.** One user request equals one undo record. Open the record before the first tool call of a request and close it after the last.
3. **Tool results stay small.** Return counts, ids, bounding boxes and validity flags, never full geometry dumps. Large results waste context and money.
4. **Tools are queries, not dumps.** Prefer tools like "select by layer or type" over "list everything".
5. **Destructive operations need confirmation.** Deleting objects, running generated scripts, and overwriting files must ask the user first unless the user has enabled auto-approve for the session.
6. **No secrets in the repo.** The API key comes from the `ANTHROPIC_API_KEY` environment variable or the OS credential store. Never commit keys, never log them.
7. **Keep the tool layer separate from the UI.** The tool layer and agent loop must be testable without Rhino running. Only the thin adapter that calls RhinoCommon should require Rhino.
8. **No new dependencies without a reason.** Prefer the BCL and RhinoCommon. If a package is needed, say why in the PR or commit message.

## Testing

- Unit-test the agent loop, tool schemas, and result formatting with fakes. These tests must run with plain `dotnet test` and no Rhino.
- Anything that needs a live Rhino document is an integration test. Run those only on a machine with Rhino 8 installed, and keep them out of the default test run.
- Tests must never call the real Claude API by default. Use a recorded or fake client. Live API tests are opt-in behind an environment variable, because they cost money.

## Style

- Follow standard C# conventions: PascalCase for types and methods, camelCase for locals, one type per file.
- Small, focused commits with clear messages.
- Update `docs/` whenever behavior or architecture changes.

## Environment notes for agents

- Building against RhinoCommon works from the NuGet package, but loading and running the plugin requires Rhino 8 on Windows or macOS. A Linux or cloud sandbox can write and unit-test code but cannot run the plugin.
- When you cannot verify something against a running Rhino, say so plainly in your summary instead of assuming it works.

## Code layout

- `src/RhinoCopilot.Core`: agent loop, Claude client, tools, `IModelAdapter`. No RhinoCommon references allowed here.
- `src/RhinoCopilot.Plugin`: Rhino plugin, Eto panel, `RhinoModelAdapter`, `RhinoHostContext`.
- `tests/RhinoCopilot.Core.Tests`: xunit tests with fakes in `Support/`.
- To add a tool, follow "Adding a tool" in `docs/DEVELOPMENT.md`.

## Where to look

| Topic | File |
|---|---|
| Overview and status | `README.md` |
| Design and tool catalog | `docs/ARCHITECTURE.md` |
| Build, run, debug, package | `docs/DEVELOPMENT.md` |
| Phases and open questions | `docs/ROADMAP.md` |
