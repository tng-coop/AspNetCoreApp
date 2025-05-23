# Python Pilot

This small script reproduces the menu-building algorithm used in the
ASP.NET Core app. It lets you experiment with the category tree and
sample publications entirely in Python.

Run it from the package root:

```bash
python -m python_pilot
```

You can also run the simplified pilot directly:

```bash
python menu_pilot.py
```

Both commands will print a simple text-based tree of categories and the
published posts associated with each category.

### Slug generation

Slugs are produced using `slugify()` which mirrors the Blazor app's
`SlugGenerator`. Colons are removed entirely while spaces and certain
punctuation are converted to hyphens.
