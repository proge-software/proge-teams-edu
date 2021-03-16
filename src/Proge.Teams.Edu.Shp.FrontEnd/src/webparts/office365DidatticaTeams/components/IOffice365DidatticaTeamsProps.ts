import { WebPartContext } from "@microsoft/sp-webpart-base";
import { ClientMode } from "../model/ClientMode";

export interface IOffice365DidatticaTeamsProps {
  description: string;
  clientMode: ClientMode;
  context: WebPartContext;
  listSite: string;
  listName: string; 
  listFilter: string;
}
