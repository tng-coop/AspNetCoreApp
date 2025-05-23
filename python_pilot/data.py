from datetime import datetime, timedelta
import uuid
from typing import List, Tuple
from .models import Category, Publication
from .utils import slugify

def sample_data() -> Tuple[List[str], List[Category], List[Publication]]:
    tenants = ["acme", "beta"]
    now = datetime.utcnow()

    # Define categories with parent referenced by slug
    defs = [
        ("Uncategorized", "uncategorized", "acme", None),
        ("About", "about", "acme", None),
        ("Ministries", "ministries", "acme", None),
        ("Service", "service", "acme", "ministries"),
        ("Outreach", "outreach", "acme", "ministries"),
        ("Food Pantry", "food-pantry", "acme", "service"),
        ("Clothing Drive", "clothing-drive", "acme", "service"),
        ("Mobile Pantry", "mobile-pantry", "acme", "food-pantry"),
        ("Home", "home", "beta", None),
    ]

    # First pass: create categories without parent_id
    cats: List[Category] = []
    for name, slug, tenant, _ in defs:
        cats.append(Category(
            id=str(uuid.uuid4()),
            name=name,
            slug=slug,
            tenant_slug=tenant,
            parent_id=None
        ))

    # Build mapping from slug to generated ID
    slug_to_id = {c.slug: c.id for c in cats}

    # Second pass: assign parent_id by slug lookup
    for cat, (_, _, _, parent_slug) in zip(cats, defs):
        cat.parent_id = slug_to_id.get(parent_slug) if parent_slug else None

    # Publications (same as before)
    pubs: List[Publication] = []
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
        pubs.append(Publication(
            id=str(uuid.uuid4()),
            title=title,
            slug=slugify(title),
            category_slug=cat_slug,
            tenant_slug="acme",
            published_at=now - timedelta(days=len(titles) - i)
        ))

    # Add a beta publication
    pubs.append(Publication(
        id=str(uuid.uuid4()),
        title="Beta Industries Overview",
        slug=slugify("Beta Industries Overview"),
        category_slug="home",
        tenant_slug="beta",
        published_at=now
    ))

    return tenants, cats, pubs
