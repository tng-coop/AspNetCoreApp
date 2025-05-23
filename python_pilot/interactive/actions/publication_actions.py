from __future__ import annotations
from typing import List
from datetime import datetime
import uuid
from rich import print

from ...models import Publication, Category
from ...slug_utils import slugify


def list_publications(publications: List[Publication]) -> None:
    print("\nPublications:")
    for pub in publications:
        print(
            f"- {pub.title} (slug: {pub.slug}, category: {pub.category_slug}, "
            f"published: {pub.published_at.isoformat()}, "
            f"featured: {pub.is_featured}, order: {pub.featured_order})"
        )


def add_publication(
    tenant_slug: str,
    categories: List[Category],
    publications: List[Publication],
) -> None:
    print("\nAdd Publication:")
    title = input("Title: ").strip()
    print("Available categories: ", ", ".join(c.slug for c in categories))
    cat_slug = input("Category slug: ").strip()
    if not any(c.slug == cat_slug for c in categories):
        print("Category not found.")
        return
    feat = input("Featured? [y/N]: ").strip().lower() == "y"
    order = 0
    if feat:
        try:
            order = int(input("Featured order: ").strip())
        except ValueError:
            print("Invalid order, defaulting to 0")
    new_pub = Publication(
        id=str(uuid.uuid4()),
        title=title,
        slug=slugify(title),
        category_slug=cat_slug,
        published_at=datetime.utcnow(),
        tenant_slug=tenant_slug,
        is_featured=feat,
        featured_order=order,
    )
    publications.append(new_pub)
    print(f"Added publication '{new_pub.title}'.")
