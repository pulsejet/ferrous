import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { ContingentArrival } from '../interfaces';
import { DataService } from '../../DataService';

@Component({
    selector: 'app-choose-emoji-dialog',
    templateUrl: './ContingentArrivalDialog.html'
})
export class ContingentArrivalDialogComponent {
    chosenEntry: number = -1;                           /* Id of old chosen entry                   */
    public nContingentArrv: ContingentArrival;          /* new ContingentArrival If needed          */
    public contingentArrivals: ContingentArrival[];     /* Passed Data                              */
    public CLNo: string;

    constructor(
        public dialogRef: MatDialogRef<ContingentArrivalDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any,
        private dataService: DataService) {
        this.contingentArrivals = data["ca"];
        this.CLNo = data["clno"];
        this.nContingentArrv = {} as ContingentArrival;
    }

    confirmSelection(chosenEntry: number) {
        this.nContingentArrv.createdOn = new Date();
        if ((this.nContingentArrv.male && this.nContingentArrv.male > 0) ||
            (this.nContingentArrv.female && this.nContingentArrv.female > 0) ||
            (this.nContingentArrv.maleOnSpot && this.nContingentArrv.maleOnSpot > 0) ||
            (this.nContingentArrv.femaleOnSpot && this.nContingentArrv.femaleOnSpot > 0)
        ) {

            if (chosenEntry != -1)
                if (!confirm("New record will be created!"))
                    return;

            /* Prepare and create the new entry */
            this.nContingentArrv.contingentLeaderNo = this.CLNo;
            let body = JSON.stringify(this.nContingentArrv);
            this.dataService.PostContingentArrival(body).subscribe(result => {
                this.dataService.NavigateLayoutSelect(result.contingentLeaderNo, result.contingentArrivalNo);
            })
            this.dialogRef.close(this.chosenEntry);

        } else {
            /* Use the old entry */
            if (chosenEntry == -1) { alert("Validation failed or nothing to do!"); return; }
            this.dataService.NavigateLayoutSelect(
                this.CLNo, this.contingentArrivals[chosenEntry].contingentArrivalNo);
            this.dialogRef.close(chosenEntry);
        }
    }

    /* Get alloted capacity for arrival */
    GetAllotedCapacity(ca: ContingentArrival): number {
        let ans: number = 0;
        for (let roomA of ca.roomAllocation) {
            if (roomA.partial <= 0) ans += Number(roomA.room.capacity)
            else ans += Number(roomA.partial);
        }
        return ans;
    }

    /* Delete an entry */
    deleteArrival(chosenEntry: number) {
        this.dataService.DeleteContingentArrival(this.contingentArrivals[chosenEntry].contingentArrivalNo).subscribe(result => {
            this.contingentArrivals.splice(chosenEntry, 1);
            this.chosenEntry = -1;
        });
    }

}