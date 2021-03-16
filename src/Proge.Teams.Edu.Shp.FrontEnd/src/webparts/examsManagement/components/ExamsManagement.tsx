import * as React from 'react';

import {
  DefaultButton,
  DetailsHeader,
  DetailsList,
  IColumn,
  IDetailsHeaderProps,
  IDetailsList,
  IGroup,
  IRenderFunction,
  IToggleStyles,
  mergeStyles,
  Toggle,
  IButtonStyles, Link, IIconProps, IDetailsListCheckboxProps, Checkbox, IDetailsGroupRenderProps, IGroupDividerProps, getTheme, mergeStyleSets, CheckboxVisibility, TextField, Label, Spinner, ProgressIndicator, Async, DetailsListLayoutMode, DetailsRow, IDetailsRowBaseProps, Stack, MessageBar, MessageBarType, IStackTokens, SearchBox, Tooltip, ITooltipHostStyles, TooltipHost, TagPicker, IBasePickerSuggestionsProps, ITag, IInputProps, Announced, PrimaryButton, ISuggestionItemProps, BaseButton, Button, MessageBarButton, IGroupHeaderProps, GroupedList, ActionButton, HighContrastSelectorBlack
} from 'office-ui-fabric-react';
import { IPersonaSharedProps, Persona, PersonaSize, PersonaPresence } from 'office-ui-fabric-react/lib/Persona';
import { FontIcon } from 'office-ui-fabric-react/lib/Icon';

import styles from './ExamsManagement.module.scss';
import { IExamsManagementProps } from './IExamsManagementProps';
import { IExamsManagementState } from './IExamsManagementState';
import { escape } from '@microsoft/sp-lodash-subset';
import * as strings from "ExamsManagementWebPartStrings";

import {
  Environment,
  EnvironmentType
} from '@microsoft/sp-core-library';
import MockHttpClient from "../model/MockHttpClient";
import { IListReq, IListGroup, IPerson } from "../model/ITeamsClasses";
import SharepointAPIConnector from '../model/SharepointAPIConnector';
import GraphApiConnector from '../model/GraphApiConnector';
import * as moment from 'moment';


const numericalSpacingStackTokens: IStackTokens = {
  childrenGap: 5,
};        

const pickerSuggestionsProps: IBasePickerSuggestionsProps = {
  suggestionsHeaderText: 'Ricerche suggerite',
  noResultsFoundText: 'Nessun suggerimento trovato'
};

const inputProps: IInputProps = {
  'aria-label': 'Tag Picker',
};

const today = moment().format("YYYY-MM-DD"); // need moment.js installed
export default class ExamsManagement extends React.Component<IExamsManagementProps, IExamsManagementState, {}> {
  
  constructor(props: IExamsManagementProps, state: IExamsManagementState) {
    super(props);

    // Initialize the state of the component
    this.state = {
      JoinedGroups: [], 
      ListReqs: [], 
      ListGrouped: [], 
      Loading: true, 
      ReqsCount: 0, 
      ListReqsToShow: [], 
      SearchTags:[], 
      isSearched:false, 
      searchResults:0, 
      PickerList:[], 
      esitoUpdate: "",
      isUpdated: false,
      erroreUpdate: "",  
      showLog: false, 
      InfoLogs: "",  

    };

  }

 private LogMessage(mex: string){
   console.log("APP_LOG = " + mex);
   const ll: string = this.state.InfoLogs + mex + "\n";
   this.setState({InfoLogs: ll});
 }
  private DateToItalian(dateeng:string) : string {
    try {
      return dateeng.split("-")[2] + "/" + dateeng.split("-")[1] + "/" + dateeng.split("-")[0]; 
    }
    catch {
      console.log("Error converting date: " + dateeng);
      return dateeng;
    }
  }


