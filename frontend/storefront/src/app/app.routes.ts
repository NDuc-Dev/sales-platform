import { Routes } from '@angular/router';
import { HomePage } from './pages/shop/home.page';
import { DashboardPage } from './pages/admin/dashboard.page';

export const routes: Routes = [
  { path: '', redirectTo: 'shop', pathMatch: 'full' },
  { path: 'shop', component: HomePage },
  { path: 'admin', component: DashboardPage }
];
