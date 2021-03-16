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
  IButtonStyles, Link, IIconProps, IDetailsListCheckboxProps, Checkbox, IDetailsGroupRenderProps, IGroupDividerProps, getTheme, mergeStyleSets, CheckboxVisibility, TextField, Label, Spinner, ProgressIndicator, Async, DetailsListLayoutMode, DetailsRow, IDetailsRowBaseProps, Stack, MessageBar, MessageBarType, IStackTokens, SearchBox, Tooltip, ITooltipHostStyles, TooltipHost, TagPicker, IBasePickerSuggestionsProps, ITag, IInputProps, Announced, PrimaryButton, ISuggestionItemProps, BaseButton, Button, MessageBarButton, IGroupHeaderProps, GroupedList
} from 'office-ui-fabric-react';
import { IPersonaSharedProps, Persona, PersonaSize, PersonaPresence } from 'office-ui-fabric-react/lib/Persona';
import { FontIcon } from 'office-ui-fabric-react/lib/Icon';

import styles from './Office365DidatticaTeams.module.scss';
import { IOffice365DidatticaTeamsProps } from './IOffice365DidatticaTeamsProps';
import { IOffice365DidatticaTeamsState } from './IOffice365DidatticaTeamsState';
import { escape } from '@microsoft/sp-lodash-subset';
import * as strings from "Office365DidatticaTeamsWebPartStrings";

import {
  Environment,
  EnvironmentType
} from '@microsoft/sp-core-library';
import MockHttpClient from "../model/MockHttpClient";
import { IListTeam, IListGroup, IOwner, IListTeamToShow } from "../model/ITeamsClasses";
import SharepointAPIConnector from '../model/SharepointAPIConnector';
import GraphApiConnector from '../model/GraphApiConnector';


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

export default class Office365DidatticaTeams extends React.Component<IOffice365DidatticaTeamsProps, IOffice365DidatticaTeamsState, {}> {
  

  constructor(props: IOffice365DidatticaTeamsProps, state: IOffice365DidatticaTeamsState) {
    super(props);

    // Initialize the state of the component
    this.state = {
      JoinedTeams: [], 
      ListTeams: [], 
      ListTeamsGrouped: [], 
      Loading: true, 
      ListTeamsCount: 0, 
      ListTeamsToShow: [], 
      SearchTags:[], 
      isSearched:false, 
      searchResults:0, 
      PickerList:[],
    };

  }


 
  private _renderListAsync(): void {
    this.setState ({JoinedTeams: [], ListTeams: [], ListTeamsGrouped: [], Loading : true});
  
    // Local environment
    if (Environment.type === EnvironmentType.Local) {
      MockHttpClient.getTeams()
      .then((teams: IListTeam[]) => {
        MockHttpClient.getJoinedTeams()
        .then((joined: string[]) => {
          this._CacheInitialData(teams, joined, "{1}AAAA|{2}BBBB");
          this.setState ({Loading : false});
        });
      });
    }
    else if (Environment.type == EnvironmentType.SharePoint || Environment.type == EnvironmentType.ClassicSharePoint) {
      SharepointAPIConnector.getListItemsCount(this.props.context, this.props.listSite,this.props.listName, this.props.listFilter).then((count) => {
        this.setState ({ListTeamsCount: count});
        SharepointAPIConnector.getListItemsDetails(this.props.context, this.props.listSite, this.props.listName, this.props.listFilter).then((teams) => {
          GraphApiConnector.getJoinedTeamsFromGraphApi(this.props.context).then((JT:string[])=>{
              GraphApiConnector.getMyCoursesFromGraphApi(this.props.context).then((JC:string)=>{
                this._CacheInitialData(teams.value, JT, JC);
                this.setState ({Loading : false});
            });
          });
        });
      });
    }
  } 
  private _CacheInitialData(teams: any, JT: string[], ext11: string): void {

    if (JT)
    {      
      this.setState({JoinedTeams: JT});
    }
    console.log("Joined: " + JT.length.toString());

    console.log("Courses: " + ext11);

    teams.sort((a, b) => {
      var aSort = a.Description + ' ' + a.CourseYear + ' ' + a.Name;
      var bSort = b.Description + ' ' + b.CourseYear + ' ' + b.Name;

      if(aSort > bSort) {
        return 1;
      } else if(aSort < bSort) {
        return -1;
      } else {
        return 0;
      }
    });
    var lt: IListTeam[] = [];
    teams.map((item: any) => {
      lt.push( {
        TeamsId: item.TeamsId ,
        Name: item.Name ,
        Description: item.Description ,
        JoinUrl: item.JoinUrl ,
        JoinCode: item.JoinCode ,
        IsMembershipLimitedToOwners: item.IsMembershipLimitedToOwners ,
        TeamType: item.TeamType,
        CDSCod: item.CDSCod,
        DepartmentId: item.Department,
        OfferYear: item.OfferYear,
        CourseYear: item.CourseYear,
        CourseMandatory: item.CourseMandatory,
        CodCourse: item.CodCourse,
        InternalId: item.InternalId,
        CourtYear: item.CourtYear ,
        Session: item.Session,
        CourseNote: item.CourseNote,
        Owners: item.Owners != null && item.Owners != undefined && item.Owners != "" 
        ? (item.Owners as string).split("@@@@").map(e => {
          var OW: IOwner =  {
            DisplayName : e.split("|")[0].length > 0 ?  e.split("|")[1] + ' ' + e.split("|")[0].substring(0, 1) + '.': e.split("|")[1],
            FullName: e.split("|")[0].length > 0 ? e.split("|")[1] + ' ' + e.split("|")[0] : e.split("|")[1],
            GivenName: e.split("|")[0],
            SurName: e.split("|")[1],
            Email: "",
            UserUPN: e.split("|")[2],
            UserId: e.split("|")[3],
            Presence: "none",
            Initial: (e.split("|")[0].length > 0 ? e.split("|")[0].substring(0, 1) : "") + (e.split("|")[1].length > 0 ? e.split("|")[1].substring(0, 1) : ""),
            ImageURL: "/_layouts/15/userphoto.aspx?size=s&username=" + e.split("|")[2],
          };
          return OW;
        })
        : [],  
        IsMember: this.state.JoinedTeams.indexOf(item.TeamsId) > -1 , // Verifico se è già membro del team
      });
    });
    console.log('Teams: ' + lt.length.toString());
 
    // calcolo i tags
    var _TeamDescription: string[] = lt.map(item => "Dipartimento: " + item.Description)
      .filter((value, index, self) => self.indexOf(value) === index);

    var _TeamNames: string[] = lt.map(item => "Corso: " + item.Name)
      .filter((value, index, self) => self.indexOf(value) === index);

    var _OwnersNamesAll: string[][] = lt.map(item => item.Owners.map(subItem => "Docente: " + subItem.FullName));
    var _OwnersNames: string[] = ([] as string[]).concat(..._OwnersNamesAll)
      .filter((value, index, self) => self.indexOf(value) === index);


    var uniqueArray: string[] = _TeamDescription.concat(_TeamNames,_OwnersNames);
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
    _TeamDescription.concat(_TeamNames,_OwnersNames).map(item => {
      st.push({key: item, name: item });
    });

    
    this.setState({ListTeams: lt, SearchTags: st});
    this._renderList(lt, true);

  }
 

