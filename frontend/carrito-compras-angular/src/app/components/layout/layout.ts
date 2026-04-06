import { Component, OnInit, signal } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';
import { getToken } from '../../utils/token.util';

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, CommonModule],
  templateUrl: './layout.html',
  styleUrl: './layout.css'
})
export class LayoutComponent implements OnInit {
  token = signal('');
  showDropdown = signal(false);

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.token.set(getToken());
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => this.token.set(getToken()));
  }

  logout(): void {
    localStorage.removeItem('userLogged');
    this.showDropdown.set(false);
    this.router.navigate(['/login']);
  }

  toggleDropdown(): void {
    this.showDropdown.update(v => !v);
  }
}
