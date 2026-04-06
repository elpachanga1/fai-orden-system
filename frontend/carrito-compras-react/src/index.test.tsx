import React from 'react';

jest.mock('react-dom/client', () => ({
  createRoot: jest.fn(),
}));

jest.mock('./App', () => () => null);

jest.mock('react-router-dom', () => ({
  BrowserRouter: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

describe('index', () => {
  it('calls createRoot with the root DOM element and renders the app', () => {
    const mockRender = jest.fn();
    const ReactDOMClient = require('react-dom/client');
    (ReactDOMClient.createRoot as jest.Mock).mockReturnValue({ render: mockRender });

    const div = document.createElement('div');
    div.id = 'root';
    document.body.appendChild(div);

    jest.isolateModules(() => {
      require('./index');
    });

    expect(ReactDOMClient.createRoot).toHaveBeenCalledWith(div);
    expect(mockRender).toHaveBeenCalled();

    document.body.removeChild(div);
  });
});
