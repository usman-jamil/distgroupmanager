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
.s4-ca {
	margin-left:0px;
}
</SharePoint:StyleBlock>
<Sharepoint:ScriptLink ID="ScriptLink1" Name="sp.ui.dialog.js" LoadAfterUI="true" Localizable="false" runat="server"></Sharepoint:ScriptLink> 
	<script type="text/javascript" src="../SiteAssets/JS/jquery-1.11.3.min.js"></script>
	<script type="text/javascript" src="../SiteAssets/JS/knockout-latest.js"></script>
	<script type="text/javascript" src="../SiteAssets/JS/knockout.simpleGrid.3.0.js"></script>
	<script type="text/javascript" src="../SiteAssets/JS/app.js"></script>
	<link rel="stylesheet" type="text/css" href="../SiteAssets/main.css" />
	<link rel="stylesheet" type="text/css" href="../SiteAssets/hide.css" />
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
		<div>
			<div class="section" style="font-family: &#39;segoe ui&#39;, &#39;lucida grande&#39;, verdana, arial, helvetica, sans-serif; line-height: 17.55px; font-size: 15px;">
				<p class="" style="margin-bottom: 0px; line-height: 18px;">
					This application will help in managing all the Distribution Lists (DLs) which you own – you will be able to perform the following actions:
					<ul>
						<li>View the existing members in the DLs</li>
						<li>Add new Members to the DLs</li>
						<li>Remove Members from the DLs</li>
					</ul>
				</p>			
			</div>
		</div>
		<div id="content-container">
			<div id="center-content" class="left">
				<fieldset>
					<legend class="ms-rteElement-H1B"><strong>Distribution Lists</strong></legend>
					<div data-bind='simpleGrid: gridViewModel, visible: userGroups().length > 0' style="display: none;"> </div>
					<!--<ul class="round" data-bind="foreach: userGroups, visible: userGroups().length > 0" >
						<li>
							<p><a class="myButton" href="#" data-bind="text: title, click: $parent.showUsers"></a></p>
						</li>
					</ul>-->
					<h3 data-bind="text: loadingMessage, visible: userGroups().length == 0" style="display: none;"></h3>
			</fieldset>
		</div>
		<div class="clear"></div>
		<p class="" style="margin-bottom: 0px; padding-bottom: 5px; line-height: 18px; font-size: 15px;">
			<strong>Note:&#160;</strong> Updates will reflect in Outlook address book after four hours
		</p>
	</div>	
	<div>		
		<script type="text/javascript" >
			window.history.replaceState("statedata", "title", "http://infospace-stg.emirates.com/sites/admanager");
		</script>	
<p>
Please contact <A href="mailto:infospace@emirates.com">Infospace Team</A> for support, queries and feedback
</p>		
	</div>
	<div id="footer">
        <div class="wrapper">
            <div class="left w80P">
                <!--Footer links-->
                <div class="copyRight">
                    
                  <IMG style="MARGIN-LEFT: 25px" src="/sites/admanager/SiteAssets/theEmiratesGroup.png">
				  <p>Copyright © 2016 Emirates Group.&nbsp; All rights reserved</p>
                <!--//Footer links-->
            </div>
			</div>
        </div>
    </div>
</asp:Content>
