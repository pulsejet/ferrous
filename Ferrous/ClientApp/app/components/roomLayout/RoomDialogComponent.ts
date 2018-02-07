import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Room } from '../interfaces';
import { DataService } from '../../DataService';

@Component({
    selector: 'room-dialog',
    templateUrl: './RoomDialogComponent.html',
    styleUrls: ['../../Custom.css']
})
export class RoomDialogComponent {
    public room: Room;

    constructor(
        public dialogRef: MatDialogRef<RoomDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any,
        private dataService: DataService) {
        this.room = { ...data };
    }

    confirmSelection() {
        this.dataService.PutRoom(this.room.roomId.toString(), this.room).subscribe((): void => {
            this.dialogRef.close();
        });
    }

}