using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Laatan_mitoitus {
    public partial class AsetuksetForm : Form {

        Asetukset asetukset;


        public AsetuksetForm(Asetukset asetukset) {
            InitializeComponent();
            this.asetukset = asetukset;
        }

        public void AlustaAsetusarvot() {
            tbBetoninLujuus.Text = asetukset.BetoninLujuus.ToString();
            tbPaksuus.Text = asetukset.LaatanPaksuus.ToString();
            tbTeraksenLujuus.Text = asetukset.TeraksenLujuus.ToString();
            tbMittaYlapinnasta.Text = asetukset.ArvioituMittaYlapinnasta.ToString();
            cbRasitusluokka.SelectedIndex = asetukset.RasitusluokkaIndx;
            tbSuojaBet.Text = asetukset.Suojabetoni.ToString();
            tbMinTeras.Text = asetukset.MinTerasKoko.ToString();
            tbVerkkoTerasMin.Text = asetukset.MinVerkkoTerasKoko.ToString();
            cbVerkonSilmavali.Text = asetukset.HaluttuVerkkoSilmaVali.ToString();
            cbTukiRaudJako.Text = asetukset.TukiRaudJako.ToString();
        }

        private void bPeruuta_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void bOk_Click(object sender, EventArgs e) {
            try {
                asetukset.BetoninLujuus = int.Parse(tbBetoninLujuus.Text);
                asetukset.LaatanPaksuus = double.Parse(tbPaksuus.Text);
                asetukset.TeraksenLujuus = int.Parse(tbTeraksenLujuus.Text);
                asetukset.ArvioituMittaYlapinnasta = double.Parse(tbMittaYlapinnasta.Text);
                asetukset.Rasitusluokka = (string)cbRasitusluokka.SelectedItem;
                asetukset.RasitusluokkaIndx = cbRasitusluokka.SelectedIndex;
                asetukset.Suojabetoni = double.Parse(tbSuojaBet.Text);
                asetukset.MinTerasKoko = int.Parse(tbMinTeras.Text);
                asetukset.HaluttuVerkkoSilmaVali = int.Parse(cbVerkonSilmavali.Text);
                asetukset.MinVerkkoTerasKoko = int.Parse(tbVerkkoTerasMin.Text);
                asetukset.TukiRaudJako = int.Parse(cbTukiRaudJako.Text);
                this.Close();
            } catch (Exception) {
                MessageBox.Show("Tarkista arvot! Kentät eivät saa sisältää muita kuin numeroita");
            }
            
        }

        private void AsetuksetForm_Load(object sender, EventArgs e) {
            AlustaAsetusarvot();
        }

        private void PoistaArsyttavaAaniPerkl(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void EnteriaPainettu(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                bOk_Click(null, null);
            }
        }

        private void label12_Click(object sender, EventArgs e) {

        }

        private void cbRasitusluokka_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void groupBox2_Enter(object sender, EventArgs e) {

        }
    }
}
