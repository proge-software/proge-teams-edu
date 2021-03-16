import { IListTeam } from "./ITeamsClasses";


export default class MockHttpClient  {

  private static _joined: string[] = ['01-01'];  
  private static _items: any = [{   Description: 'Corso 3', 
                                            Name: 'Insegnamento 1', 
                                            TeamsId: '01-01', 
                                            IsMembershipLimitedToOwners: false, 
                                            JoinCode : 'AAAAAA',
                                            TeamType : "Insegnamento",
                                            DepartmentId:"",
                                            CDSCod: "",
                                            OfferYear: "",
                                            CourseYear: "1",
                                            CourseMandatory: true,
                                            CodCourse: "AAAA",
                                            IsMember: false,
                                            InternalId: "aaaa",
                                            CourtYear: "2020",
                                            Session: "1",
                                            CourseNote: "Nota Test",
                                            Owners: "Sonia|BERGAMASCHI|sonia@unimore.it@@@@Michele|DE LUCA|deluca@unimore.it",
                                            JoinUrl:"https://teams.microsoft.com/l/team/19:d35af536c1a040d789485ba1b3c6db63%40thread.tacv2/conversations?groupId=2d09c2a0-a241-4d96-851f-6e605c6d8ebe&tenantId=e787b025-3fc6-4802-874a-9c988768f892" },
                                            {   Description: 'Corso 3', 
                                            Name: 'Insegnamento 2', 
                                            TeamsId: '01-02', 
                                            IsMembershipLimitedToOwners: false, 
                                            JoinCode : 'BBBBB',
                                            TeamType : "Insegnamento",
                                            DepartmentId:"",
                                            CDSCod: "",
                                            OfferYear: "",
                                            CourseYear: "2",
                                            CourseMandatory: true,
                                            CodCourse: "AAAA",
                                            InternalId: "aaaa",
                                            IsMember: false,
                                            CourtYear: "2019",
                                            Session: "2",
                                            CourseNote: "",
                                            Owners: "Sonia|BERGAMASCHI|sonia@unimore.it",
                                            JoinUrl:"https://teams.microsoft.com/l/team/19:d35af536c1a040d789485ba1b3c6db63%40thread.tacv2/conversations?groupId=2d09c2a0-a241-4d96-851f-6e605c6d8ebe&tenantId=e787b025-3fc6-4802-874a-9c988768f892" },
                                            {   Description: 'Corso 2', 
                                            Name: 'Insegnamento 1', 
                                            TeamsId: '02-01', 
                                            IsMembershipLimitedToOwners: true, 
                                            JoinCode : '',
                                            TeamType : "Insegnamento",
                                            DepartmentId:"",
                                            CDSCod: "",
                                            OfferYear: "",
                                            CourseYear: "1",
                                            CourseMandatory: true,
                                            CodCourse: "BBBB",
                                            InternalId: "aaaa",
                                            IsMember: false,
                                            CourtYear: "2020",
                                            Owners: "Sonia|BERGAMASCHI|sonia@unimore.it@@@@Michele|DE LUCA|deluca@unimore.it",
                                            Session: "1",
                                            CourseNote: "Nota Lunga Nota Lunga Nota Lunga Nota Lunga Nota Lunga Nota Lunga Nota Lunga Nota Lunga Nota Lunga Nota Lunga Nota Lunga Nota Lunga Nota Lunga ",
                                            JoinUrl:"https://teams.microsoft.com/l/team/19:d35af536c1a040d789485ba1b3c6db63%40thread.tacv2/conversations?groupId=2d09c2a0-a241-4d96-851f-6e605c6d8ebe&tenantId=e787b025-3fc6-4802-874a-9c988768f892" },
                                            {   Description: 'Corso 1', 
                                            Name: 'Insegnamento 1', 
                                            TeamsId: '03-01', 
                                            IsMembershipLimitedToOwners: false, 
                                            JoinCode : '',
                                            TeamType : "Insegnamento",
                                            DepartmentId:"",
                                            CDSCod: "",
                                            OfferYear: "",
                                            CourseYear: "2",
                                            CourseMandatory: true,
                                            CodCourse: "CCCC",
                                            InternalId: "aaaa",
                                            IsMember: false,
                                            CourtYear: "2020",
                                            Session: "1",
                                            CourseNote: "",
                                            JoinUrl:"https://teams.microsoft.com/l/team/19:d35af536c1a040d789485ba1b3c6db63%40thread.tacv2/conversations?groupId=2d09c2a0-a241-4d96-851f-6e605c6d8ebe&tenantId=e787b025-3fc6-4802-874a-9c988768f892" },
                                            {   Description: 'Corso 1', 
                                            Name: 'Esame 1', 
                                            TeamsId: '03-02', 
                                            IsMembershipLimitedToOwners: false, 
                                            JoinCode : '',
                                            TeamType : "Esame",
                                            DepartmentId:"",
                                            CDSCod: "",
                                            OfferYear: "",
                                            CourseYear: "2",
                                            CourseMandatory: true,
                                            CodCourse: "CCCC",
                                            InternalId: "aaaa",
                                            IsMember: false,
                                            CourseNote: "",
                                            CourtYear: "2019",
                                            Session: "2",
                                           JoinUrl:"https://teams.microsoft.com/l/team/19:d35af536c1a040d789485ba1b3c6db63%40thread.tacv2/conversations?groupId=2d09c2a0-a241-4d96-851f-6e605c6d8ebe&tenantId=e787b025-3fc6-4802-874a-9c988768f892" }]
                                            ;
                                            
 
  public static getTeams(): Promise<any> {
    return new Promise<any>((resolve) => {
      resolve(MockHttpClient._items);
    });
  }

  public static getJoinedTeams(): Promise<string[]> {
    return new Promise<string[]>((resolve) => {
      resolve(MockHttpClient._joined);
    });
  } 
}