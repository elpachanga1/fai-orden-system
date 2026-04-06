import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';
import { CartService } from '../../services/cart.service';
import { CartStateService } from '../../services/cart-state.service';
import { Item } from '../../models/interfaces';
import { getToken } from '../../utils/token.util';
import { CounterComponent } from '../counter/counter';

@Component({
  selector: 'app-shopping-cart',
  imports: [CommonModule, CounterComponent],
  templateUrl: './shopping-cart.html',
  styleUrl: './shopping-cart.css'
})
export class ShoppingCartComponent implements OnInit {
  active = signal(false);

  constructor(
    public cartState: CartStateService,
    private cartService: CartService
  ) {}

  ngOnInit(): void {
    const token = getToken();
    this.cartState.refresh(token);
  }

  toggleCart(): void {
    this.active.update(v => !v);
  }

  async onDeleteProduct(item: Item): Promise<void> {
    const token = getToken();
    await this.cartService.deleteProductFromShoppingCart(item.id, token);
    await this.cartState.refresh(token);
    Swal.fire('Deleted!', `Item ${item.id} has been deleted from the shopping cart.`, 'success');
  }

  onCleanCart(): void {
    Swal.fire({
      title: 'Are you sure?',
      text: "You won't be able to revert this!",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Yes, delete it!'
    }).then(async result => {
      if (result.isConfirmed) {
        const token = getToken();
        await this.cartService.emptyShoppingCart(token);
        this.cartState.clear();
        Swal.fire('Deleted!', 'Shopping Cart Empty', 'success');
      }
    });
  }

  onPurchase(): void {
    Swal.fire('Purchase', 'Purchase Completed', 'success').then(async () => {
      const token = getToken();
      await this.cartService.completeCartTransaction(token);
      this.cartState.clear();
    });
  }

  async handleRemoveItem(itemId: number): Promise<void> {
    const token = getToken();
    await this.cartService.deleteProductFromShoppingCart(itemId, token);
    await this.cartState.refresh(token);
  }

  async handleUpdateQuantity(productId: number, quantity: number): Promise<void> {
    const token = getToken();
    await this.cartService.addProductToShoppingCart(productId, quantity, token);
    await this.cartState.refresh(token);
  }
}
