import { getShoppingCart } from './ShoppingCartUtils';
import * as ItemService from '../services/ItemService';
import { Item } from '../entities/Interfaces';

jest.mock('../services/ItemService', () => ({
  getItems: jest.fn(),
}));

const mockGetItems = ItemService.getItems as jest.MockedFunction<typeof ItemService.getItems>;

const makeItem = (id: number, quantity: number, totalPrice: number): Item => ({
  id,
  name: `Item ${id}`,
  idProduct: id,
  quantity,
  isDeleted: false,
  totalPrice,
  productReference: {} as any,
});

describe('getShoppingCart', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('returns the sum of quantities as countProducts', async () => {
    mockGetItems.mockResolvedValue([makeItem(1, 2, 10), makeItem(2, 3, 15)]);
    const result = await getShoppingCart('token');
    expect(result.countProducts).toBe(5);
  });

  it('returns the sum of totalPrices as total', async () => {
    mockGetItems.mockResolvedValue([makeItem(1, 2, 10), makeItem(2, 3, 15)]);
    const result = await getShoppingCart('token');
    expect(result.total).toBe(25);
  });

  it('returns items sorted ascending by id', async () => {
    mockGetItems.mockResolvedValue([makeItem(3, 1, 5), makeItem(1, 1, 5), makeItem(2, 1, 5)]);
    const result = await getShoppingCart('token');
    expect(result.items.map((i) => i.id)).toEqual([1, 2, 3]);
  });

  it('passes the token to getItems', async () => {
    mockGetItems.mockResolvedValue([]);
    await getShoppingCart('my-token');
    expect(mockGetItems).toHaveBeenCalledWith('my-token');
  });

  it('returns zero countProducts, zero total and empty items for an empty cart', async () => {
    mockGetItems.mockResolvedValue([]);
    const result = await getShoppingCart('token');
    expect(result.countProducts).toBe(0);
    expect(result.total).toBe(0);
    expect(result.items).toEqual([]);
  });
});
