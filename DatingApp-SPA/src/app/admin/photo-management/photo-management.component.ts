import { AlertifyService } from './../../_services/alertify.service';
import { AdminService } from './../../_services/admin.service';
import { Component, OnInit } from '@angular/core';
import { Photo } from '../../_models/photo';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  unapprovedPhotos: Photo[];
  constructor(
    private adminService: AdminService,
    private alertify: AlertifyService
  ) {}

  ngOnInit() {
    this.getUnapprovedPhotos();
  }
  getUnapprovedPhotos() {
    this.adminService.getUnapprovedPhotos().subscribe((photos: Photo[]) => {
      this.unapprovedPhotos = photos;
    });
  }
  approvePhoto(photoId, approved) {
    this.unapprovedPhotos = this.unapprovedPhotos.filter(
      photo => photo.id !== photoId
    );
    this.adminService.approvePhoto(photoId, approved).subscribe(
      response => {
        console.log(response);
        this.alertify.success('Succeeded');
      },
      error => {
        this.alertify.error(error);
      }
    );
  }
}
