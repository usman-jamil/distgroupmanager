var JSHelper = window.JSHelper || {};
var waitDialog;

$(document).ready(function () {
    var groupsViewModel = new JSHelper.GroupsViewModel();
    ko.applyBindings(groupsViewModel);
	waitDialog = SP.UI.ModalDialog.showWaitScreenWithNoClose("Loading ...", "Loading Authorization Groups...", 150, 400);
	groupsViewModel.loadGroups();
});

trimToLengthPadded = function (txt, len, appendText) {
	var charAtLen = txt.substr(len, 1);
	while (len < txt.length && !/\s/.test(charAtLen)) {
		len++;
		charAtLen = txt.substr(len, 1);
	}
	return txt.substring(0, len) + (txt.length > len ? appendText : '');
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
			var elementsPerSubArray = 2;

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
				self.loadingMessage('you do not have any distribution lists to manage');
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
        });
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
				SP.UI.Notify.addNotification('No changes required...', false);
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
        var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.ADGroupHandler.ashx?op=groups";
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
        var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.ADGroupHandler.ashx?op=users";
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
        var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.ADGroupHandler.ashx?op=delusers";
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
        var address = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/Handlers/Emirates.SharePoint.ADGroupHandler.ashx?op=addusers";
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