import * as React from 'react';
import * as ReactDom from 'react-dom';
import { Version } from '@microsoft/sp-core-library';
import {
  IPropertyPaneConfiguration,
  PropertyPaneTextField,
  PropertyPaneChoiceGroup
} from '@microsoft/sp-property-pane';
import { BaseClientSideWebPart } from '@microsoft/sp-webpart-base';

import * as strings from 'ExamsManagementWebPartStrings';
import ExamsManagement from './components/ExamsManagement';
import { IExamsManagementProps } from './components/IExamsManagementProps';

import { MSGraphClient } from '@microsoft/sp-http';
import * as MicrosoftGraph from '@microsoft/microsoft-graph-types';
import { ClientMode } from './model/ClientMode';
import $ from 'jquery';
export interface IExamsManagementWebPartProps {
  listFilter: string;
  listSite: string;
  listName: string;
  description: string;
  clientMode: ClientMode; 
  adminGruopId: string;
}

export default class ExamsManagementWebPart extends BaseClientSideWebPart<IExamsManagementWebPartProps> {

  public render(): void {
    const element: React.ReactElement<IExamsManagementProps> = React.createElement(
      ExamsManagement,
      {
        description: this.properties.description,
        clientMode: this.properties.clientMode,
        context: this.context,
        listSite: this.properties.listSite,
        listName: this.properties.listName,
        listFilter: this.properties.listFilter,
        adminGruopId: this.properties.adminGruopId
      }
    );
    $(".CanvasZone").css('max-width','100%');
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
                PropertyPaneTextField('adminGruopId', {
                  label: strings.adminGruopId
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
