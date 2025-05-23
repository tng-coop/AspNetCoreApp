from __future__ import annotations
from typing import List
from rich import print
from rich.table import Table

from ...models import Category, Publication
from ...data import sample_data


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
    pub_table.add_column("Featured Order", style="cyan")
    for pub in publications:
        pub_table.add_row(
            pub.id,
            pub.title,
            pub.slug,
            pub.category_slug,
            pub.published_at.isoformat(),
            str(pub.featured_order),
        )
    print(pub_table)

    tenants, _, _ = sample_data()
    tenant_table = Table(title="Tenants")
    tenant_table.add_column("Slug", style="cyan")
    for t in tenants:
        tenant_table.add_row(t)
    print(tenant_table)
