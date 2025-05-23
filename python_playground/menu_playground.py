from __future__ import annotations

from dataclasses import dataclass, field
from typing import List, Optional
from datetime import datetime, timedelta
import uuid

# Use Rich for pretty output
from rich import print
from rich.table import Table

@dataclass
class Category:
    id: str
    name: str
    slug: str
    parent_id: Optional[str] = None

@dataclass
class Publication:
    id: str
    title: str
    slug: str
    category_slug: str
    published_at: datetime

@dataclass
class MenuItem:
    id: str
    title: str
    slug: str
    icon_css: str
    sort_order: int
    content_item_id: Optional[str]
    children: List['MenuItem'] = field(default_factory=list)


def slugify(text: str) -> str:
    slug = text.lower()
    for ch in [' ', ':', '’', "'", '…']:
        slug = slug.replace(ch, '-')
    while '--' in slug:
        slug = slug.replace('--', '-')
    return slug.strip('-')


def build_menu(categories: List[Category], publications: List[Publication]) -> List[MenuItem]:
    cat_by_parent: dict[Optional[str], List[Category]] = {}
    for cat in categories:
        cat_by_parent.setdefault(cat.parent_id, []).append(cat)

    slug_to_id = {c.slug: c.id for c in categories}

    pubs_by_cat: dict[str, List[Publication]] = {}
    for pub in publications:
        cat_id = slug_to_id.get(pub.category_slug)
        if cat_id:
            pubs_by_cat.setdefault(cat_id, []).append(pub)

    def map_cats(cats: List[Category]) -> List[MenuItem]:
        items: List[MenuItem] = []
        for cat in sorted(cats, key=lambda c: c.name):
            children = map_cats(cat_by_parent.get(cat.id, []))
            for pub in sorted(pubs_by_cat.get(cat.id, []), key=lambda p: p.published_at, reverse=True):
                children.append(MenuItem(
                    id=pub.id,
                    title=pub.title,
                    slug=f"{cat.slug}/{pub.slug}",
                    icon_css="bi-file-earmark-text",
                    sort_order=0,
                    content_item_id=pub.id,
                    children=[]
                ))
            items.append(MenuItem(
                id=cat.id,
                title=cat.name,
                slug=cat.slug,
                icon_css="bi-list-nested",
                sort_order=0,
                content_item_id=None,
                children=children
            ))
        return items

    return map_cats(cat_by_parent.get(None, []))


def print_menu(items: List[MenuItem], indent: int = 0) -> None:
    for item in items:
        print("    " * indent + f"- {item.title} ({item.slug})")
        print_menu(item.children, indent + 1)


def sample_data() -> tuple[List[Category], List[Publication]]:
    cats = [
        Category("1", "Uncategorized", "uncategorized"),
        Category("2", "About", "about"),
        Category("3", "Ministries", "ministries"),
        Category("4", "Service", "service", parent_id="3"),
        Category("5", "Outreach", "outreach", parent_id="3"),
        Category("6", "Food Pantry", "food-pantry", parent_id="4"),
        Category("7", "Clothing Drive", "clothing-drive", parent_id="4"),
        Category("8", "Mobile Pantry", "mobile-pantry", parent_id="6"),
    ]

    now = datetime.utcnow()
    pubs: List[Publication] = []
    titles = [
        ("about", "Getting Started with Our CMS"),
        ("ministries", "Annual Ministries Kickoff"),
        ("service", "Service Opportunities Update"),
        ("outreach", "Outreach Team Training"),
        ("food-pantry", "Food Pantry Schedule"),
        ("clothing-drive", "Clothing Drive Recap"),
        ("mobile-pantry", "Mobile Pantry Route Announced"),
        ("outreach", "Community Outreach Recap"),
        ("outreach", "Volunteer Spotlight"),
    ]
    for i, (cat_slug, title) in enumerate(titles):
        pub_slug = slugify(title)
        pubs.append(Publication(
            id=str(uuid.uuid4()),
            title=title,
            slug=pub_slug,
            category_slug=cat_slug,
            published_at=now - timedelta(days=len(titles) - i)
        ))
    return cats, pubs


