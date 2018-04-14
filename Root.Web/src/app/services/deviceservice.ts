import { Injectable, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Response } from '@angular/http';

import {Observable, Subscription} from 'rxjs/Rx';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class DeviceService {

  constructor(private http: HttpClient) {
  }

  getDevices(): Observable<any[]> {
    return this.http.get('/api/devices').map(response => <any[]>response);
  }

}