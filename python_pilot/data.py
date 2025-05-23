from datetime import datetime, timedelta
import uuid
from typing import List, Tuple
from .models import Category, Publication
from .utils import slugify

def sample_data() -> Tuple[List[str], List[Category], List[Publication]]:
    tenants = ["acme", "beta"]
    now = datetime.utcnow()
    cats: List[Category] = [
        Category("1", "Uncategorized", "uncategorized", "acme"),
        Category("2", "About", "about", "acme"),
        Category("3", "Ministries", "ministries", "acme"),
        Category("4", "Service", "service", "acme", parent_id="3"),
        Category("5", "Outreach", "outreach", "acme", parent_id="3"),
        Category("6", "Food Pantry", "food-pantry", "acme", parent_id="4"),
        Category("7", "Clothing Drive", "clothing-drive", "acme", parent_id="4"),
        Category("8", "Mobile Pantry", "mobile-pantry", "acme", parent_id="6"),
        Category("9", "Home", "home", "beta"),
    ]
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
    pubs.append(Publication(
        id=str(uuid.uuid4()),
        title="Beta Industries Overview",
        slug=slugify("Beta Industries Overview"),
        category_slug="home",
        tenant_slug="beta",
        published_at=now
    ))
    return tenants, cats, pubs
