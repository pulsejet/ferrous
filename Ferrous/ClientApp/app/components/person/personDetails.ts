import { Component, Inject } from '@angular/core';
import { Http, Headers, RequestOptions } from '@angular/http';
import { ActivatedRoute, Params, Routes, Route, Router } from '@angular/router';
import { Person } from '../interfaces';
import { style, state, animate, transition, trigger } from '@angular/core';

@Component({
    selector: 'personDetails',
    templateUrl: './personDetails.html'
})
export class PersonDetailsComponent {
    public id: string = '1';
    public startedit: number;
    public editing: boolean = false;
    public initial_person: Person;
    public person: Person = {} as Person;

    constructor(private activatedRoute: ActivatedRoute,
        public router: Router, public http: Http, @Inject('BASE_URL') baseUrl: string) {
        this.editing = false;

        this.activatedRoute.params.subscribe((params: Params) => {
            this.id = params['id'];
            this.startedit = params['edit'];
        });

        if (this.id != '0') {
            http.get(baseUrl + 'api/People/' + this.id).subscribe(result => {
                this.person  = result.json() as Person;
                this.initial_person = { ...this.person };

            }, error => console.error(error));
        }

        if (this.startedit == 1) {
            this.editing = true;
        }
    }

    public edit() {
        if (this.startedit == 1) {
            this.router.navigate(['/person/']);
            return;
        }
        this.editing = !this.editing;
        this.person = { ...this.initial_person };
    }

    public save() {
        let body = JSON.stringify(this.person);
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });

        if (this.id != '0') {
            this.http.put('/api/People/' + this.id, body, options).subscribe(result => {
                this.initial_person = { ...this.person };
                this.editing = !this.editing;
            });
        } else {
            this.http.post('/api/People', body, options).subscribe(result => { });
        }
        if (this.startedit == 1) this.router.navigate(['/person/']);
    }

    public delete() {
        if (confirm("Are you sure to delete?")) {
            this.http.delete('/api/People/' + this.id).subscribe(result => {
                this.router.navigate(['/person/']);
            });
        }
    }
}