  private _renderListAsync(): void {
    this.setState ({JoinedGroups: [], ListReqs: [], ListGrouped: [], Loading : true});
    this.LogMessage("Start Loading data");
  
    // Local environment
    if (Environment.type === EnvironmentType.Local) {
      this.LogMessage("pre->getList");
      MockHttpClient.getTeams()
      .then((Reqs: IListReq[]) => {
        this.LogMessage("pre-getJoinedGroups");
        MockHttpClient.getJoinedTeams()
        .then((joined: string[]) => {
          this.LogMessage("pre-getCurrentUser");
          MockHttpClient.getCurrentUser().then((_usr:string) =>{
            this._CacheInitialData(Reqs, joined, "{1}AAAA|{2}BBBB",_usr);
              this.setState ({Loading : false});
              this.LogMessage("End Loading data");
            });      
        });
      });
    }
    else if (Environment.type == EnvironmentType.SharePoint || Environment.type == EnvironmentType.ClassicSharePoint) {

      this.LogMessage("pre->getListCount");
      SharepointAPIConnector.getListItemsCount(this.props.context, this.props.listSite,this.props.listName, this.props.listFilter).then((count) => {
        this.setState ({ReqsCount: count});
        const select:string = "Id, appelloId, Title, TeamId, TeamRequested, TeamCreated, TeamJoinUrl, aaCalId, adCod, adDes , adId, appId, cdsCod, cdsDes, cdsId, dataFineIscr, dataInizioApp, dataInizioIscr, desApp, App_NumIscritti, ExternalId, App_Note, App_Stato, App_StatoDes, TeamCreationDate, Ins_anno_corso, Ins_aa_corte, Ins_partizionamento_cod, Ins_partizionamento, Owners, Members, InternalId, IsMembershipLimitedToOwners, DipCod, DipDes, CommissioneUsers, Sessioni, Turni, EsameComune, tipoGestAppCod, tipoGestAppDes, tipoGestPrenCod, tipoGestPrenDes, tipoDefAppCod, tipoDefAppDes";        
        this.LogMessage("pre-getJoinedGroups");

        GraphApiConnector.getMyGroups(this.props.context).then((JT:string[])=>{
          var lFilter:string = this.props.listFilter;
          const _isAdmin: Boolean = (JT.indexOf(this.props.adminGruopId) > -1);
          if (!_isAdmin) {
            const uid = "u_" + this.props.context.pageContext.user.loginName.toString().split("@")[0];
            lFilter = lFilter + " and substringof('"+ uid.toLowerCase() + "', CommissioneUsers)";
          }

          this.LogMessage("pre->getListDetail");

          SharepointAPIConnector.getListItemsDetails(this.props.context, this.props.listSite, this.props.listName, lFilter, select).then((Reqs) => {
            GraphApiConnector.getMyCoursesFromGraphApi(this.props.context).then((JC:string)=>{
                this._CacheInitialData(Reqs.value, JT, JC,this.props.context.pageContext.user.loginName.toString());
                this.setState ({Loading : false});
                this.LogMessage("End Loading data");
              });
          });
        });
      });
    }

  } 

