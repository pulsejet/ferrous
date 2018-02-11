import { PageEvent } from "@angular/material/paginator";

export class PaginatorHelper {
    pageSize: number = 10;
    pageSizeOptions: number[] = [5, 10, 25, 100, 200];
    pageEvent: PageEvent;

    public paginate(array: any[]): any[] {
        if (this.pageEvent) {
            let start = this.pageEvent.pageIndex * this.pageEvent.pageSize;
            return array.slice(start, start + this.pageEvent.pageSize);
        } else {
            return array.slice(0, this.pageSize);
        }
    }
}