from __future__ import annotations
from typing import List

from ...models import Category, Publication
from ...menu_builder import build_menu
from ...printer import print_menu
from rich import print


def show_menu(categories: List[Category], publications: List[Publication]) -> None:
    menu = build_menu(categories, publications)
    print("\nMenu Tree:")
    print_menu(menu)
