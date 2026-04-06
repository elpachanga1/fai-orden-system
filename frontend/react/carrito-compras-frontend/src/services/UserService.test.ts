import axios from 'axios';
import { AuthenticateUser } from './UserService';

jest.mock('axios', () => ({
  __esModule: true,
  default: { post: jest.fn() },
}));

const mockUser = {
  id: 1,
  username: 'admin',
  name: 'Admin User',
  session: { id: 10, userId: 1, sessionStart: null, sessionEnd: null },
};

describe('UserService', () => {
  beforeEach(() => jest.clearAllMocks());

  describe('AuthenticateUser', () => {
    it('returns user data on successful authentication', async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: mockUser });
      const result = await AuthenticateUser('admin', 'secret');
      expect(result).toEqual(mockUser);
    });

    it('calls the correct endpoint with username and password', async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: mockUser });
      await AuthenticateUser('testuser', 'testpass');
      expect(axios.post).toHaveBeenCalledWith(
        expect.stringContaining('/User/AuthenticateUser?username=testuser&password=testpass')
      );
    });

    it('returns undefined on error', async () => {
      (axios.post as jest.Mock).mockRejectedValue(new Error('Unauthorized'));
      const result = await AuthenticateUser('admin', 'wrong');
      expect(result).toBeUndefined();
    });

    it('returns undefined when the API returns undefined data', async () => {
      (axios.post as jest.Mock).mockResolvedValue({ data: undefined });
      const result = await AuthenticateUser('admin', 'secret');
      expect(result).toBeUndefined();
    });
  });
});
