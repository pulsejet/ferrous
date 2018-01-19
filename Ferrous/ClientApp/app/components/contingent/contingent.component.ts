import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { Contingent } from '../interfaces';
import { ContingentDetailsComponent } from './contingentDetails.component';
import { Router } from '@angular/router';
import { MatTableDataSource } from '@angular/material';

@Component({
    selector: 'contingent',
    templateUrl: './contingent.component.html',
    styleUrls: ['../../Custom.css']
})
export class ContingentComponent {
    public contingents: Contingent[];

    constructor(public router: Router, public http: Http, @Inject('BASE_URL') baseUrl: string) {
        http.get(baseUrl + 'api/Contingents').subscribe(result => {
            this.contingents = result.json() as Contingent[];
        }, error => console.error(error));
    }

    public delete(id = "", rowNumber: number) {
        if (confirm("Are you sure to delete?")) {
            this.http.delete('/api/Contingents/' + id).subscribe(result => {
                this.contingents.splice(rowNumber, 1);
            });
        }
    }

    public handleTableClick(contingent: Contingent) {
        if (window.innerWidth <= 768)
            this.router.navigate(['/contingentDetails/' + contingent.contingentLeaderNo]);
    }
}

