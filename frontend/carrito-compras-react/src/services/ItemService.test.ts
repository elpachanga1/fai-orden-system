import axios from 'axios';
import { getItems, getItemsByProductId } from './ItemService';

jest.mock('axios', () => ({
  __esModule: true,
  default: { get: jest.fn() },
}));

const mockItems = [
  {
    id: 1,
    name: 'Item A',
    idProduct: 1,
    quantity: 2,
    isDeleted: false,
    totalPrice: 20,
    productReference: { id: 1, sku: 'A001', name: 'Alpha', unitPrice: 10, availableUnits: 5 },
  },
];

describe('ItemService', () => {
  beforeEach(() => jest.clearAllMocks());

  describe('getItems', () => {
    it('returns all items on success', async () => {
      (axios.get as jest.Mock).mockResolvedValue({ data: mockItems });
      const result = await getItems('my-token');
      expect(result).toEqual(mockItems);
    });

    it('calls the correct endpoint with authorization header', async () => {
      (axios.get as jest.Mock).mockResolvedValue({ data: [] });
      await getItems('my-token');
      expect(axios.get).toHaveBeenCalledWith(
        expect.stringContaining('/Item/GetAllItems'),
        { headers: { Authorization: 'Bearer my-token' } }
      );
    });

    it('returns [] on error', async () => {
      (axios.get as jest.Mock).mockRejectedValue(new Error('Network error'));
      const result = await getItems('token');
      expect(result).toEqual([]);
    });
  });

  describe('getItemsByProductId', () => {
    it('returns items for the given product on success', async () => {
      (axios.get as jest.Mock).mockResolvedValue({ data: mockItems });
      const result = await getItemsByProductId(1, 'my-token');
      expect(result).toEqual(mockItems);
    });

    it('calls the correct endpoint with product id and auth header', async () => {
      (axios.get as jest.Mock).mockResolvedValue({ data: [] });
      await getItemsByProductId(42, 'my-token');
      expect(axios.get).toHaveBeenCalledWith(
        expect.stringContaining('/Product/GetItemsByProductId/42'),
        { headers: { Authorization: 'Bearer my-token' } }
      );
    });

    it('returns [] on error', async () => {
      (axios.get as jest.Mock).mockRejectedValue(new Error('Network error'));
      const result = await getItemsByProductId(1, 'token');
      expect(result).toEqual([]);
    });
  });
});
