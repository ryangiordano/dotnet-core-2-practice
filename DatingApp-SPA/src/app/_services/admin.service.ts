import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { User } from '../_models/user';

const httpOptions = {
  headers: new HttpHeaders({
    Authorization: `Bearer ${localStorage.getItem('token')}`
  })
};

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}
  getUsersWithRoles() {
    return this.http.get(`${this.baseUrl}admin/usersWithRoles`, httpOptions);
  }
  updateUserRoles(user: User, roles: {}) {
    return this.http.post(
      `${this.baseUrl}admin/editRoles/${user.userName}`,
      roles,
      httpOptions
    );
  }
  getUnapprovedPhotos() {
    return this.http.get(
      `${this.baseUrl}admin/photosForModeration`,
      httpOptions
    );
  }
  approvePhoto(photoId, approved) {
    return this.http.get(
      `${this.baseUrl}admin/approvePhoto/${photoId}/${approved}`,
      httpOptions
    );
  }
}
