﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
// 11.08.2011 - Created [Ove Andersen]
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using uComponents.Core;
using uComponents.DataTypes.DataTypeGrid.Model;

using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

[assembly: WebResource("uComponents.DataTypes.DataTypeGrid.Css.DTG_DataEditor.css", Constants.MediaTypeNames.Text.Css, PerformSubstitution = true)]
[assembly: WebResource("uComponents.DataTypes.DataTypeGrid.Scripts.jquery.dataTables.min.js", Constants.MediaTypeNames.Application.JavaScript)]
[assembly: WebResource("uComponents.DataTypes.DataTypeGrid.Scripts.DTG_DataEditor.js", Constants.MediaTypeNames.Application.JavaScript)]

namespace uComponents.DataTypes.DataTypeGrid
{
    using System.Web;

    using uComponents.DataTypes.DataTypeGrid.Constants;
    using uComponents.DataTypes.DataTypeGrid.Extensions;
    using uComponents.DataTypes.DataTypeGrid.Factories;
    using uComponents.DataTypes.DataTypeGrid.Functions;
    using uComponents.DataTypes.DataTypeGrid.Interfaces;
    using uComponents.DataTypes.DataTypeGrid.ServiceLocators;
    using uComponents.DataTypes.DataTypeGrid.Validators;

    using umbraco;

