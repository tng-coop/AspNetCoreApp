from __future__ import annotations
from typing import List

from ..models import Category, Publication
from . import actions


def interactive_menu(
    tenant_slug: str,
    categories: List[Category],
    publications: List[Publication],
) -> None:
    """CLI for managing tenants, categories and publications."""
    current_user: str = "anonymous"

    while True:
        print(f"Tenant: {tenant_slug} | User: {current_user}\n")
        print(
            "Options:\n"
            "1. Show menu tree\n"
            "2. List categories\n"
            "3. List publications\n"
            "4. Add category\n"
            "5. Add publication\n"
            "6. Empty all data\n"
            "7. Show raw data\n"
            "8. Update category parent\n"
            "9. Login/Logout\n"
            "0. Exit"
        )
        choice = input("Enter choice [0-9]: ").strip().lower()

        if choice == "0":
            print("Goodbye!")
            break
        elif choice == "9":
            if current_user == "anonymous":
                role = input("Enter role to login as (admin/superadmin): ").strip().lower()
                if role in ("admin", "superadmin"):
                    current_user = role
                    print(f"Logged in as {role}.")
                else:
                    print("Invalid role. Please choose 'admin' or 'superadmin'.")
            else:
                current_user = "anonymous"
                print("Logged out. Current user is now anonymous.")
        elif choice == "1":
            actions.show_menu(categories, publications)
        elif choice == "2":
            actions.list_categories(categories)
        elif choice == "3":
            actions.list_publications(publications)
        elif choice == "4":
            actions.add_category(tenant_slug, categories)
        elif choice == "5":
            actions.add_publication(tenant_slug, categories, publications)
        elif choice == "6":
            actions.empty_data(categories, publications)
        elif choice == "7":
            actions.show_raw_data(categories, publications)
        elif choice == "8":
            actions.update_category_parent(categories)
        else:
            print("Invalid choice, please try again.")
