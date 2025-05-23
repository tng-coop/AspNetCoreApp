from __future__ import annotations
import uuid
from typing import List, Optional, Tuple

from ..models import Category


def category_definitions() -> List[Tuple[str, str, str, Optional[str]]]:
    """Static list of category definitions."""
    return [
        ("Uncategorized", "uncategorized", "acme", None),
        ("About", "about", "acme", None),
        ("Ministries", "ministries", "acme", None),
        ("Service", "service", "acme", "ministries"),
        ("Outreach", "outreach", "acme", "ministries"),
        ("Food Pantry", "food-pantry", "acme", "service"),
        ("Clothing Drive", "clothing-drive", "acme", "service"),
        ("Mobile Pantry", "mobile-pantry", "acme", "food-pantry"),
        ("Home", "home", "beta", None),
    ]


def sample_categories() -> List[Category]:
    """Generate Category objects from the definitions."""
    defs = category_definitions()
    categories: List[Category] = []
    for name, slug, tenant, _ in defs:
        categories.append(
            Category(
                id=str(uuid.uuid4()),
                name=name,
                slug=slug,
                tenant_slug=tenant,
                parent_id=None,
            )
        )

    slug_to_id = {c.slug: c.id for c in categories}
    for cat, (_, _, _, parent_slug) in zip(categories, defs):
        cat.parent_id = slug_to_id.get(parent_slug) if parent_slug else None

    return categories
