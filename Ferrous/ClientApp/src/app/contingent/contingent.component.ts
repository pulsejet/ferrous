import { Component, Inject } from '@angular/core';
import { Contingent, Link } from '../interfaces';
import { Title } from '@angular/platform-browser';
import { DataService } from '../data.service';
import { PaginatorHelper } from '../helpers';

/* Contingent Component */
@Component({
    selector: 'app-contingent',
    templateUrl: './contingent.component.html',
})
export class ContingentComponent {
    /** Master contingents list */
    contingents: Contingent[];
    /** Master links list */
    links: Link[];
    /** CLNo entered in the search box */
    enteredCL = '';
    paginatorHelper = new PaginatorHelper;

    /** constructor for ContingentComponent */
    constructor(
        private dataService: DataService,
        private titleService: Title ) {

        this.titleService.setTitle('Contingents');

        /* Load our contingents */
        this.dataService.GetAllContingents().subscribe(result => {
            this.contingents = result.data;
            this.links = result.links;
        }, error => console.error(error));
    }

    /** Table click event */
    public handleTableClick(contingent: Contingent) {
        this.dataService.NavigateContingentDetails(this.dataService.GetLinkSelf(contingent.links));
    }

    /** Create a new record */
    public openNewRecord() {
        this.dataService.NavigateContingentDetails(this.dataService.GetLinkCreate(this.links), true);
    }
}

