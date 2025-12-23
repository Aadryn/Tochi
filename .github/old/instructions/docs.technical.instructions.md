---
applyTo: "docs/techniques/**/*.md"
---
- **Scope**: Record every technical decision as a dedicated Architecture Decision Record (ADR) and avoid mixing unrelated topics in the same file.
- **Location**: Store ADRs under `docs/techniques/`, name files with sequential identifiers such as `ADR-001-short-title.md`, never reuse numbers, and keep shared assets in `docs/techniques/assets/`.
- **Format**: Author ADRs in Markdown with a level-one `# Title` mirroring the file name, then level-two sections in this order: `Status`, `Context`, `Decision`, `Consequences`, `Validation`, and `Alternatives` (include the last only when it adds insight); keep additional sections minimal and purposeful.
- **Template**: Start from this skeleton to keep section ordering and metadata consistent, then remove unused placeholders:

```markdown
# ADR-XYZ Meaningful Title

- Date: YYYY-MM-DD
- Authors: Firstname Lastname (@handle)
- Related issues or tickets: ISSUE-123, [link]

## Status

Proposed | Accepted | Deprecated | Superseded

## Context

- Drivers
- Constraints
- Dependencies
- Assumptions

## Decision

Imperative statement of the chosen approach.

## Consequences

- Positive: ...
- Negative: ...
- Follow-up tasks: ...

## Validation

- Evidence gathered
- Experiments or proofs

## Alternatives

- Option A — reason rejected
- Option B — reason rejected
```
- **Metadata**: Beneath the title list `Date`, `Authors`, and `Related issues or tickets`; update this list whenever the ADR evolves so readers can trace ownership and discussions.
- **Content essentials**: In `Context` outline drivers, constraints, dependencies, impacted systems, assumptions, and open questions; state the selected approach in `Decision` with imperative phrases; split `Consequences` into positive and negative bullet lists and list mandatory follow-up tasks; capture validation evidence (benchmarks, experiments, PoCs) in `Validation`; explain alternatives only when they influenced the rationale and document rejection reasons.
- **Summary and audience**: Begin each specification with a concise abstract (3-5 sentences) describing intent, scope, and target audience; state reader prerequisites and call out stakeholder groups responsible for review or execution.
- **Requirements coverage**: Document functional flows, non-functional expectations (performance, availability, scalability, observability, security, privacy), and compliance constraints in dedicated subsections; reference acceptance criteria or SLAs with measurable targets.
- **Clarity and accessibility**: Use plain English, active voice, and consistent terminology; prefer tables for parameter matrices, keep sentence length under 25 words when possible, and provide text alternatives for every embedded diagram.
- **Terminology management**: Maintain a local glossary or link to the shared glossary for newly introduced terms and acronyms; define domain-specific vocabulary upon first use and avoid conflicting definitions across specifications.
- **Risks and assumptions**: Track assumptions, known limitations, unresolved questions, and risk items in dedicated lists that include impact, likelihood, mitigation, and owners; update them as the decision evolves.
- **Change history**: Append a `Revision history` table capturing version, date, author, summary of modifications, and linked approvals; align version numbers with repository tags or release identifiers when relevant.
- **Dependencies**: Enumerate upstream/downstream systems, external vendors, feature flags, and configuration switches influencing the specification; include contact points and escalation paths.
- **Traceability**: Link to affected components, repositories, experiments, diagrams, and related documentation (functional scenarios, runbooks, onboarding guides); store diagrams as PNG, SVG, or PlantUML within the repository and reference them with relative paths.
- **Lifecycle**: Use statuses `Proposed`, `Accepted`, `Deprecated`, and `Superseded`; when status changes, add a dated changelog note beneath `Status`, link to the review artefact, and include `Superseded by ADR-XYZ` or `Supersedes ADR-XYZ` where appropriate.
- **Methodology**: Raise an architecture review for each ADR, record approver names and review dates, update `Status` as decisions progress (`Proposed`, `Accepted`, `Deprecated`, `Superseded`), and cross-reference successor ADRs when one supersedes another.
- **Review cadence**: Archive meeting minutes, decision logs, or async approvals alongside the ADR (same folder or linked) and schedule periodic reassessment for critical decisions at least every two quarters.
- **Assets**: Provide supporting artefacts that materially influence the decision—sequence diagrams, architecture sketches, configuration snippets, API contracts, or code samples—and store them under version control (prefer `docs/techniques/assets/`); reference exact commit hashes for critical proof-of-concept code.
- **Evidence**: Classify validation items across performance, reliability, security, privacy/compliance, and operational readiness; for each category included, note the test type (benchmark, load test, pen test, tabletop exercise), the date executed, and links to raw results.
- **Cross-links**: Create bidirectional references between ADRs and related functional documents, runbooks, or SOPs by linking relevant sections; when multiple ADRs interact, include a short dependency note under `Context` and update the related ADRs with reciprocal mentions.
- **Maintenance**: Verify numbering before merging, run spell-check or documentation linting, ensure validation steps are actioned and recorded in `Validation`, track follow-up tasks in the related issues noted in metadata, close them once completed with references back to the ADR, and review asset links annually to prevent drift.