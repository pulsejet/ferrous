import { Component, Inject } from '@angular/core';
import { Contingent } from '../interfaces';
import { ContingentDetailsComponent } from './contingentDetails.component';
import { MatTableDataSource } from '@angular/material';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';

@Component({
    selector: 'contingent',
    templateUrl: './contingent.component.html',
    styleUrls: ['../../Custom.css']
})
export class ContingentComponent {
    public contingents: Contingent[];
    public enteredCL: string = "";

    constructor(
        private dataService: DataService,
        @Inject('BASE_URL') baseUrl: string,
        private titleService: Title ) {

        this.titleService.setTitle("Contingents");

        this.dataService.GetAllContingents().subscribe(result => {
            this.contingents = result;
        }, error => console.error(error));
    }

    public delete(id = "", rowNumber: number) {
        if (confirm("Are you sure to delete?")) {
            this.dataService.DeleteContingent(id).subscribe(result => {
                this.contingents.splice(rowNumber, 1);
            });
        }
    }

    public handleTableClick(contingent: Contingent) {
        if (window.innerWidth <= 768) this.dataService.NavigateContingentDetails(contingent.contingentLeaderNo);
    }
}

