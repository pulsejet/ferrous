import { Component, Inject } from '@angular/core';
import { Http, Headers, RequestOptions } from '@angular/http';
import { ActivatedRoute, Params, Routes, Route, Router } from '@angular/router';
import { Title } from '@angular/platform-browser';

/* Homepage */
@Component({
    selector: 'home',
    templateUrl: './home.component.html',
    styleUrls: ['../../Custom.css']
})
export class HomeComponent {
    constructor(private activatedRoute: ActivatedRoute,
        private titleService: Title,
        public router: Router, public http: Http, @Inject('BASE_URL') baseUrl: string) {
        this.titleService.setTitle("Home");
    }
    enteredCL: string;
}
