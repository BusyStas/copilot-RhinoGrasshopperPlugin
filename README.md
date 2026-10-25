# Copilot for Rhino & Grasshopper

An AI copilot that lives inside Rhino 8. You describe what you want in a chat panel ("build a spiral staircase with 14 steps", "put all the curves on a new layer and offset them 2 units"), and the copilot carries it out in the open document through a set of Rhino tools, checks the result, and keeps going until the task is done.

> Status: early development. Nothing is released yet. See [docs/ROADMAP.md](docs/ROADMAP.md).

## How it works

```
 Chat panel (Eto, docked in Rhino)
        |
        v
 Agent loop  <---->  Claude API (Messages API with tool use)
        |
        v
 Tool layer  ---->  RhinoCommon / Grasshopper SDK  ---->  Active Rhino document
```

1. The user types a request in the docked panel.
2. The plugin sends the conversation and the list of available tools to Claude.
3. Claude replies with tool calls (read the selection, add a curve, run a script, capture the viewport...).
4. The plugin executes each call on Rhino's UI thread inside a single undo record, then returns compact results to Claude.
5. The loop repeats until Claude reports the task is finished. One Ctrl+Z reverts the whole request.

Details are in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Planned features

- Docked chat panel for Rhino 8 on Windows and macOS
- Read tools: list objects and layers, inspect geometry, get the selection, bounding boxes, validity checks
- Edit tools: create and transform curves, surfaces, meshes and solids; booleans; layers and materials
- Script tool: run generated Python or C# in Rhino, with confirmation for risky operations
- Viewport capture so the model can verify its own work
- Grasshopper support: generate and edit definitions, set sliders, solve, bake results
- One undo step per request

## Requirements

- Rhino 8 (Windows or macOS)
- .NET SDK 7 or newer for building from source
- An Anthropic API key

## Getting started

Build and run instructions are in [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md).

## Working with AI coding agents

Instructions for AI coding agents (Claude Code, Augment, or similar) are in [AGENTS.md](AGENTS.md).

## License

Proprietary. All rights reserved. See [LICENSE](LICENSE).
