var JSHelper = window.JSHelper || {};
var waitDialog;
var allUsers = [];
var userGroups = [];
var userGroupsOld = [];

var selectedUsers = [];
var statusbar = null;

var oTable = null;
$(document).ready(function () {
    //var groupsViewModel = new JSHelper.GroupsViewModel();
    // ko.applyBindings(groupsViewModel);
    $('#pageTitle').html('<a href="/sites/DLManager/">Outlook Distribution List (DL) Management System </a>');
    SP.SOD.loadMultiple(['strings.js', 'sp.ui.dialog.js'], function () {

        getGroupsForUser();
    });

    $('#txtSearch').on('change keyup click paste search', function () {
        // do your stuff
       $('#userDataTable_filter input[type=search]:first').val($(this).val());
      $('#userDataTable_filter input[type=search]:first').keyup();
        
    });

    
});


function buildHtmlTable(selector, myList) {
    var columns = addAllColumnHeaders(myList, selector);
    
    for (var i = 0 ; i < myList.length ; i++) {
        var row$ = $('<tr/>');
        for (var colIndex = 0 ; colIndex < columns.length ; colIndex++) {
            var cellValue = myList[i][columns[colIndex]];

            if (cellValue == null) { cellValue = ""; }

            row$.append($('<td/>').html(cellValue));
        }
        $(selector).find('tbody').append(row$);
    }
}

function failHandler(jqXHR, textStatus, errorThrown) {
    var response = JSON.parse(jqXHR.responseText);
    var message = response ? response.error.message.value : textStatus;
    alert("Call failed. Error: " + message);
}

function getGroupsForUser()
{
    $('.tableDiv').show();
    waitDialog = SP.UI.ModalDialog.showWaitScreenWithNoClose("Loading ...", "Loading Authorization Groups...", 150, 400);
    var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.DALManager.ashx?op=groups";
     jQuery.ajax({
        url: address,
        method: "GET",
        async: true,
        headers: {
            "accept": "application/json;odata=verbose",
        }
     }).done(function (items) {
         userGroups = items;
         var grpName = '';
         if (userGroups != null && userGroups.length > 0) {
             $('.tableDiv').show();
             $('#ddlGroups').append($("<option></option>").attr("value", "0").text("Select"));
             for (var i = 0; i < userGroups.length; i++) {
                 if (i == 0)
                 {
                     grpName = userGroups[i] + "";
                 }
                 $('#ddlGroups').append($("<option></option>").attr("value", userGroups[i]).text(userGroups[i]));
             }
             if(grpName!='')
             {
               //  $('#dataTableDiv').hide();
               //    $('#LodingMSG').show(); $('#overlayButtonDiv').show(); 
              //   var obj = { group: grpName };
              //   getUserForGroup(obj);

             }
            
         }
         else {
             $('.tableDiv').hide();
             $('.noPermissionDiv').show();
         }
      
         waitDialog.close(SP.UI.DialogResult.OK);
     }).fail(function (jqXHR, textStatus, errorThrown) {
         waitDialog.close(SP.UI.DialogResult.OK);
         $('.tableDiv').hide();
         $('.noPermissionDiv').show();
         failHandler(jqXHR, textStatus, errorThrown);
     });

  
}

function getUserForGroup(obj)
{
    $('#groupNameHeader').html('Group Name: ' + obj.group);
    $('#groupName_UserCount').text('');
    selectedUsers = [];
    var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.DALManager.ashx?op=usersNew";
    var call = jQuery.ajax({
        url: address,
        method: "POST",
        async: true,
        data: obj,
        headers: {
            "accept": "application/json;odata=verbose"
        }
    }).done(function (items) {
      
        allUsers = items;

        clearAndCreateTable();
        buildHtmlTable('#userDataTable', allUsers);

        $('#groupName_UserCount').text('(' + (allUsers.length > 2000 ? 2000 : allUsers.length) + ')');
        $('#userDataTable').find('tbody tr').each(function () {
            $(this).prepend($('<td/>').html('<input onclick="chkUserChange(this)" type="checkbox" />'));
        });
        bindDataTable();
        $('#dataTableDiv').show();
        $('#LodingMSG').hide(); $('#overlayButtonDiv').hide();
		
		if(allUsers.length > 2000)
		{
			//$('#divThreshold').show();
			statusbar = SP.UI.Status.addStatus("Threshold Exceeded", 'This DL has exceeded the maximum threshold limit of 2000 users. Kindly contact <a href="http://itsc" target="_blank">ITSC</a> for further assistance.');
			SP.UI.Status.setStatusPriColor(statusbar, 'red');
		} 
    });


}
function bindDataTable() {
    oTable =  $('#userDataTable').DataTable({
        columnDefs: [
{ "targets": [0,2], "searchable": false, "orderable": false, "visible": true }
        ],
        "order": [[1, "asc"]],
          "bLengthChange": false, // start
         
        "info": false,
        "pageLength":15
    });
}
function clearAndCreateTable()
{
    $('#dataTableDiv').html('');

    var table = '<table cellspacing="0" cellpadding="0"  class="userGrid stripe" id="userDataTable">'+
                       ' <thead></thead>'+
                      '  <tbody></tbody>'+
                   ' </table>';

    $('#dataTableDiv').html(table);
}

