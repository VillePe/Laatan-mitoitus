using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Laatan_mitoitus.Formit {
    public partial class KaikkiRaudoitukset : Form {

        List<Kentta> kentat = new List<Kentta>();
        List<TextBox> kentta1 = new List<TextBox>();
        List<TextBox> kentta2 = new List<TextBox>();
        List<TextBox> kentta3 = new List<TextBox>();
        List<TextBox> kentta4 = new List<TextBox>();
        List<TextBox> kentta5 = new List<TextBox>();
        List<TextBox> kentta6 = new List<TextBox>();
        List<TextBox> kentta7 = new List<TextBox>();
        List<List<TextBox>> kentatListaLista = new List<List<TextBox>>();


        public KaikkiRaudoitukset(List<Kentta> kentat) {
            InitializeComponent();
            this.kentat = kentat;
            AlustaTekstiListat();
        }

        private void KaikkiRaudoitukset_Load(object sender, EventArgs e) {
            int laskuri = 0;
            AlustaTekstiListat();
            foreach (Kentta k in kentat) {
                kentatListaLista[laskuri][0].Text = k.RaudoitusYleensa.ToString();
                kentatListaLista[laskuri][1].Text = k.RaudoitusKeskella.ToString();
                kentatListaLista[laskuri][2].Text = Math.Round(((Math.Max(k.VaadittuRaudoitusKeskella, k.Minimiraudoitus) / (k.RaudoitusYleensa.PintaAlaVaaka + (double)k.RaudoitusKeskella.PintaAlaVaaka))*100),0) + "%";
                foreach (string suunta in k.Tukiraudat.Keys) {
                    switch (suunta) {
                        case "ylos":
                            kentatListaLista[laskuri][3].Text = k.HaeTukiraudat("ylos").ToString();
                            break;
                        case "alas":
                            kentatListaLista[laskuri][4].Text = k.HaeTukiraudat("alas").ToString();
                            break;
                        case "vasen":
                            kentatListaLista[laskuri][5].Text = k.HaeTukiraudat("vasen").ToString();
                            break;
                        case "oikea":
                            kentatListaLista[laskuri][6].Text = k.HaeTukiraudat("oikea").ToString();
                            break;
                    }
                }
                laskuri++;

            }
        }

        private void AlustaTekstiListat() {
            kentta1.Add(tbVerkkoYleensa1);
            kentta1.Add(tbVerkkoKeskella);
            kentta1.Add(tbKayttoaste);
            kentta1.Add(tbTukiVasen);
            kentta1.Add(tbTukiYlos);
            kentta1.Add(tbTukiOikea);
            kentta1.Add(tbTukiAlas);

            kentta2.Add(tbVerkkoYleensa2);
            kentta2.Add(tbVerkkoKeskella2);
            kentta2.Add(tbKayttoaste2);
            kentta2.Add(tbTukiVasen2);
            kentta2.Add(tbTukiYlos2);
            kentta2.Add(tbTukiOikea2);
            kentta2.Add(tbTukiAlas2);

            kentta3.Add(tbVerkkoYleensa3);
            kentta3.Add(tbVerkkoKeskella3);
            kentta3.Add(tbKayttoaste3);
            kentta3.Add(tbTukiVasen3);
            kentta3.Add(tbTukiYlos3);
            kentta3.Add(tbTukiOikea3);
            kentta3.Add(tbTukiAlas3);

            kentta4.Add(tbVerkkoYleensa4);
            kentta4.Add(tbVerkkoKeskella4);
            kentta4.Add(tbKayttoaste4);
            kentta4.Add(tbTukiVasen4);
            kentta4.Add(tbTukiYlos4);
            kentta4.Add(tbTukiOikea4);
            kentta4.Add(tbTukiAlas4);

            kentta5.Add(tbVerkkoYleensa5);
            kentta5.Add(tbVerkkoKeskella5);
            kentta5.Add(tbKayttoaste5);
            kentta5.Add(tbTukiVasen5);
            kentta5.Add(tbTukiYlos5);
            kentta5.Add(tbTukiOikea5);
            kentta5.Add(tbTukiAlas5);

            kentta6.Add(tbVerkkoYleensa6);
            kentta6.Add(tbVerkkoKeskella6);
            kentta6.Add(tbKayttoaste6);
            kentta6.Add(tbTukiVasen6);
            kentta6.Add(tbTukiYlos6);
            kentta6.Add(tbTukiOikea6);
            kentta6.Add(tbTukiAlas6);

            kentta7.Add(tbVerkkoYleensa7);
            kentta7.Add(tbVerkkoKeskella7);
            kentta7.Add(tbKayttoaste7);
            kentta7.Add(tbTukiVasen7);
            kentta7.Add(tbTukiYlos7);
            kentta7.Add(tbTukiOikea7);
            kentta7.Add(tbTukiAlas7);

            kentatListaLista.Add(kentta1);
            kentatListaLista.Add(kentta2);
            kentatListaLista.Add(kentta3);
            kentatListaLista.Add(kentta4);
            kentatListaLista.Add(kentta5);
            kentatListaLista.Add(kentta6);
            kentatListaLista.Add(kentta7);
        }

        private void bSulje_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
