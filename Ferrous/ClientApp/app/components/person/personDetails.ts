import { Component } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Person } from '../interfaces';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';

@Component({
    selector: 'personDetails',
    templateUrl: './personDetails.html',
    styleUrls: ['../../Custom.css']
})
export class PersonDetailsComponent {
    public id: string;                  /* current MINo                             */
    public startedit: number;           /* 1 to start editing on initial load       */
    public editing: boolean = false;    /* true if currently editing                */
    public initialPerson: Person;       /* object for reverting cancelled changes   */
    public person: Person;              /* master Person object                     */

    constructor(private activatedRoute: ActivatedRoute,
        private titleService: Title,
        private dataService: DataService,
        public router: Router) {
        this.editing = false;

        this.titleService.setTitle("Personal Details");

        /* Get URL parameters */
        this.activatedRoute.params.subscribe((params: Params) => {
            this.id = params['id'];
            this.startedit = params['edit'];
        });

        /* MINo 0 indicates a new record  *
         * Fetch if not a new record      */
        if (this.id !== '0') {
            this.dataService.GetPerson(this.id).subscribe(result => {
                this.person  = result;
                this.initialPerson = { ...this.person };

            }, error => console.error(error));
        }

        /* Start editing if edit was clicked */
        if (this.startedit === 1) {
            this.editing = true;
            this.person = {} as Person;
        }
    }

    /* Handle actions of both edit and cancel */
    public edit() {
        /* Cancel if startedit navigates to person list  */
        if (this.startedit === 1) {
            this.router.navigate(['/person/']);
            return;
        }

        /* Start/Stop editing; revert to initial values */
        this.editing = !this.editing;
        this.person = { ...this.initialPerson };
    }

    /* PUT/POST the master */
    public save() {
        let body = JSON.stringify(this.person);

        if (this.id !== '0') {
            this.dataService.PutPerson(this.id,body).subscribe((): void => {
                /* Update the initial data */
                this.initialPerson = { ...this.person };
                this.editing = !this.editing;
            });
        } else {
            /* Go back to list for new record */
            this.dataService.PostPerson(body).subscribe((): void => {
                this.router.navigate(['/person/']);
            });
        }
        if (this.startedit === 1) this.router.navigate(['/person/']);
    }

    /* Delete a record */
    public delete() {
        if (confirm("Are you sure to delete?")) {
            this.dataService.DeletePerson(this.id).subscribe((): void => {
                this.router.navigate(['/person/']);
            });
        }
    }
}