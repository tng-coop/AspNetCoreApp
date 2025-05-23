from __future__ import annotations
from typing import List
from datetime import datetime
import uuid
from rich import print

from ...models import Category, Publication
from ...slug_utils import slugify


def list_categories(categories: List[Category]) -> None:
    print("\nCategories:")
    for cat in categories:
        parent = next((c.name for c in categories if c.id == cat.parent_id), None)
        print(f"- {cat.name} (slug: {cat.slug}, parent: {parent})")


def add_category(tenant_slug: str, categories: List[Category]) -> None:
    print("\nAdd Category:")
    name = input("Name: ").strip()
    parent_slug = input("Parent slug (blank for none): ").strip() or None
    parent = next((c for c in categories if c.slug == parent_slug), None) if parent_slug else None
    new_cat = Category(
        id=str(uuid.uuid4()),
        name=name,
        slug=slugify(name),
        tenant_slug=tenant_slug,
        parent_id=parent.id if parent else None,
    )
    categories.append(new_cat)
    print(f"Added category '{new_cat.name}' under '{parent_slug or 'None'}'.")


def update_category_parent(categories: List[Category]) -> None:
    print("\nUpdate Category Parent:")
    for cat in categories:
        parent = next((c.slug for c in categories if c.id == cat.parent_id), "None")
        print(f"- {cat.slug} (current parent: {parent})")
    slug = input("Category slug: ").strip()
    target = next((c for c in categories if c.slug == slug), None)
    if not target:
        print("Not found.")
        return
    new_parent = input("New parent slug (blank to remove): ").strip() or None
    if new_parent and not any(c.slug == new_parent for c in categories):
        print("Parent not found.")
        return
    target.parent_id = next((c.id for c in categories if c.slug == new_parent), None)
    print(f"Parent of '{slug}' updated to '{new_parent or 'None'}'.")
