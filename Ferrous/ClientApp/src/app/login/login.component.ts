import { Component, OnInit } from '@angular/core';
import * as $ from 'jquery';
import { DataService } from '../data.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  public username: string;
  public password: string;

  public usernameError = '';
  public passwordError = '';

  constructor(public dataService: DataService) {}

  ngOnInit() {
  }

  continueLogin() {
    this.clearMessages();

    const self = this;

    if (self.username == null || self.username === '') {
      self.usernameError = 'Username cannot be blank';
      self.addFormError('#user_username');
    } else if (self.password == null || self.password === '') {
      self.passwordError = 'Password cannot be blank';
      self.addFormError('#user_password');
    } else {
      $.ajax({
        type: 'get', url: '/api/login/login?username=' + self.username + '&password=' + self.password,
            success: function (data, text) {
                $('#dialog').removeClass('dialog-effect-in').removeClass('shakeit');
                $('#dialog').addClass('dialog-effect-out');
                $('#successful_login').addClass('active');
                setTimeout(function() {
                  self.dataService.RefreshAPISpec().subscribe(api => {
                    self.dataService._API_SPEC = api;
                    self.dataService.loggedIn = true;
                  });

                }, 300);
            },
            error: function (request, status, error) {
                self.addFormError('#user_password');
                self.passwordError = 'The password is invalid';
            }
        });
      }
  }

  addFormError(formRow) {
    $(formRow).parents('.form-group').addClass('has-error');
    $('#dialog').removeClass('dialog-effect-in');
    $('#dialog').addClass('shakeit');
    setTimeout(function() {
      $('#dialog').removeClass('shakeit');
    }, 300);
  }

  clearMessages() {
    this.usernameError = this.passwordError = '';
    $('#login_form').find('.form-group').removeClass('has-error');
  }

}
