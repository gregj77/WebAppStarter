import {Component, Injector, ViewChild, ElementRef, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import {FormGroup, Validators, FormBuilder, AbstractControl, NgForm } from '@angular/forms';

import 'rxjs/Rx';
import { Observable } from 'rxjs/Observable';
import { AuthService } from '../services/authservice';

@Component({
  selector: 'login',
  templateUrl: './login.html'
})
export class LoginComponent implements AfterViewInit {
  login: FormGroup;

  constructor(public auth: AuthService, private fb: FormBuilder) {
    this.login = fb.group({
      'userName': ['', Validators.required],
      'password': ['', Validators.required],
      'grant_type': ['']
    });
    this.onReset();
    
  }

  ngAfterViewInit(): void {
  }

  onSubmitLogin(request: any) {
    if (this.login.valid) {
      this.auth.login(request).subscribe((result: any) => {
        if (result.token) {
          console.warn('user authenticated');
          this.onReset();
        } else if (result.error) {
          this.login.setErrors({
              status: result.error.status,            
              message: result.error.status === 401 ? 'Bad username or password' : result.error.message
          });
        }
      });

    } else {
      Object
        .keys(this.login.controls)
        .map((key: string) => this.login.controls[key])
        .forEach((c: AbstractControl) => c.markAsTouched());
    }
  }
  
  onReset() {
    this.login.setValue({
      'userName': '',
      'password': '',
      'grant_type': 'credentials'
    });
  }

  logout() {
    this.auth.logout();
  }
}
