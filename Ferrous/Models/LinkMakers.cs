using Ferrous.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ferrous.Models
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

        /// <summary>
        /// Links object for Contingents. SHOULD BE CALLED ONLY from ContingentsController
        /// </summary>
        /// <param name="contingent">Contingent object</param>
        /// <returns></returns>
        public void FillContingentsLinks(Contingents contingent)
        {
            var idObject = new { id = contingent.ContingentLeaderNo };

            contingent.Links = new LinkHelper()
                /* Contingent Actions */
                .SetOptions(User, typeof(ContingentsController), Url)
                .AddLink(nameof(ContingentsController.GetContingent), idObject, "no")
                .AddLink(nameof(ContingentsController.PutContingent), idObject, "no")
                .AddLink(nameof(ContingentsController.DeleteContingent), idObject, "no")

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
        /// Includes Links object in all necessary sub data for Building. SHOULD BE CALLED ONLY from BuildingsController
        /// </summary>
        /// <param name="building"></param>
        /// <returns></returns>
        public void FillBuildingsLinks(Building building, string clno, int cano)
        {
            var idObject = new { id = building.Location, clno = clno, cano = cano };

            building.Links = new LinkHelper()
                   .SetOptions(User, typeof(BuildingsController), Url)
                   .AddLink(nameof(BuildingsController.GetBuilding), idObject, "no")
                   .AddLink(nameof(BuildingsController.PutBuilding), idObject, "no")
                   .AddLink(nameof(BuildingsController.DeleteBuilding), idObject, "no")
                   .GetLinks();

            if (building.Room != null)
                foreach (var room in building.Room)
                    FillRoomLinks(room, clno, cano);
        }

        /// <summary>
        /// Links object for ContingnetArrival
        /// </summary>
        /// <param name="contingentArrival">ContingentArrival object</param>
        /// <returns></returns>
        public void FillContingentArrivalLinks(ContingentArrival contingentArrival)
        {
            var idObject = new { id = contingentArrival.ContingentArrivalNo };
            contingentArrival.Links = new LinkHelper()
                .SetOptions(User, typeof(ContingentArrivalsController), Url)
                .AddLink(nameof(ContingentArrivalsController.PutContingentArrival), idObject, "no")
                .AddLink(nameof(ContingentArrivalsController.DeleteContingentArrival), idObject, "no")

                .SetOptions(User, typeof(RoomAllocationsController), Url)
                .AddLink(nameof(RoomAllocationsController.PostRoomAllocation), new { }, "create_room_allocation")

                .GetLinks();
        }

        /// <summary>
        /// Links object for Room Allocation
        /// </summary>
        /// <param name="roomAllocation">RoomAllocation object</param>
        public void FillRoomAllocationLinks(RoomAllocation roomAllocation)
        {
            var idObject = new { id = roomAllocation.Sno };
            roomAllocation.Links = new LinkHelper()
                .SetOptions(User, typeof(RoomAllocationsController), Url)
                .AddLink(nameof(RoomAllocationsController.PutRoomAllocation), idObject, "no")
                .AddLink(nameof(RoomAllocationsController.DeleteRoomAllocation), idObject, "no")
                .GetLinks();
        }

        /// <summary>
        /// Fills Links object for Room
        /// </summary>
        /// <param name="room">Room object</param>
        public void FillRoomLinks(Room room, string clno, int cano)
        {
            var idObject = new { id = room.RoomId };
            var idObjectAllot = new { id = room.RoomId, clno = clno, cano = cano };

            room.Links = new LinkHelper()
                .SetOptions(User, typeof(RoomsController), Url)
                .AddLink(nameof(RoomsController.GetRoom), idObject, "no")
                .AddLink(nameof(RoomsController.PutRoom), idObject, "no")
                .AddLink(nameof(RoomsController.DeleteRoom), idObject, "no")
                .AddLink(nameof(RoomsController.RoomAllot), idObjectAllot, "allot")
                .AddLink(nameof(RoomsController.mark), idObject, "mark")
                .GetLinks();

            foreach (var roomA in room.RoomAllocation)
                FillRoomAllocationLinks(roomA);
        }

        public void FillPersonLinks(Person person)
        {
            var idObject = new { id = person.Mino };
            person.links = new LinkHelper()
                .SetOptions(User, typeof(PeopleController), Url)
                .AddLink(nameof(PeopleController.GetPerson), idObject, "no")
                .AddLink(nameof(PeopleController.PutPerson), idObject, "no")
                .AddLink(nameof(PeopleController.DeletePerson), idObject, "no")

                .SetOptions(User, typeof(ContingentsController), Url)
                .AddLink(nameof(ContingentsController.GetContingent), new { id = person.ContingentLeaderNo }, "contingent")

                .GetLinks();
        }

    }
}
