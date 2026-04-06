import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly apiUrl = environment.apiUrl;
  private readonly idUser = 1;

  constructor(private http: HttpClient) {}

  private headers(token: string): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  async addProductToShoppingCart(productId: number, quantity: number, token: string): Promise<any> {
    try {
      return await firstValueFrom(
        this.http.post<any>(
          `${this.apiUrl}/Store/AddProductToShoppingCart?IdUser=${this.idUser}&IdProduct=${productId}&Quantity=${quantity}`,
          {},
          { headers: this.headers(token) }
        )
      );
    } catch (error) {
      console.error('Error adding product to cart:', error);
    }
  }

  async emptyShoppingCart(token: string): Promise<any> {
    try {
      return await firstValueFrom(
        this.http.delete<any>(
          `${this.apiUrl}/Store/EmptyShoppingCart?IdUser=${this.idUser}`,
          { headers: this.headers(token) }
        )
      );
    } catch (error) {
      console.error('Error emptying cart:', error);
    }
  }

  async deleteProductFromShoppingCart(idItem: number, token: string): Promise<any> {
    try {
      return await firstValueFrom(
        this.http.delete<any>(
          `${this.apiUrl}/Store/DeleteProductFromShoppingCart?IdUser=${this.idUser}&IdItem=${idItem}`,
          { headers: this.headers(token) }
        )
      );
    } catch (error) {
      console.error('Error deleting product from cart:', error);
    }
  }

  async completeCartTransaction(token: string): Promise<any> {
    try {
      return await firstValueFrom(
        this.http.post<any>(
          `${this.apiUrl}/Store/CompleteCarTransaction?IdUser=${this.idUser}`,
          {},
          { headers: this.headers(token) }
        )
      );
    } catch (error) {
      console.error('Error completing transaction:', error);
    }
  }
}
