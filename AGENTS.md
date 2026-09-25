# ProjectCleaner

## Project shape
- Single Windows Forms app targeting `net8.0-windows`; there is one project file (`ProjectCleaner.csproj`) and one solution reference (`ProjectCleaner.slnx`).
- `Program.cs` is the entrypoint. `MainForm.cs` builds the UI in code; there is no designer partial.
- `Cleaner.cs` owns scan/delete logic. `UpdateChecker.cs` owns GitHub release checks and self-update behavior.
- The app stores selected roots in `%LOCALAPPDATA%\ProjectCleaner\roots.json`.

## Verified commands
- Restore: `dotnet restore ProjectCleaner.csproj`
- Build: `dotnet build ProjectCleaner.csproj --no-restore -c Release`
- PR CI uses Windows + .NET `8.0.x` and runs only restore/build; no test suite is configured.
- Super-Linter runs on PRs for C#, YAML, and Markdown.

## Important behavior
- Scan order is Visual Studio -> Unity -> .NET.
- Unity projects are detected by `ProjectSettings/ProjectVersion.txt`; .NET cleanup is only considered when a `.sln` exists at the selected root.
- `Cleaner.TryDelete` recursively deletes folders. Test cleanup paths against disposable fixtures only; never target the repo root.
- Extra runtime assemblies are resolved from a `lib` subdirectory beside the executable.
- The updater expects a `ProjectCleaner-Update.zip` asset containing `ProjectCleaner.dll` and `ProjectCleaner.deps.json`.

## Release workflow
- PR titles use `feat:`, `fix:`, `chore:`, `build:`, `ci:`, or `docs:` for automatic labels.
- Merged PRs need a `release:major`, `release:minor`, or `release:patch` label to create a tag; `dev`, `alpha`, or `beta` labels select prerelease channels.
- Release build commands (example from the automated release workflow):
  - `dotnet restore ProjectCleaner.csproj -r win-x64`
  - `dotnet publish ProjectCleaner.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=false -o ./publish_raw`
- Publish steps package `Setup.hta` into the full zip and create a separate `ProjectCleaner-Update.zip` asset containing `ProjectCleaner.dll` and `ProjectCleaner.deps.json`.

## Working conventions
- Keep changes scoped to the single app project unless a release workflow change is explicitly required.
- Re-run the verified restore/build commands after edits; there is no local lint/typecheck/test shortcut beyond CI.


- All tasks are performed according to spec-workflow-guide.
