import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { Product } from '../models/interfaces';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  async getProducts(token: string): Promise<Product[]> {
    try {
      const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
      return await firstValueFrom(
        this.http.get<Product[]>(`${this.apiUrl}/Product/GetAllProducts`, { headers })
      );
    } catch (error) {
      console.error('Error fetching products:', error);
      return [];
    }
  }
}
