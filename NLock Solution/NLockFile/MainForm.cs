using log4net;
using log4net.Config;
using NLock.NLockFile;
using NLock.NLockFile.Exceptions;
using NLock.NLockFile.Operations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace NLock
{
    public partial class MainForm : Form
    {
        #region Private variables

        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainForm));
        private NLockContainerCommons _nlockContainer;
        private string _fileName;
        private readonly OperationModes _opMode;
        private string _oldFolderSelectedPath;
        private Status _status;
        private NLockTemplateOperations _nLockTemplateOperations;
        private bool _isDirty;
        private bool _saved;
        private int _sortColumn = -1;

        #endregion Private variables

        #region Public constructors

        public MainForm(OperationModes mode, String file = null)
        {
            XmlConfigurator.Configure();
            _opMode = mode;
            _fileName = file;
            InitializeComponent();
        }

        #endregion Public constructors

        #region Private Methods

        private void InitializeForm()
        {
            this.Text = @"NLock";
            tsbLock.Enabled = false;
            tsbExtract.Enabled = false;
            saveFileDialog.FileName = String.Empty;
            saveFileDialog.InitialDirectory = String.Empty;
            openFileDialog.Multiselect = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.Title = "Add file to NLock";
            openFileDialog.RestoreDirectory = true;
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.Description = "Experimental feature..\n" +
            "Duplicate file names are automatically renamed";
            _nLockTemplateOperations = new NLockTemplateOperationsAES();
            toolStripFileCountLabel.Text = String.Empty;
            tssStatus.Text = String.Empty;
        }

        private void UpdateListView(List<NlFile> list)
        {
            if (list == null)
            {
                tsbLock.Enabled = false;
                toolStripFileCountLabel.Text = FilelistView.Items.Count.ToString();
                return;
            }
            if (list.Count <= 0)
            {
                tsbLock.Enabled = false;
                FilelistView.Items.Clear();
                toolStripFileCountLabel.Text = FilelistView.Items.Count.ToString();
                return;
            }
            FilelistView.BeginUpdate();

            try
            {
                FilelistView.Items.Clear();

                //TODO: add image icon to file list
                foreach (var key in list)
                {
                    var item = new ListViewItem(Path.GetFileName(key.FileName));
                    string type = " B";
                    double original = key.OriginalSize;
                    double compressed = key.CompressedSize;
                    if (key.OriginalSize > 1024)
                    {
                        original = key.OriginalSize / 1024.0;
                        compressed = key.CompressedSize / 1024.0;
                        type = "KiB";
                    }

                    if (key.OriginalSize > 1024 * 1024)
                    {
                        original = original / 1024;
                        compressed = compressed / 1024;
                        type = "MiB";
                    }

                    if (key.OriginalSize > 1024 * 1024 * 1024)
                    {
                        original = original / 1024;
                        compressed = compressed / 1024;
                        type = "GiB";
                    }

                    item.SubItems.Add(String.Format("{0:N2} {1}", original, type));
                    item.SubItems.Add(String.Format("{0:N2} {1}", compressed, type));

                    item.SubItems.Add(key.FilePath != null ? key.FilePath + "\\" : "\\");
                    FilelistView.Items.Add(item);

                    tsbLock.Enabled = FilelistView.Items.Count > 0;
                }
            }
            finally
            {
                FilelistView.EndUpdate();
                toolStripFileCountLabel.Text = FilelistView.Items.Count.ToString();
            }
        }

        private void Extract()
        {
            _status = Status.EXTRACTIONFAILED;
            try
            {
                var result = folderBrowserDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var extractionFolderPath = String.Concat(folderBrowserDialog.SelectedPath, "\\");
                    if (_nlockContainer.ExtractToFolder(extractionFolderPath))
                    {
                        _status = Status.SUCESSFULLYEXTRACTED;
                        //_nlockContainer.Dispose();
                        //_nlockContainer = null;
                    }
                    _oldFolderSelectedPath = extractionFolderPath;
                }
                else // result == DialogResult.Cancel
                {
                    _status = Status.EXTRACTIONCANCELLED;
                }
                folderBrowserDialog.SelectedPath = _oldFolderSelectedPath;
            }
            catch (Exception)
            {
                _status = Status.EXTRACTIONFAILED;
            }
        }

        #endregion Private Methods

        #region Private Form Events

        private void MainFormLoad(object sender, EventArgs e)
        {
            InitializeForm();
            Logger.Debug("---------------(----------- Starting NLock -----------)-----------------------");

            _nlockContainer = new NLockContainerCommons();

            if (_opMode == OperationModes.OPENNLOCK)
            {
                Logger.Debug("Open NLock Operation");
                if (_fileName != null)
                {
                    try
                    {
                        this.Text = "NLock " + _fileName;
                        _nlockContainer.LoadFromFile(_fileName);
                        UpdateListView(_nlockContainer.GetFileList());
                        tsbLock.Enabled = true;
                        tsbExtract.Enabled = true;
                        this.Focus();
                    }
                    catch (InvalidOperationException)
                    {
                        Close();
                    }
                    catch (NLockUnidentifiedUserException)
                    {
                        Close();
                    }
                }
                else
                {
                    MessageBox.Show("Invalid File!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Logger.Info("OperationModes.OPENNLOCK Null File Name");
                    Close();
                }
            }
            else if (_opMode == OperationModes.UNLOCKHERE)
            {
                Logger.Debug("Unlock here Operation");
                if (_fileName != null)
                {
                    try
                    {
                        _nlockContainer.LoadFromFile(_fileName);

                        var currentPath = String.Concat(Path.GetDirectoryName(_fileName), "\\");
                        DialogResult result;
                        if (_nlockContainer.ExtractToFolder(currentPath))
                        {
                            using (var form = new Form { WindowState = FormWindowState.Maximized, TopMost = true })
                            {
                                result = MessageBox.Show(form, "Successfully extracted files to : " + currentPath + "\n Exit ?", "Unlocking", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            using (var form = new Form { WindowState = FormWindowState.Maximized, TopMost = true })
                            {
                                result = MessageBox.Show(form, "Failed to extract files to : " + currentPath + "\n Exit ?", "Unlocking Failed", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                            }
                        }

                        if (result == DialogResult.Yes)
                        {
                            Close();
                        }
                        else
                        {
                            this.Focus();
                            tsbExtract.Enabled = true;
                            folderBrowserDialog.SelectedPath = currentPath;
                            UpdateListView(_nlockContainer.GetFileList());
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        MessageBox.Show("Invalid User!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }
                else
                {
                    Logger.Info("OperationModes.UNLOCKHERE Null File Name");
                }
            }
            else if (_opMode == OperationModes.UNLOCKTO)
            {
                Logger.Debug("Unlock to Operation");
                if (_fileName != null)
                {
                    try
                    {
                        _nlockContainer.LoadFromFile(_fileName);
                        tsbExtract.Enabled = true;
                        var unlockingPath = String.Concat(Path.GetDirectoryName(_fileName), "\\");
                        folderBrowserDialog.SelectedPath = unlockingPath;
                        Extract();

                        var result = DialogResult.None;

                        if (_status == Status.SUCESSFULLYEXTRACTED)
                        {
                            tsbExtract.Enabled = false;
                            using (var form = new Form { WindowState = FormWindowState.Maximized, TopMost = true })
                            {
                                result = MessageBox.Show(form, "Successfully extracted files to : " + unlockingPath + "\n Exit ?", "Unlocking", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            }
                        }
                        else if (_status == Status.EXTRACTIONFAILED)
                        {
                            using (var form = new Form { WindowState = FormWindowState.Maximized, TopMost = true })
                            {
                                result = MessageBox.Show(form, "Failed " + "\n Exit ?", "Unlocking", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                            }
                        }
                        else  // status == Status.EXTRACTIONCANCELLED)
                        {
                            Close();
                        }

                        if (result == DialogResult.Yes)
                        {
                            Close();
                        }
                        else // result == DialogResult.No
                        {
                            this.Focus();
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        using (var form = new Form { WindowState = FormWindowState.Maximized, TopMost = true })
                        {
                            MessageBox.Show(form, "Invalid User!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        }
                    }
                }
                else
                {
                    Logger.Info("OperationModes.UNLOCKTO Null File Name");
                }
            }
            else if (_opMode == OperationModes.LOCKFOLDER)
            {
                try
                {
                    Logger.Debug("Lock folder Operation");
                    if (_fileName != null)
                    {
                        Logger.Debug("File name : " + _fileName);
                        var copiedName = _fileName;
                        _nlockContainer.AddDirectory(_fileName, true);
                        UpdateListView(_nlockContainer.GetFileList());
                        tsbLock.Enabled = true;
                        saveFileDialog.InitialDirectory = Path.GetDirectoryName(_fileName);
                        Logger.Debug("Perform button lock");
                        tsbLock.PerformClick();

                        if (_saved)
                        {
                            using (var form = new Form { WindowState = FormWindowState.Maximized, TopMost = true })
                            {
                                var result = MessageBox.Show(form, "Successfully locked files to : " + copiedName + "\n Exit ?", "Successful", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                                if (result == DialogResult.Yes)
                                {
                                    Close();
                                }
                            }
                        }
                        else
                        {
                            Close();
                        }
                    }
                    else
                    {
                        Logger.Info("OperationModes.LOCKFOLDER Null File Name");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("No filed added, folder is empty...", "Failed..", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else // _opMode == OperationModes.NLOCKTHISFILE
            {
                Logger.Debug(" Lock NLock this file Operation");
                if (_fileName != null)
                {
                    Logger.Debug("File name : " + _fileName);
                    _nlockContainer.AddFile(_fileName);
                    UpdateListView(_nlockContainer.GetFileList());

                    Logger.Debug("Perform button lock");
                    tsbLock.Enabled = true;
                    tsbLock.PerformClick();

                    using (var form = new Form { WindowState = FormWindowState.Maximized, TopMost = true })
                    {
                        var result = MessageBox.Show(form, "Do you want to exit NLock\n Exit ?", "Locking", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        saveFileDialog.InitialDirectory = Path.GetDirectoryName(_fileName);

                        if (result == DialogResult.Yes)
                        {
                            Close();
                        }
                        else // result == DialogResult.No
                        {
                            InitializeForm();
                        }
                    }
                }
                else
                {
                    Logger.Info("OperationModes.NLOCKTHISFILE Null File Name");
                }
            }
        }

        public void AddThisFiletoNLock(string fileName)
        {
            _nlockContainer.AddFile(fileName);
            UpdateListView(_nlockContainer.GetFileList());
        }

        private void DragAdd(List<string> Files)
        {
            foreach (var file in Files)
            {
                Logger.Debug("File dragged : " + file);
                _nlockContainer.AddFile(file);
                UpdateListView(_nlockContainer.GetFileList());
            }
        }

        private void FilelistViewDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void FilelistViewDragDrop(object sender, DragEventArgs e)
        {
            if (_nlockContainer == null)
            {
                _nlockContainer = new NLockContainerCommons();
            }
            var filepaths = new List<string>();
            if (_nlockContainer == null)
            {
                _nlockContainer = new NLockContainerCommons();
            }
            foreach (var s in (string[]) e.Data.GetData(DataFormats.FileDrop, false))
            {
                if (Directory.Exists(s))
                {
                    _nlockContainer.AddDirectory(s, true);
                    UpdateListView(_nlockContainer.GetFileList());
                }
                else
                {
                    filepaths.Add(s);
                }
            }

            DragAdd(filepaths);
        }

        private void MainFormClosed(object sender, FormClosedEventArgs e)
        {
            if (_nlockContainer != null)
            {
                _nlockContainer.Dispose();
            }
            _nlockContainer = null;
        }

        private void ToolstripButtonOpenClick(object sender, EventArgs e)
        {
            tssStatus.ForeColor = System.Drawing.Color.Blue;
            tssStatus.Text = "Opening...";
            try
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FilelistView.Items.Clear();
                    this.Text = @"NLock";
                    toolStripFileCountLabel.Text = String.Empty;
                    tssStatus.ForeColor = System.Drawing.Color.Blue;
                    tssStatus.Text = "Unlocking...";
                    if (NLockTemplateOperations.IsNLock(openFileDialog.FileName) > 0)
                    {
                        if (_nlockContainer != null)
                        {
                            _nlockContainer.Dispose();
                        }
                        _nlockContainer = new NLockContainerCommons();
                        _isDirty = false;
                        _fileName = openFileDialog.FileName;

                        _nlockContainer.LoadFromFile(_fileName);
                        if (!_nlockContainer.IsLocked)
                        {
                            this.Text = "NLock " + _fileName;
                            UpdateListView(_nlockContainer.GetFileList());

                            tsbExtract.Enabled = true;

                            tssStatus.ForeColor = System.Drawing.Color.Green;
                            tssStatus.Text = "Unlocked.. User identified...!";
                        }
                        else
                        {
                            _fileName = null;
                        }
                    }
                    else
                    {
                        MessageBox.Show("File is corrupted or not a valid NLock file : " + Path.GetFileName(openFileDialog.FileName),
                            "File opening failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        tssStatus.ForeColor = System.Drawing.Color.Red;
                        tssStatus.Text = "Unlocking... Failed";
                    }
                }
                else
                {
                    tssStatus.ForeColor = System.Drawing.Color.Blue;
                    tssStatus.Text = "Canceled...";
                }
            }
            catch (NLockUnidentifiedUserException)
            {
                _fileName = null;
                tssStatus.ForeColor = System.Drawing.Color.Red;
                tssStatus.Text = "Unlocking... Failed";
                Logger.Info("Unlocking... Failed");
            }
            catch (Exception)
            {
                _fileName = null;
                tssStatus.ForeColor = System.Drawing.Color.Red;
                tssStatus.Text = "Unlocking... Failed";
                Logger.Info("Unlocking... Failed");
            }
        }

        private void ToolstripButtonAddFileClick(object sender, EventArgs e)
        {
            tssStatus.Text = String.Empty;

            if (this.Text != "NLock")
            {
                this.Text = "NLock";
            }
            if (_nlockContainer == null)
            {
                _nlockContainer = new NLockContainerCommons();
            }

            try
            {
                Logger.Debug("Add");
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in openFileDialog.FileNames)
                    {
                        _nlockContainer.AddFile(file);
                    }
                    tsbLock.Enabled = true;
                    if (_fileName != null)
                    {
                        _isDirty = true;
                    }
                    UpdateListView(_nlockContainer.GetFileList());
                }
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("Out of memory", "Not enough memory", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Error("OutOfMemoryException");
            }
        }

        private void ToolstrioButtonAddFolderClick(object sender, EventArgs e)
        {
            //TODO: Validate file names or foldernames to not to have "######s"
            Logger.Debug("btnAddFolder_Click");
            try
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    Logger.Debug("Add folder selected path : " + folderBrowserDialog.SelectedPath);

                    _nlockContainer.AddDirectory(folderBrowserDialog.SelectedPath, true);
                    UpdateListView(_nlockContainer.GetFileList());
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("No filed added, folder is empty...", "Failed..", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Error("No filed added, folder is empty...");
            }
        }

        private void ToolstrioButtonLockClick(object sender, EventArgs e)
        {
            tssStatus.ForeColor = System.Drawing.Color.Blue;
            tssStatus.Text = "Locking...";

            try
            {
                if (FilelistView.Items.Count > 0)
                {
                    using (LockForm lockForm = new LockForm())
                    {
                        lockForm.ShowDialog();
                        var result = lockForm.DialogResult;
                        if (result == DialogResult.OK)
                        {
                            _nlockContainer.Template = lockForm._templateLoginForm;

                            if (lockForm.AddPassword)
                            {
                                _nlockContainer.TemplateOperations = new NLockTemplateOperationsCommon();
                                _nlockContainer.Password = lockForm.Password;
                                _nlockContainer.AddPassword = true;
                            }
                            else
                            {
                                _nlockContainer.AddPassword = false;
                                _nlockContainer.Password = null;
                            }

                            _nlockContainer.Lock();
                            if (!lockForm.SaveFileName.EndsWith(".nlk"))
                            {
                                lockForm.SaveFileName = lockForm.SaveFileName + ".nlk";
                            }
                            _nlockContainer.Save(lockForm.SaveFileName);
                            FilelistView.Items.Clear();
                            tsbLock.Enabled = false;
                            tsbExtract.Enabled = false;
                            tssStatus.ForeColor = System.Drawing.Color.Green;
                            tssStatus.Text = "Locking Successful...";
                            _saved = true;
                            this.Text = "NLock";
                            _fileName = null;
                            _isDirty = false;
                            toolStripFileCountLabel.Text = String.Empty;

                            _nlockContainer = null;
                        }
                        else // result == DialogResult.Cancel
                        {
                            tssStatus.ForeColor = System.Drawing.Color.Red;
                            tssStatus.Text = "Locking Canceled...";
                        }
                    }
                }
            }
            catch (NLockFileContainerEmptyException)
            {
                MessageBox.Show("Failed... \nNo files added. Empty container", "Locking", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Error("Failed...No files added. Empty container");
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Failed... \nBad image captured", "Locking", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Error("Failed...Bad image captured");
            }
        }

        private void ToolstripButtonUnlockCick(object sender, EventArgs e)
        {
            Extract();
            if (_status == Status.SUCESSFULLYEXTRACTED)
            {
                tsbExtract.Enabled = false;
                MessageBox.Show("Successful...", "Extraction is", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (_status == Status.EXTRACTIONFAILED)
            {
                MessageBox.Show("Failed...", "Extraction", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else // status == Status.EXTRACTIONCANCELLED
            {
                tssStatus.ForeColor = System.Drawing.Color.Blue;
                tssStatus.Text = "Canceled...";
            }
        }

        private void ToolstripButtonRemoveItemClick(object sender, EventArgs e)
        {
            try
            {
                foreach (var file in FilelistView.SelectedItems)
                {
                    var item = (ListViewItem) file;
                    var tet = item.SubItems[0].Text;
                    var result = _nlockContainer.RemoveFile(tet);
                    if ((_fileName != null) && !_isDirty && result)
                    {
                        _isDirty = true;
                    }
                }
                if (FilelistView.Items.Count > 0)
                {
                    UpdateListView(_nlockContainer.GetFileList());
                }
                else
                {
                    toolStripFileCountLabel.Text = "0";
                    _nlockContainer = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred\n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isDirty && (_fileName != null))
            {
                var result = MessageBox.Show("Lock file..", "File is changed", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    e.Cancel = true;
                    tsbLock.PerformClick();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }

        private void ToolstripButtonAboutClick(object sender, EventArgs e)
        {
            using (AboutForm aboutfrom = new AboutForm())
            {
                aboutfrom.ShowDialog();
            }
        }

        private void ToolstripButtonPreferencesClick(object sender, EventArgs e)
        {
            using (PreferencesForm pref = new PreferencesForm())
            {
                pref.ShowDialog();
            }
        }

        /// <summary>
        ///         ref : https://msdn.microsoft.com/en-us/library/ms996467.aspx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilelistViewColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column != _sortColumn)
            {
                // Set the sort column to the new column.
                _sortColumn = e.Column;
                // Set the sort order to ascending by default.
                FilelistView.Sorting = SortOrder.Ascending;
            }
            else
            {
                FilelistView.Sorting = FilelistView.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }

            FilelistView.Sort();
            this.FilelistView.ListViewItemSorter = new ListViewItemComparer(e.Column,
                                                              FilelistView.Sorting);
        }

        #endregion Private Form Events
    }

    internal class ListViewItemComparer : System.Collections.IComparer
    {
        private int col;
        private SortOrder order;

        public ListViewItemComparer()
        {
            col = 0;
            order = SortOrder.Ascending;
        }

        public ListViewItemComparer(int column, SortOrder order)
        {
            col = column;
            this.order = order;
        }

        public int Compare(object x, object y)
        {
            int returnVal = -1;
            double xVal;
            double yVal;

            if (col == 1 || col == 2)
            {
                string test = ((ListViewItem) x).SubItems[col].Text.Split()[0];
                bool xY = double.TryParse(((ListViewItem) x).SubItems[col].Text.Split()[0], out xVal);
                bool yY = double.TryParse(((ListViewItem) y).SubItems[col].Text.Split()[0], out yVal);

                if (xY && yY)
                {
                    if (xVal - yVal > Double.Epsilon)
                    {
                        returnVal = 1;
                    }
                    else if (xVal - yVal < 0)
                    {
                        returnVal = -1;
                    }
                    else
                    {
                        returnVal = 0;
                    }
                }
            }
            else
            {
                returnVal = String.Compare(((ListViewItem) x).SubItems[col].Text,
                             ((ListViewItem) y).SubItems[col].Text);
            }

            // Determine whether the sort order is descending.
            if (order == SortOrder.Descending)
            {
                // Invert the value returned by String.Compare.
                returnVal *= -1;
            }
            return returnVal;
        }
    }
}