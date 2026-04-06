import { Injectable, signal } from '@angular/core';
import { ItemService } from './item.service';
import { ShoppingCart } from '../models/interfaces';
import { buildShoppingCart, emptyCart } from '../utils/shopping-cart.utils';

@Injectable({ providedIn: 'root' })
export class CartStateService {
  readonly cart = signal<ShoppingCart>({ ...emptyCart });

  constructor(private itemService: ItemService) {}

  async refresh(token: string): Promise<void> {
    const items = await this.itemService.getItems(token);
    this.cart.set(buildShoppingCart(items));
  }

  clear(): void {
    this.cart.set({ ...emptyCart });
  }
}
