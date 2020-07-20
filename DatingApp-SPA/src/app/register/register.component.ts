import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
<<<<<<< HEAD
=======
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { User } from '../_models/User';
import { Router } from '@angular/router';
>>>>>>> d08d734f61dbcbb46bf15f8414790a7191806ee0

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  @Output() cancelRegister = new EventEmitter();
<<<<<<< HEAD
  model: any = {};
  registerForm: FormGroup;
=======
  user: User;
  registerForm: FormGroup;
  bsConfig: Partial<BsDatepickerConfig>;
>>>>>>> d08d734f61dbcbb46bf15f8414790a7191806ee0

  constructor(
    private authService: AuthService,
    private alertify: AlertifyService,
<<<<<<< HEAD
    private fb: FormBuilder) { }

  ngOnInit() {
    this.createRegisterForm();
=======
    private fb: FormBuilder,
    private router: Router) { }

  ngOnInit() {
    this.createRegisterForm();
    this.bsConfig = {
      containerClass: 'theme-dark-blue'
    },
      this.createRegisterForm();
>>>>>>> d08d734f61dbcbb46bf15f8414790a7191806ee0
  }

  createRegisterForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      knownAs: ['', Validators.required],
<<<<<<< HEAD
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', Validators.required, Validators.minLength(4)],
=======
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4)]],
>>>>>>> d08d734f61dbcbb46bf15f8414790a7191806ee0
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  // this validation method validates the password and confirm password // 
  passwordMatchValidator(g: FormGroup) {
    return g.get('password').value === g.get('confirmPassword').value ? null : { 'mismatch': true };
  }

  register() {
<<<<<<< HEAD
    // this.authService.register(this.model).subscribe(
    //   () => { this.alertify.success('Registration successful'); },
    //   error => { this.alertify.error(error); });
    console.log(this.registerForm.value);
=======

    if (this.registerForm.valid) {
      this.user = Object.assign({}, this.registerForm.value);
      this.authService.register(this.user).subscribe(() => {
        this.alertify.success('Registration successfull!');
      },
        error => { 
          this.alertify.error(error); 
        }, () => {
          this.authService.login(this.user).subscribe(() => {
            this.router.navigate(['/members']);
          }

          );
        },

      );

    }

>>>>>>> d08d734f61dbcbb46bf15f8414790a7191806ee0
  }

  cancel() {
    this.cancelRegister.emit(false);
  }

}
