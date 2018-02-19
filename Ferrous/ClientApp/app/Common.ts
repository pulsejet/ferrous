﻿import { PageEvent } from "@angular/material/paginator";

/** Helper for using Angular Paginator */
export class PaginatorHelper {
    /** Default page size */
    pageSize: number = 10;

    /** Default options for page size */
    pageSizeOptions: number[] = [5, 10, 25, 100, 200];

    /** Event handler for changed size */
    pageEvent: PageEvent;

    /** Returns a sliced array from the current state of the paginator
     *  @param {any[]} array Input array to slice
     */
    public paginate(array: any[]): any[] {
        if (this.pageEvent) {
            let start = this.pageEvent.pageIndex * this.pageEvent.pageSize;
            return array.slice(start, start + this.pageEvent.pageSize);
        } else {
            return array.slice(0, this.pageSize);
        }
    }
}