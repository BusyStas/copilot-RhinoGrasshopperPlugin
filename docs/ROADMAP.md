# Roadmap

## Phase 0: Prove the loop
- Python script in Rhino's script editor that calls Claude with a few tools
- Goal: learn how well the model handles real modeling tasks and which tools matter most
- Exit criterion: a handful of realistic requests complete correctly

## Phase 1: Plugin skeleton (code written, awaiting first build)
- RhinoCommon plugin with a docked Eto panel
- Claude client, agent loop, one undo record per request
- Read tools and a small set of edit tools (done: get_document_info, get_selection, list_layers, create_box)
- Fake-document test harness so the loop is testable without Rhino
- Next: run `dotnet build` and `dotnet test`, fix first-build errors, load the plugin in Rhino 8 and try the tools

## Phase 2: Useful tool set
- Full initial tool catalog from `docs/ARCHITECTURE.md`
- Viewport capture fed back to the model
- Confirmation prompts and auto-approve setting
- Cancel and step limits

## Phase 3: Grasshopper
- Tools to add components, wire them, set sliders, solve, and bake
- Searchable catalog of components and parameters
- Canvas layout so generated definitions are readable

## Phase 4: Distribution
- Yak package and Package Manager listing
- Settings UI, onboarding, error reporting
- Decide on backend model: user's own API key versus a hosted proxy

## Open questions
- Who is the audience: personal use, an internal team, or a product for sale?
- Does the copilot also get exposed as an MCP server so other clients can drive Rhino?
- How much of the Rhino API documentation should be fed to the model, and how (retrieval versus system prompt)?
