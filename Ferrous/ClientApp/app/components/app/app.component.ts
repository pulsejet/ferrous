import { ChangeDetectorRef, Component, ViewChild, Injectable } from '@angular/core';
import { MediaMatcher } from '@angular/cdk/layout';
import { MatMenuTrigger } from '@angular/material';
import { Title } from '@angular/platform-browser';

@Component({
    selector: 'app',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
@Injectable()
export class AppComponent {
    mobileQuery: MediaQueryList;

    private _mobileQueryListener: () => void;

    constructor(changeDetectorRef: ChangeDetectorRef, media: MediaMatcher,
                private titleService: Title) {
        this.mobileQuery = media.matchMedia('(max-width: 600px)');
        this._mobileQueryListener = () => changeDetectorRef.detectChanges();
        this.mobileQuery.addListener(this._mobileQueryListener);
        this.titleService.setTitle("Home");
    }

    ngOnDestroy(): void {
        this.mobileQuery.removeListener(this._mobileQueryListener);
    }
}