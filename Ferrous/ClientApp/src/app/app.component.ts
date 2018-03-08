import { ChangeDetectorRef, Component, Injectable, OnDestroy } from '@angular/core';
import { MediaMatcher } from '@angular/cdk/layout';
import { Title } from '@angular/platform-browser';
import { DataService } from './data.service';

/* TODO: This code comes from the example for the angular   *
 * material side nav. Clean it up and add comments.         */
@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
@Injectable()
export class AppComponent implements OnDestroy {
    mobileQuery: MediaQueryList;

    private _mobileQueryListener: () => void;

    constructor(
        changeDetectorRef: ChangeDetectorRef,
        media: MediaMatcher,
        private titleService: Title,
        public dataService: DataService) {

        this.mobileQuery = media.matchMedia('(max-width: 600px)');
        this._mobileQueryListener = () => changeDetectorRef.detectChanges();
        this.mobileQuery.addListener(this._mobileQueryListener);
        this.titleService.setTitle('Home');

        this.dataService.GetCurrentUser().subscribe(result => {
            this.dataService.loggedIn = true;
        }, error => {
            this.dataService.loggedIn = false;
        });
    }

    ngOnDestroy(): void {
        this.mobileQuery.removeListener(this._mobileQueryListener);
    }

    logout() {
        this.dataService.Logout().subscribe(() => {
            this.dataService.loggedIn = false;
        });
    }
}
