import { Routes } from '@angular/router';
import { LayoutComponent } from './components/layout/layout';
import { AuthComponent } from './components/auth/auth';
import { HomeComponent } from './components/home/home';
import { NoMatchComponent } from './components/no-match/no-match';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: '', redirectTo: 'login', pathMatch: 'full' },
      { path: 'login', component: AuthComponent },
      { path: 'home', component: HomeComponent, canActivate: [authGuard] },
      { path: '**', component: NoMatchComponent }
    ]
  }
];
