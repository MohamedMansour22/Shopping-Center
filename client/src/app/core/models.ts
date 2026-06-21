export interface ProductImage {
  id: string;
  imageDataUri: string;
}

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  category: string;
  hasImage: boolean;
  // Primary image (first by sort order). Present on list + detail responses; used by the grid/cart.
  imageDataUri: string | null;
  // All images, ordered (primary first). List responses carry only the primary; detail carries all.
  images: ProductImage[];
  isHidden: boolean;
  createdAtUtc: string;
}

// Admin product-list visibility filter; matches the server's ProductVisibilityFilter enum names.
export type ProductVisibilityFilter = 'All' | 'VisibleOnly' | 'HiddenOnly';

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasMore: boolean;
}

export interface CartItem {
  productId: string;
  name: string;
  price: number;
  imageDataUri: string | null;
  quantity: number;
}

export interface CreateOrderItem {
  productId: string;
  quantity: number;
}

export interface CreateOrderRequest {
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  shippingAddress: string;
  items: CreateOrderItem[];
}

export interface OrderItem {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface OrderStatus {
  id: number;
  name: string;
}

export interface Order {
  id: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  shippingAddress: string;
  totalAmount: number;
  itemCount: number;
  statusId: number;
  status: string;
  items: OrderItem[];
  createdAtUtc: string;
}

// Admin order-list filters; all optional. Dates are 'YYYY-MM-DD' strings from <input type="date">.
export interface OrderListFilter {
  dateFrom?: string;
  dateTo?: string;
  customerName?: string;
  statusId?: number | null;
}

export interface Notification {
  id: string;
  type: string;
  title: string;
  message: string;
  orderId: string | null;
  isRead: boolean;
  createdAtUtc: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  expiresAtUtc: string;
}
