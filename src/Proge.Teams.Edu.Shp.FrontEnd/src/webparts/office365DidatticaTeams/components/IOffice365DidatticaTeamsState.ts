import { ITag } from "office-ui-fabric-react";
import { IListTeam, IListGroup, IListTeamToShow } from "../model/ITeamsClasses";

export interface IOffice365DidatticaTeamsState {
  JoinedTeams: string[];
  ListTeamsCount: number;
  ListTeams: IListTeam[];
  ListTeamsGrouped: IListGroup[];
  ListTeamsToShow: IListTeamToShow[];
  Loading: boolean;
  SearchTags: ITag[];  
  PickerList: ITag[];  
  isSearched: boolean;
  searchResults: number;
}