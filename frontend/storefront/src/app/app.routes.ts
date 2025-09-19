import { Routes } from '@angular/router';
import { HomePage } from './pages/shop/home.page';
import { DashboardPage } from './pages/admin/dashboard.page';
import { LoginPage } from './pages/auth/login.page';
import { ProductCreatePage } from './pages/admin/product/product-create.page/product-create.page';
import { adminGuard } from './core/auth/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'shop', pathMatch: 'full' },
  { path: 'shop', component: HomePage },
  { path: 'login', component: LoginPage },
  { path: 'admin', component: DashboardPage },
  { path: 'admin/products/create', component: ProductCreatePage, canActivate: [adminGuard] }
];
