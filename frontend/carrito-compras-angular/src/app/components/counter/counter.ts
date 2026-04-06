import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-counter',
  imports: [CommonModule],
  templateUrl: './counter.html',
  styleUrl: './counter.css'
})
export class CounterComponent implements OnInit {
  @Input() quantity: number = 1;
  @Input() productId: number = 0;
  @Input() itemId: number = 0;

  @Output() remove = new EventEmitter<void>();
  @Output() updateQuantity = new EventEmitter<{ productId: number; quantity: number }>();

  value = signal<number>(1);

  ngOnInit(): void {
    this.value.set(this.quantity);
  }

  reduce(): void {
    const updated = this.value() - 1;
    if (updated === 0) {
      this.remove.emit();
    } else {
      this.value.set(updated);
      this.updateQuantity.emit({ productId: this.productId, quantity: updated });
    }
  }

  increase(): void {
    const updated = this.value() + 1;
    this.value.set(updated);
    this.updateQuantity.emit({ productId: this.productId, quantity: updated });
  }
}
