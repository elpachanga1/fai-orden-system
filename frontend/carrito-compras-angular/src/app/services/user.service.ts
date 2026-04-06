import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { User } from '../models/interfaces';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  async authenticateUser(username: string, password: string): Promise<User | undefined> {
    try {
      const user = await firstValueFrom(
        this.http.post<User>(
          `${this.apiUrl}/User/AuthenticateUser?username=${encodeURIComponent(username)}&password=${encodeURIComponent(password)}`,
          null
        )
      );
      return user;
    } catch (error) {
      console.error('Error authenticating user:', error);
      return undefined;
    }
  }
}
