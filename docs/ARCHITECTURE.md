# Architecture

## Goals

- A copilot that works inside Rhino, not next to it
- Safe by default: one undo per request, confirmation for risky actions
- Tool layer and agent loop testable without Rhino
- Direct modeling first, Grasshopper second

## Components

### 1. Plugin shell
A RhinoCommon plugin for Rhino 8. It registers a command that shows a dockable panel and owns the plugin settings (API key source, model, auto-approve).

### 2. Chat panel
Built with Eto so the same UI code runs on Windows and macOS. It shows the conversation, streams the model's text, and displays each tool call and its outcome so the user can follow along. It also hosts confirmation prompts.

### 3. Claude client
A thin HTTPS client for the Claude Messages API with tool use. The model name is configurable. All calls are asynchronous and never run on the UI thread.

### 4. Agent loop
```
messages = [user request]
undo record opened
repeat:
    response = claude(messages, tools)
    if response has no tool calls: break
    for each tool call:
        result = run on UI thread
        append result to messages
undo record closed
```
Guards: a maximum number of steps per request, a cancel button, and a clear error message if a tool fails. A failed tool returns its error to the model so it can recover.

### 5. Tool layer
Each tool has a name, a description, a JSON schema for its input, and a function that returns a small JSON result. Tools are registered in one place so the schema sent to Claude and the code that runs it cannot drift apart.

### 6. Rhino adapter
The only code that calls RhinoCommon directly. Everything above it depends on an interface, which lets tests substitute a fake document.

## Initial tool catalog

| Group | Tool | Purpose |
|---|---|---|
| Read | `get_selection` | Ids, types and layers of selected objects |
| Read | `list_layers` | Layer names, colors, visibility |
| Read | `find_objects` | Query by layer, type, name or attribute |
| Read | `get_object_info` | Bounding box, validity, length/area/volume |
| Edit | `create_geometry` | Points, lines, curves, boxes, spheres, extrusions |
| Edit | `transform_objects` | Move, rotate, scale, mirror, array |
| Edit | `boolean_operation` | Union, difference, intersection |
| Edit | `set_attributes` | Layer, color, name, material |
| Run | `run_rhino_command` | Execute a Rhino command string |
| Run | `run_script` | Execute generated Python or C# (confirmation required) |
| See | `capture_viewport` | Image of a view so the model can check its work |

## Grasshopper layer (later)

Grasshopper is itself a RhinoCommon plugin, so the same panel and loop apply. New tools operate on the active definition: add a component, connect an output to an input, set a slider, solve, read outputs, bake. Open problems: giving the model an accurate catalog of components and their parameters, and laying out the canvas so a human can read the result.

## Safety model

- Read tools never ask for confirmation.
- Edit tools run without confirmation but are undoable in one step.
- Deleting objects, running generated scripts, and overwriting files require explicit user approval unless auto-approve is on.
- The API key is read from the environment or the OS credential store and is never logged.

## Context management

Large models cannot be described in full. The model asks narrow questions through query tools, and results are summarized (counts, ids, bounding boxes, validity). After a geometry change, the loop can attach a viewport capture so the model sees the actual result.

## Alternatives considered

- **MCP server:** exposing the same tools to an external client such as Claude Desktop. Fast to prototype, weaker integration, harder to distribute. The tool layer is designed so it can later be exposed through MCP as well.
- **Rhino.Compute:** a REST geometry server. Windows only and licensing heavy, suited to a hosted web product rather than a desktop copilot.
