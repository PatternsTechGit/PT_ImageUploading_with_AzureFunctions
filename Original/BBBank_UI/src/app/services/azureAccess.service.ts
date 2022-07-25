import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UploadImageResponse } from '../models/upload-image-response';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export default class AzureAccessService {
  constructor(private httpClient: HttpClient) { }

  uploadImageAndGetUrl(imageData: FormData): Observable<UploadImageResponse> {
    return this.httpClient.post<UploadImageResponse>(`${environment.azureFunctionsBaseUrl}UploadImageAndGetUrl`, imageData);
  }
}