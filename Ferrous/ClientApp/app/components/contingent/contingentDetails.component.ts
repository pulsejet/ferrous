import { Component, Inject } from '@angular/core';
import { Http, Headers, RequestOptions } from '@angular/http';
import { ActivatedRoute, Params, Routes, Route, Router } from '@angular/router';
import { Contingent, RoomAllocation } from '../interfaces';

@Component({
    selector: 'contingent',
    templateUrl: './contingentDetails.component.html'
})
export class ContingentDetailsComponent {
    public id: string = '1';
    public startedit: number;
    public editing: boolean = false;
    public initial_contingent: Contingent;
    public contingent: Contingent = {} as Contingent;

    constructor(private activatedRoute: ActivatedRoute,
                public router: Router, public http: Http, @Inject('BASE_URL') baseUrl: string) {
        this.http = http;
        this.editing = false;
        this.activatedRoute.params.subscribe((params: Params) => {
            this.id = params['id'];
            this.startedit = params['edit'];
        });

        if (this.id != '0') {
            http.get(baseUrl + 'api/Contingents/' + this.id).subscribe(result => {
                this.contingent = result.json() as Contingent;
                this.initial_contingent = { ...this.contingent };
            }, error => console.error(error));
        }
        if (this.startedit == 1) {
            this.editing = true;
        }
    }

    public edit() {
        if (this.startedit == 1) {
            this.router.navigate(['/contingent/']);
            return;
        }
        this.editing = !this.editing;
        this.contingent = { ...this.initial_contingent };
    }

    public save() {
        let body = JSON.stringify(this.contingent);
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });

        if (this.id != '0') {
            this.http.put('/api/Contingents/' + this.id, body, options).subscribe(result => {
                this.initial_contingent = { ...this.contingent };
                this.editing = !this.editing;
            });
        } else {
            this.http.post('/api/Contingents', body, options).subscribe(result => { });
        }
        if (this.startedit == 1) this.router.navigate(['/contingent/']);
    }

    public delete() {
        if (confirm("Are you sure to delete?")) {
            this.http.delete('/api/Contingents/' + this.id).subscribe(result => {
                this.router.navigate(['/contingent/']);
            });
        }
    }

    public unallocateRoom(roomA: RoomAllocation) {
        if (confirm("Are you sure you want to unallocate this room?")) {
            this.http.delete('/api/RoomAllocations/' + roomA.sno).subscribe(result => {
                var index = this.contingent.roomAllocation.indexOf(roomA, 0);
                this.contingent.roomAllocation.splice(index, 1)
            });
        }
    }

}