using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.GraphApi.Models
{
    public enum AutoAdmittedUsers
    {
        Everyone,
        EveryoneInSameAndFederatedCompany,
        EveryoneInCompany,
        EveryoneInCompanyExcludingGuests,
        InvitedUsers,
        OrganizerOnly
    }

    public enum PresenterOption
    {
        Everyone,
        EveryoneInCompany,
        SpecifiedPeople,
        OrganizerOnly
    }
}
