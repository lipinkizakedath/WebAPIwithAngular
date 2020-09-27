import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/Photo';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {

  photos: Photo[];

  constructor(private authService: AuthService, private alertify: AlertifyService, private userService: UserService) { }

  ngOnInit(): void {
    this.userService.getUnapprovedPhotos(this.authService.decodedToken.nameid).subscribe(
      (photos: Photo[]) => { this.photos = photos; },
      error => { this.alertify.error(error); }
    );
  }

  approvePhoto(photoId: number){
    this.userService.approvePendingPhotos(this.authService.decodedToken.nameid, photoId).subscribe(
      () => {
        this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1); this.alertify.success('Photo approved!');
      }, error => { this.alertify.error(error); }
    );

  }

  rejectPhoto(photoId: number){
    this.alertify.confirm('Confirm reject and delete?', () => {
      this.userService.rejectPendingPhotos(this.authService.decodedToken.nameid, photoId).subscribe(() => {
        this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1);
        this.alertify.success('Photo rejected succesfully!');
      }, error => {
        this.alertify.error(error);
      });
    });
  }

}
