from typing import List, Tuple

from ..models import Category, Publication
from .tenants import sample_tenants
from .categories import sample_categories
from .publications import sample_publications


def sample_data() -> Tuple[List[str], List[Category], List[Publication]]:
    """Return tenants, categories and publications used by the CLI."""
    tenants = sample_tenants()
    categories = sample_categories()
    publications = sample_publications()
    return tenants, categories, publications

__all__ = [
    "sample_data",
    "sample_tenants",
    "sample_categories",
    "sample_publications",
]
