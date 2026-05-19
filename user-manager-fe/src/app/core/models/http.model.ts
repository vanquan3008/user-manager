export interface ApiError {
  code: string;
  message: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message?: string;
  error?: ApiError;
}

export interface PagedResponse<T> {
  success: boolean;
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  message?: string;
  error?: ApiError;
}
export interface SuccessResponse {
  message: string;
}