  private _CacheInitialData(Reqs: any, JT: string[], ext11: string, usr: string): void {

   
    if (JT)
    {      
      this.setState({JoinedGroups: JT});
    }

    this.LogMessage("Joined: " + JT.length.toString());
    this.LogMessage("USR: " + usr);


    var dateRegExp  = /^(19|20)\d\d[-](0[1-9]|1[012])[-](0[1-9]|[12][0-9]|3[01])$/; // Regex per date format yyyy-mm-dd
    var lt: IListReq[] = [];
   
    Reqs.map((item: any) => {
      lt.push( {
        Id: item.Id,
        groupname: (item.TeamRequested == false && item.dataInizioApp != null && item.dataInizioApp.match(dateRegExp) && item.dataInizioApp >= today) ? 
                   "Disponibili per Creazione Teams" //true condition = Not requested - not expired
                   : (item.TeamRequested == true && item.dataInizioApp != null && item.dataInizioApp.match(dateRegExp) && item.dataInizioApp >= today) // false condition check if teams already requested or created and if not expired 
                   ? "Teams Creati/Richiesti":"Scaduti/Chiusi",
        Title: item.Title,
        TeamId: item.TeamId,               
        appelloId: item.appelloId,
        TeamRequested: item.TeamRequested,
        TeamCreated: item.TeamCreated,
        TeamJoinUrl: item.TeamJoinUrl,
        aaCalId: item.aaCalId,
        adCod: item.adCod,
        adDes: item.adDes,
        adId: item.adId,
        appId: item.appId,
        cdsCod: item.cdsCod,
        cdsDes: item.cdsDes,
        cdsId: item.cdsId,
        dataFineIscr: item.dataFineIscr,
        dataInizioApp: item.dataInizioApp,
        dataInizioIscr: item.dataInizioIscr,
        desApp: item.desApp,
        App_NumIscritti: item.App_NumIscritti,
        ExternalId: item.ExternalId,
        App_Note: item.App_Note,
        App_Stato: item.App_Stato,
        App_StatoDes: item.App_StatoDes,
        Ins_anno_corso: item.Ins_anno_corso,
        Ins_aa_corte: item.Ins_aa_corte,
        Ins_partizionamento_cod: item.Ins_partizionamento_cod,
        Ins_partizionamento: item.Ins_partizionamento,
        TeamCreationDate: item.TeamCreationDate,
        InternalId: item.InternalId,
        Owners: item.Owners != null && item.Owners != undefined && item.Owners != "" 
        ? (item.Owners as string).split("@@@@").map(e => {
          var OW: IPerson =  {
            DisplayName : e.split("|")[0].length > 0 ?  e.split("|")[1] + ' ' + e.split("|")[0].substring(0, 1) + '.': e.split("|")[1],
            FullName: e.split("|")[0].length > 0 ? e.split("|")[1] + ' ' + e.split("|")[0] : e.split("|")[1],
            GivenName: e.split("|")[0],
            SurName: e.split("|")[1],
            Email: e.split("|")[2],
            UserUPN: e.split("|")[2],
            UserId: "",
            RuoloCod: e.split("|")[3],
            RuoloDes: e.split("|")[4],
            Presence: "none",
            Initial: (e.split("|")[0].length > 0 ? e.split("|")[0].substring(0, 1) : "") + (e.split("|")[1].length > 0 ? e.split("|")[1].substring(0, 1) : ""),
            ImageURL: "/_layouts/15/userphoto.aspx?size=s&username=" + e.split("|")[2],
            IdTurno: e.split("|").length >5 ? e.split("|")[5] : "",

          };
          return OW;
        })
        : [],  
        Members: item.Members != null && item.Members != undefined && item.Members != "" 
        ? (item.Members as string).split("@@@@").map(e => {
          var OW: IPerson =  {
            DisplayName : e.split("|")[0].length > 0 ?  e.split("|")[1] + ' ' + e.split("|")[0].substring(0, 1) + '.': e.split("|")[1],
            FullName: e.split("|")[0].length > 0 ? e.split("|")[1] + ' ' + e.split("|")[0] : e.split("|")[1],
            GivenName: e.split("|")[0],
            SurName: e.split("|")[1],
            Email: "",
            UserUPN: e.split("|")[2],
            UserId: "",
            RuoloCod: "I",
            RuoloDes: "Iscritto",
            Presence: "none",
            Initial: (e.split("|")[0].length > 0 ? e.split("|")[0].substring(0, 1) : "") + (e.split("|")[1].length > 0 ? e.split("|")[1].substring(0, 1) : ""),
            ImageURL: "/_layouts/15/userphoto.aspx?size=s&username=" + e.split("|")[2],
            IdTurno: e.split("|").length >3 ? e.split("|")[3] : "",
          };
          return OW;
        })
        : [],  
        IsMember: false,
        IsMembershipLimitedToOwners: item.IsMembershipLimitedToOwners,
        DipCod:  item.DipCod,
        DipDes:  item.DipDes,
        CommissioneUsers:  item.CommissioneUsers,
        Sessioni:  item.Sessioni,
        Turni:  item.Turni,
        EsameComune:  item.EsameComune,
        tipoGestAppCod:  item.tipoGestAppCod,
        tipoGestAppDes:  item.tipoGestAppDes,
        tipoGestPrenCod:  item.tipoGestPrenCod,
        tipoGestPrenDes:  item.tipoGestPrenDes,
        tipoDefAppCod:  item.tipoDefAppCod,
        tipoDefAppDes:  item.tipoDefAppDes,       
        ContainUser: (item.Owners != null && item.Owners != undefined && item.Owners != "" && item.Owners.toString().toUpperCase().includes("|" + usr.toUpperCase())),
      });
    });

    //filter per docente
    const _isAdmin: Boolean = (this.state.JoinedGroups.indexOf(this.props.adminGruopId) > -1);
    if (!_isAdmin)
    {
        lt  = lt.filter(a => a.ContainUser == true);
    }

    //sorting
    lt.sort((a, b) => {
      var aSort = a.groupname + a.dataInizioApp;
      var bSort = b.groupname + b.dataInizioApp;

      if(aSort > bSort) {
        return 1;
      } else if(aSort < bSort) {
        return -1;
      } else {
        return 0;
      }
    });


    this.LogMessage('Teams: ' + lt.length.toString());

 
    // calcolo i tags
    var _Esame: string[] = lt.map(item => "Materia: " + item.adCod + " " + item.adDes)
      .filter((value, index, self) => self.indexOf(value) === index);

    var _cds: string[] = lt.map(item => "Corso: " + item.cdsCod + " " + item.cdsDes)
    .filter((value, index, self) => self.indexOf(value) === index);

    var _desApp: string[] = lt.map(item => "Desc. Appello: " +item.desApp)
    .filter((value, index, self) => self.indexOf(value) === index);

    var _dataInizioApp: string[] = lt.map(item => "Data Appello: " + this.DateToItalian(item.dataInizioApp))
    .filter((value, index, self) => self.indexOf(value) === index);

    var _OwnersNamesAll: string[][] = lt.map(item => item.Owners.map(subItem => "Docente: " + subItem.FullName));
    var _OwnersNames: string[] = ([] as string[]).concat(..._OwnersNamesAll)
      .filter((value, index, self) => self.indexOf(value) === index);

    var uniqueArray: string[] = _Esame.concat(_cds, _desApp, _dataInizioApp, _OwnersNames);
    uniqueArray = uniqueArray.filter((item, pos) =>{
      return uniqueArray.indexOf(item) == pos;
    });

    uniqueArray.sort((a:string, b:string) => {
      if(a > b) {
        return 1;
      } else if(a < b) {
        return -1;
      } else {
        return 0;
      }
    });

    
    var st: ITag[] = [];
    _Esame.concat(_cds, _desApp, _dataInizioApp, _OwnersNames).map(item => {
      st.push({key: item, name: item });
    });

    

    this.setState({ListReqs: lt, SearchTags: st});
    this._renderList(lt, false);

  }
   



