import { AadHttpClient, MSGraphClient } from "@microsoft/sp-http";
import { WebPartContext } from "@microsoft/sp-webpart-base";

export default class GraphApiConnector  {

    public static getJoinedTeamsFromGraphApi(_context: WebPartContext): Promise<string[]> {
        return _context.msGraphClientFactory.getClient()
        .then((client: MSGraphClient)=> {
          return client.api("/me/joinedTeams").version("v1.0").select("id, displayName").get().then((response:any) => {
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
}
