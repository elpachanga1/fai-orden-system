import axios from 'axios';
import {
  addProductToShoppingCart,
  EmptyShoppingCart,
  DeleteProductFromShoppingCart,
  UpdateProductFromShoppingCart,
  CompleteCartTransaction,
} from './CartService';

jest.mock('axios', () => ({
  __esModule: true,
  default: { get: jest.fn(), post: jest.fn(), delete: jest.fn() },
}));

describe('CartService', () => {
  beforeEach(() => jest.clearAllMocks());

  describe('addProductToShoppingCart', () => {
    it('returns response data on success', async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: { success: true } });
      const result = await addProductToShoppingCart(1, 2, 'token');
      expect(result).toEqual({ success: true });
    });

    it('calls the correct endpoint with auth header', async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: {} });
      await addProductToShoppingCart(5, 3, 'my-token');
      expect(axios.post).toHaveBeenCalledWith(
        expect.stringContaining('/Store/AddProductToShoppingCart?IdUser=1&IdProduct=5&Quantity=3'),
        {},
        { headers: { Authorization: 'Bearer my-token' } }
      );
    });

    it('returns [] on error', async () => {
      (axios.post as jest.Mock).mockRejectedValue(new Error('Network error'));
      const result = await addProductToShoppingCart(1, 1, 'token');
      expect(result).toEqual([]);
    });
  });

  describe('EmptyShoppingCart', () => {
    it('returns response data on success', async () => {
      (axios.delete as jest.Mock).mockResolvedValue({ data: 'ok' });
      const result = await EmptyShoppingCart('token');
      expect(result).toBe('ok');
    });

    it('calls the correct endpoint with auth header', async () => {
      (axios.delete as jest.Mock).mockResolvedValue({ data: {} });
      await EmptyShoppingCart('my-token');
      expect(axios.delete).toHaveBeenCalledWith(
        expect.stringContaining('/Store/EmptyShoppingCart?IdUser=1'),
        { headers: { Authorization: 'Bearer my-token' } }
      );
    });

    it('returns [] on error', async () => {
      (axios.delete as jest.Mock).mockRejectedValue(new Error('Network error'));
      const result = await EmptyShoppingCart('token');
      expect(result).toEqual([]);
    });
  });

  describe('DeleteProductFromShoppingCart', () => {
    it('returns response data on success', async () => {
      (axios.delete as jest.Mock).mockResolvedValue({ data: 'deleted' });
      const result = await DeleteProductFromShoppingCart(10, 'token');
      expect(result).toBe('deleted');
    });

    it('calls the correct endpoint with auth header', async () => {
      (axios.delete as jest.Mock).mockResolvedValue({ data: {} });
      await DeleteProductFromShoppingCart(7, 'my-token');
      expect(axios.delete).toHaveBeenCalledWith(
        expect.stringContaining('/Store/DeleteProductFromShoppingCart?IdUser=1&IdItem=7'),
        { headers: { Authorization: 'Bearer my-token' } }
      );
    });

    it('returns [] on error', async () => {
      (axios.delete as jest.Mock).mockRejectedValue(new Error('Network error'));
      const result = await DeleteProductFromShoppingCart(1, 'token');
      expect(result).toEqual([]);
    });
  });

  describe('UpdateProductFromShoppingCart', () => {
    it('returns response data on success', async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: { updated: true } });
      const result = await UpdateProductFromShoppingCart(2, 5, 'token');
      expect(result).toEqual({ updated: true });
    });

    it('calls the correct endpoint with auth header', async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: {} });
      await UpdateProductFromShoppingCart(3, 4, 'my-token');
      expect(axios.post).toHaveBeenCalledWith(
        expect.stringContaining('/Store/AddProductToShoppingCart?IdUser=1&IdProduct=3&Quantity=4'),
        {},
        { headers: { Authorization: 'Bearer my-token' } }
      );
    });

    it('returns [] on error', async () => {
      (axios.post as jest.Mock).mockRejectedValue(new Error('Network error'));
      const result = await UpdateProductFromShoppingCart(1, 1, 'token');
      expect(result).toEqual([]);
    });
  });

  describe('CompleteCartTransaction', () => {
    it('returns response data on success', async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: { completed: true } });
      const result = await CompleteCartTransaction('token');
      expect(result).toEqual({ completed: true });
    });

    it('calls the correct endpoint with auth header', async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: {} });
      await CompleteCartTransaction('my-token');
      expect(axios.post).toHaveBeenCalledWith(
        expect.stringContaining('/Store/CompleteCarTransaction?IdUser=1'),
        {},
        { headers: { Authorization: 'Bearer my-token' } }
      );
    });

    it('returns [] on error', async () => {
      (axios.post as jest.Mock).mockRejectedValue(new Error('Network error'));
      const result = await CompleteCartTransaction('token');
      expect(result).toEqual([]);
    });
  });
});
