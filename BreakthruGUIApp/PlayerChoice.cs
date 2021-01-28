using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChessBoardGUIApp
{
    public partial class PlayerChoice : Form
    {

        //public Color Side { get; set; }

        public PlayerChoice()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) //Gold
        {
            //Side = Color.Gold;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e) //Silver
        {
            //Side = Color.Silver;
            this.Close();
        }
    }
}
