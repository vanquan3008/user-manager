import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { ApiResponse, SuccessResponse } from '../models';
import type { Observable } from 'rxjs';

export interface UploadFileDto {
  url: string;
  fileName: string;
  size: number;
  contentType: string;
}

@Injectable({ providedIn: 'root' })
export class FileUploadService {
  private readonly api = `${environment.apiUrl}/api/files`;

  constructor(private http: HttpClient) {}

  /** Upload ảnh (backend chỉ chấp nhận image/*). */
  uploadImage(file: File, folder = 'common'): Observable<ApiResponse<UploadFileDto>> {
    return this.uploadFile(file, folder);
  }

  /** Upload file chung (multipart) — cùng endpoint `/api/files/upload`. */
  uploadFile(file: File, folder = 'common'): Observable<ApiResponse<UploadFileDto>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('folder', folder);
    return this.http.post<ApiResponse<UploadFileDto>>(`${this.api}/upload`, formData);
  }

  /**
   * Delete uploaded file by URL.
   * @param fileUrl The full URL of the file to delete
   */
  deleteFile(fileUrl: string): Observable<ApiResponse<SuccessResponse>> {
    const params = new HttpParams().set('fileUrl', fileUrl);
    return this.http.delete<ApiResponse<SuccessResponse>>(this.api, { params });
  }
}

