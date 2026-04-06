import axios from 'axios';
import { getProducts } from './ProductService';

jest.mock('axios', () => ({
  __esModule: true,
  default: { get: jest.fn() },
}));

const mockProducts = [
  { id: 1, sku: 'A001', name: 'Product Alpha', unitPrice: 10, availableUnits: 5 },
  { id: 2, sku: 'B002', name: 'Product Beta', unitPrice: 20, availableUnits: 3 },
];

describe('ProductService', () => {
  beforeEach(() => jest.clearAllMocks());

  describe('getProducts', () => {
    it('returns an array of products on success', async () => {
      (axios.get as jest.Mock).mockResolvedValue({ data: mockProducts });
      const result = await getProducts('my-token');
      expect(result).toEqual(mockProducts);
    });

    it('calls the correct endpoint with authorization header', async () => {
      (axios.get as jest.Mock).mockResolvedValue({ data: [] });
      await getProducts('my-token');
      expect(axios.get).toHaveBeenCalledWith(
        expect.stringContaining('/Product/GetAllProducts'),
        { headers: { Authorization: 'Bearer my-token' } }
      );
    });

    it('returns [] on error', async () => {
      (axios.get as jest.Mock).mockRejectedValue(new Error('Network error'));
      const result = await getProducts('token');
      expect(result).toEqual([]);
    });

    it('returns an empty array when the API returns no products', async () => {
      (axios.get as jest.Mock).mockResolvedValue({ data: [] });
      const result = await getProducts('token');
      expect(result).toEqual([]);
    });
  });
});
