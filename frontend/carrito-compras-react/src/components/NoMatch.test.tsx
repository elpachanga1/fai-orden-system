import React from 'react';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { NoMatch } from './NoMatch';

describe('NoMatch', () => {
  const renderNoMatch = () =>
    render(
      <MemoryRouter>
        <NoMatch />
      </MemoryRouter>
    );

  it('renders the "Nothing to see here!" heading', () => {
    renderNoMatch();
    expect(screen.getByRole('heading', { name: /nothing to see here!/i })).toBeInTheDocument();
  });

  it('renders a link to the home page', () => {
    renderNoMatch();
    const link = screen.getByRole('link', { name: /go to the home page/i });
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute('href', '/');
  });
});
