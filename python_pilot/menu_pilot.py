from .data import sample_data
from .interactive import interactive_menu


def main():
    tenants, all_categories, all_publications = sample_data()
    print("Available tenants: ", ", ".join(tenants))
    selected = input("Enter tenant slug: ").strip()
    if selected not in tenants:
        print(f"Tenant '{selected}' not found. Exiting.")
    else:
        categories = [c for c in all_categories if c.tenant_slug == selected]
        publications = [p for p in all_publications if p.tenant_slug == selected]
        interactive_menu(categories, publications)


if __name__ == "__main__":
    main()