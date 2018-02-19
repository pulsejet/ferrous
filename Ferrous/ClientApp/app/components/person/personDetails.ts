import { Component } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Person, Link } from '../interfaces';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';

@Component({
    selector: 'personDetails',
    templateUrl: './personDetails.html',
    styleUrls: ['../../Custom.css']
})
export class PersonDetailsComponent {
    /** true if currently editing */
    public editing: boolean = false;
    /** object for reverting cancelled changes */
    public initialPerson: Person;
    /** Master Person object */
    public person: Person;

    public newrecord: boolean;

    public urlLink: Link;

    /** constructor for personDetails */
    constructor(
        private activatedRoute: ActivatedRoute,
        private titleService: Title,
        public dataService: DataService,
        public router: Router) {
        this.editing = false;

        this.titleService.setTitle("Personal Details");

        /* Get URL parameters */
        this.activatedRoute.params.subscribe((params: Params) => {
            this.urlLink = this.dataService.DecodeObject(params['link']);
            this.newrecord = params['edit'] == 1;
        });

        /* MINo 0 indicates a new record  *
         * Fetch if not a new record      */
        if (!this.newrecord) {
            this.dataService.FireLink(this.urlLink).subscribe(result => {
                this.person  = result;
                this.initialPerson = { ...this.person };

            }, error => console.error(error));
        }

        /* Start editing if edit was clicked */
        if (this.newrecord) {
            this.editing = true;
            this.person = {} as Person;
        }
    }

    /** Handle actions of both edit and cancel */
    public edit() {
        /* Cancel if startedit navigates to person list  */
        if (this.newrecord) {
            this.router.navigate(['/person/']);
            return;
        }

        /* Start/Stop editing; revert to initial values */
        this.editing = !this.editing;
        this.person = { ...this.initialPerson };
    }

    /** PUT/POST the master */
    public save() {
        let body = JSON.stringify(this.person);

        if (!this.newrecord) {
            this.dataService.FireLinkUpdate(this.person.links, body).subscribe((): void => {
                /* Update the initial data */
                this.initialPerson = { ...this.person };
                this.editing = !this.editing;
            });
        } else {
            /* Go back to list for new record */
            this.dataService.FireLink(this.urlLink, body).subscribe((): void => {
                this.router.navigate(['/person/']);
            });
        }
        if (this.newrecord) this.router.navigate(['/person/']);
    }

    /** DELETE a record */
    public delete() {
        if (confirm("Are you sure to delete?")) {
            this.dataService.FireLinkDelete(this.person.links).subscribe((): void => {
                this.router.navigate(['/person/']);
            });
        }
    }
}