import { Pipe, PipeTransform } from '@angular/core';
import { Contingent } from '../components/interfaces';

@Pipe({
    name: 'filtercontingents',
    pure: false
})
export class FilterContingents implements PipeTransform {
    transform(items: Contingent[], clno: string): any {
        if (!items || !clno) {
            return items;
        }

        return items.filter(item => item.contingentLeaderNo.indexOf(clno) !== -1);
    }
}