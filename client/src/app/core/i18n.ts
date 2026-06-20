// Runtime translation dictionary for the storefront + admin.
// NOTE: the Arabic strings below are machine-assisted translations and should be
// reviewed by a native speaker before production use.

export type Lang = 'en' | 'ar';

export const LANGUAGES: { code: Lang; label: string }[] = [
  { code: 'en', label: 'English' },
  { code: 'ar', label: 'العربية' },
];

export type TranslationKey = keyof (typeof translations)['en'];

// Each entry is keyed by a dotted namespace. Use {placeholder} tokens for
// interpolation; the TranslatePipe / LanguageService.t() substitute them.
export const translations = {
  en: {
    // Shared
    'common.viewCart': 'view cart',
    'common.addToCart': 'Add to cart',
    'common.qty': 'Qty',
    'common.soldOut': 'Sold out',
    'common.noImage': 'No image',
    'common.loading': 'Loading…',
    'common.darkMode': 'Dark mode',
    'common.lightMode': 'Light mode',

    // Pagination
    'pager.prev': 'Prev',
    'pager.next': 'Next',
    'pager.pageOf': 'Page {page} of {pages}',

    // Storefront layout
    'layout.brand': 'SHOPPING CENTER',
    'layout.cart': 'Cart',
    'layout.footer': '© Shopping Center',

    // Home
    'home.eyebrow': 'New Season',
    'home.title': 'The Collection',
    'home.searchPlaceholder': 'Search the collection',
    'home.loading': 'Loading products…',
    'home.empty': 'No products are available yet. Check back soon!',
    'home.noMatch': 'No products match “{term}”.',
    'home.added': '✓ Added —',
    'home.loadError': 'Could not load products. Please try again later.',
    'home.loadMore': 'Load more',

    // Product detail
    'detail.back': '← Back to products',
    'detail.inStock': 'In stock ({count})',
    'detail.outOfStock': 'Out of stock',
    'detail.added': '✓ Added to cart —',
    'detail.notFound': 'Product not found.',

    // Cart
    'cart.title': 'Your cart',
    'cart.continue': '← Continue shopping',
    'cart.empty': 'Your cart is empty.',
    'cart.browse': 'Browse products',
    'cart.colProduct': 'Product',
    'cart.colPrice': 'Price',
    'cart.colQuantity': 'Quantity',
    'cart.colSubtotal': 'Subtotal',
    'cart.decrease': 'Decrease',
    'cart.increase': 'Increase',
    'cart.remove': 'Remove',
    'cart.emptyCart': 'Empty cart',
    'cart.total': 'Total',
    'cart.confirmEmpty': 'Empty your cart?',
    'cart.checkout': 'Proceed to checkout',

    // Checkout
    'checkout.title': 'Checkout',
    'checkout.backToCart': '← Back to cart',
    'checkout.detailsTitle': 'Your details',
    'checkout.fullName': 'Full name',
    'checkout.email': 'Email',
    'checkout.phone': 'Phone',
    'checkout.address': 'Shipping address',
    'checkout.errName': 'Please enter your name.',
    'checkout.errEmail': 'Please enter a valid email address.',
    'checkout.errPhone': 'Please enter a valid phone number.',
    'checkout.errAddress': 'Please enter your shipping address.',
    'checkout.summaryTitle': 'Order summary',
    'checkout.placeOrder': 'Place order',
    'checkout.placing': 'Placing order…',
    'checkout.errSubmit': 'Could not place your order. Please try again.',
    'checkout.thanksTitle': 'Thank you!',
    'checkout.thanksBody': 'Your order has been placed, {name}.',
    'checkout.thanksEmail': 'A confirmation will be sent to {email}.',
    'checkout.continueShopping': 'Continue shopping',

    // Login
    'login.title': 'Admin Login',
    'login.subtitle': 'Shopping Center admin panel',
    'login.email': 'Email',
    'login.password': 'Password',
    'login.signingIn': 'Signing in…',
    'login.signIn': 'Sign in',
    'login.invalid': 'Invalid email or password.',

    // Admin layout
    'admin.brand': '🛍️ Shopping Center · Admin',
    'admin.products': 'Products',
    'admin.orders': 'Orders',
    'admin.viewStore': 'View store ↗',
    'admin.logout': 'Logout',

    // Notifications (admin bell)
    'notif.title': 'Notifications',
    'notif.markAllRead': 'Mark all read',
    'notif.empty': 'No notifications yet.',

    // Admin order list
    'olist.title': 'Orders',
    'olist.countOne': '1 order',
    'olist.countMany': '{count} orders',
    'olist.empty': 'No orders have been placed yet.',
    'olist.colDate': 'Date',
    'olist.colCustomer': 'Customer',
    'olist.colContact': 'Contact',
    'olist.colItems': 'Items',
    'olist.colTotal': 'Total',
    'olist.colStatus': 'Status',
    'olist.errStatus': 'Failed to update the order status.',
    'ostatus.placed': 'Placed',
    'ostatus.delivered': 'Delivered',
    'olist.viewDetails': 'View details',
    'olist.itemProduct': 'Product',
    'olist.itemUnitPrice': 'Unit price',
    'olist.itemQty': 'Qty',
    'olist.itemLineTotal': 'Line total',
    'olist.errLoad': 'Failed to load orders.',

    // Admin order details
    'odetail.title': 'Order details',
    'odetail.back': '← Back to orders',
    'odetail.placedOn': 'Placed on',
    'odetail.customer': 'Customer',
    'odetail.status': 'Status',
    'odetail.items': 'Items',
    'odetail.save': 'Save order',
    'odetail.saving': 'Saving…',
    'odetail.saved': '✓ Order saved.',
    'odetail.errLoad': 'Could not load this order.',
    'odetail.errSave': 'Failed to save the order. Please try again.',

    // Admin product list
    'plist.title': 'Products',
    'plist.newProduct': '+ New product',
    'plist.empty': 'No products yet. Click “New product” to add your first one.',
    'plist.colName': 'Name',
    'plist.colCategory': 'Category',
    'plist.colPrice': 'Price',
    'plist.colStock': 'Stock',
    'plist.colActions': 'Actions',
    'plist.hidden': 'Hidden',
    'plist.edit': 'Edit',
    'plist.show': 'Show',
    'plist.hide': 'Hide',
    'plist.delete': 'Delete',
    'plist.confirmDelete': 'Delete “{name}”? This cannot be undone.',
    'plist.errDelete': 'Failed to delete the product.',
    'plist.errVisibility': 'Failed to update product visibility.',
    'plist.errLoad': 'Failed to load products.',

    // Admin product form
    'pform.editTitle': 'Edit product',
    'pform.newTitle': 'New product',
    'pform.back': '← Back to list',
    'pform.name': 'Name *',
    'pform.description': 'Description',
    'pform.price': 'Price *',
    'pform.stock': 'Stock quantity *',
    'pform.category': 'Category',
    'pform.currentImage': 'Current image',
    'pform.removeCurrent': 'Remove current image',
    'pform.replaceImage': 'Replace image',
    'pform.image': 'Image',
    'pform.images': 'Images (up to 5)',
    'pform.addImages': 'Add images',
    'pform.primary': 'Primary',
    'pform.removeImage': 'Remove image',
    'pform.errImageCount': 'You can upload at most 5 images.',
    'pform.saving': 'Saving…',
    'pform.saveChanges': 'Save changes',
    'pform.create': 'Create product',
    'pform.errLoad': 'Could not load the product.',
    'pform.errImageType': 'Please choose an image file.',
    'pform.errImageSize': 'Image must be 5 MB or smaller.',
    'pform.errSave': 'Failed to save product. Please try again.',
  },
  ar: {
    // Shared
    'common.viewCart': 'عرض السلة',
    'common.addToCart': 'أضف إلى السلة',
    'common.qty': 'الكمية',
    'common.soldOut': 'نفدت الكمية',
    'common.noImage': 'لا توجد صورة',
    'common.loading': 'جارٍ التحميل…',
    'common.darkMode': 'الوضع الداكن',
    'common.lightMode': 'الوضع الفاتح',

    // Pagination
    'pager.prev': 'السابق',
    'pager.next': 'التالي',
    'pager.pageOf': 'صفحة {page} من {pages}',

    // Storefront layout
    'layout.brand': 'مركز التسوق',
    'layout.cart': 'السلة',
    'layout.footer': '© مركز التسوق',

    // Home
    'home.eyebrow': 'موسم جديد',
    'home.title': 'المجموعة',
    'home.searchPlaceholder': 'ابحث في المجموعة',
    'home.loading': 'جارٍ تحميل المنتجات…',
    'home.empty': 'لا توجد منتجات متاحة بعد. عُد قريبًا!',
    'home.noMatch': 'لا توجد منتجات تطابق ”{term}“.',
    'home.added': '✓ تمت الإضافة —',
    'home.loadError': 'تعذّر تحميل المنتجات. حاول مرة أخرى لاحقًا.',
    'home.loadMore': 'تحميل المزيد',

    // Product detail
    'detail.back': 'العودة إلى المنتجات →',
    'detail.inStock': 'متوفّر ({count})',
    'detail.outOfStock': 'غير متوفّر',
    'detail.added': '✓ تمت الإضافة إلى السلة —',
    'detail.notFound': 'المنتج غير موجود.',

    // Cart
    'cart.title': 'سلتك',
    'cart.continue': 'متابعة التسوق →',
    'cart.empty': 'سلتك فارغة.',
    'cart.browse': 'تصفّح المنتجات',
    'cart.colProduct': 'المنتج',
    'cart.colPrice': 'السعر',
    'cart.colQuantity': 'الكمية',
    'cart.colSubtotal': 'الإجمالي الفرعي',
    'cart.decrease': 'إنقاص',
    'cart.increase': 'زيادة',
    'cart.remove': 'إزالة',
    'cart.emptyCart': 'إفراغ السلة',
    'cart.total': 'الإجمالي',
    'cart.confirmEmpty': 'إفراغ سلتك؟',
    'cart.checkout': 'إتمام الشراء',

    // Checkout
    'checkout.title': 'إتمام الشراء',
    'checkout.backToCart': 'العودة إلى السلة →',
    'checkout.detailsTitle': 'بياناتك',
    'checkout.fullName': 'الاسم الكامل',
    'checkout.email': 'البريد الإلكتروني',
    'checkout.phone': 'رقم الهاتف',
    'checkout.address': 'عنوان الشحن',
    'checkout.errName': 'يرجى إدخال اسمك.',
    'checkout.errEmail': 'يرجى إدخال بريد إلكتروني صحيح.',
    'checkout.errPhone': 'يرجى إدخال رقم هاتف صحيح.',
    'checkout.errAddress': 'يرجى إدخال عنوان الشحن.',
    'checkout.summaryTitle': 'ملخص الطلب',
    'checkout.placeOrder': 'تأكيد الطلب',
    'checkout.placing': 'جارٍ تقديم الطلب…',
    'checkout.errSubmit': 'تعذّر تقديم طلبك. حاول مرة أخرى.',
    'checkout.thanksTitle': 'شكرًا لك!',
    'checkout.thanksBody': 'تم تقديم طلبك يا {name}.',
    'checkout.thanksEmail': 'سيتم إرسال تأكيد إلى {email}.',
    'checkout.continueShopping': 'متابعة التسوق',

    // Login
    'login.title': 'تسجيل دخول المشرف',
    'login.subtitle': 'لوحة تحكم مركز التسوق',
    'login.email': 'البريد الإلكتروني',
    'login.password': 'كلمة المرور',
    'login.signingIn': 'جارٍ تسجيل الدخول…',
    'login.signIn': 'تسجيل الدخول',
    'login.invalid': 'بريد إلكتروني أو كلمة مرور غير صحيحة.',

    // Admin layout
    'admin.brand': '🛍️ مركز التسوق · المشرف',
    'admin.products': 'المنتجات',
    'admin.orders': 'الطلبات',
    'admin.viewStore': 'عرض المتجر ↗',
    'admin.logout': 'تسجيل الخروج',

    // Notifications (admin bell)
    'notif.title': 'الإشعارات',
    'notif.markAllRead': 'تعليم الكل كمقروء',
    'notif.empty': 'لا توجد إشعارات بعد.',

    // Admin order list
    'olist.title': 'الطلبات',
    'olist.countOne': 'طلب واحد',
    'olist.countMany': '{count} طلبات',
    'olist.empty': 'لم يتم تقديم أي طلبات بعد.',
    'olist.colDate': 'التاريخ',
    'olist.colCustomer': 'العميل',
    'olist.colContact': 'وسيلة التواصل',
    'olist.colItems': 'العناصر',
    'olist.colTotal': 'الإجمالي',
    'olist.colStatus': 'الحالة',
    'olist.errStatus': 'تعذّر تحديث حالة الطلب.',
    'ostatus.placed': 'تم الطلب',
    'ostatus.delivered': 'تم التوصيل',
    'olist.viewDetails': 'عرض التفاصيل',
    'olist.itemProduct': 'المنتج',
    'olist.itemUnitPrice': 'سعر الوحدة',
    'olist.itemQty': 'الكمية',
    'olist.itemLineTotal': 'إجمالي السطر',
    'olist.errLoad': 'تعذّر تحميل الطلبات.',

    // Admin order details
    'odetail.title': 'تفاصيل الطلب',
    'odetail.back': 'العودة إلى الطلبات →',
    'odetail.placedOn': 'تم الطلب في',
    'odetail.customer': 'العميل',
    'odetail.status': 'الحالة',
    'odetail.items': 'العناصر',
    'odetail.save': 'حفظ الطلب',
    'odetail.saving': 'جارٍ الحفظ…',
    'odetail.saved': '✓ تم حفظ الطلب.',
    'odetail.errLoad': 'تعذّر تحميل هذا الطلب.',
    'odetail.errSave': 'تعذّر حفظ الطلب. حاول مرة أخرى.',

    // Admin product list
    'plist.title': 'المنتجات',
    'plist.newProduct': '+ منتج جديد',
    'plist.empty': 'لا توجد منتجات بعد. انقر ”منتج جديد“ لإضافة أول منتج.',
    'plist.colName': 'الاسم',
    'plist.colCategory': 'الفئة',
    'plist.colPrice': 'السعر',
    'plist.colStock': 'المخزون',
    'plist.colActions': 'الإجراءات',
    'plist.hidden': 'مخفي',
    'plist.edit': 'تعديل',
    'plist.show': 'إظهار',
    'plist.hide': 'إخفاء',
    'plist.delete': 'حذف',
    'plist.confirmDelete': 'حذف ”{name}“؟ لا يمكن التراجع عن هذا.',
    'plist.errDelete': 'تعذّر حذف المنتج.',
    'plist.errVisibility': 'تعذّر تحديث ظهور المنتج.',
    'plist.errLoad': 'تعذّر تحميل المنتجات.',

    // Admin product form
    'pform.editTitle': 'تعديل المنتج',
    'pform.newTitle': 'منتج جديد',
    'pform.back': 'العودة إلى القائمة →',
    'pform.name': 'الاسم *',
    'pform.description': 'الوصف',
    'pform.price': 'السعر *',
    'pform.stock': 'كمية المخزون *',
    'pform.category': 'الفئة',
    'pform.currentImage': 'الصورة الحالية',
    'pform.removeCurrent': 'إزالة الصورة الحالية',
    'pform.replaceImage': 'استبدال الصورة',
    'pform.image': 'الصورة',
    'pform.images': 'الصور (حتى 5)',
    'pform.addImages': 'إضافة صور',
    'pform.primary': 'الرئيسية',
    'pform.removeImage': 'إزالة الصورة',
    'pform.errImageCount': 'يمكنك رفع 5 صور كحد أقصى.',
    'pform.saving': 'جارٍ الحفظ…',
    'pform.saveChanges': 'حفظ التغييرات',
    'pform.create': 'إنشاء المنتج',
    'pform.errLoad': 'تعذّر تحميل المنتج.',
    'pform.errImageType': 'يرجى اختيار ملف صورة.',
    'pform.errImageSize': 'يجب أن تكون الصورة 5 ميغابايت أو أصغر.',
    'pform.errSave': 'تعذّر حفظ المنتج. حاول مرة أخرى.',
  },
} satisfies Record<Lang, Record<string, string>>;

// Replace {token} placeholders with provided params.
export function interpolate(text: string, params?: Record<string, string | number>): string {
  if (!params) {
    return text;
  }
  return text.replace(/\{(\w+)\}/g, (match, key) =>
    key in params ? String(params[key]) : match
  );
}
