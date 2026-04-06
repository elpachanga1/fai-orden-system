import React from 'react';
import { render, screen } from '@testing-library/react';
import { Header } from './Header';

jest.mock('./ShoppingCart', () => ({
  ShoppingCartMenu: ({ shoppingCart }: { shoppingCart: any }) => (
    <div data-testid="shopping-cart-menu">
      <span data-testid="cart-count">{shoppingCart.countProducts}</span>
    </div>
  ),
}));

describe('Header', () => {
  const mockSetShoppingCart = jest.fn();

  const defaultCart = { items: [], countProducts: 0, total: 0 };

  it('renders the store title', () => {
    render(<Header shoppingCart={defaultCart} setShoppingCart={mockSetShoppingCart} />);
    expect(screen.getByRole('heading', { name: /tienda/i })).toBeInTheDocument();
  });

  it('renders the ShoppingCartMenu component', () => {
    render(<Header shoppingCart={defaultCart} setShoppingCart={mockSetShoppingCart} />);
    expect(screen.getByTestId('shopping-cart-menu')).toBeInTheDocument();
  });

  it('passes the shoppingCart prop to ShoppingCartMenu', () => {
    const cart = { items: [], countProducts: 5, total: 100 };
    render(<Header shoppingCart={cart} setShoppingCart={mockSetShoppingCart} />);
    expect(screen.getByTestId('cart-count')).toHaveTextContent('5');
  });
});
