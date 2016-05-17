(function () {
    // Private functions
    function getColumnsForScaffolding(data) {
        if ((typeof data.length !== 'number') || data.length === 0) {
            return [];
        }
        var columns = [];
        for (var propertyName in data[0]) {
            columns.push({ headerText: propertyName, rowText: propertyName });
        }
        return columns;
    }

    ko.simpleGrid = {
        // Defines a view model class you can use to populate a grid
        viewModel: function (configuration, vm) {
            this.data = configuration.data;
            this.currentPageIndex = ko.observable(0);
            this.pageSize = configuration.pageSize || 5;
			this.parentVM = vm;

            // If you don't specify columns configuration, we'll use scaffolding
            this.columns = configuration.columns || getColumnsForScaffolding(ko.unwrap(this.data));

            this.itemsOnCurrentPage = ko.computed(function () {
                var startIndex = this.pageSize * this.currentPageIndex();
                return ko.unwrap(this.data).slice(startIndex, startIndex + this.pageSize);
            }, this);

            this.maxPageIndex = ko.computed(function () {
                return Math.ceil(ko.unwrap(this.data).length / this.pageSize) - 1;
            }, this);
			
			this.showUsers = function (selection) {
				this.parentVM.showUsers(selection);
			}
			
			this.trimToLengthPadded = function (txt, len, appendText) {
				var isBlank = (!txt || /^\s*$/.test(txt));
				if(isBlank){
					return '';
				}
				var charAtLen = txt.substr(len, 1);
				while (len < txt.length && !/\s/.test(charAtLen)) {
					len++;
					charAtLen = txt.substr(len, 1);
				}
				return txt.substring(0, len) + (txt.length > len ? appendText : '');
			}
			
			this.isVisible = function (txt) {
				return !(!txt || /^\s*$/.test(txt));
			}
			
			this.pagerVisible = function () {
				return this.maxPageIndex() > 0;
			}
        }		
    };

    // Templates used to render the grid
    var templateEngine = new ko.nativeTemplateEngine();

    templateEngine.addTemplate = function(templateName, templateMarkup) {
        document.write("<script type='text/html' id='" + templateName + "'>" + templateMarkup + "<" + "/script>");
    };

    templateEngine.addTemplate("ko_simpleGrid_pageLinks", "\
                    <div class=\"ko-grid-pageLinks\" data-bind=\"visible: $root.pagerVisible()\">\
                        <span>Page:</span>\
                        <!-- ko foreach: ko.utils.range(0, maxPageIndex) -->\
                               <a href=\"#\" data-bind=\"text: $data + 1, click: function() { $root.currentPageIndex($data) }, css: { selected: $data == $root.currentPageIndex() }\">\
                            </a>\
                        <!-- /ko -->\
                    </div>");	
	
    templateEngine.addTemplate("ko_simpleGrid_grid", "\
                    <table class=\"ko-grid\" cellspacing=\"10\">\
                        <tbody data-bind=\"foreach: itemsOnCurrentPage\">\
                           <tr data-bind=\"foreach: $parent.columns\">\
                               <td data-bind=\"visible: $root.isVisible($parent[rowText])\">\
									<a class=\"myButton\" href=\"#\" data-bind=\"text: $root.trimToLengthPadded($parent[rowText], 35, '...'), click: function(data, event) { $root.showUsers($parent[rowText]); }, attr: { title: $parent[rowText] }\"></a>\
							   </td>\
                            </tr>\
                        </tbody>\
                    </table>");

    // The "simpleGrid" binding
    ko.bindingHandlers.simpleGrid = {
        init: function() {
            return { 'controlsDescendantBindings': true };
        },
        // This method is called to initialize the node, and will also be called again if you change what the grid is bound to
        update: function (element, viewModelAccessor, allBindings) {
            var viewModel = viewModelAccessor();

            // Empty the element
            while(element.firstChild)
                ko.removeNode(element.firstChild);

            // Allow the default templates to be overridden
            var gridTemplateName      = allBindings.get('simpleGridTemplate') || "ko_simpleGrid_grid",
                pageLinksTemplateName = allBindings.get('simpleGridPagerTemplate') || "ko_simpleGrid_pageLinks";

            // Render the page links
            var pageLinksContainer = element.appendChild(document.createElement("DIV"));
            ko.renderTemplate(pageLinksTemplateName, viewModel, { templateEngine: templateEngine }, pageLinksContainer, "replaceNode");				
				
            // Render the main grid
            var gridContainer = element.appendChild(document.createElement("DIV"));
            ko.renderTemplate(gridTemplateName, viewModel, { templateEngine: templateEngine }, gridContainer, "replaceNode");
        }
    };
})();