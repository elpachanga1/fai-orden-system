import { Item, ShoppingCart } from "../entities/Interfaces";
import * as ItemService from '../services/ItemService';
import {sortBy} from 'lodash';
export const getShoppingCart = async (token: string): Promise<ShoppingCart> => {
    const items: Item[] = await ItemService.getItems(token);
    const currentCountProducts = items.reduce((total, item) => total + item.quantity, 0);
    const currentTotal = items.reduce((total, item) => total + item.totalPrice, 0);
    return {
        items: sortBy(items, 'id'),
        countProducts: currentCountProducts,
        total: currentTotal
    };
}