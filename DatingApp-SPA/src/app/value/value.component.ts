import { Component, OnInit } from '@angular/core';
import { HttpClient } from '../../../node_modules/@angular/common/http';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-value',
  templateUrl: './value.component.html',
  styleUrls: ['./value.component.css']
})
export class ValueComponent implements OnInit {
  values: any;
  constructor(private http: HttpClient, private authService: AuthService) {}

  ngOnInit() {
    this.getValues();
  }
  getValues() {}
}
