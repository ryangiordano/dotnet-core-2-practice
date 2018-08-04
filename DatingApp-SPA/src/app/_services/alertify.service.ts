import { Injectable } from '@angular/core';
// letting tslint know we know what we're doing here by using alertify.
declare let alertify: any;

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {
  constructor() {}
  confirm(message: string, okCallback: () => any) {
    alertify.confirm(message, e => {
      if (e) {
        okCallback();
      } else {
      }
    });
  }
  success(message: string) {
    alertify.success(message);
  }
  warning(message: string) {
    alertify.warning(message);
  }
  error(message: string) {
    alertify.error(message);
  }
  message(message: string) {
    alertify.message(message);
  }
}