function getUserForGroup_Old(obj) {

    
    var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.DALManager.ashx?op=users";
    var call = jQuery.ajax({
        url: address,
        method: "POST",
        async: true,
        data: obj,
        headers: {
            "accept": "application/json;odata=verbose"
        }
    }).done(function (items) {
        userGroupsOld = items;

    });


}
// Adds a header row to the table and returns the set of columns.
// Need to do union of keys from all records as some records may not contain
// all records
function addAllColumnHeaders(myList, selector) {
    var columnSet = [];
    for (var i = 0 ; i < myList.length ; i++) {
        var rowHash = myList[i];
        for (var key in rowHash) {
            if ($.inArray(key, columnSet) == -1) {
                columnSet.push(key);
               
            }
        }
    }

    var tr = "";
    tr += " <tr>";
    tr += "                        <th style=\"width:20px\">Select<\/th>";
    tr += "                        <th  style=\"width:300px\">Name<\/th>";
    tr += "                        <th  style=\"width:350px\">Email<\/th>";
    tr += "                        <th style=\"min-width:100px\">Staff ID<\/th>";
    tr += "                    <\/tr>";

    $(selector).find('thead').append(tr);

    return columnSet;
}

function ddlGroupChange()
{
	if(statusbar != null) {
		SP.UI.Status.removeStatus(statusbar);
	}
  
    $('#dataTableDiv').hide();
      $('#LodingMSG').show(); $('#overlayButtonDiv').show();

    var grpName = $('#ddlGroups').val();


    var obj = { group: grpName };
    getUserForGroup(obj);


}
function chkUserChange(chk)
{
    var staffId = $(chk).closest('tr').find('td:last').text();
    if(chk.checked)
    {
        selectedUsers.push(staffId);
    }
    else
    {
        var index = getIndexOfArray(staffId, selectedUsers);
        if (index > -1) {
            selectedUsers.splice(index, 1);
        }
    }
}

function getIndexOfArray(keyword,arr)
{
    var index = -1;
    if (arr != null)
    {
        for(var i=0;i<arr.length;i++)
        {
            if(arr[i]==keyword)
            {
                index = i;
                break;
            }
        }
    }

        return index;
}

function addNewUsers()
{

    var options = SP.UI.$create_DialogOptions();
    options.url = _spPageContextInfo.webAbsoluteUrl + '/SitePages/Group ManagerNew.aspx';
    options.title = "Add New User : " + $('#ddlGroups').val();
    options.allowMaximize = false;
    options.height = 300;
    options.width = 550;
     options.dialogReturnValueCallback = Function.createDelegate(null, callbackToAddNewuser);

    SP.UI.ModalDialog.showModalDialog(options);
}

