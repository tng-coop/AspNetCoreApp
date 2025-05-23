from .models import MenuItem
from rich import print


def print_menu(items: list[MenuItem], indent: int = 0) -> None:
    """Recursively print the menu structure.

    If a category has publications, the first one (the "top" article)
    is displayed inline to the right of the category instead of on its
    own line. Remaining children are printed on subsequent lines.
    """
    for item in items:
        prefix = "    " * indent + f"- {item.title} ({item.slug})"

        # Categories have ``content_item_id`` set to ``None``. When a
        # category contains publications, show the first one to the
        # right of the category name.
        if item.content_item_id is None:
            first_pub = next((c for c in item.children if c.content_item_id), None)
            if first_pub:
                print(prefix + f" -> {first_pub.title}")
                remaining = [c for c in item.children if c is not first_pub]
                print_menu(remaining, indent + 1)
            else:
                print(prefix)
                print_menu(item.children, indent + 1)
        else:
            # Publication nodes are printed normally
            print(prefix)
            print_menu(item.children, indent + 1)