 private _renderList(lt:IListReq[], collapseGroup: boolean)
 {

   // creo un array con i gruppi divisi per Teams Da creare/In corso/chiusi e scaduti (contando e stabilendo il primo della lista )
   var counts = lt.reduce((p, c) => {
     var Name = c.groupname;
     if (!p.hasOwnProperty(Name)) {
       p[Name] = 0;
     }
     p[Name]++;
     return p;
   }, {});

   var lg: IListGroup[] = [];
   Object.keys(counts).map(k => {
     lg.push( {
       key: k, 
       name: k,
       count: counts[k], 
       startIndex: lt.map(e => { return e.groupname; }).indexOf(k), // determino lo startIndex cercando la prima occorenzaper gruppo
       level: 0, 
       isCollapsed: collapseGroup,
       nTeamsJoined: 0}); 
   });
  
   this.setState({ListReqsToShow: lt, ListGrouped:lg});

  }
  

  private _onRenderGroupHeader = (props?: IGroupHeaderProps): JSX.Element | null => {
    if (props) {
      return (
        <div className={styles.groupHeader}>
          <Stack horizontal verticalAlign="baseline"  horizontalAlign="space-between">
          <div className={ styles.groupHeaderTitle }>{`${props.group!.name}`}</div>
          </Stack>
            <Link className={styles.groupHeaderLink} onClick={this._onToggleCollapse(props)}>
              {props.group!.isCollapsed ? 'Visualizza Gli Esami (' + props.group!.count.toString() + ')'  : 'Nascondi gli Esami'}
            </Link>
        </div>
      );
    }
  }

