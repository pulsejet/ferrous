import { Component, Inject } from '@angular/core';
import { Http, Headers, RequestOptions } from '@angular/http';
import { ActivatedRoute, Params, Routes, Route, Router } from '@angular/router';
import { Person } from '../interfaces';
import { style, state, animate, transition, trigger } from '@angular/core';
import { Title } from '@angular/platform-browser';

@Component({
    selector: 'personDetails',
    templateUrl: './personDetails.html',
    styleUrls: ['../../Custom.css']
})
export class PersonDetailsComponent {
    public id: string;                  /* current MINo                             */
    public startedit: number;           /* 1 to start editing on initial load       */
    public editing: boolean = false;    /* true if currently editing                */
    public initial_person: Person;      /* object for reverting cancelled changes   */
    public person: Person;              /* master Person object                     */

    constructor(private activatedRoute: ActivatedRoute,
        private titleService: Title,
        public router: Router, public http: Http, @Inject('BASE_URL') baseUrl: string) {
        this.editing = false;

        this.titleService.setTitle("Personal Details");

        /* Get URL parameters */
        this.activatedRoute.params.subscribe((params: Params) => {
            this.id = params['id'];
            this.startedit = params['edit'];
        });

        /* MINo 0 indicates a new record  *
         * Fetch if not a new record      */
        if (this.id != '0') {
            http.get(baseUrl + 'api/People/' + this.id).subscribe(result => {
                this.person  = result.json() as Person;
                this.initial_person = { ...this.person };

            }, error => console.error(error));
        }

        /* Start editing if edit was clicked */
        if (this.startedit == 1) {
            this.editing = true;
            this.person = {} as Person;
        }
    }

    /* Handle actions of both edit and cancel */
    public edit() {
        /* Cancel if startedit navigates to person list  */
        if (this.startedit == 1) {
            this.router.navigate(['/person/']);
            return;
        }

        /* Start/Stop editing; revert to initial values */
        this.editing = !this.editing;
        this.person = { ...this.initial_person };
    }

    /* PUT/POST the master */
    public save() {
        let body = JSON.stringify(this.person);

        /* PUT for editing; POST for new record */
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });

        if (this.id != '0') {
            this.http.put('/api/People/' + this.id, body, options).subscribe(result => {
                /* Update the initial data */
                this.initial_person = { ...this.person };
                this.editing = !this.editing;
            });
        } else {
            /* Go back to list for new record */
            this.http.post('/api/People', body, options).subscribe(result => {
                this.router.navigate(['/person/'])
            });
        }
        if (this.startedit == 1) this.router.navigate(['/person/']);
    }

    /* Delete a record */
    public delete() {
        if (confirm("Are you sure to delete?")) {
            this.http.delete('/api/People/' + this.id).subscribe(result => {
                this.router.navigate(['/person/']);
            });
        }
    }
}