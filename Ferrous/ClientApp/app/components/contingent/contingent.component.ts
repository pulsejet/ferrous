import { Component, Inject } from '@angular/core';
import { Contingent } from '../interfaces';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';
import { PaginatorHelper } from '../../Common';

/* Contingent Component */
@Component({
    selector: 'contingent',
    templateUrl: './contingent.component.html',
    styleUrls: ['../../Custom.css']
})
export class ContingentComponent {

    contingents: Contingent[];                              /* master contingents list              */
    enteredCL: string = "";                                 /* CLNo entered in the search box       */
    paginatorHelper = new PaginatorHelper;                  /* helper for paginator                 */

    /* Initial Actions */
    constructor(
        private dataService: DataService,
        private titleService: Title ) {

        this.titleService.setTitle("Contingents");

        /* Load our contingents */
        this.dataService.GetAllContingents().subscribe(result => {
            this.contingents = result;
        }, error => console.error(error));
    }

    /* Table click event */
    public handleTableClick(contingent: Contingent) {
        this.dataService.NavigateContingentDetails(contingent.contingentLeaderNo);
    }
}

