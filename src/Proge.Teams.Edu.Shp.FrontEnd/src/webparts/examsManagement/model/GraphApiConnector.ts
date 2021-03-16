import { AadHttpClient, MSGraphClient } from "@microsoft/sp-http";
import { WebPartContext } from "@microsoft/sp-webpart-base";
import { IListReq } from "./ITeamsClasses";

export default class GraphApiConnector  {

    public static getMyGroups(_context: WebPartContext): Promise<string[]> {
        return _context.msGraphClientFactory.getClient()
        .then((client: MSGraphClient)=> {
          return client.api("/me/memberOf").version("v1.0").select("id, displayName").get().then((response:any) => {
            var st: string[] = [];
            st = response.value.map((item: any) => {
              return item.id;
            });       
            console.log(st);     
            return st;
          });
        }) as unknown as Promise<string[]>;
      }
    
      public static getMyCoursesFromGraphApi(_context: WebPartContext): Promise<string> {
        return _context.msGraphClientFactory.getClient()
        .then((client: MSGraphClient)=> {
          return client.api("/me").version("v1.0").select("userPrincipalName,jobTitle,displayName,onPremisesExtensionAttributes").get().then((response:any) => {
            return response.onPremisesExtensionAttributes.extensionAttribute11 ? response.onPremisesExtensionAttributes.extensionAttribute11:""; 
          });
        }) as unknown as Promise<string>;
      }


      public static updateRequestTeam(_context: WebPartContext, listSite: string, listName: string, dt:string, Id: string, el:IListReq): Promise<string> 
      {
        const siteid:string = "533546b7-bb80-4ce3-b92c-4ff07408f84e";
        const listid: string = "65b6f23c-2b27-49f7-91ef-7760cb2117ac";
        const usr = _context.pageContext.user.loginName;


        const path = "/sites/" + siteid + "/lists/" + listid + "/items/"+ el.Id + "/fields";
        return _context.msGraphClientFactory.getClient()  
        .then((client: MSGraphClient)=> {
          return client.api(path)
            .version("v1.0").update("{\"TeamRequested\": true, \"TeamRequestUser\": \""+ usr + "\", \"TeamRequestDate\": \""+ dt + "\"}")
            .then((response:any) => {
              console.log("update ok");
              return "ok";
        }).catch((error:any) => {
          console.log("Errore durante aggiornamento elemento lista: " + error.message);
          return error.message;
        });
      }).catch ((error:any) => {
        console.log("Errore durante aggiornamento elemento lista: " + error);
        return error.toString();
      });
    }      
}
