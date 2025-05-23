from typing import List, Optional
from datetime import datetime
import uuid
from rich import print
from rich.table import Table

from .models import Category, Publication
from .builder import slugify, build_menu
from .printer import print_menu


def interactive_menu(
    categories: List[Category],
    publications: List[Publication]
) -> None:
    """
    Simple CLI to manage categories and publications,
    with login/logout functionality.
    """
    current_user: str = 'anonymous'

    while True:
        print(f"\nCurrent user: {current_user}\n")
        print("Options:")
        print("1. Show menu tree")
        print("2. List categories")
        print("3. List publications")
        print("4. Add category")
        print("5. Add publication")
        print("6. Empty all data")
        print("7. Show raw data")
        print("8. Update category parent")
        print("9. Exit")
        print("L. Login")
        print("O. Logout")

        choice = input("Enter choice [1-9, L, O]: ").strip().lower()

        if choice == 'l':
            role = input("Enter role to login as (admin/superadmin): ").strip().lower()
            if role in ('admin', 'superadmin'):
                current_user = role
                print(f"Logged in as {role}.")
            else:
                print("Invalid role. Please choose 'admin' or 'superadmin'.")
        elif choice == 'o':
            current_user = 'anonymous'
            print("Logged out. Current user is now anonymous.")
        elif choice == '1':
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
                parent_id=parent.id if parent else None,
                tenant_slug=categories[0].tenant_slug if categories else ''
            )
            categories.append(new_cat)
            print(f"Added category: {new_cat.name} (slug: {new_cat.slug}, parent: {parent.name if parent else 'None'})")
        elif choice == '5':
            title = input("Publication title: ").strip()
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
                published_at=datetime.utcnow(),
                tenant_slug=publications[0].tenant_slug if publications else ''
            )
            publications.append(new_pub)
            print(f"Added publication: {new_pub.title} (slug: {new_pub.slug})")
        elif choice == '6':
            categories.clear()
            publications.clear()
            print("All data cleared.")
        elif choice == '7':
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