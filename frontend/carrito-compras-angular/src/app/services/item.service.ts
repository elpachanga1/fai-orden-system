import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { Item } from '../models/interfaces';

@Injectable({ providedIn: 'root' })
export class ItemService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  async getItems(token: string): Promise<Item[]> {
    try {
      const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
      return await firstValueFrom(
        this.http.get<Item[]>(`${this.apiUrl}/Item/GetAllItems`, { headers })
      );
    } catch (error) {
      console.error('Error fetching items:', error);
      return [];
    }
  }

  async getItemsByProductId(productId: number, token: string): Promise<Item[]> {
    try {
      const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
      return await firstValueFrom(
        this.http.get<Item[]>(`${this.apiUrl}/Product/GetItemsByProductId/${productId}`, { headers })
      );
    } catch (error) {
      console.error('Error fetching items by product:', error);
      return [];
    }
  }
}
