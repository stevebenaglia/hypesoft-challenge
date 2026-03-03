export interface Category {
  id: string;
  name: string;
  description?: string;
  productCount: number;
}

export interface Product {
  id: string;
  name: string;
  description?: string;
  price: number;
  stockQuantity: number;
  categoryId: string;
  categoryName?: string;
}

export interface PagedResult<T> {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
  data: T[];
}

export interface CategorySummary {
  categoryName: string;
  productCount: number;
}

export interface DashboardSummary {
  totalProducts: number;
  totalStockValue: number;
  lowStockProducts: Product[];
  productsByCategory: CategorySummary[];
}
