import { Component } from '@angular/core';
import { ProductListComponent } from '../product-list/product-list';
import { ShoppingCartComponent } from '../shopping-cart/shopping-cart';

@Component({
  selector: 'app-home',
  imports: [ProductListComponent, ShoppingCartComponent],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class HomeComponent {}
