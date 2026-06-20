import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CartService } from '../core/cart.service';
import { TranslatePipe } from '../core/translate.pipe';
import { SettingsControls } from '../shared/settings-controls';

@Component({
  selector: 'app-storefront-layout',
  imports: [RouterOutlet, RouterLink, TranslatePipe, SettingsControls],
  templateUrl: './storefront-layout.html',
  styleUrl: './storefront-layout.scss',
})
export class StorefrontLayout {
  private cart = inject(CartService);
  readonly cartCount = this.cart.count;
}
