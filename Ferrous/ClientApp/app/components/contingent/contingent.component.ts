import { Component, Inject } from '@angular/core';
import { Contingent } from '../interfaces';
import { ContingentDetailsComponent } from './contingentDetails.component';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';

/* Contingent Component */
@Component({
    selector: 'contingent',
    templateUrl: './contingent.component.html',
    styleUrls: ['../../Custom.css']
})
export class ContingentComponent {

    public contingents: Contingent[];   /* Master contingents list */
    public enteredCL: string = "";      /* CLNo entered in the search box */

    /* Initial Actions */
    constructor(
        private dataService: DataService,
        @Inject('BASE_URL') baseUrl: string,
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

