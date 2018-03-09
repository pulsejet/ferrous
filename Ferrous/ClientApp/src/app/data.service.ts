import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Contingent, RoomAllocation, Person, Room, Building, ContingentArrival, EnumContainer, Link } from './interfaces';
import { Router } from '@angular/router';

const SF_RoomLayouts_URL = '/roomTemplates/';

let JSON_HEADERS = new HttpHeaders();
JSON_HEADERS = JSON_HEADERS.set('Content-Type', 'application/json');

const API_SPEC_URL = '/api/export/api_spec';

/* Main Data Service */
@Injectable()
export class DataService {

    /** Stores the API_SPEC */
    public _API_SPEC: Link[];

    /** True whenever any user is logged in */
    public loggedIn = false;

    /** Can be used for passing data between components */
    public passedData: any;

    /**
     * Set the static passed data
     * @param data Data to store
     */
    SetPassedData(data: any): void { this.passedData = data; }

    /** Gets the stored data */
    GetPassedData(): any { return this.passedData; }

    constructor(private http: HttpClient, public router: Router) { }

    public RefreshAPISpec(): Observable<any> {
        return this.http.get<Link[]>(API_SPEC_URL);
    }

    public GetAPISpec() {
        return this._API_SPEC;
    }

    /**
     * Get link from rel
     * @param links Array of links
     * @param rel Required rell
     * @param encoded Returns encoded object if true
     */
    GetLink(links: Link[], rel: string = 'self'): Link {
        const found = links.find(x => x.rel === rel);
        if (found == null) {
            return {} as Link;
        }
        return found as Link;
    }

    /**
     * Retrn the link with rel "self"
     * @param links Array of links
     * @param encoded Returns encoded string if true
     */
    GetLinkSelf(links: Link[]): Link { return this.GetLink(links, 'self'); }

    /**
     * Retrn the link with rel "update"
     * @param links Arrar of links
     * @param encoded Returns enocded string if true
     */
    GetLinkUpdate(links: Link[]): Link { return this.GetLink(links, 'update'); }

    /**
     * Returns the link with rel "delete"
     * @param links Array of links
     * @param encoded Retrns encoded string if true
     */
    GetLinkDelete(links: Link[], encoded: boolean = false): Link { return this.GetLink(links, 'delete'); }

    /**
     * Return the string with rel "create"
     * @param links Array of links
     * @param encoded Returns encoded string if true
     */
    GetLinkCreate(links: Link[], encoded: boolean = false): Link { return this.GetLink(links, 'create'); }

    /**
     * Fires the link with rel "self"
     * @param links Array of links
     */
    FireLinkSelf<T>(links: Link[]): Observable<T> { return this.FireLink<T>(this.GetLinkSelf(links)); }

    /**
     * Fires the link with rel "update"
     * @param links Array of links
     * @param body JSON body to upload
     */
    FireLinkUpdate<T>(links: Link[], body: any): Observable<T> { return this.FireLink<T>(this.GetLinkUpdate(links), body); }

    /**
     * Fires the link with rel "delete"
     * @param links Array of links
     */
    FireLinkDelete<T>(links: Link[]): Observable<T> { return this.FireLink<T>(this.GetLinkDelete(links)); }

    /**
     * Encode an object for passing through URL
     * @param o Object to encode
     */
    EncodeObject(o: any): string { return btoa(JSON.stringify(o)); }

    /**
     * Decode an object encoded with "EncodeObject"
     * @param s Encoded string
     */
    DecodeObject<T>(s: string): T { return JSON.parse(atob(s)) as T; }

    /**
     * Fire a link and return the result as an observable
     * @param link Link to fire
     * @param body Optional body to upload only for POST and PUT requests
     */
    FireLink<T>(link: Link, body: any = null, options: any = null): Observable<T> {
        /* Fill in parameters */
        let URL = link.href;
        if (options != null) {
            for (const prop in options) {
                if (options.hasOwnProperty(prop)) {
                    URL = URL.replace('{' + prop + '}', options[prop]);
                }
            }
        }

        /* Use the correct method */
        switch (link.method) {
            case 'GET':
                return this.http.get<T>(URL);
            case 'POST':
                return this.http.post<T>(URL, body, { headers: JSON_HEADERS });
            case 'PUT':
                return this.http.put<T>(URL, body, { headers: JSON_HEADERS });
            case 'DELETE':
                return this.http.delete<T>(URL);
            default:
                throw new Error('no method defined for ' + URL);
        }
    }

    /* === Navigate - voids === */

    /**
     * Navigate to list of Contingents
     */
    NavigateContingentsList(): void {
        this.router.navigate(['/contingent/']);
    }

