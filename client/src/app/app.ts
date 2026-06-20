import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LanguageService } from './core/language.service';
import { ThemeService } from './core/theme.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  // Eagerly construct so their effects sync <html> lang/dir/theme on startup.
  private theme = inject(ThemeService);
  private language = inject(LanguageService);
}
