import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { StorefrontLayout } from './layouts/storefront-layout';
import { AdminLayout } from './layouts/admin-layout';

export const routes: Routes = [
  // Public storefront
  {
    path: '',
    component: StorefrontLayout,
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/storefront/home').then((m) => m.Home),
      },
      {
        path: 'product/:id',
        loadComponent: () =>
          import('./features/storefront/product-detail').then((m) => m.ProductDetail),
      },
      {
        path: 'cart',
        loadComponent: () => import('./features/storefront/cart').then((m) => m.Cart),
      },
      {
        path: 'checkout',
        loadComponent: () =>
          import('./features/storefront/checkout').then((m) => m.Checkout),
      },
    ],
  },

  // Admin panel
  {
    path: 'admin',
    component: AdminLayout,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'products' },
      {
        path: 'login',
        loadComponent: () => import('./features/login/login').then((m) => m.Login),
      },
      {
        path: 'products',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/products/product-list').then((m) => m.ProductList),
      },
      {
        path: 'products/new',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/products/product-form').then((m) => m.ProductForm),
      },
      {
        path: 'products/:id/edit',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/products/product-form').then((m) => m.ProductForm),
      },
      {
        path: 'orders',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/orders/order-list').then((m) => m.OrderList),
      },
      {
        path: 'orders/:id',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/orders/order-detail').then((m) => m.OrderDetail),
      },
    ],
  },

  { path: '**', redirectTo: '' },
];
