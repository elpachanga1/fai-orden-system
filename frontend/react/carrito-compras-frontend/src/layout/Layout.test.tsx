import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { Layout } from './Layout';
import * as tokenUtil from '../utils/tokenUtil';

jest.mock('../utils/tokenUtil', () => ({ getToken: jest.fn() }));

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  Outlet: () => <div data-testid="outlet" />,
}));

const mockNavigate = jest.fn();
jest.mock('react-router', () => ({
  ...jest.requireActual('react-router'),
  useNavigate: () => mockNavigate,
  useLocation: () => ({ pathname: '/home' }),
}));

jest.mock('react-bootstrap', () => ({
  NavDropdown: Object.assign(
    ({ children, show }: { children: React.ReactNode; show: boolean }) =>
      show ? <div role="menu">{children}</div> : null,
    {
      Item: ({ children, onClick }: { children: React.ReactNode; onClick?: () => void }) => (
        <button onClick={onClick}>{children}</button>
      ),
    }
  ),
}));

const renderLayout = () =>
  render(
    <MemoryRouter>
      <Layout />
    </MemoryRouter>
  );

describe('Layout', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorage.clear();
  });

  it('does not render the header when the token is empty', () => {
    (tokenUtil.getToken as jest.Mock).mockReturnValue('');
    renderLayout();
    expect(screen.queryByAltText('mdo')).not.toBeInTheDocument();
  });

  it('renders the avatar in the header when a token is present', () => {
    (tokenUtil.getToken as jest.Mock).mockReturnValue('valid-token');
    renderLayout();
    expect(screen.getByAltText('mdo')).toBeInTheDocument();
  });

  it('always renders the Outlet', () => {
    (tokenUtil.getToken as jest.Mock).mockReturnValue('');
    renderLayout();
    expect(screen.getByTestId('outlet')).toBeInTheDocument();
  });

  it('shows the dropdown menu when the avatar is clicked', () => {
    (tokenUtil.getToken as jest.Mock).mockReturnValue('valid-token');
    renderLayout();
    expect(screen.queryByRole('menu')).not.toBeInTheDocument();
    fireEvent.click(screen.getByAltText('mdo'));
    expect(screen.getByRole('menu')).toBeInTheDocument();
  });

  it('hides the dropdown menu on second click', () => {
    (tokenUtil.getToken as jest.Mock).mockReturnValue('valid-token');
    renderLayout();
    const avatar = screen.getByAltText('mdo');
    fireEvent.click(avatar);
    fireEvent.click(avatar);
    expect(screen.queryByRole('menu')).not.toBeInTheDocument();
  });

  it('removes userLogged from localStorage on logout', () => {
    (tokenUtil.getToken as jest.Mock).mockReturnValue('valid-token');
    localStorage.setItem('userLogged', JSON.stringify({ id: 1 }));
    renderLayout();
    fireEvent.click(screen.getByAltText('mdo'));
    fireEvent.click(screen.getByText('Logout'));
    expect(localStorage.getItem('userLogged')).toBeNull();
  });

  it('navigates to /login on logout', () => {
    (tokenUtil.getToken as jest.Mock).mockReturnValue('valid-token');
    renderLayout();
    fireEvent.click(screen.getByAltText('mdo'));
    fireEvent.click(screen.getByText('Logout'));
    expect(mockNavigate).toHaveBeenCalledWith('/login');
  });
});
