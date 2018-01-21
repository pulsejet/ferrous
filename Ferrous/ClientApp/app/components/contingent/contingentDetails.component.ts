import { Component, Inject } from '@angular/core';
import { ActivatedRoute, Params, Routes, Route, Router } from '@angular/router';
import { Contingent, RoomAllocation } from '../interfaces';
import { Location } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';

/* Contingent Details Component */
@Component({
    selector: 'contingent',
    templateUrl: './contingentDetails.component.html',
    styleUrls: ['../../Custom.css']
})
export class ContingentDetailsComponent {
    public CLNo: string;                        /* current CLNo                             */
    public startedit: number;                   /* 1 to start editing on initial load       */
    public editing: boolean = false;            /* true if currently editing                */
    public initial_contingent: Contingent;      /* object for reverting cancelled changes   */
    public contingent: Contingent;              /* master Contingent object                 */

    constructor(private activatedRoute: ActivatedRoute,
        private _location: Location,
        private dataService: DataService,
        private titleService: Title,
        @Inject('BASE_URL') baseUrl: string) {

        this.titleService.setTitle("Contingent Details");

        /* Get URL parameters */
        this.activatedRoute.params.subscribe((params: Params) => {
            this.CLNo = params['id'];
            this.startedit = params['edit'];
        });

        /* CLNo 0 indicates a new record  *
         * Fetch if not a new record      */
        if (this.CLNo != '0') {
            this.dataService.GetContingent(this.CLNo).subscribe(result => {
                this.contingent = result;
                this.initial_contingent = { ...this.contingent };   /* Shallow copy */
            }, error => {
                console.error(error);
                alert("No such Contingent or error retrieving!");
                _location.back();       /* Go back if invalid */
            });
        }

        /* Start editing if edit was clicked */
        if (this.startedit == 1) {
            this.editing = true;
            this.contingent = {} as Contingent;
        }
        
    }

    /* Handle actions of both edit and cancel */
    public edit() {
        /* Cancel if startedit navigates to contingent list  */
        if (this.startedit == 1) {
            this.dataService.NavigateContingentsList();
            return;
        }

        /* Start/Stop editing; revert to initial values */
        this.editing = !this.editing;
        this.contingent = { ...this.initial_contingent };
    }

    /* PUT/POST the master */
    public save() { 
        let body = JSON.stringify(this.contingent);

        /* PUT for editing; POST for new record */
        if (this.CLNo != '0') {
            this.dataService.PutContingent(this.CLNo, body).subscribe(result => {
                /* Update the initial data */
                this.initial_contingent = { ...this.contingent };
                this.editing = !this.editing;
                if (this.startedit == 1) this.dataService.NavigateContingentsList();
            });
        } else {
            /* Go back to list for new record */
            this.dataService.PostContingent(body).subscribe(result => {
                this.dataService.NavigateContingentsList();
            });
        }
    }

    /* Delete a record */
    public delete() {
        if (confirm("Are you sure to delete?")) {
            this.dataService.DeleteContingent(this.CLNo).subscribe(result => {
                this.dataService.NavigateContingentsList();
            });
        }
    }

    /* Remove a RoomAllocation */
    public unallocateRoom(roomA: RoomAllocation) {
        if (confirm("Are you sure you want to unallocate this room?")) {
            this.dataService.UnllocateRoom(roomA.sno).subscribe(result => {
                /* Get the index and splice it from master */
                var index = this.contingent.roomAllocation.indexOf(roomA, 0);
                this.contingent.roomAllocation.splice(index, 1)
            });
        }
    }

}