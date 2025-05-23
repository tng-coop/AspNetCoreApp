"""Utility functions for working with slugs."""

from typing import Iterable


def slugify(text: str) -> str:
    """Convert ``text`` into a URL-friendly slug matching the Blazor logic."""
    slug = text.lower()

    # Spaces and several punctuation characters become hyphens.
    for ch in [' ', '’', "'", '…']:
        slug = slug.replace(ch, '-')

    # Colons are stripped entirely rather than replaced with a hyphen.
    slug = slug.replace(':', '')

    while '--' in slug:
        slug = slug.replace('--', '-')

    return slug.strip('-')


def contains_only_ascii(text: str) -> bool:
    """Return ``True`` if ``text`` contains only ASCII characters."""
    return all(ord(c) <= 0x7F for c in text)
