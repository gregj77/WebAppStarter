import { Injectable, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Response } from '@angular/http';

import {Observable, Subscription} from 'rxjs/Rx';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class AuthService {
  user: IUser = <IUser>{};
  loggedIn$ = new BehaviorSubject<boolean>(false);
  private tokenExpiry: Subscription;

  constructor(private router: Router, private http: HttpClient) {
  }

  login(tokenRequest: ITokenRequest): Observable<IAuthResponse> {
    return this.http
      .post('/api/token', tokenRequest)
      .map((response: IToken) => { 
        console.log('got response!' + JSON.stringify(response));
        localStorage.setItem('token', response.access_token);
        this.setLoggedIn(true);
        return { token: response };
      })
      .catch(this.catchAuthError());
  }

  setLoggedIn(value: boolean) {
    if (!value) {
      localStorage.removeItem('token');
      this.user = <IUser>{};
    }
    this.loggedIn$.next(value);
  }

  logout() {
    this.setLoggedIn(false);
    this.router.navigate(['/']);
  }

  isAuthenticated() {
    return !!localStorage.getItem('token');
  }

  getAuthorizationHeader(): string {
    const token = localStorage.getItem('token');
    if (token) {
      return 'Beaer ' + token;
    }
    return null;
  }

  private catchAuthError() {
    return (res: Response) => {
      this.setLoggedIn(false);
      return Observable.of({
        error: {
          status: res.status,
          message: res.statusText
        }
      });
    };
  }
}

export interface ITokenRequest {
  userName: string;
  password: string;
  grant_type: string;
  device_id: string;
}

export interface IAuthResponse {
  token?: IToken;
  error?: IAuthError;
}

export interface IToken {
  access_token: string;
  token_type: string;
  expires_in: number;
  expires_at: string;
}

export interface IAuthError {
  status: number;
  message: string;
}

export interface IUser {
  id: number;
  name: string;
  userName: string;
}