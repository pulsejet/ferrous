import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Room } from '../interfaces';
import { DataService } from '../data.service';

@Component({
    selector: 'app-room-dialog',
    templateUrl: './room-dialog.component.html',
})
export class RoomDialogComponent {
    /** Current Room object */
    public room: Room;

    /** constructor for RoomDialogComponent */
    constructor(
        public dialogRef: MatDialogRef<RoomDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any,
        private dataService: DataService) {
        this.room = { ...data };
    }

    /** PUT changes to the Room */
    confirmSelection() {
        this.dataService.FireLinkUpdate(this.room.links, this.room).subscribe((): void => {
            this.dialogRef.close();
        });
    }

}
