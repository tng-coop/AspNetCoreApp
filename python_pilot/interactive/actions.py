from __future__ import annotations
from typing import List
from datetime import datetime
import uuid
from rich import print
from rich.table import Table

from ..models import Category, Publication
from ..menu_builder import build_menu
from ..slug_utils import slugify
from ..printer import print_menu
from ..data import sample_data


def show_menu(categories: List[Category], publications: List[Publication]) -> None:
    menu = build_menu(categories, publications)
    print("\nMenu Tree:")
    print_menu(menu)


def list_categories(categories: List[Category]) -> None:
    print("\nCategories:")
    for cat in categories:
        parent = next((c.name for c in categories if c.id == cat.parent_id), None)
        print(f"- {cat.name} (slug: {cat.slug}, parent: {parent})")


def list_publications(publications: List[Publication]) -> None:
    print("\nPublications:")
    for pub in publications:
        print(
            f"- {pub.title} (slug: {pub.slug}, category: {pub.category_slug}, "
            f"published: {pub.published_at.isoformat()})"
        )


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
    new_pub = Publication(
        id=str(uuid.uuid4()),
        title=title,
        slug=slugify(title),
        category_slug=cat_slug,
        published_at=datetime.utcnow(),
        tenant_slug=tenant_slug,
    )
    publications.append(new_pub)
    print(f"Added publication '{new_pub.title}'.")


def empty_data(categories: List[Category], publications: List[Publication]) -> None:
    categories.clear()
    publications.clear()
    print("All data cleared.")


def show_raw_data(categories: List[Category], publications: List[Publication]) -> None:
    print("\nRaw Data:")
    table = Table(title="Categories")
    table.add_column("ID", style="cyan")
    table.add_column("Name", style="green")
    table.add_column("Slug", style="magenta")
    table.add_column("Parent", style="yellow")
    for cat in categories:
        parent = next((c.slug for c in categories if c.id == cat.parent_id), "None")
        table.add_row(cat.id, cat.name, cat.slug, parent)
    print(table)

    pub_table = Table(title="Publications")
    pub_table.add_column("ID", style="cyan")
    pub_table.add_column("Title", style="green")
    pub_table.add_column("Slug", style="magenta")
    pub_table.add_column("Category", style="yellow")
    pub_table.add_column("Published At", style="blue")
    for pub in publications:
        pub_table.add_row(
            pub.id,
            pub.title,
            pub.slug,
            pub.category_slug,
            pub.published_at.isoformat(),
        )
    print(pub_table)

    tenants, _, _ = sample_data()
    tenant_table = Table(title="Tenants")
    tenant_table.add_column("Slug", style="cyan")
    for t in tenants:
        tenant_table.add_row(t)
    print(tenant_table)


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
