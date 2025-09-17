import { Routes } from '@angular/router';
import { HomePage } from './pages/shop/home.page';
import { DashboardPage } from './pages/admin/dashboard.page';
import { LoginPage } from './pages/auth/login.page';

export const routes: Routes = [
  { path: '', redirectTo: 'shop', pathMatch: 'full' },
  { path: 'shop', component: HomePage },
  { path: 'login', component: LoginPage },
  { path: 'admin', component: DashboardPage }
];
