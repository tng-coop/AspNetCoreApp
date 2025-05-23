from typing import List, Optional
from .models import Category, Publication, MenuItem
from datetime import datetime

def slugify(text: str) -> str:
    slug = text.lower()
    for ch in [' ', ':', '’', "'", '…']:
        slug = slug.replace(ch, '-')
    while '--' in slug:
        slug = slug.replace('--', '-')
    return slug.strip('-')


def build_menu(
    tenant_slug: str,
    categories: List[Category],
    publications: List[Publication]
) -> List[MenuItem]:
    cats = [c for c in categories if c.tenant_slug == tenant_slug]
    pubs = [p for p in publications if p.tenant_slug == tenant_slug]
    cat_by_parent: dict[Optional[str], List[Category]] = {}
    for cat in cats:
        cat_by_parent.setdefault(cat.parent_id, []).append(cat)
    slug_to_id = {c.slug: c.id for c in cats}
    pubs_by_cat: dict[str, List[Publication]] = {}
    for pub in pubs:
        cat_id = slug_to_id.get(pub.category_slug)
        if cat_id:
            pubs_by_cat.setdefault(cat_id, []).append(pub)
    def map_cats(subcats: List[Category]) -> List[MenuItem]:
        items: List[MenuItem] = []
        for cat in sorted(subcats, key=lambda c: c.name):
            children = map_cats(cat_by_parent.get(cat.id, []))
            for pub in sorted(
                pubs_by_cat.get(cat.id, []),
                key=lambda p: p.published_at,
                reverse=True
            ):
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
    from rich import print
    for item in items:
        prefix = "    " * indent + f"- {item.title} ({item.slug})"
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
            print(prefix)
            print_menu(item.children, indent + 1)
