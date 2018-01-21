import { Component, Inject } from '@angular/core';
import { Contingent } from '../interfaces';
import { ContingentDetailsComponent } from './contingentDetails.component';
import { MatTableDataSource } from '@angular/material';
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

    /* Delete contingent action */
    public delete(id = "", rowNumber: number) {
        if (confirm("Are you sure to delete?")) {
            this.dataService.DeleteContingent(id).subscribe(result => {
                /* Remove from master list */
                this.contingents.splice(rowNumber, 1);
            });
        }
    }

    /* Table click event for small devices  *
     * TODO: Fix the hard-coded value 768px */
    public handleTableClick(contingent: Contingent) {
        if (window.innerWidth <= 768) this.dataService.NavigateContingentDetails(contingent.contingentLeaderNo);
    }
}
