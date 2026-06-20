# Shopping Center

A single-vendor online shopping center built with **.NET 10 (Web API)**, **Angular 21**, and **SQL Server**.

This first milestone delivers the **admin panel**: an admin logs in and can **create, list, edit, and delete** products (with an image stored in the database).

## Architecture

Clean/layered backend:

```
src/
├── ShoppingCenter.Domain/          # Entities (Product)
├── ShoppingCenter.Application/     # DTOs, interfaces, ProductService
├── ShoppingCenter.Infrastructure/  # EF Core DbContext, repos, Identity + JWT
└── ShoppingCenter.API/             # Controllers, Program.cs, config
client/                             # Angular 21 admin app
```

Reference direction: `API → Infrastructure → Application → Domain` (Domain depends on nothing).

## Prerequisites

- .NET 10 SDK
- Node.js 20+ and the Angular CLI (`npm i -g @angular/cli`)
- SQL Server (this project targets `.\SQLEXPRESS`)

## Configuration

- **Connection string** — `src/ShoppingCenter.API/appsettings.json` → `ConnectionStrings:DefaultConnection`
  (`Server=.\SQLEXPRESS;Database=ShoppingCenterDb;Trusted_Connection=True;TrustServerCertificate=True;`).
- **JWT** — `appsettings.json` → `Jwt` section. The signing `Key` is a development placeholder; **replace it before any real deployment**.

## Running the backend

```bash
# from the repo root
dotnet ef database update -p src/ShoppingCenter.Infrastructure -s src/ShoppingCenter.API   # first time / after model changes
dotnet run --project src/ShoppingCenter.API --launch-profile http
```

- API: http://localhost:5256
- Swagger UI (dev): http://localhost:5256/swagger
- On startup the app applies migrations and **seeds a default admin user**.

### Seeded admin (development only)

| Email              | Password      |
| ------------------ | ------------- |
| `admin@shop.local` | `Admin#12345` |

Change these before deploying anywhere real (see `src/ShoppingCenter.Infrastructure/Data/DbSeeder.cs`).

## Running the frontend

```bash
cd client
ng serve            # http://localhost:4200
```

The single Angular app serves two areas:

- **Customer storefront** at the root `/` — public home page with a product grid (image, name, price),
  a search box, a product detail page at `/product/:id` (with quantity + add-to-cart), and a **cart** at
  `/cart`. No login required.
- **Admin panel** under `/admin` — sign in at `/admin/login` with the seeded admin, then manage products
  (`/admin/products`, create/edit/delete).

The API allows CORS from `http://localhost:4200` and `http://localhost:4201` (the fallback port if 4200 is taken).
The API base URL lives in `client/src/environments/environment.ts`.

## API endpoints

| Method | Route                      | Auth        | Description                          |
| ------ | -------------------------- | ----------- | ------------------------------------ |
| POST   | `/api/auth/login`          | Anonymous   | Returns a JWT                        |
| GET    | `/api/products`            | Anonymous   | List **visible** products (storefront), image inlined |
| GET    | `/api/products/{id}`       | Anonymous   | Get a single **visible** product (404 if hidden) |
| GET    | `/api/products/admin`      | Admin (JWT) | List **all** products incl. hidden (admin) |
| GET    | `/api/products/admin/{id}` | Admin (JWT) | Get any product incl. hidden (admin) |
| POST   | `/api/products`            | Admin (JWT) | Create a product (`multipart/form-data`, optional `Image` file) |
| PUT    | `/api/products/{id}`       | Admin (JWT) | Update a product (`multipart/form-data`; send `Image` to replace, `RemoveImage=true` to clear) |
| DELETE | `/api/products/{id}`       | Admin (JWT) | Delete a product                     |
| PUT    | `/api/products/{id}/visibility` | Admin (JWT) | Hide/show a product (`{ "isHidden": true }`) |

## Product images

Product images are uploaded as a file (browse on the create form) and stored **directly in SQL Server**
as a `varbinary(max)` column (`Products.ImageData`) plus the MIME type (`Products.ImageContentType`).
The image is returned **inline** in the product responses as a base64 data URI (`ProductDto.imageDataUri`),
so a single products call carries everything the UI needs — there is no separate image endpoint. (Trade-off:
base64 inflates the payload; for large catalogs you'd switch to served thumbnails.) Max upload size is 5 MB;
only `image/*` content types are accepted.

## Product visibility

Admins can hide a product from the storefront via the **Hide/Show** action in the admin product list
(`Products.IsHidden`). To avoid leaking hidden products, the **public** storefront endpoints
(`GET /api/products`, `/{id}`) are **always visible-only** — they never return hidden products even if an
admin JWT happens to be attached (the admin UI sends its token on every request). Admins use the separate
**`/api/products/admin`** and **`/api/products/admin/{id}`** endpoints (JWT-protected) to see and edit hidden
products. The image endpoint stays role-aware so the admin list can still show hidden thumbnails.

## Design system

The storefront uses a minimal, editorial fashion aesthetic (from a Figma reference): a **monochrome + warm-neutral**
palette with **serif display headings** (Cormorant Garamond) and a **geometric sans** for UI/nav (Jost), set as CSS
custom-property tokens in `client/src/styles.scss` (`--ink`, `--bg-warm`, `--line`, `--font-serif`, `--font-sans`,
etc.) plus shared `.btn-ink` / `.btn-line` button classes. Fonts load via Google Fonts in `client/src/index.html`.
The admin panel reuses the same tokens (ink replaces the old blue accent).

## Shopping cart

The storefront cart is **client-side** (stored in the browser's `localStorage`, key `sc_cart`) since the
storefront is anonymous — no backend or login required. `CartService`
(`client/src/app/core/cart.service.ts`) exposes `items`, `count`, and `totalPrice` signals plus
`add/setQuantity/remove/clear`. Add to cart (with quantity) from a product detail page; view/edit at `/cart`;
the header shows a live item-count badge. Cross-device/persistent carts would come with customer accounts.

## Not yet implemented (next steps)

Checkout, customer accounts, and real payments (Stripe/PayPal). These build on the current foundation.
