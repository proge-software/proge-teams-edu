import { ITag } from "office-ui-fabric-react";
import { IListReq, IListGroup } from "../model/ITeamsClasses";

export interface IExamsManagementState {
  JoinedGroups: string[];
  ReqsCount: number;
  ListReqs: IListReq[];
  ListGrouped: IListGroup[];
  ListReqsToShow: IListReq[];
  Loading: boolean;
  SearchTags: ITag[];  
  PickerList: ITag[];  
  isSearched: boolean;
  searchResults: number;
  isUpdated: boolean;
  esitoUpdate: string;
  erroreUpdate: string; 
  showLog: boolean;
  InfoLogs:string;

}