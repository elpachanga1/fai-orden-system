import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ProductList } from './ProductList';
import * as ProductService from '../services/ProductService';
import * as CartService from '../services/CartService';
import * as ShoppingCartUtils from '../utils/ShoppingCartUtils';
import * as tokenUtil from '../utils/tokenUtil';

jest.mock('../services/ProductService', () => ({
  getProducts: jest.fn(),
}));
jest.mock('../services/CartService', () => ({
  addProductToShoppingCart: jest.fn(),
  DeleteProductFromShoppingCart: jest.fn(),
  EmptyShoppingCart: jest.fn(),
  CompleteCartTransaction: jest.fn(),
}));
jest.mock('../utils/ShoppingCartUtils', () => ({
  getShoppingCart: jest.fn(),
}));
jest.mock('../utils/tokenUtil', () => ({
  getToken: jest.fn(),
}));

const mockProducts = [
  {
    id: 1,
    sku: 'A001',
    name: 'Product Alpha',
    unitPrice: 10,
    availableUnits: 5,
    image: 'http://example.com/alpha.jpg',
  },
  {
    id: 2,
    sku: 'B002',
    name: 'Product Beta',
    unitPrice: 20,
    availableUnits: 3,
    image: 'http://example.com/beta.jpg',
  },
];

const mockCart = { items: [], countProducts: 1, total: 10 };

describe('ProductList', () => {
  const setShoppingCart = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (tokenUtil.getToken as jest.Mock).mockReturnValue('mock-token');
    (ProductService.getProducts as jest.Mock).mockResolvedValue(mockProducts);
    (CartService.addProductToShoppingCart as jest.Mock).mockResolvedValue({});
    (ShoppingCartUtils.getShoppingCart as jest.Mock).mockResolvedValue(mockCart);
  });

  it('renders all products returned from the service', async () => {
    render(<ProductList setShoppingCart={setShoppingCart} />);
    await screen.findByText('Product Alpha');
    expect(screen.getByText('Product Beta')).toBeInTheDocument();
  });

  it('renders price for each product', async () => {
    render(<ProductList setShoppingCart={setShoppingCart} />);
    await screen.findByText('$10');
    expect(screen.getByText('$20')).toBeInTheDocument();
  });

  it('renders product images with correct alt text', async () => {
    render(<ProductList setShoppingCart={setShoppingCart} />);
    await screen.findByAltText('Product Alpha');
    expect(screen.getByAltText('Product Beta')).toBeInTheDocument();
  });

  it('renders an "Añadir al carrito" button for each product', async () => {
    render(<ProductList setShoppingCart={setShoppingCart} />);
    await waitFor(() =>
      expect(screen.getAllByRole('button', { name: /añadir al carrito/i })).toHaveLength(2)
    );
  });

  it('calls getProducts with the token on mount', async () => {
    render(<ProductList setShoppingCart={setShoppingCart} />);
    await waitFor(() => expect(ProductService.getProducts).toHaveBeenCalledWith('mock-token'));
  });

  it('calls addProductToShoppingCart with the correct product id and quantity', async () => {
    render(<ProductList setShoppingCart={setShoppingCart} />);
    const buttons = await screen.findAllByRole('button', { name: /añadir al carrito/i });
    // Products are sorted by sku, so A001 (id=1) comes first
    fireEvent.click(buttons[0]);
    await waitFor(() =>
      expect(CartService.addProductToShoppingCart).toHaveBeenCalledWith(1, 1, 'mock-token')
    );
  });

  it('updates the shopping cart after adding a product', async () => {
    render(<ProductList setShoppingCart={setShoppingCart} />);
    const buttons = await screen.findAllByRole('button', { name: /añadir al carrito/i });
    fireEvent.click(buttons[0]);
    await waitFor(() => expect(setShoppingCart).toHaveBeenCalledWith(mockCart));
  });

  it('renders nothing when the service returns an empty list', async () => {
    (ProductService.getProducts as jest.Mock).mockResolvedValue([]);
    render(<ProductList setShoppingCart={setShoppingCart} />);
    await waitFor(() => expect(ProductService.getProducts).toHaveBeenCalled());
    expect(screen.queryAllByRole('button', { name: /añadir al carrito/i })).toHaveLength(0);
  });
});
