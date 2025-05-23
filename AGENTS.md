# AGENTS Instructions

This file outlines future plans for slug handling in the project.

## Slug Handling Roadmap

- **Manual slug entry**: When creating new content, slugs must be provided explicitly. The system will not generate slugs from titles.
- **Default slug**: Blank slugs are not allowed. If no slug is provided, the literal value `default` will be used. Slugs should contain only ASCII letters, numbers, and hyphens.
- **Unique and URL-friendly**: When present, a slug must be unique within its context and use URL-friendly characters.

## Testing Guidance

Do not run any test.   Dotnet tests are not currently supported. The project maintainer will run the tests manually via their CLI.


## EF Migration Handling

- dotnet EF migrations are not automatically managed by the agent.
- Do not run `dotnet ef migrations` or related commands. The project maintainer manually runs the `reset-db-and-migrations.sh` script via their CLI.

