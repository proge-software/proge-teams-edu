  
  export interface IListTeam {
    TeamsId: string;
    Name: string;
    Description: string;
    JoinUrl: string;
    JoinCode: string;
    IsMembershipLimitedToOwners: boolean;
    TeamType : string;
    DepartmentId: string;
    CDSCod: string;
    OfferYear: string;
    CourseYear: string;
    CourseMandatory: boolean;
    CodCourse: string;
    CourtYear: string;
    Session: string;
    InternalId: string;
    IsMember: boolean;
    CourseNote: string;
    Owners?: IOwner[];
  }

  export interface IOwner {
    DisplayName: string;
    FullName: string;
    GivenName:string;
    SurName: string;
    Email: string;
    UserUPN: string;
    UserId: string;
    Presence: string;
    Initial: string;
    ImageURL: string;
  }

  export interface IListTeamToShow {
    TeamsId: string;
    Name: string;
    Description: string;
    JoinUrl: string;
    CourseYear: string;
    CourseMandatory: boolean;
    CodCourse: string;
    CourseNote: string;
    InternalId: string;
    IsMember: boolean;
    Owners?: IOwner[];
  }

  export interface IListGroup {
    key: string;
    name: string;
    startIndex: number;
    count: number;
    level: number;
    isCollapsed: boolean;
    nTeamsJoined: number;
  }
   