import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AuthMenu } from './Auth';
import * as UserService from '../services/UserService';

jest.mock('../services/UserService', () => ({
  AuthenticateUser: jest.fn(),
}));

jest.mock('sweetalert2', () => ({
  __esModule: true,
  default: { fire: jest.fn().mockResolvedValue({}) },
}));

const mockNavigate = jest.fn();
jest.mock('react-router', () => ({
  ...jest.requireActual('react-router'),
  useNavigate: () => mockNavigate,
}));

import Swal from 'sweetalert2';

const renderAuthMenu = () =>
  render(
    <MemoryRouter>
      <AuthMenu />
    </MemoryRouter>
  );

describe('AuthMenu', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorage.clear();
  });

  it('renders username input, password input and sign-up button', () => {
    renderAuthMenu();
    expect(screen.getByLabelText(/username/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign up/i })).toBeInTheDocument();
  });

  it('renders the application heading', () => {
    renderAuthMenu();
    expect(screen.getByText(/online ordering system/i)).toBeInTheDocument();
  });

  it('shows a warning when the username field is empty', async () => {
    renderAuthMenu();
    fireEvent.click(screen.getByRole('button', { name: /sign up/i }));
    await waitFor(() =>
      expect(Swal.fire).toHaveBeenCalledWith(
        'Oops...',
        'Username and Password Should not be Void',
        'warning'
      )
    );
  });

  it('shows a warning when only password is missing', async () => {
    renderAuthMenu();
    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'user' } });
    fireEvent.click(screen.getByRole('button', { name: /sign up/i }));
    await waitFor(() =>
      expect(Swal.fire).toHaveBeenCalledWith(
        'Oops...',
        'Username and Password Should not be Void',
        'warning'
      )
    );
  });

  it('navigates to /home and stores user in localStorage on valid credentials', async () => {
    const mockUser = {
      id: 1,
      username: 'admin',
      name: 'Admin',
      session: { id: 10, userId: 1, sessionStart: null, sessionEnd: null },
    };
    (UserService.AuthenticateUser as jest.Mock).mockResolvedValue(mockUser);

    renderAuthMenu();
    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'admin' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'secret' } });
    fireEvent.click(screen.getByRole('button', { name: /sign up/i }));

    await waitFor(() => expect(mockNavigate).toHaveBeenCalledWith('/home'));
    expect(localStorage.getItem('userLogged')).toBe(JSON.stringify(mockUser));
  });

  it('shows a warning when credentials are invalid', async () => {
    (UserService.AuthenticateUser as jest.Mock).mockResolvedValue(undefined);

    renderAuthMenu();
    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'admin' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'wrong' } });
    fireEvent.click(screen.getByRole('button', { name: /sign up/i }));

    await waitFor(() =>
      expect(Swal.fire).toHaveBeenCalledWith('Oops...', 'Invalid Credentials', 'warning')
    );
    expect(mockNavigate).not.toHaveBeenCalled();
  });

  it('calls AuthenticateUser with provided credentials', async () => {
    (UserService.AuthenticateUser as jest.Mock).mockResolvedValue(undefined);

    renderAuthMenu();
    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'testuser' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'testpass' } });
    fireEvent.click(screen.getByRole('button', { name: /sign up/i }));

    await waitFor(() =>
      expect(UserService.AuthenticateUser).toHaveBeenCalledWith('testuser', 'testpass')
    );
  });
});
