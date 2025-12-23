---
applyTo: "docs/**/*.md"
---
# Documentation Rules

## Voice
- Write in present tense and active voice.
- Address the reader as you.
- State facts and commands; remove hypotheticals and filler.

## Structure
- Start with a one-sentence purpose.
- Use headings for major topics and bullet lists for details.
- Insert code blocks for code and inline backticks for identifiers.
- Link to related resources when they help the reader act.

## Style
- Keep sentences short and concrete.
- Reuse official terminology consistently.
- Include only examples that prove the task.

## Functional Documentation
- Write documentation inside the `docs/fonctionnelles` folder.

## Technical Documentation
- Write documentation inside the `docs/techniques` folder.

## Best Practices
- Keep every document in sync with the latest product behavior; update it as part of each change.
- Reference source specifications and tickets with working links to preserve traceability.
- Include review notes or owners when content requires periodic validation.
- Flag open questions or pending decisions explicitly and time-box their resolution.
- Use multiple files to separate concerns and improve discoverability.
- Name files and folders clearly to reflect their content and purpose.
- Avoid superfluous text; ensure every sentence drives action or understanding.

# Mandatory Formats
- Follow https://learn.microsoft.com/en-us/azure/devops/project/wiki/wiki-file-structure?view=azure-devops for wiki-style documentation and https://learn.microsoft.com/en-us/azure/devops/project/wiki/markdown-guidance?view=azure-devops
- Always use Markdown syntax for formatting. With FrontMatter headers for metadata when applicable.