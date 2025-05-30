```md
This file outlines future plans for slug handling, category assignment, and related processes in the project.

# No SLN production
SLN file updates are to be done with dotnet command.   Do not directly edit SLN files.  Instead, show instructions for running the command to update the SLN file.

# Testing Guidance

- **No automated tests in CI**: Do not run any .NET tests automatically. The project maintainer runs `dotnet test` or related commands manually as needed.

# EF Migration Handling

- **Manual migrations**: EF Core migrations are not triggered by the agent. Changes to the model require the maintainer to run `reset-db-and-migrations.sh`.
- **No EF commands**: Do not invoke `dotnet ef migrations` or `dotnet ef database update`. The migration script handles full database reset and migration.
```
