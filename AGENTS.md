```md
This file outlines future plans for slug handling, category assignment, and related processes in the project.

# Slug Handling Roadmap

- **Slug mandatory**: Every content entity—Publications and Categories—must have a non-null, non-empty slug. The slug property is marked `.IsRequired()` in EF Core, enforcing database-level non-null constraints.
- **Manual slug entry only**: Slugs must be explicitly provided via the write DTO for both articles and categories. The system does **not** auto-generate slugs from titles at any layer.
- **Default fallback**: If the provided slug is blank or consists solely of whitespace, the services will unconditionally assign the literal value `default` before any uniqueness checks.
- **DB non-empty guarantee**: Although EF Core's `.IsRequired()` enforces non-null at the database level, it does not by itself prevent empty strings; our default-fallback logic ensures that the DB never stores a blank slug.
- **Uniqueness & URL constraints**:
  - EF Core defines unique indexes on `Slug` for Publications, Categories, MenuItems, and Tenants.
  - Service-layer helpers (`GenerateUniqueSlugAsync`) append `-1`, `-2`, etc., until a unique slug is found.
  - Slugs are restricted to ASCII letters, digits, and hyphens only.

# Category Assignment

- **Single category per article**: `Publication` has a `CategoryId` foreign key so each article belongs to exactly one category.
- **Hierarchical categories**: The `Category` entity uses a self-referencing FK (`ParentCategoryId`) with `OnDelete(DeleteBehavior.Restrict)`, allowing multi-level trees but preventing cycles.
- **Navigation and seeding**:
  - `CategorySeeder` creates a depth-4 hierarchy as example data.
  - `TreeMenuService` builds a navigable menu by recursively traversing `Category.Children` and published publications.

# Article Details

- **Content structure**: The `Publication` entity requires `Title`, `Slug`, and `Html`. It also tracks `Status` (Draft, Published, Scheduled), `CreatedAt`, optional `PublishedAt`, and `FeaturedOrder` for ordering.
- **Metadata and revision history**:
  - Publications may have optional tags and a summary field at the application layer (not persisted by default).
  - Every update snapshots the previous state into `PublicationRevision`, preserving title, HTML, and category.
- **Publishing workflow**: 
  - New publications default to `Draft`. Calling `PublishAsync` sets `Status = Published` and stamps `PublishedAt = now`.
  - `UnpublishAsync` reverts to `Draft` and clears `PublishedAt`.

# Testing Guidance

- **No automated tests in CI**: Do not run any .NET tests automatically. The project maintainer runs `dotnet test` or related commands manually as needed.

# EF Migration Handling

- **Manual migrations**: EF Core migrations are not triggered by the agent. Changes to the model require the maintainer to run `reset-db-and-migrations.sh`.
- **No EF commands**: Do not invoke `dotnet ef migrations` or `dotnet ef database update`. The migration script handles full database reset and migration.
```
