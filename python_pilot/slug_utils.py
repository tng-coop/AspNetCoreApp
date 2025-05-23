"""Utility functions for working with slugs."""


def slugify(text: str) -> str:
    """Convert a string into a URL-friendly slug."""
    slug = text.lower()
    for ch in [' ', ':', '’', "'", '…']:
        slug = slug.replace(ch, '-')
    while '--' in slug:
        slug = slug.replace('--', '-')
    return slug.strip('-')
