using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagerCG
{
    public partial class DatosFile : Form
    {
        private static DatosFile _instancia = null;
        private DatosFile()
        {
            InitializeComponent();
            lbfile.Text = "";
            lbTime.Text = "";
        }

        public static DatosFile Instancia => _instancia ?? (_instancia = new DatosFile());
        public ListViewItem ListViewItem { get; set; }

        public void ShowDatos()
        {
            lbfile.Text = ListViewItem.SubItems[0].Text;
            lbTime.Text = ListViewItem.SubItems[1].Text;
            numericUpDownFrames.Value = Convert.ToDecimal(ListViewItem.SubItems[2].Text);
            numericUpDownRatio.Value = Convert.ToDecimal(ListViewItem.SubItems[3].Text);
            this.Show();
        }

        private void numericUpDownFrames_ValueChanged(object sender, EventArgs e)
        {
            ListViewItem.SubItems[2].Text = numericUpDownFrames.Value.ToString();
        }

        private void numericUpDownRatio_ValueChanged(object sender, EventArgs e)
        {
            ListViewItem.SubItems[3].Text = numericUpDownRatio.Value.ToString();
        }
    }
}
