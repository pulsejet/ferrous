import { Component, Inject } from '@angular/core';
import { ActivatedRoute, Params, Routes, Route, Router } from '@angular/router';
import { Contingent, RoomAllocation } from '../interfaces';
import { Location } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';

@Component({
    selector: 'contingent',
    templateUrl: './contingentDetails.component.html',
    styleUrls: ['../../Custom.css']
})
export class ContingentDetailsComponent {
    public id: string = '1';
    public startedit: number;
    public editing: boolean = false;
    public initial_contingent: Contingent;
    public contingent: Contingent = {} as Contingent;

    constructor(private activatedRoute: ActivatedRoute,
        private _location: Location,
        private dataService: DataService,
        private titleService: Title,
        @Inject('BASE_URL') baseUrl: string) {

        this.titleService.setTitle("Contingent Details");

        this.editing = false;
        this.activatedRoute.params.subscribe((params: Params) => {
            this.id = params['id'];
            this.startedit = params['edit'];
        });

        if (this.id != '0') {
            this.dataService.GetContingent(this.id).subscribe(result => {
                this.contingent = result;
                this.initial_contingent = { ...this.contingent };
            }, error => {
                console.error(error);
                alert("No such Contingent or error retrieving!");
                _location.back();
            });
        }
        if (this.startedit == 1) {
            this.editing = true;
        }
        
    }

    public edit() {
        if (this.startedit == 1) {
            this.dataService.NavigateContingentsList();
            return;
        }
        this.editing = !this.editing;
        this.contingent = { ...this.initial_contingent };
    }

    public save() { 
        let body = JSON.stringify(this.contingent);

        if (this.id != '0') {
            this.dataService.PutContingent(this.id, body).subscribe(result => {
                this.initial_contingent = { ...this.contingent };
                this.editing = !this.editing;
                if (this.startedit == 1) this.dataService.NavigateContingentsList();
            });
        } else {
            this.dataService.PostContingent(body).subscribe(result => {
                this.dataService.NavigateContingentsList();
            });
        }
    }

    public delete() {
        if (confirm("Are you sure to delete?")) {
            this.dataService.DeleteContingent(this.id).subscribe(result => {
                this.dataService.NavigateContingentsList();
            });
        }
    }

    public unallocateRoom(roomA: RoomAllocation) {
        if (confirm("Are you sure you want to unallocate this room?")) {
            this.dataService.UnllocateRoom(roomA.sno).subscribe(result => {
                var index = this.contingent.roomAllocation.indexOf(roomA, 0);
                this.contingent.roomAllocation.splice(index, 1)
            });
        }
    }

}