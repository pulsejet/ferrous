import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { Person } from '../interfaces';
import { PersonDetailsComponent } from './personDetails';
import { Router } from '@angular/router';

@Component({
    selector: 'person',
    templateUrl: './person.html'
})
export class PersonComponent {
    public people: Person[];

    constructor(public router: Router, public http: Http, @Inject('BASE_URL') baseUrl: string) {
        http.get(baseUrl + 'api/People').subscribe(result => {
            this.people = result.json() as Person[];
        }, error => console.error(error));
    }

    public delete(id = "", rowNumber: number) {
        if (confirm("Are you sure to delete?")) {
            this.http.delete('/api/People/' + id).subscribe(result => {
                this.people.splice(rowNumber, 1);
            });
        }
    }
}