  private _onRenderCell = (nestingDepth?: number, item?: IListReq, itemIndex?: number): React.ReactNode => {
    const addFriendIcon: IIconProps = { iconName: 'AddGroup' };
    const teamsIcon: IIconProps = {iconName: 'TeamsLogo'};
    const TeamURL = "https://teams.microsoft.com/l/" + item.InternalId;
    const OwnerList = 
    <Stack padding="5px" verticalAlign="center" disableShrink tokens={numericalSpacingStackTokens}>
      {
        item.Owners != undefined && item.Owners != null 
        ? item.Owners.map((owner) => { 
            var docentePersona: IPersonaSharedProps = {
              imageUrl: owner.ImageURL,
              imageInitials: owner.Initial,
              text: owner.FullName,
              imageAlt: owner.FullName + " (" + owner.RuoloDes +  ")",
              secondaryText: owner.RuoloDes,
            };
  
          return <Stack.Item><Persona {...docentePersona} presence={PersonaPresence.none} size={PersonaSize.size40} hidePersonaDetails={false} /></Stack.Item>;  
          } )
        : <Stack.Item><label className="ms-font-m ms-fontColor-black">Nessuno</label></Stack.Item>
      } 
    </Stack>;

    const TurniList = 
    <div>
      {
        
        item.Turni != undefined && item.Turni != null 
        ? item.Turni.split("@@@@").map((turno : string) => { 
          const TurnoId = turno.split("|")[0];
          const TurnoOra = turno.split("|")[3];
          const TurnoDes = turno.split("|")[4];
          const TurnoIscritti = item.Members != undefined && item.Members != null 
                                ? item.Members.filter(m=> m.IdTurno == TurnoId).length : 0;

          return <div className="ms-Grid-row">
            <div className="ms-Grid-col ms-sm12 ms-xl5">Orario: <strong>{TurnoOra}</strong></div>
            <div className="ms-Grid-col ms-sm8 ms-xl5">Nota: <strong>{TurnoDes}</strong></div>
            <div className="ms-Grid-col ms-sm4 ms-xl2">N. Iscritti: <strong>{TurnoIscritti}</strong></div>
          </div>;
          
          } )
        : <label className="ms-font-m ms-fontColor-black">n/a</label>
      } 
    </div>

    const SessioniList = 
    <div>
      {
        
        item.Sessioni != undefined && item.Sessioni != null 
        ? item.Sessioni.split("@@@@").map((sessione : string) => { 
          const SessAnno = sessione.split("|")[0];
          const SessPer1 = sessione.split("|")[1].substring(0,10);
          const SessPer2 = sessione.split("|")[2].substring(0,10);
          const SessDes = sessione.split("|")[3];

          return <div className="ms-Grid-row">
            <div className="ms-Grid-col ms-sm4 ms-xl2">Anno: <strong>{SessAnno}</strong></div>
            <div className="ms-Grid-col ms-sm8 ms-xl4">Periodo: <strong>{SessPer1} - {SessPer2}</strong></div>
            <div className="ms-Grid-col ms-sm12 ms-xl6">Descrizione: <strong>{SessDes}</strong></div>
          </div>;
          
          } )
        : <label className="ms-font-m ms-fontColor-black">n/a</label>
      } 
    </div>

    const button = 
          item.TeamCreated ? 
            <div>  
              <TextField title="Join URL Del Team" readOnly defaultValue={item.TeamJoinUrl} />
            </div>
            : !item.TeamRequested && item.dataInizioApp  >= today ?
              <div>
                <DefaultButton className={styles.button} iconProps={addFriendIcon} 
                onClick={(event) => {
                  event.preventDefault();
                  this._onAddGroup(item.Id);
                  }} >
                  Richiedi Creazione del Team
                </DefaultButton>
              </div>
              :
              <div></div>      
    ;
    const statoRichiesta = 
          item.TeamRequested && !item.TeamCreated ? "Team richiesto" 
          :  item.TeamRequested && item.TeamCreated ?  "Team Creato" 
            : !item.TeamRequested && item.dataInizioApp  >= today ? "Da richiedere" : "Appello Chiuso";

    return (
      <div className={styles.groupDetail}>
        <div className={styles.DetailRow}>
          <div className={styles.DetailAppello}>Appello del:<br/> <strong>{this.DateToItalian(item.dataInizioApp)}</strong></div>
          <div className={styles.DetailDesc}>
            <div className={styles.DetailDescLine}>Corso: <strong>{item.cdsCod} {item.cdsDes}</strong></div>
            <div className={styles.DetailDescLine}>Corte: <strong>{item.Ins_aa_corte}</strong> - Anno: <strong>{item.Ins_anno_corso}</strong> - <strong>{item.Ins_partizionamento}</strong></div> 
          </div>
        </div>
        <div className={styles.DetailRow}>
          <div className={styles.DetailAppello}>
            <div className={styles.DetailAppelloLine}>Iscrizioni<br/><strong>{this.DateToItalian(item.dataInizioIscr)} <br/>{this.DateToItalian(item.dataFineIscr)}</strong></div>
            <div className={styles.DetailAppelloLine}></div>
            <div className={styles.DetailAppelloLine}>N. Iscritti: <strong>{item.App_NumIscritti}</strong></div>
          </div>
          <div className={styles.DetailDesc}>
            <div className={styles.DetailDescLine}>Materia: <strong>{item.adDes} ({item.adCod})</strong></div>
            <div className={styles.DetailDescLine}>Codice esse3: <strong>{item.appId}</strong> - Descr.: <strong>{item.desApp}</strong></div>
            <div className={styles.DetailDescLine}>Tipo Gestione: <strong>{item.tipoGestAppCod}</strong> - Stato esse3: <strong>{item.App_StatoDes}</strong></div>
            <div className={styles.DetailDescLine}>Note esse3: {item.App_Note}</div>
          </div>
        </div>
        <div className={styles.DetailRow}>
          <div className={styles.DetailAppello}><strong>Turni:</strong></div>
          <div className={styles.DetailDesc}>{TurniList}</div>
        </div>
        <div className={styles.DetailRow}>
          <div className={styles.DetailAppello}><strong>Sessioni:</strong></div>
          <div className={styles.DetailDesc}>{SessioniList}</div>
        </div>
        <div className={styles.DetailRow}>
          <div className={styles.DetailAppello}><br/><strong>Commissione:</strong></div>
          <div className={styles.DetailDesc}>{OwnerList}</div>
        </div>
        <div className={styles.DetailRow}>
          <div className={styles.DetailAppello}>
            <div className={styles.DetailAppelloLine}><strong>Microsoft Team</strong></div>
          </div>
          <div className={styles.DetailDesc}>
            <div className={styles.DetailDescLine}>Nome del team: <strong>{item.Title}</strong></div>
          </div>
        </div>
        <div className={styles.DetailRow}>
          <div className={styles.DetailAppello}>
            <div className={styles.DetailAppelloLine}>Stato: <strong>{statoRichiesta}</strong></div>
          </div>
          <div className={styles.DetailDesc}>
            {button}
          </div>
        </div>
      </div>
      );
    }


