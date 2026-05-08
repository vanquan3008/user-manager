export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message?: string;
  error?: { code: string; message: string };
}

export interface PagedResponse<T> {
  success: boolean;
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  message?: string;
  error?: { code: string; message: string };
}
