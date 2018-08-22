import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { PaginatedResult } from '../_models/pagination';
import { map } from '../../../node_modules/rxjs/operators';

const httpOptions = {
  headers: new HttpHeaders({
    Authorization: `Bearer ${localStorage.getItem('token')}`
  })
};

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}
  getUsers(
    page?,
    itemsPerPage?,
    userParams?,
    likesParam?
  ): Observable<PaginatedResult<User[]>> {
    const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<
      User[]
    >();

    let params = new HttpParams();
    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }
    if (userParams != null) {
      params = params.append('gender', userParams.gender);
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('minAge', userParams.minAge);
      params = params.append('orderBy', userParams.orderBy);
    }

    if (likesParam === 'Likers') {
      params = params.append('likers', 'true');
    }

    if (likesParam === 'Likees') {
      params = params.append('Likees', 'true');
    }
    const options = Object.assign({}, httpOptions, {
      observe: 'response',
      params
    });

    return this.http
      .get<User[]>(`${this.baseUrl}users`, {
        observe: 'response',
        params,
        headers: new HttpHeaders({
          Authorization: `Bearer ${localStorage.getItem('token')}`
        })
      })
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') != null) {
            paginatedResult.pagination = JSON.parse(
              response.headers.get('Pagination')
            );
          }
          return paginatedResult;
        })
      );
  }
  getUser(id): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}users/${id}`, httpOptions);
  }
  updateUser(id: number, user: User) {
    return this.http.put(this.baseUrl + 'users/' + id, user, httpOptions);
  }
  setMainPhoto(userId: number, id: number) {
    return this.http.post(
      `${this.baseUrl}users/${userId}/photos/${id}/setMain`,
      {},
      httpOptions
    );
  }
  deletePhoto(userId: number, id: number) {
    return this.http.delete(
      `${this.baseUrl}users/${userId}/photos/${id}`,
      httpOptions
    );
  }

  sendLike(id: number, recipientId: number) {
    return this.http.post(
      `${this.baseUrl}users/${id}/like/${recipientId}`,
      {},
      httpOptions
    );
  }
}
