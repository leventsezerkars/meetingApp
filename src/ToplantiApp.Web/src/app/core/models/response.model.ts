export interface ApiResponse<T = null> {
  success: boolean;
  message: string | null;
  statusCode: number;
  data: T;
}

export interface PaginatedResponse<T> extends ApiResponse<T[]> {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
