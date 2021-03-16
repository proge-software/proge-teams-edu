import { WebPartContext } from "@microsoft/sp-webpart-base";
import {
    SPHttpClient,
    SPHttpClientResponse
  } from '@microsoft/sp-http';
import { IListReq } from "./ITeamsClasses";
  
export default class SharepointAPIConnector  {
  
    public static getListItemsDetails(_context: WebPartContext, listSite: string, listName: string, listFilter: string, listSelect: string): Promise<any> {
      return _context.spHttpClient.get(listSite + `/_api/web/lists/getbytitle('${listName}')/items?$select=${listSelect}&$Filter=${listFilter}&$top=4000`, SPHttpClient.configurations.v1)
        .then((response: SPHttpClientResponse) => {
          return response.json();
        });
    }

    public static getListItemsCount(_context: WebPartContext, listSite: string, listName: string, listFilter: string): Promise<number> {
      return _context.spHttpClient.get(listSite + `/_api/web/lists/getbytitle('${listName}')/itemCount?$Filter=${listFilter}`, SPHttpClient.configurations.v1)
      .then((response: SPHttpClientResponse): Promise<{value: number}> =>{
        return response.json();
      }).then((response: {value: number}): number => {
          return response.value;
        });
    } 

   
  }