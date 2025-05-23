from typing import List
from rich import print
from rich.table import Table
from .models import Category, Publication
from .data import sample_data
from .utils import build_menu, print_menu, slugify
from datetime import datetime

def interactive_menu(
    tenant_slug: str,
    categories: List[Category],
    publications: List[Publication]
) -> None:
    while True:
        print(f"
Tenant: {tenant_slug}
Options:")
        options = [
            "1. Show menu tree",
            "2. List categories",
            "3. List publications",
            "4. Add category",
            "5. Add publication",
            "6. Empty all data",
            "7. Show raw data",
            "8. Update category parent",
            "9. Exit"
        ]
        for opt in options:
            print(opt)
        choice = input("Enter choice [1-9]: ").strip()
        if choice == '9':
            print("Goodbye!")
            break

if __name__ == "__main__":
    # Run using module: python3 -m python_pilot
    tenants, categories, publications = sample_data()
    print("Available tenants: ", ", ".join(tenants))
    selected = input("Enter tenant slug: ").strip()
    if selected not in tenants:
        print(f"Tenant '{selected}' not found. Exiting.")
    else:
        interactive_menu(selected, categories, publications)
