from .data import sample_data
from .cli import interactive_menu

def main():
    tenants, categories, publications = sample_data()
    print("Available tenants: ", ", ".join(tenants))
    selected = input("Enter tenant slug: ").strip()
    if selected not in tenants:
        print(f"Tenant '{selected}' not found. Exiting.")
    else:
        interactive_menu(selected, categories, publications)

if __name__ == "__main__":
    main()
