import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpModule, XHRBackend, RequestOptions } from '@angular/http';
import { RouterModule } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';


import { AppComponent } from './app.component';
import { DevicesComponent } from './components/devices';
import { AuthService } from './services/authservice';
import { AuthGuard } from './guard/authguard';
import { ServiceInterceptor } from './services/serviceinterceptor';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { LoginComponent } from './components/login';
import { ReactiveFormsModule } from '@angular/forms';
import { DeviceService } from './services/deviceservice';


@NgModule({
  declarations: [
    AppComponent,
    DevicesComponent,
    LoginComponent
  ],
  imports: [
    HttpClientModule,
    BrowserModule,
    ReactiveFormsModule,
    NoopAnimationsModule,    
    RouterModule.forRoot([
      {
        path: '',
        redirectTo: '/',
        pathMatch: 'full'
      },
      {
        path: 'devices',
        component: DevicesComponent,
        canActivate: [AuthGuard]
      },
      {
        path: 'login',
        component: LoginComponent        
      }
    ])    
  ],
  providers: [ 
    AuthGuard,
    AuthService,
    DeviceService,
    { provide: HTTP_INTERCEPTORS, useClass: ServiceInterceptor, multi: true}
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
