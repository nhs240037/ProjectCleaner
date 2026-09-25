# Product Overview

## Product Purpose

ProjectCleaner is a Windows Forms desktop application that cleans up unnecessary files and folders from Visual Studio, Unity, and .NET development projects. It automates the detection and removal of build artifacts, IntelliSense caches, and other development-generated files that are not needed in version control or production deployments.

## Target Users

Primary users are software developers who work with Visual Studio, Unity, and .NET projects. These developers:
- Wastefully commit large binary files or caches to version control
- Accidentally upload build artifacts to deployment targets
- Need to reclaim disk space from accumulated development debris
- Want consistent project hygiene across their development environment

## Key Features

1. **Multi-Project Type Detection**: Automatically identifies and cleans Visual Studio (`.vs` folder), Unity (`ProjectSettings/ProjectVersion.txt`), and .NET (`bin/`, `obj/`, `.sln`) projects
2. **Batch Root Management**: Users can add, remove, and select multiple project roots for cleaning
3. **Interactive Progress Display**: Shows real-time scanning progress with a pseudo-progress bar during operations
4. **Selective Cleanup**: Users can review detected files/folders and choose which ones to delete
5. **Persistent Configuration**: Remembers selected project roots between sessions via `%LOCALAPPDATA%\ProjectCleaner\roots.json`
6. **Automatic Updates**: Checks GitHub for new releases and supports self-updating

## Business Objectives

- Minimize development environment clutter and disk usage
- Reduce accidental commits of generated files
- Improve onboarding experience for new team members
- Ensure clean project handoffs between developers

## Success Metrics

- Average project cleanup time < 10 seconds for scan + delete
- 99%+ accurate detection of project types and relevant files
- < 5% false positive rate on file/folder detection
- User retention > 80% after 30 days

## Product Principles

1. **Conservative by Default**: Only suggest files that are known to be safe to delete, never require aggressive cleanup policies
2. **User Control**: Always show what will be deleted and require explicit confirmation
3. **Performance Focused**: Use parallel scanning but avoid UI blocking
4. **Reliability First**: Never attempt to delete files that don't exist or are already removed

## Monitoring & Visibility

- **Dashboard Type**: Desktop app with console logging
- **Real-time Updates**: Immediate UI feedback during scanning
- **Key Metrics Displayed**: Progress percentage, file/folder counts, operation success/failure
- **Sharing Capabilities**: Log output can be copied to clipboard for sharing issues

## Future Vision

ProjectCleaner will evolve into a cross-platform CLI tool with optional GUI, supporting additional IDEs and build systems.

### Potential Enhancements
- **Remote Access**: Clean remote development environments
- **Analytics**: Track most commonly cleaned files to improve detection
- **Collaboration**: Share cleanup profiles across team members