function callbackToAddNewuser(result, target) {
    if (result == SP.UI.DialogResult.OK) {
        var changedArray = JSON.parse(target);
        var newItems = $.map(changedArray, function (n, i) {
            return n.toLowerCase().replace(/i:0#.w\|emirates\\/gi, "");
        });
        var uniqueNames = [];
        $.each(newItems, function (i, el) {
            if ($.inArray(el, uniqueNames) === -1) uniqueNames.push(el);
        });
        addUserToGroup(uniqueNames);
    }
}
function addUserToGroup(arrUsers)
{
    if (arrUsers != null && arrUsers.length > 0) {
        var grpName = $('#ddlGroups').val();

        var addObj = { users: JSON.stringify(arrUsers), group: grpName };
        var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.DALManager.ashx?op=addusers";
        var call = jQuery.ajax({
            url: address,
            method: "POST",
            async: true,
            data: addObj,
            headers: {
                "accept": "application/json;odata=verbose"
            }
        }).done(function () {
            alert('DL Group Updated.');
           // ddlGroupChange();
        }).fail(function () {
        });
    }
}
//delete user

function deleteSelectedUser() {
    if (selectedUsers.length > 0) {
        if (confirm('are you sure you want to delete user?')) {

            var grpName = $('#ddlGroups').val();

            var delObj = { users: JSON.stringify(selectedUsers), group: grpName };
            var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.DALManager.ashx?op=delusers";
            var call = jQuery.ajax({
                url: address,
                method: "POST",
                async: true,
                data: delObj,
                headers: {
                    "accept": "application/json;odata=verbose"
                }
            }).done(function () {
               
                ddlGroupChange();
            }).fail(function () {
            });

        }
    }
    else
    {
        alert('please select user.');
    }
}

JSHelper.GroupsViewModel = function () {
    var self = this;
    self.userGroups = ko.observableArray([]);
	self.multiDimensionalData = ko.observableArray([]);
	self.loadingMessage = ko.observable('');
	self.user = ko.observableArray();
	self.previouslyLoadedItems = [];
	self.previouslyLoadedGroup = "";

	self.loadUser = function (userArray) {
		self.user(userArray);
	};
	
    self.loadGroups = function () {
        JSHelper.DataOperations.getGroupsForUser().done(function (items) {
            var output = [];
            $.each(items, function (index, item) {
                var row = {
                    title: item
                };

                output.push(row);
            });
			
			output.sort(function(a, b){
				var x = a.title.toLowerCase(), y = b.title.toLowerCase();
				return x.localeCompare(y);
			});
					
			var matrix = [], i, k;
			var elementsPerSubArray = 1;

			for (i = 0, k = -1; i < output.length; i++) {
				if (i % elementsPerSubArray === 0) {
					k++;
					matrix[k] = [];
				}
				
				matrix[k].push(output[i].title);
			}
			
			var tableData = [];
			$.each(matrix, function (i, arr) {
				var row = {};
				
				$.each(arr, function (j, item) {
					row["column" + j] = item;
				});
				
				tableData.push(row);
			});

			self.userGroups(tableData);

			if(output.length == 0) {
				self.loadingMessage('You do not own any Distribution Lists');
			}
			waitDialog.close(SP.UI.DialogResult.OK);
        }).fail(function (jqXHR, textStatus, errorThrown) {
			waitDialog.close(SP.UI.DialogResult.OK);
            self.failHandler(jqXHR, textStatus, errorThrown);
        });
    };
	
	self.trimToLengthPadded = function (txt, len, appendText) {
		var charAtLen = txt.substr(len, 1);
		while (len < txt.length && !/\s/.test(charAtLen)) {
			len++;
			charAtLen = txt.substr(len, 1);
		}
		return txt.substring(0, len) + (txt.length > len ? appendText : '');
	};

	
	self.gridViewModel = new ko.simpleGrid.viewModel({
        data: self.userGroups,
        columns: [
            { headerText: "", rowText: "column0" },
            { headerText: "", rowText: "column1" }
        ],
        pageSize: 5
    }, self);

    self.showUsers = function (group) {
        var obj = { group: group };
		self.previouslyLoadedGroup = group;

		waitDialog = SP.UI.ModalDialog.showWaitScreenWithNoClose("Loading ...", "Loading Members...", 150, 400);

		JSHelper.DataOperations.getUsersForGroup(obj).done(function (items) {
		    waitDialog.close(SP.UI.DialogResult.OK);
		    allUsers = items;
		    $('#userDataTable').find('thead').html('');
		    $('#userDataTable').find('tbody').html('');
		    buildHtmlTable('#userDataTable', allUsers);

		    $('#userDataTable').find('thead tr').prepend($('<th/>').html('Select'));
		    $('#userDataTable').find('tbody tr').prepend($('<td/>').html('<input type="checkbox" />'));


		}).fail(function (jqXHR, textStatus, errorThrown) {
		    alert(errorThrown);
		    self.failHandler(jqXHR, textStatus, errorThrown);
		});

        /*JSHelper.DataOperations.getUsersForGroup(obj).done(function (items) {
			var options = SP.UI.$create_DialogOptions();
			options.url = _spPageContextInfo.webAbsoluteUrl + '/SitePages/Group Manager.aspx';
			options.title = "Manage: " + group;
			options.allowMaximize = false;
			self.previouslyLoadedItems = items;
			options.args = JSON.stringify(items);
			options.height = 600;
			options.width = 800;
			waitDialog.close(SP.UI.DialogResult.OK);
			options.dialogReturnValueCallback = Function.createDelegate(null, self.dialogCallback);
			
			SP.UI.ModalDialog.showModalDialog(options);
        }).fail(function (jqXHR, textStatus, errorThrown) {
			waitDialog.close(SP.UI.DialogResult.OK);
            self.failHandler(jqXHR, textStatus, errorThrown);
        });*/
    }
	
	self.dialogCallback = function (result, target) {
		if (result == SP.UI.DialogResult.OK) {
			var changedArray = JSON.parse(target);
			var newItems = $.map(changedArray, function( n, i ) {
			  return n.toLowerCase().replace(/i:0#.w\|emirates\\/gi, "");
			});
			
			var uniqueNames = [];
			$.each(newItems, function(i, el){
				if($.inArray(el, uniqueNames) === -1) uniqueNames.push(el);
			});
			
			var oldItems = $.map(self.previouslyLoadedItems, function( n, i ) {
			  return n.toLowerCase();
			});
			
			var deleted = [];
			$.each(oldItems, function(i, el){
				if($.inArray(el, uniqueNames) === -1) deleted.push(el);
			});

			var added = [];
			$.each(uniqueNames, function(i, el){
				if($.inArray(el, oldItems) === -1) added.push(el);
			});
			
			var changesRequired = deleted.length > 0 || added.length > 0;
			
			if(changesRequired){
				waitDialog = SP.UI.ModalDialog.showWaitScreenWithNoClose("Saving ...", "Performing Directory Operation...", 150, 400);
				
				var def = [];
				var delObj = { users: JSON.stringify(deleted), group: self.previouslyLoadedGroup };
				var addObj = { users: JSON.stringify(added), group: self.previouslyLoadedGroup };
				if(deleted.length > 0) {
					def.push(JSHelper.DataOperations.delUsersForGroup(delObj));
				}
				
				if(added.length > 0) {
					def.push(JSHelper.DataOperations.addUsersToGroup(addObj));
				}
				
				$.when.apply($, def).then(function(obj1, obj2) {
					waitDialog.close(SP.UI.DialogResult.OK);
					if (typeof obj1 == 'object') {
						if(obj1[0] == true && obj2[0] == true) {
							SP.UI.Notify.addNotification('All operations performed <strong>successfully</strong>', false);
						} else if(obj1[0] == true || obj2[0] == true) {
							SP.UI.Notify.addNotification('There were some <strong>errors</strong>', false);
						} else {
							SP.UI.Notify.addNotification('The operation was <strong>unsuccessful</strong>', false);
						}
					} else if (typeof obj1 == 'boolean' && obj1 == true) {
						SP.UI.Notify.addNotification('All operations performed <strong>successfully</strong>', false);
					} else {
						SP.UI.Notify.addNotification('The operation was <strong>unsuccessful</strong>', false);
					}
				}, function() {
					SP.UI.Notify.addNotification('The AD Operation <strong>failed</strong>', false);
				});
			} else {
				SP.UI.Notify.addNotification('No changes to save...', false);
			}
		} else if(result == SP.UI.DialogResult.cancel) {
			if(target == 'issue') {
				SP.UI.Notify.addNotification('Unable to load members, please try again...', false);
			}
		}
	};

    self.failHandler = function (jqXHR, textStatus, errorThrown) {
        var response = JSON.parse(jqXHR.responseText);
        var message = response ? response.error.message.value : textStatus;
        alert("Call failed. Error: " + message);
    };
};

JSHelper.DataOperations = function () {
    var _getGroupsForUser = function () {
        var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.DALManager.ashx?op=groups";
        var call = jQuery.ajax({
            url: address,
            method: "GET",
            async: true,
            headers: {
				"accept": "application/json;odata=verbose",
			}
        });

        return call;
    };

    var _getUsersForGroup = function (obj) {
        var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.DALManager.ashx?op=users";
        var call = jQuery.ajax({
            url: address,
            method: "POST",
            async: true,
            data: obj,
            headers: {
                "accept": "application/json;odata=verbose"
            }
        });

        return call;
    };
	
    var _delUsersForGroup = function (obj) {
        var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.DALManager.ashx?op=delusers";
        var call = jQuery.ajax({
            url: address,
            method: "POST",
            async: true,
            data: obj,
            headers: {
                "accept": "application/json;odata=verbose"
            }
        });

        return call;
    };	
	
    var _addUsersToGroup = function (obj) {
        var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.DALManager.ashx?op=addusers";
        var call = jQuery.ajax({
            url: address,
            method: "POST",
            async: true,
            data: obj,
            headers: {
                "accept": "application/json;odata=verbose"
            }
        });

        return call;
    };		

    return {
        "getGroupsForUser": _getGroupsForUser,
        "getUsersForGroup": _getUsersForGroup,
		"delUsersForGroup": _delUsersForGroup,
		"addUsersToGroup": _addUsersToGroup
    };
}();