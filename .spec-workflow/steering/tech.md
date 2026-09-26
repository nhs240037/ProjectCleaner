# Technology Stack

## Project Type

Windows Forms desktop application targeting .NET 8.0 for Windows platforms. Single-file release build distributed as a self-contained executable.

## Core Technologies

### Primary Language(s)
- **Language**: C# 12.0
- **Runtime/Compiler**: .NET 8.0
- **Language-specific tools**: dotnet CLI, Visual Studio 2022+

### Key Dependencies/Libraries
- **System.Windows.Forms**: UI framework for WinForms interface
- **System.Text.Json**: Configuration file serialization (roots.json)
- **System.Drawing.Common**: For Color and Size types (Windows Forms legacy)

### Application Architecture
- **Architecture**: Monolithic Windows Forms application
- Single-form UI with programmatically built controls (no designer files)
- UI logic and business logic co-located but separated by component (MainForm, Cleaner, UpdateChecker)

### Data Storage
- **Primary storage**: JSON files in `%LOCALAPPDATA%\ProjectCleaner\` directory
- **Caching**: None (stateless scanning operations)
- **Data formats**: JSON for configuration, filesystem for project cleanup

### External Integrations
- **APIs**: GitHub REST API for release checks and update downloads
- **Protocols**: HTTP/HTTPS for GitHub API and update downloads
- **Authentication**: None (public GitHub releases)

## Development Environment

### Build & Development Tools
- **Build System**: MSBuild via dotnet CLI
- **Package Management**: NuGet (implicit via .csproj)
- **Development workflow**: `dotnet build`, `dotnet publish`, Visual Studio debugger

### Code Quality Tools
- **Static Analysis**: C# compiler warnings/errors, Super-Linter CI
- **Formatting**: dotnet format (if configured)
- **Testing Framework**: None configured (CI only verifies restore/build)
- **Documentation**: XML doc comments, this steering document

### Version Control & Collaboration
- **VCS**: Git
- **Branching Strategy**: GitHub Flow (feature branches, PR reviews)
- **Code Review Process**: GitHub pull requests with required reviews

### Dashboard Development
- N/A (desktop application only)

## Deployment & Distribution
- **Target Platform(s)**: Windows x64 (self-contained)
- **Distribution Method**: Zip file download via GitHub Releases
- **Installation Requirements**: Windows 10+ (modern Win32 API support)
- **Update Mechanism**: Application downloads `ProjectCleaner-Update.zip` asset and replaces itself

## Technical Requirements & Constraints

### Performance Requirements
- Compile time: < 5 seconds
- Release build: < 100MB output
- Startup time: < 2 seconds

### Compatibility Requirements  
- **Platform Support**: Windows x64, .NET 8.0
- **Dependency Versions**: .NET 8.0 LTS
- **Standards Compliance**: Semantic versioning for releases

### Security & Compliance
- **Security Requirements**: No data collection, no network communication except update checks
- **Compliance Standards**: None (local application)
- **Threat Model**: Update download integrity, arbitrary file delete prevention

### Scalability & Reliability
- **Expected Load**: Single-project operations
- **Availability Requirements**: High (must not hang during operations)
- **Growth Projections**: Modular component design allows for future feature expansion

## Technical Decisions & Rationale

### Decision Log
1. **WinForms over WPF**: Chosen for simpler deployment and smaller runtime footprint; simpler code structure with programatically-built UI
2. **Self-contained publish**: Ensures users don't need .NET runtime; larger but simpler distribution
3. **Single-form UI**: Reduces complexity and improves performance; matches modern desktop app patterns
4. **GitHub Releases for updates**: Free, reliable, no infrastructure to maintain
5. **JSON configuration**: Human-readable, easy to debug, compatible with all platforms

## Known Limitations

- **Limitation 1**: No test suite exists; relies on CI build verification only
- **Why it exists**: Small utility project, focused on functionality over test coverage
- **Impact**: Risk of regressions in edge cases; manual testing required for new features

- **Limitation 2**: UI is built programmatically (no designer file)
- **Why it exists**: Project convention stated "no designer partial"
- **Impact**: More verbose code, harder to maintain large UI changes