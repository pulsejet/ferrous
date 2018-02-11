import { Component } from '@angular/core';
import { Person } from '../interfaces';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';
import { PaginatorHelper } from '../../Common';

/* Person Component */
@Component({
    selector: 'person',
    templateUrl: './person.html',
    styleUrls: ['../../Custom.css']
})
export class PersonComponent {
    public people: Person[];                    /* master list of people            */
    paginatorHelper = new PaginatorHelper;      /* helper for paginator             */

    constructor(
        private titleService: Title,
        private dataService: DataService) {

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

