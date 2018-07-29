import { AuthService } from './../../_services/auth.service';
import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // Assign to new event emitter
  @Output() cancelRegister = new EventEmitter();
  model: any = {};
  constructor(private authService: AuthService) {}

  ngOnInit() {}
  register() {
    this.authService.register(this.model).subscribe(
      () => {
        console.log('success');
      },
      err => {
        console.error(err);
      }
    );
  }
  cancel() {
    this.cancelRegister.emit(false);
  }
}
