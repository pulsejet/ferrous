import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { Contingent } from '../interfaces';
import { ContingentDetailsComponent } from './contingentDetails.component';
import { Router } from '@angular/router';
import { MatTableDataSource } from '@angular/material';

@Component({
    selector: 'contingent',
    templateUrl: './contingent.component.html'
})
export class ContingentComponent {
    public contingents: Contingent[];
    public dataSource: any;
    displayedColumns = ['contingentLeaderNo', 'male', 'female', 'arrivedM', 'arrivedF', 'delete', 'edit', 'details'];

    constructor(public router: Router, public http: Http, @Inject('BASE_URL') baseUrl: string) {
        http.get(baseUrl + 'api/Contingents').subscribe(result => {
            this.contingents = result.json() as Contingent[];
            this.dataSource = new MatTableDataSource(this.contingents);
        }, error => console.error(error));
    }

    public delete(elem: Contingent) {
        if (confirm("Are you sure to delete?")) {
            this.http.delete('/api/Contingents/' + elem.contingentLeaderNo).subscribe(result => {
                let index = this.contingents.indexOf(elem);
                this.contingents.splice(index, 1);
                this.dataSource = new MatTableDataSource(this.contingents);
            });
        }
    }
}

