using Ferrous.Controllers;
using Ferrous.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ferrous.Misc
{
    public class LinksMaker
    {
        private ClaimsPrincipal User;
        private IUrlHelper Url;

        public LinksMaker(ClaimsPrincipal User, IUrlHelper Url)
        {
            this.User = User;
            this.Url = Url;
        }

        public List<Link> API_SPEC()
        {
            var list = new LinkHelper()
                .SetOptions(User, typeof(ContingentsController), Url)
                .AddLink(nameof(ContingentsController.GetContingents), null, "contingents")

                .SetOptions(User, typeof(BuildingsController), Url)
                .AddLink(nameof(BuildingsController.GetBuildingsExtended), null, "buildings")

                .SetOptions(User, typeof(PeopleController), Url)
                .AddLink(nameof(PeopleController.GetPeople), null, "people")

                .SetOptions(User, typeof(LoginController), Url)
                .AddLink(nameof(LoginController.login), null, "login")
                .AddLink(nameof(LoginController.Logout), null, "logout")
                .GetLinks();

            /* Add websocket */
            list.Add(new Link("building_websocket", "GET", WebSocketHubs.BuildingUpdateHub.BuildingWebsocketUrl));
            list.Add(new Link("building_websocket_join", "GET", nameof(WebSocketHubs.BuildingUpdateHub.JoinBuilding)));
            return list;
        }

        /// <summary>
        /// Fill Links object for Contingents.
        /// </summary>
        /// <param name="contingent">Contingent object</param>
        /// <returns></returns>
        public void FillContingentsLinks(Contingents contingent)
        {
            var idObject = new { id = contingent.ContingentLeaderNo };

            contingent.Links = new LinkHelper()
                /* Contingent Actions */
                .SetOptions(User, typeof(ContingentsController), Url)
                .AddLink(nameof(ContingentsController.GetContingent), idObject)
                .AddLink(nameof(ContingentsController.PutContingent), idObject)
                .AddLink(nameof(ContingentsController.DeleteContingent), idObject)

                /* POST a ContingentArrival */
                .SetOptions(User, typeof(ContingentArrivalsController), Url)
                .AddLink(
                    nameof(ContingentArrivalsController.PostContingentArrival), 
                    new { }, 
                    "create_contingent_arrival"
                )
                .GetLinks();

            foreach (var ca in contingent.ContingentArrival)
                FillContingentArrivalLinks(ca);

            foreach (var ra in contingent.RoomAllocation)
                FillRoomAllocationLinks(ra);

            foreach (var person in contingent.Person)
                FillPersonLinks(person);
        }

        /// <summary>
        /// Fills Links object for Building.
        /// </summary>
        /// <param name="building"></param>
        /// <returns></returns>
        public void FillBuildingsLinks(Building building, string clno, int cano)
        {
            var idObject = new { id = building.Location, clno = clno, cano = cano };

            building.Links = new LinkHelper()
                   .SetOptions(User, typeof(BuildingsController), Url)
                   .AddLink(nameof(BuildingsController.GetBuilding), idObject)
                   .AddLink(nameof(BuildingsController.PutBuilding), idObject)
                   .AddLink(nameof(BuildingsController.DeleteBuilding), idObject)
                   .GetLinks();

            if (building.Room != null)
                foreach (var room in building.Room)
                    FillRoomLinks(room, clno, cano);
        }

        /// <summary>
        /// Fill Links object for ContingentArrival
        /// </summary>
        /// <param name="contingentArrival">ContingentArrival object</param>
        /// <returns></returns>
        public void FillContingentArrivalLinks(ContingentArrival contingentArrival)
        {
            var idObject = new { id = contingentArrival.ContingentArrivalNo };
            contingentArrival.Links = new LinkHelper()
                .SetOptions(User, typeof(ContingentArrivalsController), Url)
                .AddLink(nameof(ContingentArrivalsController.PutContingentArrival), idObject)
                .AddLink(nameof(ContingentArrivalsController.DeleteContingentArrival), idObject)

                .SetOptions(User, typeof(RoomAllocationsController), Url)
                .AddLink(nameof(RoomAllocationsController.PostRoomAllocation), new { }, "create_room_allocation")

                .GetLinks();
        }

        /// <summary>
        /// Fill Links object for Room Allocation
        /// </summary>
        /// <param name="roomAllocation">RoomAllocation object</param>
        public void FillRoomAllocationLinks(RoomAllocation roomAllocation)
        {
            var idObject = new { id = roomAllocation.Sno };
            roomAllocation.Links = new LinkHelper()
                .SetOptions(User, typeof(RoomAllocationsController), Url)
                .AddLink(nameof(RoomAllocationsController.DeleteRoomAllocation), idObject)
                .GetLinks();
        }

        /// <summary>
        /// Fill Links object for Room
        /// </summary>
        /// <param name="room">Room object</param>
        public void FillRoomLinks(Room room, string clno, int cano)
        {
            var idObject = new { id = room.RoomId };
            var idObjectAllot = new { id = room.RoomId, clno = clno, cano = cano };

            room.Links = new LinkHelper()
                .SetOptions(User, typeof(RoomsController), Url)
                .AddLink(nameof(RoomsController.GetRoom), idObject)
                .AddLink(nameof(RoomsController.PutRoom), idObject)
                .AddLink(nameof(RoomsController.DeleteRoom), idObject)
                .AddLink(nameof(RoomsController.RoomAllot), idObjectAllot, "allot")
                .AddLink(nameof(RoomsController.mark), idObject, "mark")
                .GetLinks();

            foreach (var roomA in room.RoomAllocation)
                FillRoomAllocationLinks(roomA);
        }

        /// <summary>
        /// Fills links object for Person object
        /// </summary>
        /// <param name="person">Person object to fill</param>
        public void FillPersonLinks(Person person)
        {
            var idObject = new { id = person.Mino };
            person.links = new LinkHelper()
                .SetOptions(User, typeof(PeopleController), Url)
                .AddLink(nameof(PeopleController.GetPerson), idObject)
                .AddLink(nameof(PeopleController.PutPerson), idObject)
                .AddLink(nameof(PeopleController.DeletePerson), idObject)

                .SetOptions(User, typeof(ContingentsController), Url)
                .AddLink(nameof(ContingentsController.GetContingent), new { id = person.ContingentLeaderNo }, "contingent")

                .GetLinks();
        }

    }
}
