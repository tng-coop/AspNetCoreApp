from .models import MenuItem
from rich import print


def print_menu(items: list[MenuItem], indent: int = 0) -> None:
    """
    Recursively prints the menu structure to the console.
    """
    for item in items:
        print("    " * indent + f"- {item.title} ({item.slug})")
        print_menu(item.children, indent + 1)
