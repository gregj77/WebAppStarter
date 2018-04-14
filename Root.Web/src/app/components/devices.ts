import {Component, Injector, ViewChild, ElementRef, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import {FormGroup, Validators, FormBuilder, AbstractControl, NgForm } from '@angular/forms';

import 'rxjs/Rx';
import { Observable } from 'rxjs/Observable';
import { DeviceService } from '../services/deviceservice';

@Component({
  selector: 'device',
  templateUrl: './devices.html'
})
export class DevicesComponent implements AfterViewInit {
  devices: any[] = [];

  constructor(private svc: DeviceService) {    
  }

  ngAfterViewInit(): void {
    console.log('getting devices...');
    this.svc.getDevices().subscribe(result => this.devices = result);
  }

}
