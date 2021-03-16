import { WebPartContext } from "@microsoft/sp-webpart-base";
import { ClientMode } from "../model/ClientMode";

export interface IExamsManagementProps {
  description: string;
  clientMode: ClientMode;
  context: WebPartContext;
  listSite: string;
  listName: string; 
  listFilter: string;
  adminGruopId: string;
}
