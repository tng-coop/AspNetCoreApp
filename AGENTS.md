# AGENTS Instructions

This file outlines future plans for slug handling in the project.

## Slug Handling Roadmap

- **Automatic slug generation**: When creating new content, slugs will be automatically generated from titles.
- **ASCII slug prompt**: If a generated slug contains non-ASCII characters, contributors will be prompted to provide an ASCII-only alternative.
- **Unique and URL-friendly**: Slugs must remain unique within their context and should only use URL-friendly characters (letters, numbers, and hyphens).

## Testing Guidance

After modifying slug-related logic, run the existing test suite to ensure everything functions as expected. Use `dotnet test` for unit tests and `npm test` for any JavaScript-based checks.

