from datetime import datetime, timedelta
import uuid
from typing import List, Tuple

from ..models import Publication
from ..utils import slugify


def publication_titles() -> List[Tuple[str, str]]:
    """Static list of publication titles and their category slugs."""
    return [
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


def sample_publications(now: datetime | None = None) -> List[Publication]:
    """Generate Publication objects using the provided or current time."""
    if now is None:
        now = datetime.utcnow()
    titles = publication_titles()
    pubs: List[Publication] = []
    for i, (cat_slug, title) in enumerate(titles):
        is_feat = cat_slug == "about"
        pubs.append(
            Publication(
                id=str(uuid.uuid4()),
                title=title,
                slug=slugify(title),
                category_slug=cat_slug,
                tenant_slug="acme",
                published_at=now - timedelta(days=len(titles) - i),
                is_featured=is_feat,
                featured_order=1 if is_feat else 0,
            )
        )

    pubs.append(
        Publication(
            id=str(uuid.uuid4()),
            title="Beta Industries Overview",
            slug=slugify("Beta Industries Overview"),
            category_slug="home",
            tenant_slug="beta",
            published_at=now,
            is_featured=False,
            featured_order=0,
        )
    )
    return pubs
