import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ProductImage } from '../../core/models';
import { ProductService } from '../../core/product.service';
import { TranslatePipe } from '../../core/translate.pipe';

const MAX_IMAGES = 5;

// A newly selected file plus its object-URL preview.
interface NewImage {
  file: File;
  url: string;
}

@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './product-form.html',
  styleUrl: './products.scss',
})
export class ProductForm implements OnInit {
  private fb = inject(FormBuilder);
  private productService = inject(ProductService);
  private router = inject(Router);

  // Bound from the route param (provided via withComponentInputBinding). On the "create" route
  // there is no :id param, and router input binding sets this to undefined (overriding the
  // default), so every read must treat it as possibly undefined.
  readonly id = input<string>('');
  readonly isEdit = computed(() => !!this.id());

  readonly error = signal<string | null>(null);
  readonly submitting = signal(false);
  readonly loading = signal(false);

  readonly maxImages = MAX_IMAGES;
  // Existing images retained from the loaded product (edit mode); removing drops them from this list.
  readonly existingImages = signal<ProductImage[]>([]);
  // Newly selected files to upload.
  readonly newImages = signal<NewImage[]>([]);
  readonly totalImages = computed(() => this.existingImages().length + this.newImages().length);
  readonly canAddMore = computed(() => this.totalImages() < MAX_IMAGES);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    price: [0, [Validators.required, Validators.min(0)]],
    stockQuantity: [0, [Validators.required, Validators.min(0)]],
    category: ['', [Validators.maxLength(100)]],
  });

  ngOnInit(): void {
    // Router component-input binding sets the id input before ngOnInit (not in the constructor).
    const id = this.id();
    if (id) {
      this.loading.set(true);
      // Admin endpoint so a hidden product can still be loaded for editing.
      this.productService.getByIdForAdmin(id).subscribe({
        next: (p) => {
          this.form.patchValue({
            name: p.name,
            description: p.description,
            price: p.price,
            stockQuantity: p.stockQuantity,
            category: p.category,
          });
          this.existingImages.set(p.images ?? []);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('pform.errLoad');
          this.loading.set(false);
        },
      });
    }
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    this.error.set(null);

    for (const file of files) {
      if (this.totalImages() >= MAX_IMAGES) {
        this.error.set('pform.errImageCount');
        break;
      }
      if (!file.type.startsWith('image/')) {
        this.error.set('pform.errImageType');
        continue;
      }
      if (file.size > 5 * 1024 * 1024) {
        this.error.set('pform.errImageSize');
        continue;
      }
      this.newImages.update((imgs) => [...imgs, { file, url: URL.createObjectURL(file) }]);
    }

    // Reset the input so selecting the same file again still fires a change event.
    input.value = '';
  }

  removeExistingImage(id: string): void {
    this.existingImages.update((imgs) => imgs.filter((img) => img.id !== id));
  }

  removeNewImage(index: number): void {
    this.newImages.update((imgs) => {
      const removed = imgs[index];
      if (removed) {
        URL.revokeObjectURL(removed.url);
      }
      return imgs.filter((_, i) => i !== index);
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.error.set(null);
    this.submitting.set(true);

    const v = this.form.getRawValue();
    const data = new FormData();
    data.append('Name', v.name);
    data.append('Description', v.description);
    data.append('Price', String(v.price));
    data.append('StockQuantity', String(v.stockQuantity));
    data.append('Category', v.category);

    // New uploads (both create and edit).
    for (const img of this.newImages()) {
      data.append('Images', img.file, img.file.name);
    }

    const done = {
      next: () => this.router.navigate(['/admin/products']),
      error: () => {
        this.error.set('pform.errSave');
        this.submitting.set(false);
      },
    };

    if (this.isEdit()) {
      // Existing images to retain, in display order; the server deletes any not listed.
      for (const img of this.existingImages()) {
        data.append('KeepImageIds', img.id);
      }
      this.productService.update(this.id(), data).subscribe(done);
    } else {
      this.productService.create(data).subscribe(done);
    }
  }
}
