# Contributing to NuciWeb.HTTP

Thank you for your interest in contributing to NuciWeb.HTTP! This document outlines the process and standards for contributions.

## 📑 Table of Contents

- [Code of Conduct](#-code-of-conduct)
- [Getting Started](#-getting-started)
- [Development Workflow](#-development-workflow)
- [Code Standards](#-code-standards)
- [Testing](#-testing)
- [Pull Request Process](#-pull-request-process)
- [Issue Reporting](#-issue-reporting)
- [License](#-license)

## 🤝 Code of Conduct

By participating in this project, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md). Please report unacceptable behavior to the maintainers.

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git
- A code editor (VS Code, Visual Studio, Rider, etc.)

### Setup

```bash
# Clone the repository
git clone https://github.com/hmlendea/nuciweb.http.git
cd nuciweb.http

# Restore dependencies
dotnet restore NuciWeb.HTTP.slnx

# Build the solution
dotnet build NuciWeb.HTTP.slnx

# Run tests
dotnet test NuciWeb.HTTP.slnx
```

## 🔄 Development Workflow

1. **Fork** the repository
2. **Create a branch** from `main` with a descriptive name:
   - `feature/description` for new features
   - `fix/description` for bug fixes
   - `docs/description` for documentation updates
   - `refactor/description` for code improvements
3. **Make changes** following the code standards below
4. **Run tests** to ensure nothing is broken
5. **Submit a pull request** against the `main` branch

## 📝 Code Standards

### C# Style

- Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use the project's `.editorconfig` settings (enforced by build)
- Target .NET 10 (`net10.0`)
- Use nullable reference types (`<Nullable>enable</Nullable>`)
- Disable implicit usings (`<ImplicitUsings>disable</ImplicitUsings>`)

### Naming Conventions

| Element | Convention |
|---------|------------|
| Classes, interfaces, enums, records | `PascalCase` |
| Methods, properties, events | `PascalCase` |
| Parameters, local variables | `camelCase` |
| Private fields | `_camelCase` |
| Constants | `PascalCase` |
| Interfaces | Prefix with `I` (e.g., `IUserAgentFetcher`) |

### File Organization

- One type per file (exceptions: small related types)
- File name matches the type name
- Use `NuciWeb.HTTP` namespace for the main library
- Use `NuciWeb.HTTP.UnitTests` namespace for tests

### Documentation

- Public APIs must have XML documentation comments
- Use `<summary>`, `<param>`, `<returns>`, `<exception>` tags
- Keep comments concise and accurate

### Dependencies

- Minimize external dependencies
- Prefer framework APIs over third-party packages
- Update `NuciWeb.HTTP.csproj` for new package references
- Run `dotnet restore` after changes

## ✅ Testing

### Test Framework

- **NUnit 4.x** for unit tests
- **Moq** for mocking
- **Microsoft.NET.Test.Sdk** for test runner

### Running Tests

```bash
# Run all tests
dotnet test NuciWeb.HTTP.slnx

# Run with coverage
dotnet test NuciWeb.HTTP.slnx --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test NuciWeb.HTTP.slnx --filter "FullyQualifiedName~HttpClientCreatorTests"

# Run specific test method
dotnet test NuciWeb.HTTP.slnx --filter "FullyQualifiedName~HttpClientCreatorTests.CreateAsync_ReturnsHttpClient"
```

### Test Standards

- Place tests in `NuciWeb.HTTP.UnitTests` project
- Mirror the source project structure
- Name test classes: `{TypeName}Tests`
- Name test methods: `{MethodName}_{Scenario}_{ExpectedResult}`
- Use `Arrange-Act-Assert` pattern
- Test both success and failure paths
- Mock external dependencies (HTTP, DNS, network)

### Test Categories

| Category | Description |
|----------|-------------|
| Unit | Fast, isolated, no external dependencies |
| Integration | Requires network, external services |

Mark integration tests with `[Category("Integration")]` and exclude from CI by default.

## 🔀 Pull Request Process

### Before Submitting

- [ ] All tests pass (`dotnet test`)
- [ ] Code builds without warnings (`dotnet build`)
- [ ] New code has XML documentation
- [ ] Changes are focused and minimal
- [ ] Commit messages are clear and descriptive

### PR Requirements

1. **Title**: Clear, concise summary (e.g., "Add timeout configuration to HttpClientCreator")
2. **Description**: Explain the change, motivation, and testing performed
3. **Linked Issues**: Reference related issues (e.g., "Fixes #123")
4. **Breaking Changes**: Clearly mark if any

### Review Process

1. Automated checks must pass (build, tests)
2. At least one maintainer review required
3. Address all review comments
4. Maintainers will merge after approval

### Commit Messages

Follow conventional commits:

```
type(scope): description

[optional body]

[optional footer]
```

Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `build`, `ci`

Examples:
- `feat(http): add custom User-Agent provider support`
- `fix(network): handle DNS resolution timeout`
- `docs(readme): update installation instructions`

## 🐛 Issue Reporting

### Bug Reports

Include:
- .NET version and OS
- Steps to reproduce
- Expected vs actual behavior
- Relevant code snippets
- Stack traces (if applicable)

### Feature Requests

Include:
- Use case and motivation
- Proposed API design (if applicable)
- Alternatives considered
- Willingness to implement

### Security Issues

**Do not** report security vulnerabilities publicly. See [SECURITY.md](SECURITY.md) for the disclosure process.

## 📄 License

By contributing, you agree that your contributions will be licensed under the [GPL-3.0-or-later](LICENSE) license.

---

*For questions about contributing, open a [discussion](https://github.com/hmlendea/nuciweb.http/discussions) or contact the maintainers.*