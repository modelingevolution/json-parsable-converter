# Deployment Guide

This document outlines the deployment process for the JsonParsableConverter NuGet package.

## Prerequisites

### 1. NuGet API Key
You need a NuGet.org API key to publish packages:

1. Go to [nuget.org](https://www.nuget.org) and sign in
2. Click your username → "API Keys"
3. Create a new API key with:
   - **Key Name**: `JsonParsableConverter-GitHub-Actions`
   - **Package Owner**: Your username/organization
   - **Scopes**: `Push new packages and package versions`
   - **Packages**: `ModelingEvolution.JsonParsableConverter` (or leave empty for all packages)
   - **Glob Pattern**: `*` (or specific pattern if needed)

### 2. GitHub Repository Secrets
Add the NuGet API key as a GitHub secret:

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add:
   - **Name**: `NUGET_API_KEY`
   - **Secret**: Your NuGet API key from step 1

## Deployment Methods

### Method 1: Automatic Deployment via Git Tags (Recommended)

This is the standard approach for releasing new versions:

```bash
# Navigate to project directory
cd /path/to/json-parsable-converter

# Ensure all changes are committed
git add .
git commit -m "Prepare for v1.0.0 release"
git push origin main

# Create and push a version tag
git tag v1.0.0
git push origin v1.0.0
```

**What happens next:**
1. GitHub Actions detects the new tag
2. Runs the "Publish to NuGet" workflow
3. Builds and tests the project
4. Creates NuGet packages with version 1.0.0
5. Publishes to NuGet.org
6. Creates a GitHub release with artifacts

### Method 2: Manual Deployment via GitHub Actions

For ad-hoc releases or testing:

1. Go to your GitHub repository
2. Click **Actions** tab
3. Select **"Publish to NuGet"** workflow
4. Click **"Run workflow"**
5. Enter the version number (e.g., `1.0.1`)
6. Click **"Run workflow"**

## Version Management

### Version Format
Use [Semantic Versioning](https://semver.org/):
- **Major.Minor.Patch** (e.g., 1.0.0, 1.2.3, 2.0.0)
- **Pre-release**: 1.0.0-beta, 1.0.0-alpha.1, 1.0.0-rc.1

### Updating Version
The version is automatically determined from the Git tag:
- Tag `v1.0.0` → NuGet version `1.0.0`
- Tag `v1.2.3-beta` → NuGet version `1.2.3-beta`

### Version in Project File
The project file uses a base version that gets overridden during deployment:
```xml
<Version>1.0.0</Version>
```
This is used for local development builds. The actual published version comes from the Git tag.

## Workflow Files

### 1. Publish Workflow (`.github/workflows/publish.yml`)
- **Triggers**: Git tags matching `v*` pattern, manual dispatch
- **Purpose**: Build, test, pack, and publish to NuGet.org
- **Artifacts**: NuGet packages (.nupkg) and symbol packages (.snupkg)

### 2. CI Workflow (`.github/workflows/ci.yml`)
- **Triggers**: Pull requests, pushes to main/master
- **Purpose**: Continuous integration testing
- **Platforms**: Ubuntu, Windows, macOS

## Release Process

### 1. Prepare Release
```bash
# Update CHANGELOG.md if you have one
# Update any version references in documentation
# Ensure all tests pass locally
dotnet test

# Commit all changes
git add .
git commit -m "Prepare for release v1.0.0"
git push origin main
```

### 2. Create Release Tag
```bash
# Create annotated tag with release notes
git tag -a v1.0.0 -m "Release v1.0.0

- Initial release of JsonParsableConverter
- Support for IParsable<T> types
- Attribute-based JSON conversion
- Comprehensive test coverage"

# Push the tag to trigger deployment
git push origin v1.0.0
```

### 3. Monitor Deployment
1. Go to **Actions** tab in GitHub
2. Watch the "Publish to NuGet" workflow execution
3. Check for any errors in the logs
4. Verify the package appears on [nuget.org](https://www.nuget.org/packages/ModelingEvolution.JsonParsableConverter/)

### 4. Verify Release
```bash
# Test installing the published package
dotnet new console -n TestInstall
cd TestInstall
dotnet add package ModelingEvolution.JsonParsableConverter --version 1.0.0
dotnet build
```

## Troubleshooting

### Common Issues

#### 1. NuGet Push Fails
- **Error**: "The API key provided is invalid"
- **Solution**: Check that `NUGET_API_KEY` secret is correctly set

#### 2. Version Already Exists
- **Error**: "A package with this version already exists"
- **Solution**: Increment the version number and create a new tag

#### 3. Tests Fail in CI
- **Error**: Build or test failures in GitHub Actions
- **Solution**: Run tests locally first, fix issues, then commit and tag

#### 4. Missing Dependencies
- **Error**: "Unable to restore packages"
- **Solution**: Ensure all PackageReference entries are valid

### Manual Package Creation (For Testing)

```bash
# Build and pack locally
dotnet build --configuration Release
dotnet pack src/JsonParsableConverter/JsonParsableConverter.csproj --configuration Release --output ./artifacts

# Test the package locally
dotnet nuget push ./artifacts/*.nupkg --source ./local-feed --skip-duplicate
```

## Security Considerations

1. **API Key Security**: Never commit NuGet API keys to source control
2. **Scope Limitation**: Use API keys with minimal required scopes
3. **Regular Rotation**: Rotate API keys periodically
4. **Monitoring**: Monitor package downloads and usage

## Package Information

- **Package ID**: `ModelingEvolution.JsonParsableConverter`
- **Target Framework**: .NET 9.0
- **License**: MIT
- **Repository**: https://github.com/modelingevolution/json-parsable-converter
- **NuGet Page**: https://www.nuget.org/packages/ModelingEvolution.JsonParsableConverter/

## Support

For deployment issues:
1. Check GitHub Actions logs
2. Verify secrets are correctly configured  
3. Ensure NuGet API key has correct permissions
4. Contact repository maintainers if needed