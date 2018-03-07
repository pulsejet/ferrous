import { Component, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Title } from '@angular/platform-browser';

/* Homepage */
@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
})
export class HomeComponent {
    constructor(private activatedRoute: ActivatedRoute,
        private titleService: Title,
        public router: Router, @Inject('BASE_URL') baseUrl: string) {
        this.titleService.setTitle('Home');
    }
    enteredCL: string;
}
