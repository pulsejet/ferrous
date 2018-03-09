import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ContingentArrival, Link } from '../interfaces';
import { DataService } from '../data.service';

@Component({
    selector: 'app-contingent-arrival-dialog',
    templateUrl: './contingent-arrival-dialog.component.html'
})
export class ContingentArrivalDialogComponent {
    /** Id of old chosen entry */
    chosenEntry = -1;
    /** New ContingentArrival if necessary */
    public nContingentArrv: ContingentArrival;
    /** Passed Data */
    public contingentArrivals: ContingentArrival[];
    /** CLNo for saving */
    public CLNo: string;

    public links: Link[];

    /** constructor for ContingentArrivalDialogComponent */
    constructor(
        public dialogRef: MatDialogRef<ContingentArrivalDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any,
        private dataService: DataService) {
        this.contingentArrivals = data['ca'];
        this.CLNo = data['clno'];
        this.links = data['links'];
        this.nContingentArrv = {} as ContingentArrival;
    }

    /**
     * Validate and save everything
     * @param chosenEntry Index of old selected selected entry if any
     */
    confirmSelection(chosenEntry: number) {
        this.nContingentArrv.createdOn = new Date();
        if ((this.nContingentArrv.male && this.nContingentArrv.male > 0) ||
            (this.nContingentArrv.female && this.nContingentArrv.female > 0) ||
            (this.nContingentArrv.maleOnSpot && this.nContingentArrv.maleOnSpot > 0) ||
            (this.nContingentArrv.femaleOnSpot && this.nContingentArrv.femaleOnSpot > 0)
        ) {

            if (chosenEntry !== -1) {
                if (!confirm('New record will be created!')) {
                    return;
                }
            }

            /* Prepare and create the new entry */
            this.nContingentArrv.contingentLeaderNo = this.CLNo;
            const body = JSON.stringify(this.nContingentArrv);
            this.dataService.FireLink<ContingentArrival>(
                this.dataService.GetLink(
                    this.links, 'create_contingent_arrival'), body).subscribe(result => {
                this.dataService.NavigateLayoutSelect(result, this.CLNo);
            });

            this.dialogRef.close(this.chosenEntry);

        } else {
            /* Use the old entry */
            if (chosenEntry === -1) { alert('Validation failed or nothing to do!'); return; }
            this.dataService.NavigateLayoutSelect(this.contingentArrivals[chosenEntry], this.CLNo);
            this.dialogRef.close(chosenEntry);
        }
    }

    /**
     * Get alloted capacity for ContingentArrival
     * @param ca ContingentArrival to check
     */
    getAllotedCapacity(ca: ContingentArrival): number {
        if (ca.roomAllocation == null) { return 0; }
        let ans = 0;
        for (const roomA of ca.roomAllocation) {
            if (roomA.partial <= 0) {
                ans += Number(roomA.room.capacity);
            } else {
                ans += Number(roomA.partial);
            }
        }
        return ans;
    }

    /**
     * Delete a selected ContingentArrival
     * @param chosenEntry Index of chosen entry
     */
    deleteArrival(chosenEntry: number) {
        this.dataService.FireLinkDelete(this.contingentArrivals[chosenEntry].links).subscribe(result => {
            this.contingentArrivals.splice(chosenEntry, 1);
            this.chosenEntry = -1;
        });
    }

}
