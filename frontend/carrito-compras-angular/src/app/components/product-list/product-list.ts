import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { CartStateService } from '../../services/cart-state.service';
import { Product } from '../../models/interfaces';
import { getToken } from '../../utils/token.util';

@Component({
  selector: 'app-product-list',
  imports: [CommonModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css'
})
export class ProductListComponent implements OnInit {
  products = signal<Product[]>([]);

  constructor(
    private productService: ProductService,
    private cartService: CartService,
    private cartState: CartStateService
  ) {}

  ngOnInit(): void {
    const token = getToken();
    this.productService.getProducts(token).then(products => {
      const sorted = [...products].sort((a, b) => a.sku.localeCompare(b.sku));
      this.products.set(sorted);
    });
  }

  async onAddProduct(product: Product): Promise<void> {
    const token = getToken();
    await this.cartService.addProductToShoppingCart(product.id, 1, token);
    await this.cartState.refresh(token);
  }
}
