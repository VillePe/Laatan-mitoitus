using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Laatan_mitoitus {
    public partial class RaudoitusForm : Form {

        Raudoittaja raudoittaja;
        Kentta kentta;

        public RaudoitusForm(Raudoittaja raudoittaja, Kentta kentta) {
            InitializeComponent();
            this.raudoittaja = raudoittaja;
            this.kentta = kentta;
        }

        private void RaudoitusForm_Load(object sender, EventArgs e) {

            // Alustetaan ikkunan kentät
            lKentta.Text = "Kenttä " + kentta.Numero.ToString();
            tbTarvRaud.Text = Math.Round(raudoittaja.RaudoitusVaadMaara,2).ToString();
            tbMittaYlapinnasta.Text = raudoittaja.ToteutunutMittaYlapinnasta.ToString();
            tbVerkkoKeskella.Text = raudoittaja.VerkkoKeskella.ToString();
            tbKuvaVerkkoKeskella.Text = raudoittaja.VerkkoKeskella.ToString();
            tbVerkkoYleensa.Text = raudoittaja.VerkkoYleensa.ToString();
            tbKuvaVerkkoYleensa.Text = raudoittaja.VerkkoYleensa.ToString();
            tbRaudoitusMaaraYleensa.Text = raudoittaja.VerkkoYleensa.PintaAlaVaaka.ToString();
            tbRaudoitusMaaraKeskella.Text = (raudoittaja.VerkkoKeskella.PintaAlaVaaka + raudoittaja.VerkkoYleensa.PintaAlaVaaka).ToString();
            tbMinRaud.Text = Math.Round(raudoittaja.RaudoitusVahMaara,0).ToString();
            tbKayttoaste.Text = Math.Round(Math.Max(raudoittaja.RaudoitusVaadMaara, kentta.Minimiraudoitus) / (raudoittaja.VerkkoYleensa.PintaAlaVaaka + raudoittaja.VerkkoKeskella.PintaAlaVaaka),3)*100 + "";
            tbBetoniPeite.Text = raudoittaja.Suojabetoni.ToString();
            foreach (string suunta in kentta.Tukiraudat.Keys) {
                switch (suunta) {
                    case "ylos":
                        tbTukiYlos.Text = kentta.HaeTukiraudat("ylos").Koko + " - k" + kentta.HaeTukiraudat("ylos").Jako;
                        tbTukiKuvaYlos.Text = kentta.HaeTukiraudat("ylos").Koko + " - k" + kentta.HaeTukiraudat("ylos").Jako;
                        break;
                    case "alas":
                        tbTukiAlas.Text = kentta.HaeTukiraudat("alas").Koko + " - k" + kentta.HaeTukiraudat("alas").Jako;
                        tbTukiKuvaAlas.Text = kentta.HaeTukiraudat("alas").Koko + " - k" + kentta.HaeTukiraudat("alas").Jako;
                        break;
                    case "vasen":
                        tbTukiVasen.Text = kentta.HaeTukiraudat("vasen").Koko + " - k" + kentta.HaeTukiraudat("vasen").Jako;
                        tbTukiKuvaVasen.Text = kentta.HaeTukiraudat("vasen").Koko + " - k" + kentta.HaeTukiraudat("vasen").Jako;
                        break;
                    case "oikea":
                        tbTukiOikea.Text = kentta.HaeTukiraudat("oikea").Koko + " - k" + kentta.HaeTukiraudat("oikea").Jako;
                        tbTukiKuvaOikea.Text = kentta.HaeTukiraudat("oikea").Koko + " - k" + kentta.HaeTukiraudat("oikea").Jako;
                        break;
                }
            }
        }

        private void bSulje_Click(object sender, EventArgs e) {
            Close();
        }
    }
}