  private _onAddGroup(_Id: string) {
    //event.preventDefault();
    if (confirm("Confermi la richiesta per la creazione del Team? La rihciesta verrà evasa entro 2 ore."))
    {
      const _lt:IListReq = this.state.ListReqs.filter(a=> a.Id = _Id)[0];
      //_lt.__metadata = {type : "SP.Data." + this.props.listName + "ListItem"};
      //if (SharepointAPIConnector.updateRequestTeam(this.props.context, this.props.listSite,this.props.listName, _Id, _lt))

      const DATE_TIME_FORMAT = 'YYYY-MM-DD HH:mm:SS'; 
      const _now:string = moment(new Date(), DATE_TIME_FORMAT).toString();
      GraphApiConnector.updateRequestTeam(this.props.context, this.props.listSite, this.props.listName, _now, _Id, _lt)

      .then((esito: string) => {
        if (esito == "ok")
        {

          this.setState({isUpdated:true, esitoUpdate: "Positivo"});
          
          // aggiorno lista a video
          var lupd: IListReq[] = this.state.ListReqsToShow; 
          var updateItem:IListReq = lupd.filter(i=> i.Id == _Id)[0];
          var index:number = lupd.indexOf(updateItem);
          lupd[index].TeamRequested = true; 
          this._renderList(lupd,false);
        }
        else
        {
          this.setState({isUpdated:true, esitoUpdate: "Negativo", erroreUpdate: esito});
        }
      });
    }
}
  
