import { getToken } from './tokenUtil';

describe('getToken', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('returns the token when userLogged has a valid sessionReference.token', () => {
    localStorage.setItem(
      'userLogged',
      JSON.stringify({ sessionReference: { token: 'my-token-123' } })
    );
    expect(getToken()).toBe('my-token-123');
  });

  it('returns an empty string when localStorage has no userLogged key', () => {
    expect(getToken()).toBe('');
  });

  it('returns an empty string when sessionReference is absent', () => {
    localStorage.setItem('userLogged', JSON.stringify({ id: 1, username: 'user' }));
    expect(getToken()).toBe('');
  });

  it('returns an empty string when token property is missing inside sessionReference', () => {
    localStorage.setItem('userLogged', JSON.stringify({ sessionReference: {} }));
    expect(getToken()).toBe('');
  });
});
