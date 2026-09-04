# Repository Guidelines

## Project Structure & Module Organization

Shabakat is a Windows-only .NET 10 MAUI Blazor Hybrid application. Keep business entities and enums in `Domain/`; DTOs, interfaces, models, helpers, and use-case services in `Application/`; EF Core repositories, migrations, and invoice templates in `Infrastructure/`; and thin Razor UI components in `Components/`. Platform bootstrap code lives in `Platforms/`, while images, fonts, and raw assets belong in `Resources/`. Edit Tailwind input only in `Styles/app.css`; `wwwroot/app.css` is generated. The optional backup worker is isolated under `cloudflare/backup-worker/`.

## Build, Test, and Development Commands

Run commands from the repository root:

```powershell
npm ci                         # install pinned frontend tooling
npm run tw:watch               # rebuild Tailwind while editing UI
dotnet restore Shabakat.slnx   # restore NuGet dependencies
dotnet build Shabakat.slnx     # compile the Windows application
dotnet run --project Shabakat.csproj
npm run tw:build               # produce minified wwwroot/app.css
```

Use `dotnet build Shabakat.slnx -c Release` before release-oriented changes.

## Coding Style & Naming Conventions

Use four-space indentation in C# and Razor, nullable reference types, file-scoped namespaces, and async methods ending in `Async`. Use PascalCase for public types and members, camelCase for locals, and `_camelCase` for private fields. Name service contracts `I{Name}Service`, implementations `{Name}Service`, DTOs by purpose (`Request`, `Response`), and Razor components in PascalCase. Preserve layer boundaries and register dependencies in `Application/Helper/DIContainer.cs`. Do not add mobile targets, scoped `.razor.css`, extra CSS frameworks, or hand-edit generated CSS.

## Testing Guidelines

There is no automated test project; `npm test` is a placeholder. Compile in Release and manually exercise affected Blazor flows. Name future projects `{Feature}.Tests` and methods `Method_WhenCondition_ReturnsResult`.

## Commit & Pull Request Guidelines

Agents must never stage files or create commits; all Git commits are performed manually by the repository owner. When asked for a message, suggest a one-line Conventional Commit such as `fix: preserve invoice due date in list view`. Keep summaries imperative, lowercase, and without a period. Pull requests should explain behavior changes, validation, linked issues, and UI screenshots. Call out migrations, configuration changes, and compatibility risks.

## Security & Agent Instructions

Never commit `appsettings.Local.json`, Cloudflare credentials, databases, or build artifacts.

Agents must read `.cursor/rules/shabakat-stack.mdc` and `.cursor/rules/commit-messages.mdc` before suggesting commits. Before applicable work, load the matching project skill:

- `.cursor/skills/blazor-expert/SKILL.md` for Razor components, lifecycle, state, routing, forms, and JS interop.
- `.cursor/skills/maui-blazor-development/SKILL.md` for MAUI integration, DI, navigation, and platform services.
- `.cursor/skills/maui-ai-debugging/SKILL.md` for building, running, inspecting, or debugging the live Windows app.
