from typing import List, Optional
from datetime import datetime
import uuid
from rich import print
from rich.table import Table

from .models import Category, Publication, MenuItem
from .builder import build_menu, slugify
from .printer import print_menu
from .data import sample_data


def interactive_menu(
    tenant_slug: str,
    categories: List[Category],
    publications: List[Publication]
) -> None:
    """
    CLI to manage tenants, categories, and publications,
    with login/logout functionality and raw data display.
    """
    current_user: str = 'anonymous'

    while True:
        print(f"Tenant: {tenant_slug} | User: {current_user}\n")
        print("Options:\n"
              "1. Show menu tree\n"
              "2. List categories\n"
              "3. List publications\n"
              "4. Add category\n"
              "5. Add publication\n"
              "6. Empty all data\n"
              "7. Show raw data\n"
              "8. Update category parent\n"
              "9. Login/Logout\n"
              "0. Exit")
        choice = input("Enter choice [0-9]: ").strip().lower()

        if choice == '0':
            print("Goodbye!")
            break
        elif choice == '9':
            if current_user == 'anonymous':
                role = input("Enter role to login as (admin/superadmin): ").strip().lower()
                if role in ('admin', 'superadmin'):
                    current_user = role
                    print(f"Logged in as {role}.")
                else:
                    print("Invalid role. Please choose 'admin' or 'superadmin'.")
            else:
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
            print("\nAdd Category:")
            name = input("Name: ").strip()
            parent_slug = input("Parent slug (blank for none): ").strip() or None
            parent = next((c for c in categories if c.slug == parent_slug), None) if parent_slug else None
            new_cat = Category(
                id=str(uuid.uuid4()),
                name=name,
                slug=slugify(name),
                tenant_slug=tenant_slug,
                parent_id=parent.id if parent else None
            )
            categories.append(new_cat)
            print(f"Added category '{new_cat.name}' under '{parent_slug or 'None'}'.")
        elif choice == '5':
            print("\nAdd Publication:")
            title = input("Title: ").strip()
            print("Available categories: ", ", ".join(c.slug for c in categories))
            cat_slug = input("Category slug: ").strip()
            if not any(c.slug == cat_slug for c in categories):
                print("Category not found.")
                continue
            new_pub = Publication(
                id=str(uuid.uuid4()),
                title=title,
                slug=slugify(title),
                category_slug=cat_slug,
                published_at=datetime.utcnow(),
                tenant_slug=tenant_slug
            )
            publications.append(new_pub)
            print(f"Added publication '{new_pub.title}'.")
        elif choice == '6':
            categories.clear()
            publications.clear()
            print("All data cleared.")
        elif choice == '7':
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
                pub_table.add_row(pub.id, pub.title, pub.slug, pub.category_slug, pub.published_at.isoformat())
            print(pub_table)

            # Show tenants as well
            tenants, _, _ = sample_data()
            tenant_table = Table(title="Tenants")
            tenant_table.add_column("Slug", style="cyan")
            for t in tenants:
                tenant_table.add_row(t)
            print(tenant_table)
        elif choice == '8':
            print("\nUpdate Category Parent:")
            for cat in categories:
                parent = next((c.slug for c in categories if c.id == cat.parent_id), "None")
                print(f"- {cat.slug} (current parent: {parent})")
            slug = input("Category slug: ").strip()
            target = next((c for c in categories if c.slug == slug), None)
            if not target:
                print("Not found.")
                continue
            new_parent = input("New parent slug (blank to remove): ").strip() or None
            if new_parent and not any(c.slug == new_parent for c in categories):
                print("Parent not found.")
                continue
            target.parent_id = next((c.id for c in categories if c.slug == new_parent), None)
            print(f"Parent of '{slug}' updated to '{new_parent or 'None'}'.")
        else:
            print("Invalid choice, please try again.")