 private _renderList(teams:IListTeam[], collapseGroup: boolean)
 {

  var lt: IListTeamToShow[] = [];
  teams.map((item: IListTeam) => {
    lt.push({
      TeamsId: item.TeamsId,
      Name: item.Name,
      Description: item.Description,
      JoinUrl: item.JoinUrl,
      CourseYear: item.CourseYear,
      CourseMandatory: item.CourseMandatory,
      CodCourse: item.CodCourse,
      InternalId: item.InternalId,
      CourseNote: item.CourseNote,
      Owners: item.Owners,  
      IsMember: item.IsMember
    });
  });
  console.log('Teams Rendered: ' + lt.length.toString());
 
   // creo un array con i gruppi divisi per decription (contando e stabilendo il primo della lista )
   var counts = teams.reduce((p, c) => {
     var Name = c.Description;
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
       name: teams[teams.map(e => { return e.Description; }).indexOf(k)].CodCourse + '@' + k, 
       count: counts[k], 
       startIndex: teams.map(e => { return e.Description; }).indexOf(k), // determino lo startIndex cercando la prima occorenza della description
       level: 0, 
       isCollapsed: collapseGroup,
       nTeamsJoined: 0}); 
   });
   console.log('Teams Grouped: ' + lg.length.toString());

   this.setState({ListTeamsToShow: lt, ListTeamsGrouped:lg});
 }

  

  private _onRenderGroupHeader = (props?: IGroupHeaderProps): JSX.Element | null => {
    if (props) {
      return (
        <div className={styles.groupHeader}>
          <Stack horizontal verticalAlign="baseline"  horizontalAlign="space-between">
          <div className={ styles.groupHeaderTitle }>{`${props.group!.name.split("@")[1]}`}</div>
          <div className={ styles.groupHeaderCourse }><label title="Codice Corso di laurea" aria-describedby="Codice Corso di laurea" >{props.group.name.split("@")[0]}</label></div>
          </Stack>
            <Link className={styles.groupHeaderLink} onClick={this._onToggleCollapse(props)}>
              {props.group!.isCollapsed ? 'Visualizza Gli Insegnamenti (' + props.group!.count.toString() + ')'  : 'Nascondi Insegnamenti'}
            </Link>
        </div>
      );
    }
  }
  
  private _onRenderCell = (nestingDepth?: number, item?: IListTeamToShow, itemIndex?: number): React.ReactNode => {
    const addFriendIcon: IIconProps = { iconName: 'AddFriend' };
    const teamsIcon: IIconProps = {iconName: 'TeamsLogo'};
    const TeamURL = "https://teams.microsoft.com/l/" + item.InternalId;
    const OwnerList = 
    <Stack padding="5px" verticalAlign="center" horizontal disableShrink tokens={numericalSpacingStackTokens}>
      <Stack.Item><label className="ms-font-m">Docenti:</label></Stack.Item>   
      {
        item.Owners != undefined && item.Owners != null 
        ? item.Owners.map((owner) => { 
            var docentePersona: IPersonaSharedProps = {
              imageUrl: owner.ImageURL,
              imageInitials: owner.Initial,
              text: owner.DisplayName,
              imageAlt: owner.FullName,
              secondaryText: owner.Email,
            };
  
          return <Stack.Item><Persona {...docentePersona} presence={PersonaPresence.none} size={PersonaSize.size16} hidePersonaDetails={false} /></Stack.Item>;  
          } )
        : <Stack.Item><label className="ms-font-m ms-fontColor-black">Nessuno</label></Stack.Item>
      } 
    </Stack>;
    const button = item.IsMember 
    ? <div> 
        <Label className={styles.iscrittolabel}>Utente iscritto al Team</Label>
        <DefaultButton className={styles.button} iconProps={teamsIcon} href={TeamURL} target="_blank">Apri in Teams</DefaultButton>
      </div>
    : <div>
        <Label className={styles.noniscrittolabel}>Non iscritto al Team</Label>
        <DefaultButton className={styles.button} iconProps={addFriendIcon} href={item.JoinUrl} target="_blank">Richiedi Accesso</DefaultButton>
      </div>;
    return (
      <div className={styles.groupDetail}>
        <div className={styles.DetailYear}>{item.CourseYear}° Anno</div>
        <div className={styles.DetailName}>
          <div className={styles.DetailTeamName}>{item.Name}</div>
          <div className={styles.DetailTeamNote}>{item.CourseNote}</div>
        </div>
        <div className={styles.DetailButton}>{button}</div>
        <div className={styles.DetailOwners}>{OwnerList}</div>
      </div>
      );

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

    var new_ListTeams: IListTeam[] = [];
    if (tags.length >0)
    {
      tags.forEach(element => {
        var _ByDescName: IListTeam[] = this.state.ListTeams.filter(item => 
          ("Dipartimento: " + item.Description).toUpperCase() == element.name.toUpperCase() 
          || ("Corso: " + item.Name).toUpperCase() == element.name.toUpperCase()); 
        var _ByOnwer: IListTeam[] = this.state.ListTeams.filter(item => item.Owners.some(own => ("Docente: " + own.FullName).toUpperCase() == element.name.toUpperCase()));
        
        new_ListTeams = new_ListTeams.concat(_ByDescName, _ByOnwer); 

      });
      this.setState({isSearched:true, searchResults:new_ListTeams.length});
      this._renderList(new_ListTeams, false);
    }
    else
    {
      this.setState({isSearched:false, searchResults:0});
      this._renderList(this.state.ListTeams, true);
    }

  }

  private OnShowAllCollapsed = (event: any) => {
    this.setState({PickerList: []});
    this.onSearch([]);
  }

  private onDismissAlert = (event?: any) => {
    this.setState({isSearched:false});
  }
  
  public componentDidMount() {
    this._renderListAsync();
 } 

  public render(): React.ReactElement<IOffice365DidatticaTeamsProps> {
//                <Toggle className="ms-fontSize-l ms-fontColor-white"  onChange={this._onToggleCollapseAll} />
    return (
      <div className={ styles.office365DidatticaTeams }>
        <div className={ styles.container }>
          <div className={ styles.row }>
            <div className={ styles.column }>
              <div className="ms-Grid-row">
                <div className="ms-Grid-col ms-xl4 ms-sm12">
                <Stack padding="5px" verticalAlign="center" horizontal disableShrink tokens={numericalSpacingStackTokens}>
                  <FontIcon iconName="TeamsLogo" className={ styles.subTitle } />
                  <label className={ styles.subTitle }> Elenco insegnamenti</label>
                </Stack>
                </div>
                <div className="ms-Grid-col ms-xl8 ms-sm12 ms-textAlignRight  ms-fontColor-black">
                  <TooltipHost
                    content="Ricerca per nome corso, insegnamento o docente"
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
                  <ProgressIndicator label="Caricamento insegnamenti..." />
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
                        La ricerca ha visualizzato {this.state.searchResults} insegnamenti. <Link  onClick={this.OnShowAllCollapsed} >Reset Ricerca</Link>
      
                      </MessageBar>
                    </div>
                    <GroupedList 
                      items={this.state.ListTeamsToShow} 
                      onRenderCell={this._onRenderCell} 
                      groupProps={{
                        onRenderHeader: this._onRenderGroupHeader,
                        showEmptyGroups: true,
                      }}
                      groups={this.state.ListTeamsGrouped} 
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );

  }
  
}
