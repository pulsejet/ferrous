import { Component, Inject } from '@angular/core';
import { ActivatedRoute, Params, Routes, Route, Router } from '@angular/router';
import { Contingent, RoomAllocation, ContingentArrival } from '../interfaces';
import { Location } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { MatSnackBar } from '@angular/material';
import { DataService } from '../../DataService';
import { ContingentArrivalDialogComponent } from './ContingentArrivalDialog';
import { MatDialog } from '@angular/material';

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
        public snackBar: MatSnackBar,
        public dialog: MatDialog,
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

                /* Alert the user */
                this.snackBar.open("Room unallocated", "Dismiss", {
                    duration: 2000,
                });
            });
        }
    }

    public StartAllocation() {
        let dialog = this.dialog.open(ContingentArrivalDialogComponent, {
            data: { ca: this.contingent.contingentArrival, clno: this.CLNo }
        });
    }

    /* Get alloted capacity for arrival */
    public GetArrivedContingent(female: boolean): string {
        let curr: number = 0;
        let currO: number = 0;

        for (let ca of this.contingent.contingentArrival) {
            curr += Number(female ? ca.female : ca.male);
            currO += Number(female ? ca.femaleOnSpot : ca.maleOnSpot);
        }
        if (currO > 0) return curr + " + " + currO;
        else return curr.toString();
    }

    /* Get no of people by sex */
    public GetPeopleBySex(female: boolean): string {
        let curr: number = 0;

        /* Count people */
        for (let person of this.contingent.person) {
            if (person.sex && ((female && person.sex.toUpperCase() == "F") ||
                (!female && person.sex.toUpperCase() == "M")))
                curr++;
        }

        return curr.toString();
    }
}