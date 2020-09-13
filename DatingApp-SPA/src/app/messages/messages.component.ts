import { Message } from '../_models/Message';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AlertifyService } from '../_services/alertify.service';
import { AuthService } from '../_services/auth.service';
import { UserService } from '../_services/user.service';
import { PaginatedResults, Pagination } from '../_models/Pagination';


@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {

  messages: Message[];
  pagination: Pagination;
  messageContainer = 'Unread';

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private alertify: AlertifyService,
    private route: ActivatedRoute
  ) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.messages = data['messages'].result;
      this.pagination = data['messages'].pagination;
    });
  }

  loadMessages() {
    this.userService.getMessage(this.authService.decodedToken.nameid,
      this.pagination.currentPage, this.pagination.itemsPerPage, this.messageContainer).subscribe(
        (res: PaginatedResults<Message[]>) => {
          this.messages = res.result;
          this.pagination = res.pagination;
        }, error => { this.alertify.error(error); }
      );
  }

  deleteMessage(id: number) {
    this.alertify.confirm('Confirm delete', () => {
      this.userService.deleteMessage(id, this.authService.decodedToken.nameid).subscribe(
        () => {
          this.messages.splice(this.messages.findIndex(m => m.id === id), 1);
          this.alertify.success('Message deleted!');
        }, error => { this.alertify.error(error); }

      );
    });
  }

  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    this.loadMessages();
  }

}
