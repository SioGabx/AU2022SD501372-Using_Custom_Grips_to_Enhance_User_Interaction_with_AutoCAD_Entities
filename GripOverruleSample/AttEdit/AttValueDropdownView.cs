using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GripOverruleSample.AttEdit
{
    public partial class AttValueDropdownView : Form
    {
        public AttValueDropdownView()
        {
            InitializeComponent();
        }

        public AttValueDropdownView(IEnumerable<string> attValues, string defaultVal) : this()
        {
            foreach (var val in attValues)
            {
                cboValues.Items.Add(val);
            }

            if (!string.IsNullOrEmpty(defaultVal))
            {
                cboValues.Text= defaultVal;
            }
            btnOK.Enabled = cboValues.Text.Trim().Length > 0;
        }

        public string AttributeValue => cboValues.Text;

        private void AttValueDropdownView_Load(object sender, EventArgs e)
        {

        }

        private void cboValues_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cboValues_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = cboValues.Text.Trim().Length > 0;
        }
    }
}
