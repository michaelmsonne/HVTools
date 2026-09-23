# Contributing to HVTools

First off, thank you for considering contributing to HVTools! It's people like you that make HVTools such a great tool for the Hyper-V community.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Enhancements](#suggesting-enhancements)
  - [Code Contributions](#code-contributions)
- [Development Setup](#development-setup)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Commit Message Guidelines](#commit-message-guidelines)
- [Community](#community)

## Code of Conduct

This project and everyone participating in it is governed by the [HVTools Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainer.

## How Can I Contribute?

### Reporting Bugs

Bugs are tracked as [GitHub issues](https://github.com/michaelmsonne/HVTools/issues). Before creating a bug report, please check the existing issues to avoid duplicates.

When you create a bug report, please include as many details as possible:

- **Use a clear and descriptive title** for the issue
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples** to demonstrate the steps
- **Describe the behavior you observed** and what you expected to see
- **Include screenshots or animated GIFs** if applicable
- **Include your environment details**:
  - Windows version (10, 11, Server 2022, etc.)
  - .NET version
  - HVTools version
  - Hyper-V version
  - Cluster configuration (if applicable)
- **Include relevant log files** from the application

Use our [Bug Report Template](.github/ISSUE_TEMPLATE/01_BUG_REPORT.md) when creating issues.

### Suggesting Enhancements

Enhancement suggestions are also tracked as [GitHub issues](https://github.com/michaelmsonne/HVTools/issues).

When creating an enhancement suggestion, please include:

- **Use a clear and descriptive title**
- **Provide a detailed description** of the suggested enhancement
- **Explain why this enhancement would be useful** to most HVTools users
- **List any similar features** in other tools (like RVTools for VMware)
- **Include mockups or examples** if applicable

Use our [Feature Request Template](.github/ISSUE_TEMPLATE/02_FEATURE_REQUEST.md) when suggesting enhancements.

### Code Contributions

We welcome code contributions! Here's how you can help:

HVTools is MIT licensed, but please keep improvements in this repository so the community can continue building the most complete version together.

#### Good First Issues

Look for issues labeled `good first issue` - these are perfect for newcomers to the project.

#### Areas We Need Help

- **PowerShell cmdlet improvements** - Optimizing Hyper-V queries
- **UI enhancements** - Improving the user interface and experience
- **Export formats** - Adding new export capabilities
- **Documentation** - Improving guides, examples, and code comments
- **Testing** - Writing unit tests and integration tests
- **Bug fixes** - Resolving reported issues
- **Performance optimization** - Making the tool faster and more efficient

## Development Setup

### Prerequisites

1. **Visual Studio 2022** (17.12.0 or later)
2. **.NET 10.0 SDK**
3. **Git** for version control
4. **Hyper-V** environment for testing (local or remote)

### Setting Up Your Development Environment

1. **Fork the repository** on GitHub
   
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/HVTools.git
   cd HVTools
   ```

3. **Add the upstream remote**:
   ```bash
   git remote add upstream https://github.com/michaelmsonne/HVTools.git
   ```

4. **Open the solution** in Visual Studio:
   ```bash
   cd src
   start HVTools.sln
   ```

5. **Restore NuGet packages**:
   - Visual Studio will automatically restore packages
   - Or manually: `dotnet restore`

6. **Build the solution**:
   - Press `Ctrl+Shift+B` in Visual Studio
   - Or use: `dotnet build`

7. **Run the application**:
   - Press `F5` to run with debugging
   - Ensure you have Hyper-V access (local or remote) for testing

### Running Tests

Currently, the project is setting up its test infrastructure. Check back soon for testing guidelines.

```bash
dotnet test
```

## Pull Request Process

1. **Create a new branch** for your work:
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/your-bug-fix
   ```

2. **Make your changes**:
   - Follow the [Coding Standards](#coding-standards)
   - Write meaningful commit messages
   - Keep changes focused and atomic
   - Add comments for complex logic

3. **Test your changes**:
   - Test all affected functionality
   - Test on different Hyper-V configurations if possible
   - Verify no existing features are broken

4. **Update documentation**:
   - Update README.md if needed
   - Update code comments
   - Update CHANGELOG.md with your changes

5. **Commit your changes**:
   ```bash
   git add .
   git commit -m "feat: add new feature description"
   ```
   See [Commit Message Guidelines](#commit-message-guidelines) below.

6. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create a Pull Request**:
   - Go to the [HVTools repository](https://github.com/michaelmsonne/HVTools)
   - Click "New Pull Request"
   - Select your branch
   - Fill out the PR template with all relevant information
   - Link any related issues

8. **Wait for review**:
   - Maintainers will review your PR
   - Address any feedback or requested changes
   - Once approved, your PR will be merged!

### Pull Request Requirements

- ? Code follows project coding standards
- ? All tests pass (when test infrastructure is available)
- ? No merge conflicts with main branch
- ? Documentation updated if needed
- ? Commit messages follow guidelines
- ? PR description clearly explains the changes
- ? Screenshots included for UI changes

## Coding Standards

### C# Code Style

- **Follow Microsoft's C# Coding Conventions**: [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **Use meaningful variable and method names**
- **Keep methods focused and small** (single responsibility principle)
- **Use async/await** for all asynchronous operations
- **Dispose of resources properly** (use `using` statements)
- **Handle exceptions appropriately** - don't swallow exceptions without logging

### Code Formatting

```csharp
// Use PascalCase for class names and public members
public class VmDetailsInfo
{
    // Use PascalCase for properties
    public string VmName { get; set; }
    
    // Use camelCase for private fields with underscore prefix
    private readonly string _hostName;
    
    // Use meaningful method names
    public async Task<List<VmInfo>> GetVirtualMachinesAsync()
    {
        // Method implementation
    }
}
```

### PowerShell Integration

- **Use try-catch blocks** around PowerShell invocations
- **Properly dispose** of PowerShell instances
- **Log PowerShell errors** with appropriate context
- **Use PowerShell runspaces** efficiently

### Comments

- **Add XML documentation** for public classes and methods:
  ```csharp
  /// <summary>
  /// Retrieves detailed information about a virtual machine.
  /// </summary>
  /// <param name="vmName">The name of the virtual machine.</param>
  /// <returns>A VmDetailsInfo object containing VM details.</returns>
  public VmDetailsInfo GetVmDetails(string vmName)
  ```
- **Use inline comments** sparingly and only for complex logic
- **Avoid obvious comments** - code should be self-documenting when possible

### UI Guidelines

- **Follow existing UI patterns** in the application
- **Use consistent spacing and alignment**
- **Ensure accessibility** (keyboard navigation, screen readers)
- **Test on different DPI settings**
- **Provide meaningful error messages** to users

## Commit Message Guidelines

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification.

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Code style changes (formatting, missing semi-colons, etc.)
- **refactor**: Code refactoring without changing functionality
- **perf**: Performance improvements
- **test**: Adding or updating tests
- **chore**: Build process or auxiliary tool changes
- **ci**: CI/CD configuration changes

### Examples

```bash
feat(vm-inventory): add support for exporting to Excel format

- Added ExcelExporter class
- Updated export dialog to include Excel option
- Added ClosedXML dependency

Closes #123
```

```bash
fix(cluster): resolve cluster node detection issue

Fixed a bug where cluster nodes were not being detected correctly
when connecting via cluster name instead of node name.

Fixes #456
```

```bash
docs(readme): update installation instructions

- Added troubleshooting section
- Updated prerequisites
- Added screenshots
```

### Scope Examples

- `vm-inventory`
- `cluster`
- `export`
- `ui`
- `auth`
- `powershell`
- `logging`

## Community

### Getting Help

- **GitHub Discussions**: [Ask questions and discuss ideas](https://github.com/michaelmsonne/HVTools/discussions)
- **Issues**: [Report bugs or request features](https://github.com/michaelmsonne/HVTools/issues)
- **Documentation**: [Visit hvtools.app](https://hvtools.app)

### Staying Updated

- **Watch the repository** to get notifications of new issues and PRs
- **Star the repository** if you find it useful
- **Follow releases** to stay informed of new versions

### Recognition

Contributors will be:
- Listed in release notes
- Mentioned in the CHANGELOG.md
- Added to the project's contributors page
- Acknowledged in the project README (for significant contributions)

## Questions?

Don't hesitate to ask questions! You can:
- Open a [GitHub Discussion](https://github.com/michaelmsonne/HVTools/discussions)
- Comment on an existing issue
- Reach out via the contact methods in the README

---

Thank you for contributing to HVTools! ??

Your contributions help make Hyper-V management easier for administrators around the world.