  private _onToggleCollapse(props: IGroupDividerProps): () => void {
    return () => {
      props.onToggleCollapse!(props.group);
    };
  }

  private getTextFromItem = (item: ITag): string => {
    return item.name;
  }
  
  private listContainsDocument = (tag: ITag, tagList: ITag[]) => {
    if (!tagList || !tagList.length || tagList.length === 0) {
      return false;
    }
    return tagList.filter(compareTag => compareTag.key === tag.key).length > 0;
  }

  private onFilterChanged = (filterText: string, tagList: ITag[]) =>  {
    var SearchItems: ITag[] = 
      (filterText != "" && filterText.length > 2 && this.state.SearchTags != undefined)  
      ? this.state.SearchTags.filter(a => a.name.toUpperCase().indexOf(filterText.toUpperCase()) >= 0).filter(b => !this.listContainsDocument(b, tagList))
      : [];
      return SearchItems;
  }

  private onPickerChange = (tags: ITag[]) => {
    this.setState({PickerList: tags});
    this.onSearch(tags);
    
  } 
  private onSearch = (tags: ITag[]) => {

    var new_list: IListReq[] = [];
    if (tags.length >0)
    {
      tags.forEach(element => {      
        var _ByDes: IListReq[] = this.state.ListReqs.filter(item => 
            ("Materia: " + item.adCod + " " + item.adDes).toUpperCase() == element.name.toUpperCase() 
            || ("Corso: " + item.cdsCod + " " + item.cdsDes).toUpperCase() == element.name.toUpperCase()
            || ("Desc. Appello: " + item.desApp).toUpperCase() == element.name.toUpperCase()
            || ("Data Appello: " + this.DateToItalian(item.dataInizioApp)).toUpperCase() == element.name.toUpperCase()
        ); 
        var _ByOnwer: IListReq[] = this.state.ListReqs.filter(item => item.Owners.some(own => ("docente: " + own.FullName).toUpperCase() == element.name.toUpperCase()));        
        new_list = new_list.concat(_ByDes, _ByOnwer); 

      });
      this.setState({isSearched:true, searchResults:new_list.length});
      this._renderList(new_list, false);
    }
    else
    {
      this.setState({isSearched:false, searchResults:0});
      this._renderList(this.state.ListReqs, false);
    }

  }


  private _LogShowCollapse= (event: any) =>{
    const new_s:boolean = !(this.state.showLog);
    this.setState({showLog: new_s}); 
  }
 

  private OnShowAllCollapsed = (event: any) => {
    this.setState({PickerList: []});
    this.onSearch([]);
  }

  private onDismissAlert = (event?: any) => {
    this.setState({isSearched:false, isUpdated:false});
  }

  
  public componentDidMount() {
    this.LogMessage("componentDidMount");
    this._renderListAsync();
 } 

