var JSHelper = window.JSHelper || {};

$(document).ready(function () {
	SP.SOD.loadMultiple(['strings.js', 'sp.ui.dialog.js'], function () {
		var userArray = null;
		try {
			userArray = JSON.parse(window.frameElement.dialogArgs);
		}
		catch(err) {
			SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.cancel, 'issue');
		}
		
		var groupViewModel = new JSHelper.GroupViewModel(userArray);
		ko.applyBindings(groupViewModel);
	});
});

JSHelper.GroupViewModel = function (initUsers) {
    var self = this;
	
	try {
		self.users = ko.observableArray(initUsers);
	}
	catch(err) {
		SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.cancel, 'issue');
	}

    self.SaveData = function () {
		var savedArray = self.users();
		SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.OK, JSON.stringify(savedArray));
    };
	
	self.CancelSave = function () {
		SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.cancel, 'Cancel');
    };
};