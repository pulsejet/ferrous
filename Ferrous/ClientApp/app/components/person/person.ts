import { Component, Inject } from '@angular/core';
import { Person } from '../interfaces';
import { PersonDetailsComponent } from './personDetails';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';

/* Person Component */
@Component({
    selector: 'person',
    templateUrl: './person.html',
    styleUrls: ['../../Custom.css']
})
export class PersonComponent {
    public people: Person[];    /* master list of people */

    constructor(
        private titleService: Title,
        private dataService: DataService,
        @Inject('BASE_URL') baseUrl: string) {

        this.titleService.setTitle("People");

        /* Populate the master */
        dataService.GetAllPeople().subscribe(result => {
            this.people = result;
        }, error => console.error(error));
    }

    /* Handle table click */
    public handleTableClick(person: Person) {
        this.dataService.NavigatePersonDetails(person.mino);
    }
}

