import React from 'react';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import Home from './Home';

jest.mock('./Header', () => ({
  Header: () => <div data-testid="header" />,
}));

jest.mock('./ProductList', () => ({
  ProductList: () => <div data-testid="product-list" />,
}));

const mockNavigate = jest.fn();
jest.mock('react-router', () => ({
  ...jest.requireActual('react-router'),
  useNavigate: () => mockNavigate,
}));

const renderHome = () =>
  render(
    <MemoryRouter>
      <Home />
    </MemoryRouter>
  );

describe('Home', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorage.clear();
  });

  it('redirects to /login when no user is stored in localStorage', () => {
    renderHome();
    expect(mockNavigate).toHaveBeenCalledWith('/login');
  });

  it('redirects to /login when localStorage contains an empty object', () => {
    localStorage.setItem('userLogged', '{}');
    renderHome();
    expect(mockNavigate).toHaveBeenCalledWith('/login');
  });

  it('renders Header and ProductList when a user is logged in', () => {
    localStorage.setItem('userLogged', JSON.stringify({ id: 1, username: 'admin' }));
    renderHome();
    expect(screen.getByTestId('header')).toBeInTheDocument();
    expect(screen.getByTestId('product-list')).toBeInTheDocument();
  });

  it('does not redirect when a user is logged in', () => {
    localStorage.setItem('userLogged', JSON.stringify({ id: 1, username: 'admin' }));
    renderHome();
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
