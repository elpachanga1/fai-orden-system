import { Item, ShoppingCart } from '../models/interfaces';

export function buildShoppingCart(items: Item[]): ShoppingCart {
  const sorted = [...items].sort((a, b) => a.id - b.id);
  const countProducts = sorted.reduce((sum, item) => sum + item.quantity, 0);
  const total = sorted.reduce((sum, item) => sum + item.totalPrice, 0);
  return { items: sorted, countProducts, total };
}

export const emptyCart: ShoppingCart = {
  items: [],
  countProducts: 0,
  total: 0,
};
