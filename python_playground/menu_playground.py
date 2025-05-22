from __future__ import annotations

from dataclasses import dataclass, field
from typing import List, Optional
from datetime import datetime, timedelta
import uuid

@dataclass
class Category:
    id: str
    name: str
    slug: str
    parent_id: Optional[str] = None

@dataclass
class Publication:
    id: str
    title: str
    slug: str
    category_slug: str
    published_at: datetime

@dataclass
class MenuItem:
    id: str
    title: str
    slug: str
    icon_css: str
    sort_order: int
    content_item_id: Optional[str]
    children: List['MenuItem'] = field(default_factory=list)

def slugify(text: str) -> str:
    slug = text.lower()
    slug = slug.replace(" ", "-")
    slug = slug.replace(":", "")
    slug = slug.replace("’", "")
    slug = slug.replace("'", "")
    slug = slug.replace("…", "")
    while "--" in slug:
        slug = slug.replace("--", "-")
    return slug.strip('-')

def build_menu(categories: List[Category], publications: List[Publication]) -> List[MenuItem]:
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
            for pub in sorted(pubs_by_cat.get(cat.id, []), key=lambda p: p.published_at, reverse=True):
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
    for item in items:
        print("    " * indent + f"- {item.title} ({item.slug})")
        print_menu(item.children, indent + 1)

def sample_data() -> tuple[List[Category], List[Publication]]:
    cats = [
        Category("1", "Uncategorized", "uncategorized"),
        Category("2", "About", "about"),
        Category("3", "Ministries", "ministries"),
        Category("4", "Service", "service", parent_id="3"),
        Category("5", "Outreach", "outreach", parent_id="3"),
        Category("6", "Food Pantry", "food-pantry", parent_id="4"),
        Category("7", "Clothing Drive", "clothing-drive", parent_id="4"),
        Category("8", "Mobile Pantry", "mobile-pantry", parent_id="6"),
    ]

    now = datetime.utcnow()
    pubs = []
    titles = [
        ("about", "Getting Started with Our CMS"),
        ("ministries", "Annual Ministries Kickoff"),
        ("service", "Service Opportunities Update"),
        ("outreach", "Outreach Team Training"),
        ("food-pantry", "Food Pantry Schedule"),
        ("clothing-drive", "Clothing Drive Recap"),
        ("mobile-pantry", "Mobile Pantry Route Announced"),
        ("outreach", "Community Outreach Recap"),
        ("outreach", "Volunteer Spotlight"),
    ]
    for i, (cat_slug, title) in enumerate(titles):
        pub_slug = slugify(title)
        pubs.append(Publication(
            id=str(uuid.uuid4()),
            title=title,
            slug=pub_slug,
            category_slug=cat_slug,
            published_at=now - timedelta(days=len(titles) - i)
        ))
    return cats, pubs

if __name__ == "__main__":
    categories, publications = sample_data()
    menu = build_menu(categories, publications)
    print_menu(menu)
