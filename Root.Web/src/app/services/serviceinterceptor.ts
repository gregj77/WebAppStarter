import {Injectable} from "@angular/core";
import {HttpInterceptor, HttpRequest, HttpHandler, HttpEvent} from "@angular/common/http";
import {Observable} from "rxjs/Rx";

@Injectable()
export class ServiceInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const url = 'http://localhost:42000';
    let token = localStorage.getItem('token'); 
    if (token) {
      req = req.clone({
        url: url + req.url,
        setHeaders: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        }
      });
   } else {
    req = req.clone({
      url: url + req.url,
      setHeaders: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    });
   }

    return next.handle(req);
  }
}