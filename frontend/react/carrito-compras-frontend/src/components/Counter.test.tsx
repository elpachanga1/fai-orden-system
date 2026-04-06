import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { Counter } from './Counter';

describe('Counter', () => {
  const removeProductCallback = jest.fn();
  const handleUpdateQuantity = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders the initial quantity', () => {
    render(
      <Counter
        removeProductCallback={removeProductCallback}
        handleUpdateQuantity={handleUpdateQuantity}
        productId={1}
        quantity={3}
      />
    );
    expect(screen.getByText('3')).toBeInTheDocument();
  });

  it('shows trash icon when quantity is 1', () => {
    render(
      <Counter
        removeProductCallback={removeProductCallback}
        handleUpdateQuantity={handleUpdateQuantity}
        productId={1}
        quantity={1}
      />
    );
    expect(screen.getByAltText('Remove item')).toBeInTheDocument();
    expect(screen.queryByAltText('Decrease quantity')).not.toBeInTheDocument();
  });

  it('shows remove-circle icon when quantity is greater than 1', () => {
    render(
      <Counter
        removeProductCallback={removeProductCallback}
        handleUpdateQuantity={handleUpdateQuantity}
        productId={1}
        quantity={2}
      />
    );
    expect(screen.getByAltText('Decrease quantity')).toBeInTheDocument();
    expect(screen.queryByAltText('Remove item')).not.toBeInTheDocument();
  });

  it('increases the displayed quantity when add-circle is clicked', () => {
    render(
      <Counter
        removeProductCallback={removeProductCallback}
        handleUpdateQuantity={handleUpdateQuantity}
        productId={1}
        quantity={2}
      />
    );
    fireEvent.click(screen.getByAltText('Increase quantity'));
    expect(screen.getByText('3')).toBeInTheDocument();
  });

  it('calls handleUpdateQuantity with incremented value on increase', () => {
    render(
      <Counter
        removeProductCallback={removeProductCallback}
        handleUpdateQuantity={handleUpdateQuantity}
        productId={1}
        quantity={2}
      />
    );
    fireEvent.click(screen.getByAltText('Increase quantity'));
    expect(handleUpdateQuantity).toHaveBeenCalledWith(1, 3);
  });

  it('decreases the displayed quantity when reduce button is clicked', () => {
    render(
      <Counter
        removeProductCallback={removeProductCallback}
        handleUpdateQuantity={handleUpdateQuantity}
        productId={1}
        quantity={3}
      />
    );
    fireEvent.click(screen.getByAltText('Decrease quantity'));
    expect(screen.getByText('2')).toBeInTheDocument();
  });

  it('calls handleUpdateQuantity with decremented value on reduce', () => {
    render(
      <Counter
        removeProductCallback={removeProductCallback}
        handleUpdateQuantity={handleUpdateQuantity}
        productId={1}
        quantity={3}
      />
    );
    fireEvent.click(screen.getByAltText('Decrease quantity'));
    expect(handleUpdateQuantity).toHaveBeenCalledWith(1, 2);
  });

  it('calls removeProductCallback when quantity reaches zero', () => {
    render(
      <Counter
        removeProductCallback={removeProductCallback}
        handleUpdateQuantity={handleUpdateQuantity}
        productId={1}
        quantity={1}
      />
    );
    fireEvent.click(screen.getByAltText('Remove item'));
    expect(removeProductCallback).toHaveBeenCalledTimes(1);
    expect(handleUpdateQuantity).not.toHaveBeenCalled();
  });

  it('does not call removeProductCallback when decreasing from more than 1', () => {
    render(
      <Counter
        removeProductCallback={removeProductCallback}
        handleUpdateQuantity={handleUpdateQuantity}
        productId={1}
        quantity={2}
      />
    );
    fireEvent.click(screen.getByAltText('Decrease quantity'));
    expect(removeProductCallback).not.toHaveBeenCalled();
  });
});
