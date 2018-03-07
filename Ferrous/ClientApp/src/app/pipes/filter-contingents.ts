import { Pipe, PipeTransform } from '@angular/core';
import { Contingent } from '../interfaces';

@Pipe({
    name: 'filter_contingents',
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
