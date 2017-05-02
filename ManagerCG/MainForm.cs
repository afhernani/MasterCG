/*
 * Creado por SharpDevelop.
 * Usuario: hernani
 * Fecha: 05/11/2016
 * Hora: 15:03
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ffmpegUI;

namespace ManagerCG
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		//variables globales -modificables.
		int nproc = 1; //numero de procesos
		int nframes = 23; // numero de frames
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}

        #region openDir
		void BtnAbrirClick(object sender, EventArgs e)
		{
			try
			{
				btnAbrir.Enabled = false;
				OpenDatos datos = OpenDatos.Instance();
				datos.CompletedFiles += ArrayToFiles;
				labelcarga.Visible=true;
				datos.ThreadOpenDir();
			} 
			catch (Exception ex)
			{
				Debug.WriteLine("Error al Abrir: " +  ex.Message);
				btnAbrir.Enabled = true;
			}	
		}

	    List<string> ListFiles = new List<string>();
		
		void ArrayToFiles(string[] files)
		{
			if(this.InvokeRequired)
			{
				OpenDatos.FilesHandler call = 
					new OpenDatos.FilesHandler(ArrayToFiles);
				this.Invoke(call, new object[]{files});		
			}
			else
			{
				ListFiles.Clear();
				ListFiles.AddRange(files);
				foreach (var ex in ListFiles)
					Debug.WriteLine(ex);
				t = Task.Factory.StartNew(LoadListToListView);
				labelcarga.Visible = false;
				btnAbrir.Enabled=true;
			}
		}

	    private Task t;
		void LoadListToListView()
		{
            listView.Items.Clear();
			try 
			{
			    foreach (string file in ListFiles)
			    {
			        if (System.IO.File.Exists(file))
			        {

                        itemMovie _item = new itemMovie(file);
			            AddItemToListView(_item);

			        }
			    }
				
			} catch (Exception ex) 
			{
				Debug.WriteLine("LoadListToListView : " + ex.Message);
			}
		}

	    public delegate void AddItemToListHandler(itemMovie item);

	    private void AddItemToListView(itemMovie item)
	    {
	        if (this.InvokeRequired)
	        {
	            AddItemToListHandler call =
	                new AddItemToListHandler(AddItemToListView);
	            this.Invoke(call, new object[] {item});
	        }
	        else
	        {
                Debug.WriteLine($"Add item: {item.NameFile}");            
                ListViewItem listviewitem = 
                    new ListViewItem(item.NameFile);
	            listviewitem.Tag = item;
	            listviewitem.SubItems.Add("Time").Text=item.Time.ToString();
                listviewitem.SubItems.Add("Frames").Text=item.Frames.ToString();
                listviewitem.SubItems.Add("Ratio").Text=item.Ratio.ToString();
                listviewitem.SubItems.Add("Process").Text="OK";
                listView.Items.Add(listviewitem);
	        }
	    }
        
        #endregion
        
        
        #region listview events

        private void listView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            int index = listView.SelectedItems.Count;
            switch (index)
            {
                case 1:
                    pictureBox1.Image = ((itemMovie)listView.SelectedItems[index-1].Tag).Thumbs;
                    Debug.WriteLine("item selected: " + listView.SelectedItems[index - 1].SubItems[0].Text);
                    break;
                
                default:
                    if (index >= 1)
                    {
                        foreach (ListViewItem selecitem in listView.SelectedItems)
                        {
                            Debug.WriteLine($"itemselects -> {selecitem.Text}");
                        }
                        pictureBox1.Image = ((itemMovie)listView.SelectedItems[index-2].Tag).Thumbs;
                        Debug.WriteLine("item selected: "+listView.SelectedItems[index - 2].SubItems[0].Text);
                    }
                    break;
            }
            
        }


        private void borrarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count >= 1)
            {
                foreach (ListViewItem selecitem in listView.SelectedItems)
                {
                    listView.Items.Remove(selecitem);
                    Debug.WriteLine($"Remove -> {selecitem.Text}");
                }
            }
        }

        private void modificarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count >= 1)
            {
                DatosFile datos = DatosFile.Instancia;
                datos.ListViewItem = listView.SelectedItems[0];
                datos.ShowDatos();
            }
        }

        #endregion

        #region drag-drog
        #endregion


        #region estructura-work

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (listView.Items.Count == 0) return;
            btnAceptar.Enabled = false; //desabilitamos.
            btnAbrir.Enabled = false; //desabilita abrir ficheros
            Index = 0;
            
            this.progressBar1.Value = 0;
            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = listView.Items.Count-1;

            ThrowProcessMakeGif();
        }

	    private int Index { get; set; }

        private void ThrowProcessMakeGif()
        {
            if (Index <= listView.Items.Count - 1)
            {
                listView.Items[Index].Selected = true;
                listView.Items[Index].Focused = true;
                listView.Items[Index].SubItems[4].Text = @"Working";
                string file = listView.Items[Index].SubItems[0].Text;
                int rate = Convert.ToInt32(listView.Items[Index].SubItems[3].Text);
                int numframe = Convert.ToInt32(listView.Items[Index].SubItems[2].Text);
                double time = Convert.ToDouble(listView.Items[Index].SubItems[1].Text);
                int num = (int)time/numframe; //numero de frames no puede ser o. al igual que num
                if (num == 0) num = 1;
                try
                {
                    Converter conv = new Converter();
                    conv.FrameRate = rate;
                    conv.MadeFilmGif += MadeGif;
                    conv.MakeGifThread(file, num);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void MadeGif(object sender, OutputPackage package)
        {
            this.BeginInvoke(new Converter.MakeFilmGifHandler(Madegifmadeend), new object[] { sender, package });
        }

        private void Madegifmadeend(object sender, OutputPackage package)
        {
            this.progressBar1.Value = Index;
            ((itemMovie)listView.Items[Index].Tag).Thumbs = (Image)Image.FromStream(package.VideoStream).Clone();
            pictureBox1.Image = ((itemMovie)listView.Items[Index].Tag).Thumbs;
            listView.Items[Index].SubItems[4].Text = "End";
            Index++;
            if (Index <= listView.Items.Count - 1)
            {
                ThrowProcessMakeGif();
            }
            else
            {
                btnAceptar.Enabled = true; //desabilitamos.
                btnAbrir.Enabled = true; //desabilita abrir ficheros
            }
        }

        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.FormSize.Width !=0 &&
                Properties.Settings.Default.FormSize.Height !=0)
            {
                this.Size = Properties.Settings.Default.FormSize;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.WindowState==FormWindowState.Normal)
            {
                Properties.Settings.Default.FormSize = this.Size;
                //save
                Properties.Settings.Default.Save();
            }
        }
    }
}
