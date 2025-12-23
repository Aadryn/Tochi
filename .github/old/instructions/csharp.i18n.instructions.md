---
applyTo: "**/*.cs,**/*.razor,**/*.css,**/*.js"
---

# C# Localization Guidelines

These directives apply to every change that affects localization across any portal.

## General Guidelines

- Extract every string for internationalization (i18n).
- Manage localized strings using `.resx` resource files.
- Store all localization assets using UTF-8 encoding for files and data exchange.

## Resource Organization

- Maintain a single consolidated `.resx` file per project (e.g., `GroupeAdp.GenAi.Hostings.WebApp.Default.Endpoint.Resources`) and centralize all project-level strings there.

## Localization Usage

- Inject `IStringLocalizer<T>` or `IViewLocalizer` through dependency injection; never instantiate `ResourceManager` manually.
- Centralize cross-feature or validation strings inside shared resource classes (e.g., `SharedResources`) to avoid duplication.
- Do not introduce hard-coded user-facing strings in code or Razor markup—always reference resource keys.

## Culture Configuration

- Configure `fr-FR` as the default and fallback culture for every hosting endpoint.
- Register `SupportedCultures` and `SupportedUICultures` with at least `fr-FR` and `en-US`, and explicitly include any additional cultures the product exposes.
- Provide a resilient fallback path so that unsupported cultures degrade gracefully to `fr-FR` rather than failing.

## Localization Pipeline

- Register services with `AddLocalization`, `AddViewLocalization`, and `AddDataAnnotationsLocalization` in each hosting startup to ensure UI, validation, and backend resources are wired consistently.
- Configure `RequestLocalizationOptions` in one place per hosting project and call `UseRequestLocalization` before routing middleware.
- Persist supported culture lists in configuration (e.g., `appsettings`) and read them during startup to avoid code duplication.
- Enable localization logging (e.g., missing-resource logging) in development environments to surface gaps early.

## Formatting and Persistence

- Use `CultureInfo.CurrentCulture` and `CultureInfo.CurrentUICulture` when formatting dates, numbers, timespans, and currencies for end users.
- Use `CultureInfo.InvariantCulture` for persistence, serialization, logging, and telemetry to guarantee deterministic representations.

## Validation and User Messages

- Externalize `DisplayAttribute`, `RequiredAttribute`, `StringLengthAttribute`, and similar metadata into shared resource classes and reference them via `ResourceType` and `ErrorMessageResourceName`.
- For parameterized strings, rely on indexed placeholders (`{0}`, `{1}`) and pass typed values through `string.Format` or the localizer indexer so word order can vary per culture.

## Accessibility and UX

- Design layouts to accommodate text expansion and contraction; avoid fixed-width elements that truncate translated strings.
- Validate that RTL languages (when introduced) render correctly by leveraging CSS logical properties and mirroring layouts where needed.
- Use localization frameworks that support pluralization and gender-specific forms; never concatenate raw strings to build localized sentences.

## Quality Enforcement

- Add automated tests (for example `ResourceKeyTests`) or Roslyn analyzers that verify required resource keys exist for each supported culture and that `.resx` files stay synchronized.
- Fail tests when placeholder counts or formats differ between cultures to prevent runtime formatting errors.
- Integrate localization checks into CI (dotnet test + analyzer rules) so regressions are blocked before merging.