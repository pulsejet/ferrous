import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Contingent, RoomAllocation, Person, Room, Building, ContingentArrival, EnumContainer, Link } from './components/interfaces'
import { Router } from '@angular/router';

const API_Contingents_URL: string = '/api/Contingents/';
const API_Buildings_URL: string = '/api/Buildings/';
const API_People_URL: string = '/api/People/';

const SF_RoomLayouts_URL = '/roomTemplates/';

const API_Logout_URL: string = '/api/login/logout';

var JSON_HEADERS = new HttpHeaders();
JSON_HEADERS = JSON_HEADERS.set('Content-Type', 'application/json');

/* Main Data Service */
@Injectable()
export class DataService {

    public passedData: any;
    SetPassedData(data: any): void { this.passedData = data; }
    GetPassedData(): any { return this.passedData; }

    constructor(private http: HttpClient, public router: Router) { }

    /* Code for Link management */
    GetLink(links: Link[], rel: string = "self", encoded: boolean = false): any {
        let found = links.find(x => x.rel === rel);
        if (found == null) return null;
        if (encoded) return this.EncodeObject(found);
        return found;
    }

    GetLinkSelf(links: Link[], encoded: boolean = false): any { return this.GetLink(links, "self", encoded); }
    GetLinkUpdate(links: Link[], encoded: boolean = false): any { return this.GetLink(links, "update", encoded); }
    GetLinkDelete(links: Link[], encoded: boolean = false): any { return this.GetLink(links, "delete", encoded); }
    GetLinkCreate(links: Link[], encoded: boolean = false): any { return this.GetLink(links, "create", encoded); }

    FireLinkSelf(links: Link[]): Observable<any> { return this.FireLink(this.GetLinkSelf(links)); }
    FireLinkUpdate(links: Link[], body: any): Observable<any> { return this.FireLink(this.GetLinkUpdate(links), body); }
    FireLinkDelete(links: Link[]): Observable<any> { return this.FireLink(this.GetLinkDelete(links)); }

    EncodeObject(o: any): string { return encodeURIComponent(btoa(JSON.stringify(o))) }
    DecodeObject(s: string): any { return JSON.parse(atob(decodeURIComponent(s))) }

    FireLink(link: Link, body: any = null): Observable<any> {
        switch (link.method) {
            case "GET":
                return this.http.get(link.href);
            case "POST":
                return this.http.post(link.href, body, { headers: JSON_HEADERS });
            case "PUT":
                return this.http.put(link.href, body, { headers: JSON_HEADERS });                
            case "DELETE":
                return this.http.delete(link.href);
            default:
                throw new Error("no method defined for " + link.href);
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

    NavigatePersonDetails(link: Link, newRecord: boolean = false): void {
        this.router.navigate(['/personDetails', this.EncodeObject(link), (newRecord ? '1' : '0')]);
    }

    /**
     * Navigate to Location Selection 
     * @param clno CLNo of the contingent being allocated rooms; Use "mark" if marking
     * @param contingentArrivalNo contingentArrivalNo for which rooms are being allocated
     */
    NavigateLayoutSelect(clno:string, contingentArrivalNo: number): void {
        this.router.navigate(['/locationSelect', clno, contingentArrivalNo]);
    }

    NavigateRoomLayout(link: Link, location: string, clno: string): void {
        this.router.navigate(['/roomLayout', this.EncodeObject(link), location, clno]);
    }

    /** All Contingents */
    GetAllContingents(): Observable<EnumContainer> {
        return this.http.get<EnumContainer>(API_Contingents_URL);
    }

    /** All People */
    GetAllPeople(): Observable<EnumContainer> {
        return this.http.get<EnumContainer>(API_People_URL);
    }

    /**
     * Gets contingent-arrival-specific extended Building[]
     * @param clno CLNo of the contingent
     * @param cano ContingentArrivalNo applicable
     */
    GetAllBuildingsExtended(clno: string, cano: number): Observable<EnumContainer> {
        return this.http.get<EnumContainer>(API_Buildings_URL + "e/" + clno + "/" + cano.toString());
    }

    /* === RoomLayout === */

    /**
     * Get room layout HTML
     * @param location Location code
     */
    GetRoomLayout(location: string): Observable<any> {
        return this.http.get(SF_RoomLayouts_URL + location + '.html', { responseType: 'text' });
    }

    MarkRoom(room: Room, status: number): Observable<any> {
        let link: Link = { ... this.GetLink(room.links, "mark") };
        link.href += "?status=" + status.toString();
        return this.FireLink(link);
    }

    AllotRoom(room: Room): Observable<any> {
        let link: Link = {... this.GetLink(room.links, "allot") };
        if (room.partialallot || this.RoomCheckPartial(room)) {
            if (room.partialsel == null) throw new Error("Partial number not set!");
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
        let count: number = 0;
        for (let roomA of room.roomAllocation) {
            if (roomA.partial != null) count += Number(roomA.partial);
        }
        return count;
    }

    /**
     * DELETE a RoomAllocation
     * @param sno SNo of RoomAllocation
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
     * End the session
     */
    Logout(): Observable<any> {
        return this.http.get(API_Logout_URL);
    }
}