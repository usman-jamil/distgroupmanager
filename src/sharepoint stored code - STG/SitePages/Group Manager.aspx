<%-- _lcid="1033" _version="15.0.4420" _dal="1" --%>
<%-- _LocalBinding --%>
<%@ Page language="C#" MasterPageFile="~masterurl/default.master"    Inherits="Microsoft.SharePoint.WebPartPages.WebPartPage,Microsoft.SharePoint,Version=15.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c"  %> <%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Import Namespace="Microsoft.SharePoint" %> <%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<asp:Content ContentPlaceHolderId="PlaceHolderPageTitle" runat="server">
	<SharePoint:ListItemProperty Property="BaseName" maxlength="40" runat="server"/>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderAdditionalPageHead" runat="server">
	<meta name="GENERATOR" content="Microsoft SharePoint" />
	<meta name="ProgId" content="SharePoint.WebPartPage.Document" />
	<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
	<meta name="CollaborationServer" content="SharePoint Team Web Site" />
	<SharePoint:ScriptBlock runat="server">
	var navBarHelpOverrideKey = "WSSEndUser";
	</SharePoint:ScriptBlock>
	<SharePoint:StyleBlock runat="server">
		body #s4-leftpanel {
			display:none;
		}
		#s4-ribbonrow {
			display:none;
		}
		.s4-ca {
			margin-left:0px;
		}
	</SharePoint:StyleBlock>
	<Sharepoint:ScriptLink ID="ScriptLink1" Name="sp.ui.dialog.js" LoadAfterUI="true" Localizable="false" runat="server"></Sharepoint:ScriptLink> 
	<script type="text/javascript" src="../SiteAssets/JS/jquery-1.11.3.min.js"></script>
	<script type="text/javascript" src="../SiteAssets/JS/knockout-latest.js"></script>
	<script type="text/javascript" src="../SiteAssets/JS/pp.js"></script>
	<script type="text/javascript" src="../SiteAssets/JS/gm.js"></script>
	<link rel="stylesheet" type="text/css" href="../SiteAssets/groups.css" />
	</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderSearchArea" runat="server">
	<SharePoint:DelegateControl runat="server"
		ControlId="SmallSearchInputBox" />
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderPageDescription" runat="server">
	<SharePoint:ProjectProperty Property="Description" runat="server"/>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderMain" runat="server">
	<div class="ms-hide">
		<WebPartPages:WebPartZone runat="server" title="loc:TitleBar" id="TitleBar" AllowLayoutChange="false" AllowPersonalization="false" Style="display:none;" />
	</div>
	<div id="main-content">
		<div class="clear"></div>
		<div id="content-container">
			<div id="center-content" class="left">  
				<div class="left">Please use <strong>Ctrl+F</strong> to search for a DL member</div>
				<div class="right"><button data-bind="click: CancelSave">Cancel</button></div>
				<div class="right"><button data-bind="click: SaveData">Save</button></div>
				<div class="clear"><br /></div>
				<div class="left" id="editor-user" data-bind="clientPeoplePicker: users"></div>
				<div class="clear"><br /></div>
				<div class="right"><button data-bind="click: CancelSave">Cancel</button></div>
				<div class="right"><button data-bind="click: SaveData">Save</button></div>
			</div>
		</div>
	</div>			
</asp:Content>
