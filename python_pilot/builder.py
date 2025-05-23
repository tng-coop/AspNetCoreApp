from typing import List, Optional
from .models import Category, Publication, MenuItem


def slugify(text: str) -> str:
    slug = text.lower()
    for ch in [' ', ':', '’', "'", '…']:
        slug = slug.replace(ch, '-')
    while '--' in slug:
        slug = slug.replace('--', '-')
    return slug.strip('-')


def build_menu(
    categories: List[Category],
    publications: List[Publication]
) -> List[MenuItem]:
    """
    Build a hierarchical menu of categories and publications.
    """
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
            # Append publications under this category, newest first
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

    # Entrypoint: top-level categories have parent_id None
    return map_cats(cat_by_parent.get(None, []))
