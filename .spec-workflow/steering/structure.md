# Project Structure

## Directory Organization

```
ProjectCleaner/
├── Program.cs                 # Application entrypoint and assembly resolver
├── MainForm.cs                # Main WinForms UI (programatically built)
├── Cleaner.cs                 # Scan/delete business logic
├── UpdateChecker.cs           # GitHub release checks and self-update behavior
├── FolderPicker.cs            # Folder selection dialog helper
├── PseudoProgressBar.cs       # Reusable progress bar component
├── ProjectCleaner.csproj      # Project file
├── ProjectCleaner.sln         # Solution file
├── ProjectCleaner.slnx        # Solution file (new format)
├── Setup.hta                  # Installer script
├── ipchDeleter.bat            # Legacy batch script
└── AGENTS.md                  # Project guidance
```

## Naming Conventions

### Files
- **Components/Modules**: PascalCase (e.g., `MainForm.cs`, `Cleaner.cs`)
- **Services/Handlers**: PascalCase (e.g., `UpdateChecker.cs`)
- **Utilities/Helpers**: PascalCase (e.g., `FolderPicker.cs`)
- **Tests**: N/A (no test files exist)

### Code
- **Classes/Types**: PascalCase (e.g., `Candidate`, `AppState`)
- **Methods/Functions**: PascalCase (e.g., `ScanRoots`, `TryDelete`)
- **Constants**: PascalCase (e.g., `VersionChannel`)
- **Variables**: camelCase (e.g., `_rootsList`, `currentVersion`)
- **Private fields**: camelCase with leading underscore (e.g., `_progressTimer`)

## Import Patterns

### Import Order
1. External dependencies (System.*)
2. Internal modules (ProjectCleaner namespace)
3. Relative imports (not used in this project)

### Module/Package Organization
```
- All files are in the `ProjectCleaner` namespace
- No sub-namespaces are used
- Files are organized by responsibility (UI, business logic, update logic)
```

## Code Structure Patterns

### Module/Class Organization
```
1. Using statements
2. Namespace declaration
3. Class/struct/enum/record definitions
4. Public methods
5. Private methods
6. Helper functions
```

### Function/Method Organization
- Input validation first
- Core logic in the middle
- Error handling throughout
- Clear return points

### File Organization Principles
- One class per file (except small helper classes)
- Related functionality grouped together
- Public API at the top/bottom
- Implementation details hidden

## Code Organization Principles

1. **Single Responsibility**: Each file should have one clear purpose
2. **Modularity**: Code should be organized into reusable modules
3. **Testability**: Structure code to be easily testable
4. **Consistency**: Follow patterns established in the codebase

## Module Boundaries
- **UI (MainForm)**: Handles user interaction and layout
- **Business Logic (Cleaner)**: Handles scanning and deletion
- **Update Logic (UpdateChecker)**: Handles GitHub release checks and updates
- **Entry Point (Program)**: Handles application startup and assembly resolution
- **Dependencies Direction**: UI can depend on business logic, but not vice versa

## Code Size Guidelines
- **File size**: No hard limit; keep files focused
- **Function/Method size**: Keep methods under ~50 lines where possible
- **Class/Module complexity**: Prefer simple, readable code
- **Nesting depth**: Maximum 3 levels where possible

## Dashboard/Monitoring Structure
N/A (desktop application only)

## Documentation Standards
- All public APIs must have documentation
- Complex logic should include inline comments
- README files for major modules
- Follow language-specific documentation conventions