    NavigateContingentDetails(link: Link, newRecord: boolean = false): void {
        this.router.navigate(['/contingentDetails', this.EncodeObject(link), (newRecord ? '1' : '0')]);
    }

    /**
     * Navigate to list of People
     */
    NavigatePeopleList(): void {
        this.router.navigate(['/person/']);
    }

    /**
     * Navigate to person details
     * @param link "self" link for the person
     * @param newRecord true for creating a new record. If true, link must be the "create" link
     */
    NavigatePersonDetails(link: Link, newRecord: boolean = false): void {
        this.router.navigate(['/personDetails', this.EncodeObject(link), (newRecord ? '1' : '0')]);
    }

    /**
     * Navigate to Location Selection
     * @param ca ContingentArrival for which rooms are being allocated
     * @param clno ContingentLeaderNo
     */
    NavigateLayoutSelect(ca: ContingentArrival, clno: string): void {
        this.router.navigate(['/locationSelect', this.EncodeObject(this.GetLink(ca.links, 'buildings')), clno]);
    }

    /**
     * Navigate to room layout
     * @param link "self" link for the building
     * @param location Location for local purposes
     * @param clno ContingentLeaderNo for local highlighting
     */
    NavigateRoomLayout(link: Link, location: string, clno: string): void {
        this.router.navigate(['/roomLayout', this.EncodeObject(link), location, clno]);
    }

    /** All Contingents */
    GetAllContingents(): Observable<EnumContainer> {
        return this.FireLink<EnumContainer>(this.GetLink(this.GetAPISpec(), 'contingents'));
    }

    /** All People */
    GetAllPeople(): Observable<EnumContainer> {
        return this.FireLink<EnumContainer>(this.GetLink(this.GetAPISpec(), 'people'));
    }

    /**
     * Gets contingent-arrival-specific extended Building[]
     * @param link Link to follow
     */
    GetAllBuildingsExtended(link: Link): Observable<EnumContainer> {
        return this.FireLink<EnumContainer>(link);
    }

    /* === RoomLayout === */

    /**
     * Get room layout HTML
     * @param location Location code
     */
    GetRoomLayout(location: string): Observable<any> {
        return this.http.get(SF_RoomLayouts_URL + location + '.html', { responseType: 'text' });
    }

    /**
     * Mark a room with a given status
     * @param room Room object
     * @param status Status to mark
     */
    MarkRoom(room: Room, status: number): Observable<any> {
        const link: Link = { ... this.GetLink(room.links, 'mark') };
        link.href += '?status=' + status.toString();
        return this.FireLink(link);
    }

    /**
     * Allot a room
     * @param room Room object
     */
    AllotRoom(room: Room): Observable<any> {
        const link: Link = {... this.GetLink(room.links, 'allot') };
        if (room.partialallot || this.RoomCheckPartial(room)) {
            if (room.partialsel == null) { throw new Error('Partial number not set!'); }
            link.href += '?partialno=' + room.partialsel;
        }

        return this.FireLink(link);
    }

    /**
     * Returns true if room is fully occupied
     * @param room Room to check
     */
    RoomCheckOccupied(room: Room): boolean {
        return this.RoomGetPartialNo(room) < 0;
    }

    /**
     * Returns if room is partially occupied (returns true even if partial is more than capacity)
     * @param room Room to check
     */
    RoomCheckPartial(room: Room): boolean {
        return this.RoomGetPartialNo(room) > 0;
    }

    /**
     * Get number of people occupying room partially
     * @param room Room to check
     */
    RoomGetPartialNo(room: Room): number {
        let count = 0;
        for (const roomA of room.roomAllocation) {
            if (roomA.partial != null) {
                count += Number(roomA.partial);
            }
        }
        return count;
    }

    /**
     * Fires the "delete" link in a RoomAllocation
     * @param roomA RoomAllocation object
     */
    UnallocateRoom(roomA: RoomAllocation): Observable<any> {
        return this.FireLinkDelete(roomA.links);
    }

    /* === Quick Extras which shouldn't be here === */

    /**
     * Returns true if object is a valid number (in range)
     * @param num object to be checked
     * @param min optional minimum bound (defaults to -999999)
     * @param max optional maximum bound (defaults to +999999)
     */
    CheckValidNumber(num: number, min: number = -999999, max: number = 999999): boolean {
        return (num != null &&
            !isNaN(Number(num)) &&
            Number(num) >= min &&
            Number(num) <= max);
    }

    /**
     * Get if a user is logged in
     * TODO: Do this with the API spec
     */
    GetCurrentUser(): Observable<any> {
        return this.FireLink(this.GetLink(this.GetAPISpec(), 'getuser'));
    }

    /**
     * End the session
     */
    Logout(): Observable<any> {
        return this.FireLink(this.GetLink(this.GetAPISpec(), 'logout'));
    }
}
