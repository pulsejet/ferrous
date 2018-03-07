import { Component } from '@angular/core';
import { Person, Link } from '../interfaces';
import { Title } from '@angular/platform-browser';
import { DataService } from '../data.service';
import { PaginatorHelper } from '../helpers';

/* Person Component */
@Component({
    selector: 'app-person',
    templateUrl: './person.component.html',
})
export class PersonComponent {
    /** Master list of people */
    public people: Person[];
    paginatorHelper = new PaginatorHelper;

    public links: Link[];

    /** constructor for PersonComponent */
    constructor(
        private titleService: Title,
        private dataService: DataService) {

        this.titleService.setTitle('People');

        /* Populate the master */
        dataService.GetAllPeople().subscribe(result => {
            this.people = result.data;
            this.links = result.links;
        }, error => console.error(error));
    }

    /** Handle table click */
    public handleTableClick(person: Person) {
        this.dataService.NavigatePersonDetails(this.dataService.GetLinkSelf(person.links));
    }

    /** Create a new record */
    public openNewRecord() {
        this.dataService.NavigatePersonDetails(this.dataService.GetLinkCreate(this.links), true);
    }
}

