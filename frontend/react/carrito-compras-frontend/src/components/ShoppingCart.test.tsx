import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ShoppingCartMenu } from './ShoppingCart';
import * as CartService from '../services/CartService';
import * as ShoppingCartUtils from '../utils/ShoppingCartUtils';
import * as tokenUtil from '../utils/tokenUtil';
import Swal from 'sweetalert2';

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

jest.mock('sweetalert2', () => ({
  __esModule: true,
  default: { fire: jest.fn().mockResolvedValue({ isConfirmed: false }) },
}));

// Stub Counter so ShoppingCart tests are isolated from Counter internals
jest.mock('./Counter', () => ({
  Counter: ({ quantity }: { quantity: number }) => (
    <span data-testid="counter">{quantity}</span>
  ),
}));

const emptyCart = { items: [], countProducts: 0, total: 0 };

const mockItem = {
  id: 101,
  name: 'Item A',
  idProduct: 1,
  quantity: 2,
  isDeleted: false,
  totalPrice: 30,
  productReference: {
    id: 1,
    sku: 'A001',
    name: 'Product Alpha',
    unitPrice: 15,
    availableUnits: 10,
  },
};

const cartWithItems = {
  items: [mockItem],
  countProducts: 2,
  total: 30,
};

describe('ShoppingCartMenu', () => {
  const setShoppingCart = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (tokenUtil.getToken as jest.Mock).mockReturnValue('mock-token');
    (ShoppingCartUtils.getShoppingCart as jest.Mock).mockResolvedValue(emptyCart);
    (CartService.DeleteProductFromShoppingCart as jest.Mock).mockResolvedValue({});
    (CartService.EmptyShoppingCart as jest.Mock).mockResolvedValue({});
    (CartService.CompleteCartTransaction as jest.Mock).mockResolvedValue({});
    (CartService.addProductToShoppingCart as jest.Mock).mockResolvedValue({});
  });

  it('displays the product count from the shoppingCart prop', () => {
    render(<ShoppingCartMenu shoppingCart={cartWithItems} setShoppingCart={setShoppingCart} />);
    expect(screen.getByText('2', { selector: '#contador-productos' })).toBeInTheDocument();
  });

  it('shows "El carrito está vacío" when there are no items', () => {
    render(<ShoppingCartMenu shoppingCart={emptyCart} setShoppingCart={setShoppingCart} />);
    expect(screen.getByText('El carrito está vacío')).toBeInTheDocument();
  });

  it('renders item name and total price when cart has items', () => {
    render(<ShoppingCartMenu shoppingCart={cartWithItems} setShoppingCart={setShoppingCart} />);
    expect(screen.getByText('Product Alpha')).toBeInTheDocument();
    expect(screen.getByText('$30', { selector: '.precio-producto-carrito' })).toBeInTheDocument();
  });

  it('renders a Counter for each cart item', () => {
    render(<ShoppingCartMenu shoppingCart={cartWithItems} setShoppingCart={setShoppingCart} />);
    const counters = screen.getAllByTestId('counter');
    expect(counters).toHaveLength(1);
    expect(counters[0]).toHaveTextContent('2');
  });

  it('renders "Vaciar Carrito" and "Finalizar Compra" buttons when cart has items', () => {
    render(<ShoppingCartMenu shoppingCart={cartWithItems} setShoppingCart={setShoppingCart} />);
    expect(screen.getByRole('button', { name: /vaciar carrito/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /finalizar compra/i })).toBeInTheDocument();
  });

  it('toggles cart visibility when the cart icon is clicked', () => {
    render(<ShoppingCartMenu shoppingCart={emptyCart} setShoppingCart={setShoppingCart} />);
    // eslint-disable-next-line testing-library/no-node-access
    const cartPanel = document.querySelector('.container-cart-products')!;
    expect(cartPanel).toHaveClass('hidden-cart');
    // eslint-disable-next-line testing-library/no-node-access
    fireEvent.click(document.querySelector('.container-cart-icon')!);
    expect(cartPanel).not.toHaveClass('hidden-cart');
  });

  it('hides the cart panel again on second click', () => {
    render(<ShoppingCartMenu shoppingCart={emptyCart} setShoppingCart={setShoppingCart} />);
    // eslint-disable-next-line testing-library/no-node-access
    const icon = document.querySelector('.container-cart-icon')!;
    fireEvent.click(icon);
    fireEvent.click(icon);
    // eslint-disable-next-line testing-library/no-node-access
    expect(document.querySelector('.container-cart-products')).toHaveClass('hidden-cart');
  });

  it('calls DeleteProductFromShoppingCart and updates cart when delete icon is clicked', async () => {
    (ShoppingCartUtils.getShoppingCart as jest.Mock).mockResolvedValue(emptyCart);
    render(<ShoppingCartMenu shoppingCart={cartWithItems} setShoppingCart={setShoppingCart} />);

    // eslint-disable-next-line testing-library/no-node-access
    const closeIcon = document.querySelector('.icon-close')!;
    fireEvent.click(closeIcon);

    await waitFor(() =>
      expect(CartService.DeleteProductFromShoppingCart).toHaveBeenCalledWith(
        mockItem.id,
        'mock-token'
      )
    );
    await waitFor(() => expect(setShoppingCart).toHaveBeenCalled());
  });

  it('calls EmptyShoppingCart and clears cart when "Vaciar Carrito" is confirmed', async () => {
    (Swal.fire as jest.Mock).mockResolvedValue({ isConfirmed: true });
    render(<ShoppingCartMenu shoppingCart={cartWithItems} setShoppingCart={setShoppingCart} />);

    fireEvent.click(screen.getByRole('button', { name: /vaciar carrito/i }));

    await waitFor(() => expect(CartService.EmptyShoppingCart).toHaveBeenCalledWith('mock-token'));
    await waitFor(() =>
      expect(setShoppingCart).toHaveBeenCalledWith({ items: [], countProducts: 0, total: 0 })
    );
  });

  it('does not empty the cart when "Vaciar Carrito" is cancelled', async () => {
    (Swal.fire as jest.Mock).mockResolvedValue({ isConfirmed: false });
    render(<ShoppingCartMenu shoppingCart={cartWithItems} setShoppingCart={setShoppingCart} />);

    fireEvent.click(screen.getByRole('button', { name: /vaciar carrito/i }));

    await waitFor(() => expect(Swal.fire).toHaveBeenCalled());
    expect(CartService.EmptyShoppingCart).not.toHaveBeenCalled();
  });

  it('calls CompleteCartTransaction and clears cart when "Finalizar Compra" is clicked', async () => {
    (Swal.fire as jest.Mock).mockResolvedValue({});
    render(<ShoppingCartMenu shoppingCart={cartWithItems} setShoppingCart={setShoppingCart} />);

    fireEvent.click(screen.getByRole('button', { name: /finalizar compra/i }));

    await waitFor(() => expect(CartService.CompleteCartTransaction).toHaveBeenCalledWith('mock-token'));
    await waitFor(() =>
      expect(setShoppingCart).toHaveBeenCalledWith({ items: [], countProducts: 0, total: 0 })
    );
  });

  it('fetches and sets the shopping cart on mount', async () => {
    (ShoppingCartUtils.getShoppingCart as jest.Mock).mockResolvedValue(cartWithItems);
    render(<ShoppingCartMenu shoppingCart={emptyCart} setShoppingCart={setShoppingCart} />);
    await waitFor(() => expect(setShoppingCart).toHaveBeenCalledWith(cartWithItems));
  });
});