def interactive_menu(categories: List[Category], publications: List[Publication]) -> None:
    """
    Simple CLI to interactively manage categories and publications
    """
    while True:
        print("\nOptions:")
        print("1. Show menu tree")
        print("2. List categories")
        print("3. List publications")
        print("4. Add category")
        print("5. Add publication")
        print("6. Empty all data")
        print("7. Show raw data")
        print("8. Update category parent")
        print("9. Exit")
        choice = input("Enter choice [1-9]: ").strip()

        if choice == '1':
            menu = build_menu(categories, publications)
            print("\nMenu Tree:")
            print_menu(menu)
        elif choice == '2':
            print("\nCategories:")
            for cat in categories:
                parent = next((c.name for c in categories if c.id == cat.parent_id), None)
                print(f"- {cat.name} (slug: {cat.slug}, parent: {parent})")
        elif choice == '3':
            print("\nPublications:")
            for pub in publications:
                print(f"- {pub.title} (slug: {pub.slug}, category: {pub.category_slug}, published: {pub.published_at.isoformat()})")
        elif choice == '4':
            # Show existing categories for reference
            if categories:
                print("\nExisting Categories:")
                for cat in categories:
                    parent = next((c.name for c in categories if c.id == cat.parent_id), "None")
                    print(f"- {cat.name} (slug: {cat.slug}, parent: {parent})")
            name = input("Category name: ").strip()
            parent_slug = input("Parent category slug (leave blank for none): ").strip() or None
            parent = next((c for c in categories if c.slug == parent_slug), None) if parent_slug else None
            new_cat = Category(
                id=str(uuid.uuid4()),
                name=name,
                slug=slugify(name),
                parent_id=parent.id if parent else None
            )
            categories.append(new_cat)
            print(f"Added category: {new_cat.name} (slug: {new_cat.slug}, parent: {parent.name if parent else 'None'})")
        elif choice == '5':
            title = input("Publication title: ").strip()
            # Show categories to choose from
            if categories:
                print("\nAvailable category slugs:")
                print(", ".join(c.slug for c in categories))
            cat_slug = input("Category slug: ").strip()
            if not any(c.slug == cat_slug for c in categories):
                print("Category slug not found. Please add the category first.")
                continue
            new_pub = Publication(
                id=str(uuid.uuid4()),
                title=title,
                slug=slugify(title),
                category_slug=cat_slug,
                published_at=datetime.utcnow()
            )
            publications.append(new_pub)
            print(f"Added publication: {new_pub.title} (slug: {new_pub.slug})")
        elif choice == '6':
            categories.clear()
            publications.clear()
            print("All data cleared.")
        elif choice == '7':
            # Pretty print raw data with Rich
            if categories:
                table = Table(title="Categories")
                table.add_column("ID", style="cyan", no_wrap=True)
                table.add_column("Name", style="green")
                table.add_column("Slug", style="magenta")
                table.add_column("Parent", style="yellow")
                for cat in categories:
                    parent = next((c.name for c in categories if c.id == cat.parent_id), "None")
                    table.add_row(cat.id, cat.name, cat.slug, parent)
                print(table)
            else:
                print("[red]No categories available[/red]")

            if publications:
                pub_table = Table(title="Publications")
                pub_table.add_column("ID", style="cyan", no_wrap=True)
                pub_table.add_column("Title", style="green")
                pub_table.add_column("Slug", style="magenta")
                pub_table.add_column("Category", style="yellow")
                pub_table.add_column("Published At", style="blue")
                for pub in publications:
                    pub_table.add_row(pub.id, pub.title, pub.slug, pub.category_slug, pub.published_at.isoformat())
                print(pub_table)
            else:
                print("[red]No publications available[/red]")
        elif choice == '8':
            if not categories:
                print("[red]No categories to update[/red]")
                continue
            print("\nCategories:")
            for cat in categories:
                parent = next((c.slug for c in categories if c.id == cat.parent_id), "None")
                print(f"- {cat.name} (slug: {cat.slug}, current parent: {parent})")
            cat_slug = input("Enter slug of category to update: ").strip()
            target = next((c for c in categories if c.slug == cat_slug), None)
            if not target:
                print("Category slug not found.")
                continue
            new_parent_slug = input("New parent category slug (leave blank to remove parent): ").strip() or None
            if new_parent_slug:
                parent_cat = next((c for c in categories if c.slug == new_parent_slug), None)
                if not parent_cat:
                    print("Parent slug not found.")
                    continue
                target.parent_id = parent_cat.id
                print(f"Updated parent of {target.slug} to {parent_cat.slug}")
            else:
                target.parent_id = None
                print(f"Removed parent of {target.slug}")
        elif choice == '9':
            print("Goodbye!")
            break
        else:
            print("Invalid choice, please try again.")


if __name__ == "__main__":
    categories, publications = sample_data()
    interactive_menu(categories, publications)