  public render(): React.ReactElement<IExamsManagementProps> {
    return (
      <div className={ styles.ExamsManagement }>
        <div className={ styles.container }>
          <div className={ styles.row }>
            <div className={ styles.column }>
              <div className="ms-Grid-row">
                <div className="ms-Grid-col ms-xl4 ms-sm12">
                <Stack padding="5px" verticalAlign="center" horizontal disableShrink tokens={numericalSpacingStackTokens}>
                  <FontIcon iconName="TeamsLogo"  className={ styles.subTitle } />
                  <label className={ styles.subTitle }  > Elenco appelli di esame</label>
                </Stack>
                </div>
                <div className="ms-Grid-col ms-xl8 ms-sm12 ms-textAlignRight  ms-fontColor-black">
                  <TooltipHost
                    content="Ricerca per materia, corso, descrizione appello, data appello o docente"
                    id="id_toolTipRicerca"
                    calloutProps={{ gapSpace: 0 }}
                    styles={{ root: { display: 'inline-block', width:"100%" } }}
                  >     
                    <TagPicker 
                      aria-describedby="id_toolTipRicerca ms-fontColor-black" 
                      className="ms-fontSize-m"
                      styles={{itemsWrapper: {color:"black"}, root:{width:"100%"}}}
                      onResolveSuggestions={this.onFilterChanged}
                      getTextFromItem={this.getTextFromItem}
                      pickerSuggestionsProps={pickerSuggestionsProps}
                      inputProps={inputProps}
                      selectedItems={this.state.PickerList}
                      itemLimit={5}
                      resolveDelay={500}
                      onChange={this.onPickerChange}
                      
                    />
                  </TooltipHost>
                </div>
              </div>
              <div hidden={!this.state.Loading} className="ms-Grid-row">
                <div className="ms-Grid-col ms-sm12">
                  <ProgressIndicator label="Caricamento Esami..." />
                </div>              
              </div>              
              <div className="ms-Grid-row">
                <div className="ms-Grid-col ms-sm12">
                  <div className={styles.group}>
                    <div hidden={!this.state.isSearched}>
                        <MessageBar
                        messageBarType={MessageBarType.warning}
                        isMultiline={false}
                        dismissButtonAriaLabel="Close"
                        onDismiss={this.onDismissAlert}>
                        La ricerca ha visualizzato {this.state.searchResults} esami. <Link  onClick={this.OnShowAllCollapsed} >Reset Ricerca</Link>
                      </MessageBar>
                    </div>
                    <div hidden={!this.state.isUpdated}>
                        <MessageBar
                        messageBarType={this.state.erroreUpdate != "" ? MessageBarType.error : MessageBarType.success}
                        isMultiline={false}
                        dismissButtonAriaLabel="Close"
                        onDismiss={this.onDismissAlert}>
                        L'aggiornamento ha avuto esito {this.state.esitoUpdate} <Link  onClick={this.OnShowAllCollapsed} >Aggiorna la lista</Link>
                        <div>{this.state.erroreUpdate != "" ? "Errore: " + this.state.erroreUpdate : "La richiesta è stata presa in carico dal sistema, verrà evasa entro 2 ore."}</div>
                      </MessageBar>
                    </div>
                    <GroupedList 
                      items={this.state.ListReqsToShow} 
                      onRenderCell={this._onRenderCell} 
                      groupProps={{
                        onRenderHeader: this._onRenderGroupHeader,
                        showEmptyGroups: true,
                      }}
                      groups={this.state.ListGrouped} 
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div className="ms-Grid-row">
          <div className="ms-Grid-col ms-sm1">
            <Link onClick={this._LogShowCollapse}>./.</Link>
          </div>              
          <div hidden={!this.state.showLog} className="ms-Grid-col ms-sm11">
          <TextField label="Logs applicativi"  multiline rows={10} defaultValue={this.state.InfoLogs} />
            
          </div>              
        </div>           
        </div>
    );

  }
 
  
}
