import * as React from 'react';
import * as ReactDom from 'react-dom';
import { Version } from '@microsoft/sp-core-library';
import {
  IPropertyPaneConfiguration,
  PropertyPaneTextField,
  PropertyPaneChoiceGroup
} from '@microsoft/sp-property-pane';

import { BaseClientSideWebPart } from '@microsoft/sp-webpart-base';

import * as strings from 'Office365DidatticaTeamsWebPartStrings';
import MockHttpClient from './model/MockHttpClient';
import Office365DidatticaTeams from './components/Office365DidatticaTeams';
import { IOffice365DidatticaTeamsProps } from './components/IOffice365DidatticaTeamsProps';

import { MSGraphClient } from '@microsoft/sp-http';
import * as MicrosoftGraph from '@microsoft/microsoft-graph-types';
import { ClientMode } from './model/ClientMode';
import { IListTeam } from './model/ITeamsClasses';

export interface IOffice365DidatticaTeamsWebPartProps {
  listFilter: string;
  listSite: string;
  listName: string;
  description: string;
  clientMode: ClientMode; 
}

export default class Office365DidatticaTeamsWebPart extends BaseClientSideWebPart<IOffice365DidatticaTeamsWebPartProps> {

  public render(): void {

    const element: React.ReactElement<IOffice365DidatticaTeamsProps> = React.createElement(
      Office365DidatticaTeams,
      {
        description: this.properties.description,
        clientMode: this.properties.clientMode,
        context: this.context,
        listSite: this.properties.listSite,
        listName: this.properties.listName,
        listFilter: this.properties.listFilter
      }
    );

    ReactDom.render(element, this.domElement);
  }

  protected onDispose(): void {
    ReactDom.unmountComponentAtNode(this.domElement);
  }


  protected getPropertyPaneConfiguration(): IPropertyPaneConfiguration {
    return {
      pages: [
        {
          header: {
            description: strings.PropertyPaneDescription
          },
          groups: [
            {
              groupName: strings.BasicGroupName,
              groupFields: [
                PropertyPaneTextField('description', {
                  label: strings.DescriptionFieldLabel
                }),
                PropertyPaneTextField('listSite', {
                  label: strings.ListSite
                }),
                PropertyPaneTextField('listName', {
                  label: strings.ListName
                }),
                PropertyPaneTextField('listFilter', {
                  label: strings.ListFilter
                }),
                PropertyPaneChoiceGroup('clientMode', {
                  label: strings.ClientModeLabel,
                  options: [
                    { key: ClientMode.aad, text: "AadHttpClient"},
                    { key: ClientMode.graph, text: "MSGraphClient"},
                  ]
                })              ]
            }
          ]
        }
      ]
    };
  }
  
}
