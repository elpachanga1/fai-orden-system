import React from 'react';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import App from './App';

jest.mock('./layout/Layout', () => {
  const { Outlet } = jest.requireActual('react-router-dom');
  return {
    Layout: () => (
      <div data-testid="layout">
        <Outlet />
      </div>
    ),
  };
});

jest.mock('./components/Auth', () => ({
  AuthMenu: () => <div data-testid="auth-menu" />,
}));

jest.mock('./components/Home', () => () => <div data-testid="home" />);

jest.mock('./components/NoMatch', () => ({
  NoMatch: () => <div data-testid="no-match" />,
}));

const renderAt = (path: string) =>
  render(
    <MemoryRouter initialEntries={[path]}>
      <App />
    </MemoryRouter>
  );

describe('App', () => {
  it('renders the Layout wrapper on every route', () => {
    renderAt('/login');
    expect(screen.getByTestId('layout')).toBeInTheDocument();
  });

  it('redirects / to /login and renders AuthMenu', () => {
    renderAt('/');
    expect(screen.getByTestId('auth-menu')).toBeInTheDocument();
  });

  it('renders AuthMenu at /login', () => {
    renderAt('/login');
    expect(screen.getByTestId('auth-menu')).toBeInTheDocument();
  });

  it('renders Home at /home', () => {
    renderAt('/home');
    expect(screen.getByTestId('home')).toBeInTheDocument();
  });

  it('renders NoMatch for unknown routes', () => {
    renderAt('/unknown-route');
    expect(screen.getByTestId('no-match')).toBeInTheDocument();
  });
});
