from __future__ import annotations
from dataclasses import dataclass, field
from typing import List, Optional
from datetime import datetime

@dataclass
class Category:
    id: str
    name: str
    slug: str
    tenant_slug: str
    parent_id: Optional[str] = None

@dataclass
class Publication:
    id: str
    title: str
    slug: str
    category_slug: str
    tenant_slug: str
    published_at: datetime
    is_featured: bool = False
    featured_order: int = 0

@dataclass
class MenuItem:
    id: str
    title: str
    slug: str
    icon_css: str
    sort_order: int
    content_item_id: Optional[str]
    children: List['MenuItem'] = field(default_factory=list)
