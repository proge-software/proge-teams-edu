import { SizeRange } from "@microsoft/microsoft-graph-types";

  
  export interface IListReq {
    Id: string,
    groupname: string;
    appelloId: string;
    Title: string;
    TeamId: string;
    TeamRequested: boolean;
    TeamCreated: boolean;
    TeamJoinUrl: string;
    aaCalId: string;
    adCod: string;
    adDes : string;
    adId: string;
    appId: string;
    cdsCod: string;
    cdsDes: string;
    cdsId: boolean;
    dataFineIscr: string;
    dataInizioApp: string;
    dataInizioIscr: string;
    Ins_anno_corso: string;
    Ins_aa_corte:string;
    Ins_partizionamento_cod:string;
    Ins_partizionamento:string;
    desApp: string;
    App_NumIscritti: number;
    ExternalId: string;
    App_Note: string;
    App_Stato: string;
    App_StatoDes: string;
    TeamCreationDate: string;
    Owners?: IPerson[];
    Members?: IPerson[];
    IsMember: boolean;
    InternalId: string;
    IsMembershipLimitedToOwners: boolean;
    DipCod: string;
    DipDes: string;
    CommissioneUsers: string;
    Sessioni: string;
    Turni: string;
    EsameComune: string;
    tipoGestAppCod: string;
    tipoGestAppDes: string;
    tipoGestPrenCod: string;
    tipoGestPrenDes: string;
    tipoDefAppCod: string;
    tipoDefAppDes: string;    
    ContainUser: boolean;
  }

  export interface IPerson {
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
    RuoloCod: string;
    RuoloDes: string;
    IdTurno: string;
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
   