    /// <summary>
    /// The DataType Grid Control
    /// </summary>
    [ClientDependency.Core.ClientDependency(ClientDependency.Core.ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient")]
    [ClientDependency.Core.ClientDependency(ClientDependency.Core.ClientDependencyType.Css, "ui/ui-lightness/jquery-ui.custom.css", "UmbracoClient")]
    [ClientDependency.Core.ClientDependency(ClientDependency.Core.ClientDependencyType.Javascript, "controls/Images/ImageViewer.js", "UmbracoRoot")]
    public class DataEditor : Control, INamingContainer, IDataEditor
    {
        /// <summary>
        /// Value stored by a datatype instance
        /// </summary>
        private readonly IData data;

        /// <summary>
        /// The datatype definition id
        /// </summary>
        private readonly int dataTypeDefinitionId;

        /// <summary>
        /// The settings.
        /// </summary>
        private readonly PreValueEditorSettings settings;

        /// <summary>
        /// The prevalue editor settings factory
        /// </summary>
        private readonly IPrevalueEditorSettingsFactory prevalueEditorSettingsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEditor"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="dataTypeDefinitionId">The data type definition id.</param>
        /// <param name="instanceId">The instance id.</param>
        public DataEditor(IData data, PreValueEditorSettings settings, int dataTypeDefinitionId, string instanceId)
        {
            // Set up dependencies
            this.prevalueEditorSettingsFactory = new PrevalueEditorSettingsFactory();

            this.settings = settings;
            this.data = data;

            this.dataTypeDefinitionId = dataTypeDefinitionId;
            this.instanceId = instanceId;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        public List<StoredValueRow> Rows { get; set; }

        /// <summary>
        /// Gets or sets the grid.
        /// </summary>
        public Table Grid { get; set; }

        /// <summary>
        /// Gets or sets the grid.
        /// </summary>
        public Panel Toolbar { get; set; }

        /// <summary>
        /// Gets or sets the insert controls.
        /// </summary>
        public Panel InsertControls { get; set; }

        /// <summary>
        /// Gets or sets the edit controls.
        /// </summary>
        public Panel EditControls { get; set; }

        /// <summary>
        /// Gets or sets the delete controls.
        /// </summary>
        public Panel DeleteControls { get; set; }

        /// <summary>
        /// Gets or sets the current row.
        /// </summary>
        /// <value>The current row.</value>
        public int CurrentRow
        {
            get
            {
                if (ViewState["CurrentRow"] != null)
                {
                    return (int)ViewState["CurrentRow"];
                }

                return 0;
            }

            set
            {
                ViewState["CurrentRow"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the data string.
        /// </summary>
        /// <value>The data string.</value>
        public string DataString
        {
            get
            {
                if (ViewState["DataString"] != null)
                {
                    Helper.Log.Debug<DataType>(string.Format("DTG: Returned value from ViewState: {0}", ViewState["DataString"]));

                    return ViewState["DataString"].ToString();
                }

                Helper.Log.Warn<DataType>(string.Format("DTG: ViewState did not contain data."));

                return string.Empty;
            }

            set
            {
                Helper.Log.Debug<DataType>(string.Format("DTG: Stored the following data in ViewState: {0}", value));

                ViewState["DataString"] = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to show the grid header.
        /// </summary>
        /// <value>
        /// Whether to show the header.
        /// </value>
        public HiddenField ShowGridHeader { get; set; }

        /// <summary>
        /// Gets or sets whether to show the footer.
        /// </summary>
        /// <value>
        /// Whether to show the footer.
        /// </value>
        public HiddenField ShowGridFooter { get; set; }

        /// <summary>
        /// Gets or sets the number of rows per page.
        /// </summary>
        /// <value>The number of rows per page.</value>
        public HiddenField RowsPerPage { get; set; }

        /// <summary>
        /// Gets or sets the datatables translation.
        /// </summary>
        /// <value>The datatables translation.</value>
        public LiteralControl DataTablesTranslation { get; set; }

        /// <summary>
        /// Gets or sets the stored prevalues.
        /// </summary>
        /// <value>The stored pre values.</value>
        public IEnumerable<PreValueRow> ColumnConfigurations { get; set; }

        /// <summary>
        /// Gets er sets the insert data types
        /// </summary>
        /// <value>The insert data types.</value>
        public List<StoredValue> InsertDataTypes { get; set; }

        /// <summary>
        /// Gets or sets the edit data types.
        /// </summary>
        /// <value>The edit data types.</value>
        public List<StoredValue> EditDataTypes { get; set; }

        /// <summary>
        /// Gets or sets the programmatic identifier assigned to the server control.
        /// </summary>
        /// <value></value>
        /// <returns>The programmatic identifier assigned to the control.</returns>
        public override string ID
        {
            get
            {
                return this.id ?? (this.id = string.Concat("DTG_", this.dataTypeDefinitionId, "_", this.instanceId));
            }
        }

        /// <summary>
        /// Gets the control id.
        /// </summary>
        private string id;

        /// <summary>
        /// The unique instance id
        /// </summary>
        private readonly string instanceId;

        #endregion

        #region IDataEditor Members

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            this.data.Value = string.IsNullOrEmpty(this.DataString) ? this.data.Value : this.DataString;

            // Get new values
            this.Rows = this.GetStoredValues();

            // Refresh grid
            this.RefreshGrid();

            // Clear input controls
            this.ClearControls();

            Helper.Log.Debug<DataType>(string.Format("DTG: Saved the following data to database: {0}", this.data.Value));
        }

        /// <summary>
        /// Stores this instance temporarily.
        /// </summary>
        public void Store()
        {
            // Make sure sort order is correct
            this.SetRowsSortOrder();

            // Start data
            var str = "<items>";

            foreach (var container in this.Rows.OrderBy(x => x.SortOrder))
            {
                // Start
                str += string.Concat("<item id='", container.Id.ToString(), "' sortOrder='", container.SortOrder, "'>");

                foreach (var v in container.Cells)
                {
                    if (v.Value.Data.Value == null)
                    {
                        v.Value.Data.Value = string.Empty;
                    }

                    str += string.Concat(
                        "<",
                        v.Alias,
                        " nodeName='",
                        v.Name,
                        "' nodeType='",
                        v.Value.DataTypeDefinitionId,
                        "'>",
                        HttpUtility.HtmlEncode(v.Value.Data.Value.ToString()),
                        "</",
                        v.Alias,
                        ">");
                }

                // End row
                str += "</item>";
            }

            // End data
            str += "</items>";

            // Save values
            DataString = str;

            // Refresh grid
            RefreshGrid();

            // Clear input controls
            ClearControls();
        }

        /// <summary>
        /// Gets a value indicating whether [show label].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show label]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool ShowLabel
        {
            get
            {
                return this.settings.ShowLabel;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [treat as rich text editor].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [treat as rich text editor]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool TreatAsRichTextEditor
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the editor.
        /// </summary>
        public Control Editor
        {
            get
            {
                return this;
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Refreshes the grid.
        /// </summary>
        private void RefreshGrid()
        {
            // Remove all rows
            Grid.Rows.Clear();

            // Re-add rows
            GenerateHeaderRow();
            GenerateValueRows();
        }

        /// <summary>
        /// The clear controls.
        /// </summary>
        private void ClearControls()
        {
            InsertDataTypes = GetInsertDataTypes();
            GenerateInsertControls();

            CurrentRow = 0;

            EditDataTypes = GetEditDataTypes();
            GenerateEditControls();
        }

        /// <summary>
        /// Sets the sort order.
        /// </summary>
        private void SetRowsSortOrder()
        {
            this.Rows = this.Rows.OrderBy(x => x.SortOrder).ToList();

            for (var i = 0; i < this.Rows.Count(); i++)
            {
                this.Rows[i].SortOrder = i;
            }
        }

        /// <summary>
        /// Generates the header row.
        /// </summary>
        private void GenerateHeaderRow()
        {
            var tr = new TableRow { TableSection = TableRowSection.TableHeader };

            // Add ID header cell
            tr.Cells.Add(new TableHeaderCell { Text = Helper.Dictionary.GetDictionaryItem("ID", "ID") });

            tr.Cells.Add(new TableHeaderCell { Text = Helper.Dictionary.GetDictionaryItem("Actions", "Actions") });

            // Add prevalue cells
            foreach (var s in this.ColumnConfigurations)
            {
                var th = new TableHeaderCell { Text = s.Name };

                // If the name starts with a hash, get the dictionary item
                if (s.Name.StartsWith("#"))
                {
                    var key = s.Name.Substring(1, s.Name.Length - 1);

                    th.Text = uQuery.GetDictionaryItem(key, key);
                }

                tr.Cells.Add(th);
            }

            Grid.Rows.Add(tr);
        }

        /// <summary>
        /// Generates the value rows.
        /// </summary>
        private void GenerateValueRows()
        {
            foreach (var row in this.Rows.OrderBy(x => x.SortOrder))
            {
                var tr = new TableRow();

                // Add ID column
                var id = new TableCell();
                id.Controls.Add(new Label { Text = row.Id.ToString() });

                tr.Cells.Add(id);

                // Delete button
                var actions = new TableCell() { CssClass = "actions"};

                var dInner = new HtmlGenericControl("span");
                dInner.Attributes["class"] = "ui-button-text";
                dInner.InnerText = Helper.Dictionary.GetDictionaryItem("Delete", "Delete");

                var dIcon = new HtmlGenericControl("span");
                dIcon.Attributes["class"] = "ui-button-icon-primary ui-icon ui-icon-close";

                var deleteRow = new LinkButton
                    {
                        ID = "DeleteButton_" + row.Id,
                        CssClass =
                            "deleteRowDialog ui-button ui-widget ui-state-default ui-corner-all ui-button-icon-only",
                        CommandArgument = row.Id.ToString(),
                        OnClientClick = "return confirm('Are you sure you want to delete this?')"
                    };
                deleteRow.Click += deleteRow_Click;

                deleteRow.Controls.Add(dIcon);
                deleteRow.Controls.Add(dInner);

                // Edit button
                var eInner = new HtmlGenericControl("span");
                eInner.Attributes["class"] = "ui-button-text";
                eInner.InnerText = Helper.Dictionary.GetDictionaryItem("Edit", "Edit");

                var eIcon = new HtmlGenericControl("span");
                eIcon.Attributes["class"] = "ui-button-icon-primary ui-icon ui-icon-pencil";

                var editRow = new LinkButton
                    {
                        ID = "EditButton_" + row.Id,
                        CssClass = "editRowDialog ui-button ui-widget ui-state-default ui-corner-all ui-button-icon-only",
                        CommandArgument = row.Id.ToString()
                    };
                editRow.Click += this.editRow_Click;

                editRow.Controls.Add(eIcon);
                editRow.Controls.Add(eInner);

                // Move up button
                var mUpInner = new HtmlGenericControl("span");
                mUpInner.Attributes["class"] = "ui-button-text";
                mUpInner.InnerText = Helper.Dictionary.GetDictionaryItem("MoveUp", "Move up");

                var mUpIcon = new HtmlGenericControl("span");
                mUpIcon.Attributes["class"] = "ui-button-icon-primary ui-icon ui-icon-arrowthick-1-n";

                var moveRowUp = new LinkButton
                    {
                        ID = "MoveUpButton_" + row.Id,
                        CssClass = "moveRowUp ui-button ui-widget ui-state-default ui-corner-all ui-button-icon-only",
                        CommandArgument = row.Id.ToString()
                    };
                moveRowUp.Click += this.moveRowUp_Click;

                moveRowUp.Controls.Add(mUpIcon);
                moveRowUp.Controls.Add(mUpInner);

                // Move up button
                var mDownInner = new HtmlGenericControl("span");
                mDownInner.Attributes["class"] = "ui-button-text";
                mDownInner.InnerText = Helper.Dictionary.GetDictionaryItem("MoveDown", "Move down");

                var mDownIcon = new HtmlGenericControl("span");
                mDownIcon.Attributes["class"] = "ui-button-icon-primary ui-icon ui-icon-arrowthick-1-s";

                var moveRowDown = new LinkButton
                    {
                        ID = "MoveDownButton_" + row.Id,
                        CssClass = "moveRowDown ui-button ui-widget ui-state-default ui-corner-all ui-button-icon-only",
                        CommandArgument = row.Id.ToString()
                    };
                moveRowDown.Click += this.moveRowDown_Click;

                moveRowDown.Controls.Add(mDownIcon);
                moveRowDown.Controls.Add(mDownInner);

                actions.Controls.Add(deleteRow);
                actions.Controls.Add(editRow);
                actions.Controls.Add(moveRowUp);
                actions.Controls.Add(moveRowDown);

                tr.Cells.Add(actions);

                // Print stored values
                foreach (var storedConfig in this.ColumnConfigurations)
                {
                    var td = new TableCell();

                    foreach (var value in row.Cells)
                    {
                        var text = new Label { Text = DataTypeFactoryServiceLocator.Instance.GetDisplayValue(value.Value) };

                        if (value.Name.Equals(storedConfig.Name))
                        {
                            td.Controls.Add(text);
                        }
                    }

                    tr.Cells.Add(td);
                }

                Grid.Rows.Add(tr);
            }
        }

        /// <summary>
        /// Generates the footer row.
        /// </summary>
        private void GenerateFooterToolbar()
        {
            var inner = new HtmlGenericControl("span") { InnerText = Helper.Dictionary.GetDictionaryItem("Add", "Add") };
            inner.Attributes["class"] = "ui-button-text";

            var icon = new HtmlGenericControl("span");
            icon.Attributes["class"] = "ui-button-icon-primary ui-icon ui-icon-plus";

            var addRowDialog = new LinkButton
                {
                    ID = "InsertRowDialog",
                    CssClass =
                        "insertRowDialog ui-button ui-widget ui-state-default ui-corner-all ui-button-text-icon-primary"
                };
            addRowDialog.Click += this.addRowDialog_Click;

            addRowDialog.Controls.Add(icon);
            addRowDialog.Controls.Add(inner);

            Toolbar.Controls.Add(addRowDialog);
        }

        /// <summary>
        /// Generates the validation controls.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="name">The name.</param>
        /// <param name="config">The config.</param>
        /// <param name="list">The list.</param>
        private void GenerateValidationControls(Control parent, string name, StoredValue config, IList<StoredValue> list)
        {
            var control = parent.FindControl(config.Value.DataEditor.Editor.ID);

            // If the name starts with a hash, get the dictionary item
            if (config.Name.StartsWith("#"))
            {
                var key = config.Name.Substring(1, config.Name.Length - 1);

                config.Name = uQuery.GetDictionaryItem(key, key);
            }

            // Mandatory
            if (this.ColumnConfigurations.Single(x => x.Alias == config.Alias).Mandatory && control != null)
            {
                try
                {
                    var wrapper = new Panel();

                    var validator = new ClientSideRequiredFieldValidator(name, config, false);

                    wrapper.Controls.Add(validator);
                    parent.Controls.Add(wrapper);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Trace.Warn("DataTypeGrid", "EditorControl (" + config.Value.DataTypeName + ") does not support validation", ex);
                }
            }

            // Regex
            if (!string.IsNullOrEmpty(this.ColumnConfigurations.First(x => x.Alias == config.Alias).ValidationExpression) && control != null)
            {
                try
                {
                    var wrapper = new Panel();

                    var regex = new Regex(this.ColumnConfigurations.First(x => x.Alias == config.Alias).ValidationExpression);
                    var validator = new ClientSideRegexValidator(name, config, false)
                                        {
                                            ValidationExpression = regex.ToString()
                                        };

                    wrapper.Controls.Add(validator);
                    parent.Controls.Add(wrapper);
                }
                catch (ArgumentException ex)
                {
                    parent.Controls.Add(
                        new HtmlGenericControl("span")
                            {
                                InnerText =
                                    string.Concat(
                                        "Regex validation expression is invalid. Validation will not occur.",
                                        "<!-- ",
                                        ex,
                                        " -->")
                            });
                }
            }
        }

        /// <summary>
        /// Generates the insert controls.
        /// </summary>
        private void GenerateInsertControls()
        {
            InsertControls.Controls.Clear();

            InsertControls.Controls.Add(new LiteralControl("<ul class='controls'>"));

            foreach (var config in InsertDataTypes)
            {
                var control = config.Value.DataEditor.Editor;
                control.ID = "insert_" + config.Alias;

                // Initialize the datatype so it works with DTG
                DataTypeFactoryServiceLocator.Instance.Initialize(config.Value, new DataTypeLoadEventArgs(this, this.InsertControls));
                config.Value.DataEditor.Editor.Load +=
                    (sender, args) =>
                    DataTypeFactoryServiceLocator.Instance.Configure(config.Value, new DataTypeLoadEventArgs(this, this.InsertControls));

                InsertControls.Controls.Add(new LiteralControl("<li class='control'>"));

                var title = new Label() { CssClass = "control-label", Text = config.Name };

                // If the name starts with a hash, get the dictionary item
                if (config.Name.StartsWith("#"))
                {
                    var key = config.Name.Substring(1, config.Name.Length - 1);

                    title.Text = uQuery.GetDictionaryItem(key, key);
                }

                this.InsertControls.Controls.Add(title);

                InsertControls.Controls.Add(control);
                GenerateValidationControls(InsertControls, "insert", config, InsertDataTypes);

                InsertControls.Controls.Add(new LiteralControl("</li>"));
            }

            InsertControls.Controls.Add(new LiteralControl("</ul>"));

            var iInner = new HtmlGenericControl("span")
                { InnerText = Helper.Dictionary.GetDictionaryItem("Add", "Add") };
            iInner.Attributes["class"] = "ui-button-text";

            var iIcon = new HtmlGenericControl("span");
            iIcon.Attributes["class"] = "ui-button-icon-primary ui-icon ui-icon-plus";

            var addRow = new LinkButton
                {
                    ID = "InsertButton",
                    CssClass =
                        "insertButton ui-button ui-widget ui-state-default ui-corner-all ui-button-text-icon-primary"
                };
            addRow.Click += addRow_Click;

            addRow.Controls.Add(iIcon);
            addRow.Controls.Add(iInner);

            this.InsertControls.Controls.Add(addRow);
        }

        /// <summary>
        /// Handles the Click event of the addRow control.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        protected void addRow_Click(object sender, EventArgs e)
        {
            var row = new StoredValueRow { Id = this.GetAvailableId(), SortOrder = this.Rows.Count() + 1 };

            foreach (var t in this.InsertDataTypes)
            {
                // Save value to datatype
                DataTypeFactoryServiceLocator.Instance.Save(t.Value, new DataTypeSaveEventArgs(this, DataTypeAction.Add));

                // Create new storedvalue object
                var v = new StoredValue { Name = t.Name, Alias = t.Alias, Value = t.Value };

                row.Cells.Add(v);
            }

            this.Rows.Add(row);

            Store();
            Save();
        }

        /// <summary>
        /// Generates the edit controls.
        /// </summary>
        private void GenerateEditControls()
        {
            this.EditControls.Controls.Clear();

            this.EditControls.Controls.Add(new LiteralControl("<ul class='controls'>"));

            foreach (var config in this.EditDataTypes)
            {
                var control = config.Value.DataEditor.Editor;
                control.ID = "edit_" + config.Alias;

                // Initialize the datatype so it works with DTG
                DataTypeFactoryServiceLocator.Instance.Initialize(config.Value, new DataTypeLoadEventArgs(this, this.EditControls));
                config.Value.DataEditor.Editor.Load +=
                    (sender, args) =>
                    DataTypeFactoryServiceLocator.Instance.Configure(config.Value, new DataTypeLoadEventArgs(this, this.EditControls));

                this.EditControls.Controls.Add(new LiteralControl("<li class='control'>"));

                var title = new Label() { CssClass = "control-label", Text = config.Name };

                // If the name starts with a hash, get the dictionary item
                if (config.Name.StartsWith("#"))
                {
                    var key = config.Name.Substring(1, config.Name.Length - 1);

                    title.Text = uQuery.GetDictionaryItem(key, key);
                }

                this.EditControls.Controls.Add(title);

                this.EditControls.Controls.Add(control);
                this.GenerateValidationControls(this.EditControls, "edit", config, this.EditDataTypes);

                this.EditControls.Controls.Add(new LiteralControl("</li>"));
            }

            this.EditControls.Controls.Add(new LiteralControl("</ul>"));

            var uInner = new HtmlGenericControl("span")
                { InnerText = Helper.Dictionary.GetDictionaryItem("Update", "Update") };
            uInner.Attributes["class"] = "ui-button-text";

            var uIcon = new HtmlGenericControl("span");
            uIcon.Attributes["class"] = "ui-button-icon-primary ui-icon ui-icon-pencil";

            var updateRow = new LinkButton
                {
                    ID = "UpdateButton",
                    CssClass =
                        "updateButton ui-button ui-widget ui-state-default ui-corner-all ui-button-text-icon-primary"
                };
            updateRow.Click += this.updateRow_Click;

            updateRow.Controls.Add(uIcon);
            updateRow.Controls.Add(uInner);

            this.EditControls.Controls.Add(updateRow);
        }

        /// <summary>
        /// Handles the Click event of the editRow control.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        protected void editRow_Click(object sender, EventArgs e)
        {
            this.CurrentRow = int.Parse(((LinkButton)sender).CommandArgument);

            this.EditDataTypes = this.GetEditDataTypes();
            this.GenerateEditControls();

            ScriptManager.RegisterClientScriptBlock(
                this,
                GetType(),
                "OpenEditDialog_" + this.ID,
                "$(function() {$('#" + this.ClientID + "_ctrlEdit').uComponents().datatypegrid('openDialog'); });",
                true);
        }

        /// <summary>
        /// Handles the Click event of the moveRowUp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void moveRowUp_Click(object sender, EventArgs e)
        {
            this.CurrentRow = int.Parse(((LinkButton)sender).CommandArgument);

            for (var i = 0; i < this.Rows.Count; i++)
            {
                if (this.Rows[i].Id == this.CurrentRow)
                {
                    // Reorder the specified row
                    if (i > 0)
                    {
                        this.Rows[i].SortOrder--;

                        // Move conflicting row
                        this.Rows[i - 1].SortOrder++;
                    }
                    else if (i == 0)
                    {
                        this.Rows[i].SortOrder = this.Rows.Count;

                        // Move conflicting row
                        this.Rows[this.Rows.Count - 1].SortOrder = 0;
                    }
                }
            }

            this.Store();
            this.Save();
        }

        /// <summary>
        /// Handles the Click event of the moveRowDown control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void moveRowDown_Click(object sender, EventArgs e)
        {
            this.CurrentRow = int.Parse(((LinkButton)sender).CommandArgument);

            for (var i = 0; i < this.Rows.Count; i++)
            {
                if (this.Rows[i].Id == this.CurrentRow)
                {
                    // Reorder the specified row
                    if (i < this.Rows.Count - 1)
                    {
                        this.Rows[i].SortOrder++;

                        // Move conflicting row
                        this.Rows[i + 1].SortOrder--;
                    }
                    else if (i == this.Rows.Count - 1)
                    {
                        this.Rows[i].SortOrder = 0;

                        // Move conflicting row
                        this.Rows[0].SortOrder++;
                    }
                }
            }

            this.Store();
            this.Save();
        }

        /// <summary>
        /// Handles the Click event of the addRowDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void addRowDialog_Click(object sender, EventArgs e)
        {
            this.ClearControls();

            ScriptManager.RegisterClientScriptBlock(
                this, 
                GetType(), 
                "OpenInsertDialog_" + this.ID,
                "$(function() {$('#" + this.ClientID + "_ctrlInsert').uComponents().datatypegrid('openDialog'); });",
                true);
        }

        /// <summary>
        /// Handles the Click event of the updateRow control.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        protected void updateRow_Click(object sender, EventArgs e)
        {
            foreach (var row in this.Rows.Where(row => row.Id == this.CurrentRow))
            {
                foreach (var cell in row.Cells)
                {
                    // Save value to datatype
                    DataTypeFactoryServiceLocator.Instance.Save(cell.Value, new DataTypeSaveEventArgs(this, DataTypeAction.Update));
                }
            }

            this.Store();
            this.Save();
        }

        /// <summary>
        /// Generates the delete controls.
        /// </summary>
        /// <param name="rowId">
        /// The row Id.
        /// </param>
        private void GenerateDeleteControls(Guid rowId)
        {
        }

        /// <summary>
        /// Handles the Click event of the deleteRow control.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        protected void deleteRow_Click(object sender, EventArgs e)
        {
            var rowToDelete = new StoredValueRow();
            foreach (
                StoredValueRow row in Rows.Where(row => row.Id.ToString().Equals(((LinkButton)sender).CommandArgument)))
            {
                rowToDelete = row;
            }

            Rows.Remove(rowToDelete);

            Store();
            Save();
        }

        /// <summary>
        /// Gets the stored values.
        /// </summary>
        /// <returns></returns>
        private List<StoredValueRow> GetStoredValues()
        {
            var values = new List<StoredValueRow>();

            // Add root element if value is empty
            if (string.IsNullOrEmpty(this.data.Value.ToString()))
            {
                this.data.Value = "<items></items>";
            }

            var doc = new XmlDocument();
            doc.LoadXml(this.data.Value.ToString());

            // Create and add XML declaration. 
            var xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            var root = doc.DocumentElement;
            doc.InsertBefore(xmldecl, root);

            // Get stored values from database
            if (root.ChildNodes.Count > 0)
            {
                foreach (XmlNode container in root.ChildNodes)
                {
                    // <DataTypeGrid>
                    var valueRow = new StoredValueRow();

                    if (container.Attributes["id"] != null)
                    {
                        valueRow.Id = int.Parse(container.Attributes["id"].Value);
                    }

                    if (container.Attributes["sortOrder"] != null)
                    {
                        valueRow.SortOrder = int.Parse(container.Attributes["sortOrder"].Value);
                    }

                    foreach (PreValueRow config in this.ColumnConfigurations)
                    {
                        var value = new StoredValue { Name = config.Name, Alias = config.Alias };

                        var datatypeid = config.DataTypeId;

                        if (datatypeid != 0)
                        {
                            var dtd = DataTypeDefinition.GetDataTypeDefinition(datatypeid);
                            var dt = dtd.DataType;
                            dt.Data.Value = string.Empty;
                            value.Value = dt;

                            foreach (XmlNode node in container.ChildNodes)
                            {
                                if (config.Alias.Equals(node.Name))
                                {
                                    value.Value.Data.Value = node.InnerText;
                                }
                            }

                            valueRow.Cells.Add(value);
                        }
                    }

                    values.Add(valueRow);
                }
            }

            // Set the configuration
            return values;
        }

        /// <summary>
        /// Gets the insert data types.
        /// </summary>
        private List<StoredValue> GetInsertDataTypes()
        {
            var list = new List<StoredValue>();

            foreach (var config in this.ColumnConfigurations)
            {
                var dtd = DataTypeDefinition.GetDataTypeDefinition(config.DataTypeId);
                var dt = dtd.DataType;

                var s = new StoredValue { Name = config.Name, Alias = config.Alias, Value = dt };

                list.Add(s);
            }

            return list;
        }

        /// <summary>
        /// The get edit data types.
        /// </summary>
        /// <returns>
        /// </returns>
        private List<StoredValue> GetEditDataTypes()
        {
            var list = new List<StoredValue>();

            if (this.CurrentRow > 0)
            {
                list = this.GetStoredValueRow(this.CurrentRow).Cells;
            }
            else
            {
                foreach (var config in this.ColumnConfigurations)
                {
                    var dtd = DataTypeDefinition.GetDataTypeDefinition(config.DataTypeId);
                    var dt = dtd.DataType;

                    var s = new StoredValue { Name = config.Name, Alias = config.Alias, Value = dt };

                    list.Add(s);
                }
            }

            return list;
        }

        /// <summary>
        /// The get stored value row.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// </returns>
        private StoredValueRow GetStoredValueRow(int id)
        {
            foreach (var row in this.Rows.Where(row => row.Id == id))
            {
                return row;
            }

            return new StoredValueRow();
        }

        /// <summary>
        /// Gets an available id.
        /// </summary>
        /// <returns>
        /// The get available id.
        /// </returns>
        public int GetAvailableId()
        {
            var newId = 1;

            foreach (StoredValueRow row in Rows)
            {
                if (newId <= row.Id)
                {
                    newId = row.Id + 1;
                }
            }

            return newId;
        }

        /// <summary>
        /// Gets the datatables translation.
        /// </summary>
        /// <returns>The translation.</returns>
        private string GetDataTablesTranslation()
        {
            var translation =
                string.Format(
                    @"<script type=""text/javascript"">$.fn.uComponents().dictionary().dataTablesTranslation = {{""sEmptyTable"":""{0}"",""sInfo"":""{1}"",""sInfoEmpty"":""{2}"",""sInfoFiltered"":""{3}"",""sInfoPostFix"":""{4}"",""sInfoThousands"":""{5}"",""sLengthMenu"":""{6}"",""sLoadingRecords"":""{7}"",""sProcessing"":""{8}"",""sSearch"":""{9}"",""sZeroRecords"":""{10}"",""oPaginate"": {{""sFirst"":""{11}"",""sLast"":""{12}"",""sNext"":""{13}"",""sPrevious"":""{14}""}},""oAria"":{{""sSortAscending"":""{15}"",""sSortDescending"":""{16}""}}}}</script>",
                    Helper.Dictionary.GetDictionaryItem("DataTables.sEmptyTable", "No data available in table"),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sInfo", "Showing _START_ to _END_ of _TOTAL_ entries"),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sInfoEmpty", "Showing 0 to 0 of 0 entries"),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sInfoFiltered", "(filtered from _MAX_ total entries"),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sInfoPostFix", string.Empty),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sInfoThousands", ","),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sLengthMenu", "Show _MENU_ entries"),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sLoadingRecords", "Loading..."),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sProcessing", "Processing..."),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sSearch", "Search:"),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sZeroRecords", "No matching records found"),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sFirst", "First"),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sLast", "Last"),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sNext", "Next"),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sPrevious", "Previous"),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sSortAscending", ": activate to sort column ascending"),
                    Helper.Dictionary.GetDictionaryItem("DataTables.sSortDescending", ": activate to sort column descending"));

            return translation;
        }

        #endregion

        #region Events

        /// <summary>
        /// Initialize the control, make sure children are created
        /// </summary>
        /// <param name="e">
        /// An <see cref="T:System.EventArgs"/> object that contains the event data.
        /// </param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Adds the client dependencies
            this.AddAllDtgClientDependencies();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.EnsureChildControls();

            // DEBUG: Reset stored values
            // this.Data.Value = "<items><item id='1'><name nodeName='Name' nodeType='-88' >Anna</name><age nodeName='Age' nodeType='-51' >25</age><picture nodeName='Picture' nodeType='1035' ></picture></item><item id='6'><name nodeName='Name' nodeType='-88' >Ove</name><gender nodeName='Gender' nodeType='-88'>Male</gender><age nodeName='Age' nodeType='-51' >23</age><picture nodeName='Picture' nodeType='1035' ></picture></item></items>";

            // Set default value if none exists
            if (this.data.Value == null)
            {
                Helper.Log.Debug<DataType>(string.Format("DTG: No values exist in database for this property"));

                this.data.Value = string.Empty;
            }
            else
            {
                Helper.Log.Debug<DataType>(
                    string.Format("DTG: Retrieved the following data from database: {0}", this.data.Value));
            }

            this.ShowGridHeader = new HiddenField() { ID = "ShowGridHeader", Value = this.settings.ShowGridHeader.ToString() };
            this.ShowGridFooter = new HiddenField() { ID = "ShowGridFooter", Value = this.settings.ShowGridFooter.ToString() };
            this.DataTablesTranslation = new LiteralControl() { ID = "DataTablesTranslation", Text = this.GetDataTablesTranslation() };
            this.RowsPerPage = new HiddenField() { ID = "RowsPerPage", Value = this.settings.RowsPerPage.ToString() };
            this.Grid = new Table { ID = "tblGrid", CssClass = "display" };
            this.Toolbar = new Panel { ID = "pnlToolbar", CssClass = "Toolbar" };

            this.ColumnConfigurations = this.prevalueEditorSettingsFactory.GetColumnConfigurations(this.dataTypeDefinitionId);

            // Use data from viewstate if possible
            // TODO: Quality Check! Could create problems for some datatypes
            if (!string.IsNullOrEmpty(this.DataString))
            {
                this.data.Value = this.DataString;
            }

            Rows = this.GetStoredValues();
            InsertDataTypes = GetInsertDataTypes();
            EditDataTypes = GetEditDataTypes();

            InsertControls = new Panel { ID = "ctrlInsert", CssClass = "InsertControls" };
            EditControls = new Panel { ID = "ctrlEdit", CssClass = "EditControls" };
            DeleteControls = new Panel { ID = "ctrlDelete", CssClass = "DeleteControls" };

            // Generate header row
            GenerateHeaderRow();

            // Generate rows with edit, delete and row data
            GenerateValueRows();

            // Generate header row
            GenerateFooterToolbar();

            // Generate insert controls
            GenerateInsertControls();

            // Generate edit controls
            GenerateEditControls();

            this.Controls.Add(this.ShowGridHeader);
            this.Controls.Add(this.ShowGridFooter);
            this.Controls.Add(this.RowsPerPage);
            this.Controls.Add(this.DataTablesTranslation);
            this.Controls.Add(this.Grid);
            this.Controls.Add(this.Toolbar);
            this.Controls.Add(this.InsertControls);
            this.Controls.Add(this.EditControls);
            this.Controls.Add(this.DeleteControls);
        }

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.
        /// </param>
        protected override void Render(HtmlTextWriter writer)
        {
            // Prints the grid
            writer.AddAttribute("id", ClientID);
            writer.AddAttribute("class", "dtg");

            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            this.ShowGridHeader.RenderControl(writer);
            this.ShowGridFooter.RenderControl(writer);
            this.RowsPerPage.RenderControl(writer);
            this.DataTablesTranslation.RenderControl(writer);
            Grid.RenderControl(writer);
            Toolbar.RenderControl(writer);

            // Prints the insert, edit and delete controls);
            InsertControls.RenderControl(writer);
            EditControls.RenderControl(writer);
            DeleteControls.RenderControl(writer);

            writer.RenderEndTag();
        }

        #endregion
    }
}
