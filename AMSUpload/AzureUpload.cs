using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace AMSUpload
{
	public class AzureUpload : Form
	{
		private string MSSQLConnString = "Server=ex6vaxxh6a.database.windows.net;Database=robomateplus;User Id=roboadmin@ex6vaxxh6a;Password=SQL#2016;";

		private string strBase;

		private string processName = "";

		private string CurrentCourse = "";

		private string FilePath = "";

		private string Message = "";

		private bool isSuccess = false;

		private string strFileList = "";

		private string FailureList = "";

		private string SuccessList = "";

		private IContainer components = null;

		private TableLayoutPanel tableLayoutPanel1;

		private TabControl tabControl1;

		private TabPage tabCList;

		private TabPage tabPendingUpload;

		private Panel panel1;

		public Label label1;

		private ComboBox cbCourseList;

		private Button btnProcessData;

		private DataGridView dgContentList;

		private TabPage tabPendingEncode;

		private Panel panel2;

		private Button btnEncode;

		private TextBox txtResult;

		private DataGridView dgPendingUpload;

		private DataGridView dgPendingEncode;

		private TextBox txtFolder;

		private Button btnSelect;

		private FolderBrowserDialog folderBrowserDialog1;

		private Button btnUpload;

		private BackgroundWorker AMSBGProcess;

		private Panel panel3;

		private TextBox txtFailure;

		private ProgressBar progressBar1;

		private Panel panel4;

		private Label lblProgress;

		private TabPage tabUpdate;

		private Button btnUpdate;

		private TableLayoutPanel tableLayoutPanel2;

		private TextBox txtFiles;

		private TextBox txtUploadList;

		public AzureUpload()
		{
			InitializeComponent();
		}

		private void AzureUpload_Load(object sender, EventArgs e)
		{
			PopulateCourseList();
		}

		private void PopulateCourseList()
		{
			string cmdText = "select distinct  CourseId, cm.CourseCode, Concat(cm.CourseCode, ' :: ', CourseName) 'CName' from CourseMaster cm where cm.isdeleted = 0 and cm.IsActive = 1 order by CourseId";
			using SqlConnection sqlConnection = new SqlConnection(MSSQLConnString);
			DataSet dataSet = new DataSet();
			SqlCommand selectCommand = new SqlCommand(cmdText, sqlConnection);
			sqlConnection.Open();
			SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
			sqlDataAdapter.SelectCommand = selectCommand;
			sqlDataAdapter.Fill(dataSet);
			if (sqlConnection.State == ConnectionState.Open)
			{
				sqlConnection.Close();
			}
			if (dataSet == null || dataSet.Tables.Count <= 0)
			{
				return;
			}
			foreach (DataRow row in dataSet.Tables[0].Rows)
			{
				cbCourseList.DataSource = dataSet.Tables[0];
				cbCourseList.DisplayMember = "CName";
				cbCourseList.ValueMember = "CourseCode";
			}
		}

		private void btnProcessData_Click(object sender, EventArgs e)
		{
			using robomateplusEntities robomateplusEntities2 = new robomateplusEntities();
			List<AzureUploadTracking> list = robomateplusEntities2.AzureUploadTracking.Where((AzureUploadTracking _c) => _c.CourseCode == cbCourseList.SelectedValue.ToString()).ToList();
			if (list == null || list.Count() == 0)
			{
				InitializeCourseContent();
			}
			else
			{
				UpdateCourseContent();
			}
			list = (from _c in robomateplusEntities2.AzureUploadTracking
					where _c.CourseCode == cbCourseList.SelectedValue.ToString()
					where _c.IsDeleted == (bool?)false
					select _c).ToList();
			dgContentList.DataSource = list;
			tabControl1.SelectedIndex = 0;
			MessageBox.Show("Processing Done");
		}

		private void UpdateCourseContent()
		{
			robomateplusEntities f = new robomateplusEntities();
			try
			{
				List<ProductContentMaster> list = (from _s in f.ProductContentMaster
												   from _d in f.AzureUploadTracking.Where((AzureUploadTracking mapping) => _s.ProductContentCode == mapping.ProductContentCode).DefaultIfEmpty()
												   where _s.CourseCode == cbCourseList.SelectedValue.ToString() && _s.ContentTypeCode == "01" && _s.IsDeleted == false && _s.IsActive == true && _d.ProductContentCode == null
												   select _s).ToList();
				foreach (ProductContentMaster item in list)
				{
					string productContentFileUrl = item.ProductContentFileUrl;
					if (!string.IsNullOrEmpty(productContentFileUrl))
					{
						AzureUploadTracking azureUploadTracking = new AzureUploadTracking
						{
							CourseCode = cbCourseList.SelectedValue.ToString(),
							FileUrl = productContentFileUrl,
							ProductContentCode = item.ProductContentCode,
							EncodeCompleted = false,
							EncodeStarted = false,
							EncSetupCompleted = false,
							IsDeleted = false,
							Mp4Uploaded = false
						};
						IAsset assetDetails = AMSOperation.GetAssetDetails(productContentFileUrl.Replace("\\", "/"));
						if (assetDetails != null)
						{
							azureUploadTracking.Mp4Uploaded = true;
							azureUploadTracking.UploadedOn = assetDetails.Created;
						}
						f.AzureUploadTracking.Add(azureUploadTracking);
						f.SaveChanges();
					}
				}
				List<AzureUploadTracking> list2 = f.AzureUploadTracking.Where((AzureUploadTracking _s) => _s.CourseCode == cbCourseList.SelectedValue.ToString() && _s.ProductContentCode != null && _s.Mp4Uploaded == (bool?)false).ToList();
				foreach (AzureUploadTracking item2 in list2)
				{
					if (!string.IsNullOrEmpty(item2.FileUrl))
					{
						IAsset assetDetails2 = AMSOperation.GetAssetDetails(item2.FileUrl.Replace("\\", "/"));
						if (assetDetails2 != null)
						{
							item2.Mp4Uploaded = true;
							item2.UploadedOn = assetDetails2.Created;
							f.Entry(item2).State = EntityState.Modified;
							f.SaveChanges();
						}
					}
				}
				list2 = (from _s in f.AzureUploadTracking
						 from _d in f.ProductContentMaster.Where((ProductContentMaster mapping) => _s.ProductContentCode == mapping.ProductContentCode && mapping.IsActive == true && mapping.IsDeleted == false).DefaultIfEmpty()
						 where _s.CourseCode == cbCourseList.SelectedValue.ToString() && _s.IsDeleted == (bool?)false && _d.ProductContentCode == null
						 select _s).ToList();
				foreach (AzureUploadTracking item3 in list2)
				{
					item3.IsDeleted = true;
					f.Entry(item3).State = EntityState.Modified;
					f.SaveChanges();
				}
			}
			finally
			{
				if (f != null)
				{
					((IDisposable)f).Dispose();
				}
			}
		}

		private void InitializeCourseContent()
		{
			try
			{
				using robomateplusEntities robomateplusEntities2 = new robomateplusEntities();
				List<ProductContentMaster> list = robomateplusEntities2.ProductContentMaster.Where((ProductContentMaster _c) => _c.CourseCode == cbCourseList.SelectedValue.ToString() && _c.ContentTypeCode == "01" && _c.IsActive == true && _c.IsDeleted == false).ToList();
				if (list != null)
				{
					foreach (ProductContentMaster item in list)
					{
						string productContentFileUrl = item.ProductContentFileUrl;
						if (string.IsNullOrEmpty(productContentFileUrl))
						{
							continue;
						}
						AzureUploadTracking azureUploadTracking = new AzureUploadTracking
						{
							CourseCode = cbCourseList.SelectedValue.ToString(),
							FileUrl = productContentFileUrl,
							ProductContentCode = item.ProductContentCode,
							EncodeCompleted = false,
							EncodeStarted = false,
							EncSetupCompleted = false,
							IsDeleted = false
						};
						IAsset assetDetails = AMSOperation.GetAssetDetails(productContentFileUrl.Replace("\\", "/"));
						if (assetDetails == null)
						{
							azureUploadTracking.Mp4Uploaded = false;
						}
						else
						{
							azureUploadTracking.Mp4Uploaded = true;
							azureUploadTracking.UploadedOn = assetDetails.Created;
							assetDetails = AMSOperation.GetAssetDetails(item.ProductContentCode);
							if (assetDetails != null && azureUploadTracking.UploadedOn < assetDetails.Created)
							{
								azureUploadTracking.EncodeCompleted = true;
								azureUploadTracking.EncodedOn = assetDetails.Created;
							}
						}
						robomateplusEntities2.AzureUploadTracking.Add(azureUploadTracking);
					}
				}
				robomateplusEntities2.SaveChanges();
			}
			catch (DbEntityValidationException ex)
			{
				string text = "";
				foreach (DbEntityValidationResult entityValidationError in ex.EntityValidationErrors)
				{
					text = string.Concat(text, "Entity of type ", entityValidationError.Entry.Entity.GetType().Name, " in state ", entityValidationError.Entry.State, " has the following validation errors:\r\n");
					foreach (DbValidationError validationError in entityValidationError.ValidationErrors)
					{
						text = text + "- Property: " + validationError.PropertyName + ", Error: " + validationError.ErrorMessage + "\r\n";
					}
				}
				MessageBox.Show(text);
			}
			catch (Exception)
			{
			}
		}

		private void BtnEncode_Click(object sender, EventArgs e)
		{
			if (!AMSBGProcess.IsBusy)
			{
				processName = "Encode";
				CurrentCourse = cbCourseList.SelectedValue.ToString();
				AMSBGProcess.RunWorkerAsync();
			}
			else
			{
				MessageBox.Show("A process '" + processName + "' is running");
			}
		}

		private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
		{
			using robomateplusEntities robomateplusEntities2 = new robomateplusEntities();
			btnEncode.Visible = false;
			btnUpload.Visible = false;
			btnUpdate.Visible = false;
			Point point2 = (btnUpload.Location = (btnUpdate.Location = btnEncode.Location));
			TabPage selectedTab = (sender as TabControl).SelectedTab;
			if (selectedTab == tabControl1.TabPages["tabCList"])
			{
				List<AzureUploadTracking> dataSource = (from _c in robomateplusEntities2.AzureUploadTracking
														where _c.CourseCode == cbCourseList.SelectedValue.ToString()
														where _c.IsDeleted == (bool?)false
														select _c).ToList();
				dgContentList.DataSource = dataSource;
			}
			if (selectedTab == tabControl1.TabPages["tabPendingUpload"])
			{
				List<AzureUploadTracking> dataSource2 = (from _c in robomateplusEntities2.AzureUploadTracking
														 where _c.CourseCode == cbCourseList.SelectedValue.ToString()
														 where _c.IsDeleted == (bool?)false && _c.Mp4Uploaded == (bool?)false
														 select _c).ToList();
				dgPendingUpload.DataSource = dataSource2;
				btnUpload.Visible = true;
			}
			if (selectedTab == tabControl1.TabPages["tabPendingEncode"])
			{
				List<AzureUploadTracking> dataSource3 = (from _c in robomateplusEntities2.AzureUploadTracking
														 where _c.CourseCode == cbCourseList.SelectedValue.ToString()
														 where _c.IsDeleted == (bool?)false && _c.Mp4Uploaded == (bool?)true && _c.EncodeCompleted == (bool?)false
														 select _c).ToList();
				dgPendingEncode.DataSource = dataSource3;
				btnEncode.Visible = true;
			}
			if (selectedTab == tabControl1.TabPages["tabUpdate"])
			{
				btnUpdate.Visible = true;
			}
		}

		private void btnSelect_Click(object sender, EventArgs e)
		{
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				txtFolder.Text = folderBrowserDialog1.SelectedPath;
				strBase = txtFolder.Text.Substring(0, txtFolder.Text.LastIndexOf("\\") + 1);
			}
		}

		private void btnUpload_Click(object sender, EventArgs e)
		{
			if (!AMSBGProcess.IsBusy)
			{
				processName = "Upload";
				CurrentCourse = cbCourseList.SelectedValue.ToString();
				AMSBGProcess.RunWorkerAsync();
			}
			else
			{
				MessageBox.Show("A process '" + processName + "' is running");
			}
		}

		private void AMSBGProcess_DoWork(object sender, DoWorkEventArgs e)
		{
			if (string.IsNullOrEmpty(strBase))
			{
				MessageBox.Show("Please select the base folder.");
				return;
			}
			string text = strBase.Replace("\\", "/");
			BackgroundWorker backgroundWorker = sender as BackgroundWorker;
			if (processName != "Update")
			{
				using robomateplusEntities robomateplusEntities2 = new robomateplusEntities();
				List<AzureUploadTracking> list = ((processName == "Encode") ? robomateplusEntities2.AzureUploadTracking.Where((AzureUploadTracking _c) => _c.CourseCode == CurrentCourse && _c.IsDeleted == (bool?)false && _c.Mp4Uploaded == (bool?)true && _c.EncodeCompleted == (bool?)false).Take(5000).ToList() : robomateplusEntities2.AzureUploadTracking.Where((AzureUploadTracking _c) => _c.CourseCode == CurrentCourse && _c.IsDeleted == (bool?)false && _c.Mp4Uploaded == (bool?)false).Take(5000).ToList());
				DateTime now = DateTime.Now;
				if (list != null && list.Count > 0)
				{
					int count = list.Count;
					int num = 0;
					foreach (AzureUploadTracking item in list)
					{
						FilePath = item.FileUrl;
						Message = "";
						isSuccess = false;
						if (File.Exists(text + FilePath) && Path.GetExtension(text + FilePath).ToLower() == ".mp4")
						{
							AMSOperation.isSuccess = false;
							if (processName == "Upload")
							{
								Message = AMSOperation.DoUpload(FilePath.Replace("\\", "/"), text + FilePath);
								if (AMSOperation.isSuccess)
								{
									item.Mp4Uploaded = true;
									item.UploadedOn = DateTime.Now;
									robomateplusEntities2.Entry(item).State = EntityState.Modified;
									robomateplusEntities2.SaveChanges();
								}
							}
							AMSOperation.isSuccess = false;
							Message = AMSOperation.DoEncoding(FilePath.Replace("\\", "/"), item.ProductContentCode, text + FilePath);
							if (AMSOperation.isSuccess)
							{
								item.EncodeCompleted = true;
								item.EncodedOn = DateTime.Now;
								robomateplusEntities2.Entry(item).State = EntityState.Modified;
								robomateplusEntities2.SaveChanges();
								isSuccess = true;
								Message = "Done";
							}
							else
							{
								FailureList = FailureList + FilePath + " :: " + Message + "\r\n";
								isSuccess = false;
							}
						}
						else
						{
							Message = "Physical File not Found";
							FailureList = FailureList + FilePath + " :: " + Message + "\r\n";
							isSuccess = false;
						}
						num++;
						backgroundWorker.ReportProgress(num * 100 / count);
						DateTime now2 = DateTime.Now;
						int num2 = (int)(now2 - now).TotalSeconds;
						int num3 = num2 / 60;
						int num4 = num3 / 60;
						int num5 = num2 - num3 * 60;
						num3 -= num4 * 60;
						num2 = num2 * (count - num) / num;
						num3 = num2 / 60;
						num4 = num3 / 60;
						num5 = num2 - num3 * 60;
						num3 -= num4 * 60;
					}
				}
			}
			if (!(processName == "Update"))
			{
				return;
			}
			string[] array = Regex.Split(txtFiles.Text, "\r\n");
			string text2 = "";
			DateTime now3 = DateTime.Now;
			using robomateplusEntities robomateplusEntities3 = new robomateplusEntities();
			for (int i = 0; i < array.Length; i++)
			{
				int num6 = array.Length;
				int num7 = 0;
				string path = array[i];
				if (File.Exists(text + path) && Path.GetExtension(text + path).ToLower() == ".mp4")
				{
					AzureUploadTracking azureUploadTracking = robomateplusEntities3.AzureUploadTracking.Where((AzureUploadTracking _c) => _c.CourseCode == CurrentCourse && _c.IsDeleted == (bool?)false && _c.FileUrl == path.Replace(" ", "_").Replace("/", "\\")).FirstOrDefault();
					if (azureUploadTracking != null)
					{
						AMSOperation.isSuccess = false;
						Message = AMSOperation.DoUpload(path.Replace("\\", "/"), text + path);
						if (AMSOperation.isSuccess)
						{
							azureUploadTracking.Mp4Uploaded = true;
							azureUploadTracking.UploadedOn = DateTime.Now;
							robomateplusEntities3.Entry(azureUploadTracking).State = EntityState.Modified;
							robomateplusEntities3.SaveChanges();
						}
					}
					AMSOperation.isSuccess = false;
					Message = AMSOperation.DoEncoding(path.Replace("\\", "/"), azureUploadTracking.ProductContentCode, text + path);
					if (AMSOperation.isSuccess)
					{
						azureUploadTracking.EncodeCompleted = true;
						azureUploadTracking.EncodedOn = DateTime.Now;
						robomateplusEntities3.Entry(azureUploadTracking).State = EntityState.Modified;
						robomateplusEntities3.SaveChanges();
						isSuccess = true;
						Message = "Done";
					}
				}
				TextBox textBox = txtUploadList;
				textBox.Text = textBox.Text + path + "\r\n";
				text2 = text2 + path + "\r\n";
				num7++;
				backgroundWorker.ReportProgress(num7 * 100 / num6);
				DateTime now4 = DateTime.Now;
				int num8 = (int)(now4 - now3).TotalSeconds;
				int num9 = num8 / 60;
				int num10 = num9 / 60;
				int num11 = num8 - num9 * 60;
				num9 -= num10 * 60;
				lblProgress.Text = "Processed " + num7 + " out of " + num6 + "\r\nTime Elapsed : ";
				lblProgress.Text += ((num10 > 9) ? (num10 + ":") : ("0" + num10 + ":"));
				lblProgress.Text += ((num9 > 9) ? (num9 + ":") : ("0" + num9 + ":"));
				lblProgress.Text += ((num11 > 9) ? num11.ToString() : ("0" + num11));
				num8 = num8 * (num6 - num7) / num7;
				num9 = num8 / 60;
				num10 = num9 / 60;
				num11 = num8 - num9 * 60;
				num9 -= num10 * 60;
				lblProgress.Text += " Expected Time ";
				lblProgress.Text += ((num10 > 9) ? (num10 + ":") : ("0" + num10 + ":"));
				lblProgress.Text += ((num9 > 9) ? (num9 + ":") : ("0" + num9 + ":"));
				lblProgress.Text += ((num11 > 9) ? num11.ToString() : ("0" + num11));
			}
		}

		private void AMSBGProcess_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			progressBar1.Value = e.ProgressPercentage;
		}

		private void AMSBGProcess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			txtFailure.Text = FailureList;
			txtResult.Text = SuccessList;
			MessageBox.Show("Completed");
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			if (AMSBGProcess.WorkerSupportsCancellation)
			{
				AMSBGProcess.CancelAsync();
			}
		}

		private void btnUpdate_Click(object sender, EventArgs e)
		{
			if (!AMSBGProcess.IsBusy)
			{
				strFileList = "";
				DirSearch(txtFolder.Text);
				txtFiles.Text = strFileList;
				processName = "Update";
				CurrentCourse = cbCourseList.SelectedValue.ToString();
				AMSBGProcess.RunWorkerAsync();
			}
			else
			{
				MessageBox.Show("A process '" + processName + "' is running");
			}
		}

		private void DirSearch(string sDir)
		{
			try
			{
				string[] directories = Directory.GetDirectories(sDir);
				foreach (string text in directories)
				{
					string[] files = Directory.GetFiles(text);
					foreach (string text2 in files)
					{
						strFileList = strFileList + text2.Replace(strBase, "").Replace("\\", "/") + "\r\n";
					}
					DirSearch(text);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabCList = new System.Windows.Forms.TabPage();
			this.dgContentList = new System.Windows.Forms.DataGridView();
			this.tabPendingUpload = new System.Windows.Forms.TabPage();
			this.dgPendingUpload = new System.Windows.Forms.DataGridView();
			this.tabPendingEncode = new System.Windows.Forms.TabPage();
			this.dgPendingEncode = new System.Windows.Forms.DataGridView();
			this.tabUpdate = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.txtFiles = new System.Windows.Forms.TextBox();
			this.txtUploadList = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnProcessData = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.cbCourseList = new System.Windows.Forms.ComboBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.btnUpdate = new System.Windows.Forms.Button();
			this.btnUpload = new System.Windows.Forms.Button();
			this.btnSelect = new System.Windows.Forms.Button();
			this.txtFolder = new System.Windows.Forms.TextBox();
			this.btnEncode = new System.Windows.Forms.Button();
			this.panel3 = new System.Windows.Forms.Panel();
			this.txtFailure = new System.Windows.Forms.TextBox();
			this.txtResult = new System.Windows.Forms.TextBox();
			this.panel4 = new System.Windows.Forms.Panel();
			this.lblProgress = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.AMSBGProcess = new System.ComponentModel.BackgroundWorker();
			this.tableLayoutPanel1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabCList.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)this.dgContentList).BeginInit();
			this.tabPendingUpload.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)this.dgPendingUpload).BeginInit();
			this.tabPendingEncode.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)this.dgPendingEncode).BeginInit();
			this.tabUpdate.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel4.SuspendLayout();
			base.SuspendLayout();
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70f));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30f));
			this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.panel4, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.20163f));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 74.74541f));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.84929f));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(943, 491);
			this.tableLayoutPanel1.TabIndex = 0;
			this.tabControl1.Controls.Add(this.tabCList);
			this.tabControl1.Controls.Add(this.tabPendingUpload);
			this.tabControl1.Controls.Add(this.tabPendingEncode);
			this.tabControl1.Controls.Add(this.tabUpdate);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(3, 58);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(654, 361);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(tabControl1_SelectedIndexChanged);
			this.tabCList.Controls.Add(this.dgContentList);
			this.tabCList.Location = new System.Drawing.Point(4, 22);
			this.tabCList.Name = "tabCList";
			this.tabCList.Padding = new System.Windows.Forms.Padding(3);
			this.tabCList.Size = new System.Drawing.Size(646, 335);
			this.tabCList.TabIndex = 0;
			this.tabCList.Text = "Content List";
			this.tabCList.UseVisualStyleBackColor = true;
			this.dgContentList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgContentList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgContentList.Location = new System.Drawing.Point(3, 3);
			this.dgContentList.Name = "dgContentList";
			this.dgContentList.Size = new System.Drawing.Size(640, 329);
			this.dgContentList.TabIndex = 0;
			this.tabPendingUpload.Controls.Add(this.dgPendingUpload);
			this.tabPendingUpload.Location = new System.Drawing.Point(4, 22);
			this.tabPendingUpload.Name = "tabPendingUpload";
			this.tabPendingUpload.Padding = new System.Windows.Forms.Padding(3);
			this.tabPendingUpload.Size = new System.Drawing.Size(646, 335);
			this.tabPendingUpload.TabIndex = 1;
			this.tabPendingUpload.Text = "Upload Pending";
			this.tabPendingUpload.UseVisualStyleBackColor = true;
			this.dgPendingUpload.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgPendingUpload.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgPendingUpload.Location = new System.Drawing.Point(3, 3);
			this.dgPendingUpload.Name = "dgPendingUpload";
			this.dgPendingUpload.Size = new System.Drawing.Size(640, 329);
			this.dgPendingUpload.TabIndex = 0;
			this.tabPendingEncode.Controls.Add(this.dgPendingEncode);
			this.tabPendingEncode.Location = new System.Drawing.Point(4, 22);
			this.tabPendingEncode.Name = "tabPendingEncode";
			this.tabPendingEncode.Size = new System.Drawing.Size(646, 335);
			this.tabPendingEncode.TabIndex = 2;
			this.tabPendingEncode.Text = "Encode Pending";
			this.tabPendingEncode.UseVisualStyleBackColor = true;
			this.dgPendingEncode.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgPendingEncode.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgPendingEncode.Location = new System.Drawing.Point(0, 0);
			this.dgPendingEncode.Name = "dgPendingEncode";
			this.dgPendingEncode.Size = new System.Drawing.Size(646, 335);
			this.dgPendingEncode.TabIndex = 0;
			this.tabUpdate.Controls.Add(this.tableLayoutPanel2);
			this.tabUpdate.Location = new System.Drawing.Point(4, 22);
			this.tabUpdate.Name = "tabUpdate";
			this.tabUpdate.Size = new System.Drawing.Size(646, 335);
			this.tabUpdate.TabIndex = 3;
			this.tabUpdate.Text = "Update Selected";
			this.tabUpdate.UseVisualStyleBackColor = true;
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
			this.tableLayoutPanel2.Controls.Add(this.txtFiles, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.txtUploadList, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(646, 335);
			this.tableLayoutPanel2.TabIndex = 0;
			this.txtFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtFiles.Location = new System.Drawing.Point(3, 3);
			this.txtFiles.Multiline = true;
			this.txtFiles.Name = "txtFiles";
			this.txtFiles.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtFiles.Size = new System.Drawing.Size(317, 329);
			this.txtFiles.TabIndex = 0;
			this.txtUploadList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtUploadList.Location = new System.Drawing.Point(326, 3);
			this.txtUploadList.Multiline = true;
			this.txtUploadList.Name = "txtUploadList";
			this.txtUploadList.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtUploadList.Size = new System.Drawing.Size(317, 329);
			this.txtUploadList.TabIndex = 1;
			this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.panel1.Controls.Add(this.btnProcessData);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.cbCourseList);
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(654, 49);
			this.panel1.TabIndex = 1;
			this.btnProcessData.Location = new System.Drawing.Point(409, 8);
			this.btnProcessData.Name = "btnProcessData";
			this.btnProcessData.Size = new System.Drawing.Size(75, 23);
			this.btnProcessData.TabIndex = 2;
			this.btnProcessData.Text = "Process";
			this.btnProcessData.UseVisualStyleBackColor = true;
			this.btnProcessData.Click += new System.EventHandler(btnProcessData_Click);
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(29, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Select Course";
			this.cbCourseList.FormattingEnabled = true;
			this.cbCourseList.Location = new System.Drawing.Point(123, 10);
			this.cbCourseList.Name = "cbCourseList";
			this.cbCourseList.Size = new System.Drawing.Size(280, 21);
			this.cbCourseList.TabIndex = 0;
			this.tableLayoutPanel1.SetColumnSpan(this.panel2, 2);
			this.panel2.Controls.Add(this.btnUpdate);
			this.panel2.Controls.Add(this.btnUpload);
			this.panel2.Controls.Add(this.btnSelect);
			this.panel2.Controls.Add(this.txtFolder);
			this.panel2.Controls.Add(this.btnEncode);
			this.panel2.Location = new System.Drawing.Point(3, 425);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(937, 42);
			this.panel2.TabIndex = 2;
			this.btnUpdate.Location = new System.Drawing.Point(836, 3);
			this.btnUpdate.Name = "btnUpdate";
			this.btnUpdate.Size = new System.Drawing.Size(75, 23);
			this.btnUpdate.TabIndex = 5;
			this.btnUpdate.Text = "Update";
			this.btnUpdate.UseVisualStyleBackColor = true;
			this.btnUpdate.Visible = false;
			this.btnUpdate.Click += new System.EventHandler(btnUpdate_Click);
			this.btnUpload.Location = new System.Drawing.Point(737, 4);
			this.btnUpload.Name = "btnUpload";
			this.btnUpload.Size = new System.Drawing.Size(75, 23);
			this.btnUpload.TabIndex = 4;
			this.btnUpload.Text = "Start Upload";
			this.btnUpload.UseVisualStyleBackColor = true;
			this.btnUpload.Visible = false;
			this.btnUpload.Click += new System.EventHandler(btnUpload_Click);
			this.btnSelect.Location = new System.Drawing.Point(524, 4);
			this.btnSelect.Name = "btnSelect";
			this.btnSelect.Size = new System.Drawing.Size(111, 23);
			this.btnSelect.TabIndex = 3;
			this.btnSelect.Text = "Select Folder";
			this.btnSelect.UseVisualStyleBackColor = true;
			this.btnSelect.Click += new System.EventHandler(btnSelect_Click);
			this.txtFolder.Location = new System.Drawing.Point(108, 4);
			this.txtFolder.Name = "txtFolder";
			this.txtFolder.Size = new System.Drawing.Size(410, 20);
			this.txtFolder.TabIndex = 2;
			this.btnEncode.Location = new System.Drawing.Point(656, 4);
			this.btnEncode.Name = "btnEncode";
			this.btnEncode.Size = new System.Drawing.Size(75, 23);
			this.btnEncode.TabIndex = 0;
			this.btnEncode.Text = "Start Ecode";
			this.btnEncode.UseVisualStyleBackColor = true;
			this.btnEncode.Visible = false;
			this.btnEncode.Click += new System.EventHandler(BtnEncode_Click);
			this.panel3.Controls.Add(this.txtFailure);
			this.panel3.Controls.Add(this.txtResult);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(663, 58);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(277, 361);
			this.panel3.TabIndex = 3;
			this.txtFailure.Location = new System.Drawing.Point(14, 203);
			this.txtFailure.Multiline = true;
			this.txtFailure.Name = "txtFailure";
			this.txtFailure.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtFailure.Size = new System.Drawing.Size(254, 133);
			this.txtFailure.TabIndex = 2;
			this.txtResult.Location = new System.Drawing.Point(14, 22);
			this.txtResult.Multiline = true;
			this.txtResult.Name = "txtResult";
			this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtResult.Size = new System.Drawing.Size(254, 133);
			this.txtResult.TabIndex = 1;
			this.panel4.Controls.Add(this.lblProgress);
			this.panel4.Controls.Add(this.progressBar1);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel4.Location = new System.Drawing.Point(663, 3);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(277, 49);
			this.panel4.TabIndex = 4;
			this.lblProgress.AutoSize = true;
			this.lblProgress.Location = new System.Drawing.Point(14, 26);
			this.lblProgress.Name = "lblProgress";
			this.lblProgress.Size = new System.Drawing.Size(0, 13);
			this.lblProgress.TabIndex = 4;
			this.progressBar1.Location = new System.Drawing.Point(14, 3);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(271, 23);
			this.progressBar1.Step = 1;
			this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.progressBar1.TabIndex = 3;
			this.AMSBGProcess.WorkerReportsProgress = true;
			this.AMSBGProcess.WorkerSupportsCancellation = true;
			this.AMSBGProcess.DoWork += new System.ComponentModel.DoWorkEventHandler(AMSBGProcess_DoWork);
			this.AMSBGProcess.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(AMSBGProcess_ProgressChanged);
			this.AMSBGProcess.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(AMSBGProcess_RunWorkerCompleted);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(943, 491);
			base.Controls.Add(this.tableLayoutPanel1);
			base.Name = "AzureUpload";
			this.Text = "Azure Upload Tool";
			base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			base.Load += new System.EventHandler(AzureUpload_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabCList.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)this.dgContentList).EndInit();
			this.tabPendingUpload.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)this.dgPendingUpload).EndInit();
			this.tabPendingEncode.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)this.dgPendingEncode).EndInit();
			this.tabUpdate.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			base.ResumeLayout(false);
		}
	}
}
