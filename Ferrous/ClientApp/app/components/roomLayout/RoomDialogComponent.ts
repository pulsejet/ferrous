﻿import { Component, Inject, ViewChild, ElementRef } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { ContingentArrival, Room } from '../interfaces';
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
        this.dataService.PutRoom(this.room.id.toString(), this.room).subscribe(r => {
            this.dialogRef.close();
        });
    }

}