<%-- _lcid="1033" _version="15.0.4420" _dal="1" --%>
<%-- _LocalBinding --%>

<%@ Page Language="C#" MasterPageFile="~masterurl/default.master" Inherits="Microsoft.SharePoint.WebPartPages.WebPartPage,Microsoft.SharePoint,Version=15.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c" %>

<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<asp:Content ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    <sharepoint:listitemproperty property="BaseName" maxlength="40" runat="server" />
</asp:Content>

<asp:Content ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <meta name="GENERATOR" content="Microsoft SharePoint" />
    <meta name="ProgId" content="SharePoint.WebPartPage.Document" />
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="CollaborationServer" content="SharePoint Team Web Site" />
    <sharepoint:scriptblock runat="server">
	var navBarHelpOverrideKey = "WSSEndUser";
	</sharepoint:scriptblock>
    <sharepoint:styleblock runat="server">
body #s4-leftpanel {
	display:none;
}
.s4-ca {
	margin-left:0px;
}
</sharepoint:styleblock>
    <sharepoint:scriptlink id="ScriptLink1" name="sp.ui.dialog.js" loadafterui="true" localizable="false" runat="server"></sharepoint:scriptlink>
    <script type="text/javascript" src="../SiteAssets/JS/jquery-1.11.3.min.js"></script>
    <script type="text/javascript" src="../SiteAssets/DL Manager/dlmanager.js?version=1"></script>
    <link rel="stylesheet" type="text/css" href="../SiteAssets/DL Manager/main.css?version=1" />
    <link rel="stylesheet" type="text/css" href="../SiteAssets/DL Manager/hide.css?version=1" />
    <script type="text/javascript" src="../SiteAssets/DL Manager/datatable/js/jquery.dataTables.min.js"></script>
    <link rel="stylesheet" href="../SiteAssets/DL Manager/datatable/css/jquery.dataTables.min.css" type="text/css" />
</asp:Content>
<asp:Content ContentPlaceHolderID="PlaceHolderSearchArea" runat="server">
    <sharepoint:delegatecontrol runat="server"
        controlid="SmallSearchInputBox" />
</asp:Content>
<asp:Content ContentPlaceHolderID="PlaceHolderPageDescription" runat="server">
    <sharepoint:projectproperty property="Description" runat="server" />
</asp:Content>
<asp:Content ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <div class="ms-hide">
        <webpartpages:webpartzone runat="server" title="loc:TitleBar" id="TitleBar" allowlayoutchange="false" allowpersonalization="false" style="display: none;" />
    </div>
    <div id="main-content">
        <div class="section">
            <div>
                <div class="mainMessage" >
                    <p class="" style="margin-bottom: 0px; line-height: 18px;">
                        This application will help you manage the Distribution Lists (DLs) owned by you.<br /> Please select the Distribution List (DL) from the drop-down list to add or delete users.
					<%--<ul>
                        <li>View the existing members in the DLs</li>
                        <li>Add new Members to the DLs</li>
                        <li>Remove Members from the DLs</li>
                    </ul>--%>
                    </p>

                </div>
                <div class="clear">
                    <p class="noteMessage" >
                        <strong>Note(s):&#160;</strong><ul>
						<li>This DL management solution caters to DL's having less than <b>5000</b> users only. </li>
						<li>If you want to manage DL’s with 5000 or more users or add/remove Nested DL's (DL within a DL), kindly contact <a href="http://itsc" target="_blank"> ITSC </a></li>
						<li>Updates might take up to <b><u>four hours</u></b> to get reflected in Outlook Address  Book. </li>
						</ul>
                    </p>
                </div>
                <div id="divThreshold" style="display:none" class="clear">
                    <p style="color:red;"> 
					The DL that is loading in the below screen has exceeded the Max. Threshold limit</p>
					<p style="color:red;"> Kindly raise an <a href="http://itsc" target="_blank"> ITSC </a> Fault Ticket to update the DL
                    </p>
                </div>
            </div>
        </div>


        <div id="content-container1">
            
            <div class="tableDiv" style="display:none">

                <table cellspacing="0" cellpadding="0" width="100%">

                    <tr>
                        <td>&nbsp;</td>

                    </tr>
                    <tr>
                        <td>
                            <div class="buttonsDiv">
                                <table cellspacing="0" cellpadding="0" width="100%">
                                    <tr>
                                        <td>
                                            <div class="floatLeft">Group:&nbsp;</div>
                                            <select onchange="ddlGroupChange()" id="ddlGroups"></select></td>
                                        <td>
                                            <input onclick="addNewUsers();" type="button" value="+ Add a User" /></td>
                                        <td>
                                            <input type="button" onclick="deleteSelectedUser();" value="- Delete a User" /></td>
                                        <%--onclick="deleteSelectedUser();" --%>
                                        <td>
                                            <input type="search" id="txtSearch" /><div class="floatRight">Search:&nbsp;</div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <div id="overlayButtonDiv" class="overlayButtons" style="display: none"></div>
                        </td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>

                    </tr>
                    <tr>
                      <td align="left" class="groupNameHeader"><span id="groupNameHeader"></span>&nbsp;<span id="groupName_UserCount"></span></td>

                    </tr>
                    <tr>
                        <td>&nbsp;</td>

                    </tr>
                    <tr>
                        <td style="border-top: 1px solid #000;">
                            <div id="dataTableDiv">
                            </div>
                            <div style="text-align: center;display:none" id="LodingMSG">
                                <br />
                                <img height="100" width="100" src="../SiteAssets/DL Manager/datatable/images/loading.gif" />
                            </div>
                        </td>

                    </tr>
                </table>

            </div>
            <%-- 
                <div id="center-content" class="left">
				<fieldset>
					<legend class="ms-rteElement-H1B"><strong>Distribution Lists</strong></legend>
					<div data-bind='simpleGrid: gridViewModel, visible: userGroups().length > 0' style="display: none;"> </div>
					<!--<ul class="round" data-bind="foreach: userGroups, visible: userGroups().length > 0" >
						<li>
							<p><a class="myButton2" href="#" data-bind="text: title, click: $parent.showUsers"></a></p>
						</li>
					</ul>-->
					<h3 data-bind="text: loadingMessage, visible: userGroups().length == 0" style="display: none;"></h3>
			</fieldset>
		</div>--%>
            <div class="clear"></div>
            <div style="display:none" class="clear noPermissionDiv">
                    <p class="noteMessage" >
                        <strong>You do not own any Distribution Lists.</strong>
                    </p>
                </div>

        </div>
        <div class="clear lastTextDiv">
            <script type="text/javascript">
              window.history.replaceState("statedata", "title", "http://infospace-stg.emirates.com/sites/admanager");
            </script>

        </div>
        <div id="footer">
            <div class="wrapper">
                <div class="left w80P">
                    <!--Footer links-->
                    <div class="copyRight">

                        <img  src="/sites/admanager/SiteAssets/theEmiratesGroup.png">
                        <p>Copyright © 2016 Emirates Group.&nbsp; All rights reserved</p>
                        <!--//Footer links-->
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
