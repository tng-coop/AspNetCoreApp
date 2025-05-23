"""Backwards-compatible module aggregating slug and menu utilities."""

from .slug_utils import slugify
from .menu_builder import build_menu

__all__ = ["slugify", "build_menu"]
