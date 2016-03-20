using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Laatan_mitoitus {
    public partial class Paaikkuna : Form {

        public static string HAKEMISTO;

        private const int RECTANGLES = 7;
        Rectangle[] rectangles = new Rectangle[RECTANGLES];
        TextBox[] mitatVaaka;
        TextBox[] mitatPysty;
        Kentta[] kentat = new Kentta[7];
        List<Kentta> kentatLista = new List<Kentta>();
        double kuormat;
        Asetukset asetukset;
        Mallintaja mallintaja;
        bool tasauksetLaskettu = false;
        Tiedostonkasittelija lokiKirjoittaja;


        public Paaikkuna() {
            InitializeComponent();
            
            asetukset = new Asetukset();
            asetukset.AsetaAlkuperaiset();
            tiedostonAvaus.InitialDirectory = Application.StartupPath;
            tiedostonTallennus.InitialDirectory = Application.StartupPath;
            HAKEMISTO = Application.StartupPath;
            lokiKirjoittaja = new Tiedostonkasittelija(HAKEMISTO);
        }

        private void TyhjennaKentat() {
            for (int i = 0; i < kentat.Length; i++) {
                kentat[i] = null;
            }
            bPyyhi_Click(null, null);
            bRaudoita.Enabled = false;
            bLuoPiirros.Enabled = false;
            tasauksetLaskettu = false;
        }

        private void AsetaJaLaskeKentat() {

        }

        #region Properties
        
        public string Vertical_1_2_3 {
            get {
                return tbPysty_1_2_3.Text;
            }
        }

        public string Vertical_5 {
            get {
                return tbPysty_5.Text;
            }
        }

        public string Vertical_7 {
            get {
                return tbPysty_7.Text;
            }
        }

        public string Horizontal_1_4 {
            get {
                return tbVaaka_1_4.Text;
            }
        }

        public string Horizontal_2_5 {
            get {
                return tbVaaka_2_5.Text;
            }
        }

        public string Horizontal_3_6 {
            get {
                return tbVaaka_3_6.Text;
            }
        }

        public string Horizontal_7 {
            get {
                return tbVaaka_7.Text;
            }
        }
        #endregion

        #region Tekstiboksit TextChanged
        private void tb_pysty_1_2_3_TextChanged(object sender, EventArgs e) {
            l_vertical_1_2_3.Text = tbPysty_1_2_3.Text;
            TyhjennaKentat();
        }

        private void tbVaaka_1_4_TextChanged(object sender, EventArgs e) {
            TyhjennaKentat();
        }

        private void tb_vaaka_7_TextChanged(object sender, EventArgs e) {
            if (!string.IsNullOrWhiteSpace(tbVaaka_2_5.Text)) {
                try {
                    double vaaka_2_5 = double.Parse(tbVaaka_2_5.Text);
                    double vaaka_7 = double.Parse(tbVaaka_7.Text);
                    if (vaaka_2_5 - vaaka_7 < 0) {
                        l_horizontal_reika.Text = "";
                        return;
                    }
                    l_horizontal_reika.Text = (vaaka_2_5 - vaaka_7).ToString();
                } catch (Exception) {
                }
            }
            TyhjennaKentat();
        }

        private void tb_vaaka_2_5_TextChanged(object sender, EventArgs e) {
            if (!string.IsNullOrWhiteSpace(tbVaaka_7.Text)) {
                try {
                    double vaaka_2_5 = double.Parse(tbVaaka_2_5.Text);
                    double vaaka_7 = double.Parse(tbVaaka_7.Text);
                    if (vaaka_2_5 - vaaka_7 < 0) {
                        l_horizontal_reika.Text = "";
                        return;
                    }
                    l_horizontal_reika.Text = (vaaka_2_5 - vaaka_7).ToString();
                } catch (Exception) {
                }
            }
            TyhjennaKentat();
        }

        private void tb_vertical_5_TextChanged(object sender, EventArgs e) {
            if (!string.IsNullOrWhiteSpace(tbPysty_7.Text)) {
                try {
                    double vertical_5 = double.Parse(tbPysty_5.Text);
                    double vertical_7 = double.Parse(tbPysty_7.Text);
                    if (vertical_5 + vertical_7 < 0) {
                        tbPysty_4_6.Text = "";
                        return;
                    }
                    tbPysty_4_6.Text = (vertical_5 + vertical_7).ToString();
                } catch (Exception) {
                }
            }
            TyhjennaKentat();
        }

        private void tb_vertical_7_TextChanged(object sender, EventArgs e) {
            if (!string.IsNullOrWhiteSpace(tbPysty_5.Text)) {
                try {
                    double vertical_5 = double.Parse(tbPysty_5.Text);
                    double vertical_7 = double.Parse(tbPysty_7.Text);
                    if (vertical_5 + vertical_7 < 0) {
                        tbPysty_4_6.Text = "";
                        return;
                    }
                    tbPysty_4_6.Text = (vertical_5 + vertical_7).ToString();
                } catch (Exception) {
                }
            }
            TyhjennaKentat();
        }

        private void tb_vertical_4_6_TextChanged(object sender, EventArgs e) {
            try {
                double vertical_4_6 = double.Parse(tbPysty_4_6.Text);
                tbPysty_5.Text = (vertical_4_6 / 2.0).ToString();
                tbPysty_7.Text = (vertical_4_6 / 2.0).ToString();
            } catch (Exception) { }
            TyhjennaKentat();
        }

        private void tbVaaka_3_6_TextChanged(object sender, EventArgs e) {
            TyhjennaKentat();
        }

        private void tbKuorma_TextChanged(object sender, EventArgs e) {
            TyhjennaKentat();
        }

        private void tbHyoty_TextChanged(object sender, EventArgs e) {
            TyhjennaKentat();
        }
        #endregion
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e) {
            for (int i = 0; i < RECTANGLES; i++) {
                if (rectangles[i].Contains(e.Location)) {
                    Laskija f = null;
					try	{
						f = HaeLaskelmat(i);
					} catch (FormatException ex) {
						lIlmoitus.Text = ex.Message;
						return;
					} catch (Exception ex) {
						lokiKirjoittaja.KirjoitaLokiin(ex.StackTrace);
						return;
					}

                    #region virheenkorjaus
                    if (valikkoObjMomentti.Checked) {
                        f.VirheenkorjausMomentti = true;
                    } else {
                        f.VirheenkorjausMomentti = false;
                    }
                    if (valikkoObjJäykistys.Checked) {
                        f.VirheenkorjausJaykistys = true;
                    } else {
                        f.VirheenkorjausJaykistys = false;
                    }
                    #endregion virheenkorjaus
                    try {
                        f.AsetaArvot();
                        f.Show();
                    } catch (Exception ex) {
                        lIlmoitus.Text = ex.Message;
                    }
                    
                }
            }
        }

        /// <summary>
        /// Hakee oikean kentän numeron i perusteella. Tällä metodilla luodaan kaikki kentät
        /// </summary>
        /// <param name="i">Kentän numero - 1</param>
        /// <returns>Kentän, jolle lasketaan momentit</returns>
        private Kentta HaeKentta(int i) {
            Kentta kentta = null;
            if (kentat[i] != null) {
                return kentat[i];
            }
            int mittaVaaka_1_4 = (int)(double.Parse(tbVaaka_1_4.Text) * 1000);
            int mittaVaaka_2_5 = (int)(double.Parse(tbVaaka_2_5.Text) * 1000);
            int mittaVaaka_3_6 = (int)(double.Parse(tbVaaka_3_6.Text) * 1000);
            int mittaVaaka_7 = (int)(double.Parse(tbVaaka_7.Text) * 1000);
            int mittaPysty_1_2_3 = (int)(double.Parse(tbPysty_1_2_3.Text) * 1000);
            int mittaPysty_4_6 = (int)(double.Parse(tbPysty_4_6.Text) * 1000);
            int mittaPysty_5 = (int)(double.Parse(tbPysty_5.Text) * 1000);
            int mittaPysty_7 = (int)(double.Parse(tbPysty_7.Text) * 1000);           
            switch (++i) {
                case 1:
                    kentta = new Kentta(i, double.Parse(tbVaaka_1_4.Text), double.Parse(tbPysty_1_2_3.Text));
                    kentta.Tuentatapaus = Kentta.Tuentatapaukset.TukiaKaksiVierekkain;
                    kentta.VasenAlanurkka = new Point(0, mittaPysty_4_6);
                    if (double.Parse(tbVaaka_1_4.Text) <= double.Parse(tbPysty_1_2_3.Text)) {
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiL_PSivuMomentti, 0, "1.2");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiP_LSivuMomentti, 1, "1.4");
                    } else {
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiP_LSivuMomentti, 0, "1.2");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiL_PSivuMomentti, 1, "1.4");
                    }

                    break;
                case 2:
                    kentta = new Kentta(i, double.Parse(tbVaaka_2_5.Text), double.Parse(tbPysty_1_2_3.Text));
                    kentta.VasenAlanurkka = new Point(mittaVaaka_1_4, mittaPysty_4_6);
                    // Jos vaakapituus on lyhyempi, silloin tukematon sivu on lyhyempi
                    if (double.Parse(tbVaaka_2_5.Text) <= double.Parse(tbPysty_1_2_3.Text)) {
                        kentta.Tuentatapaus = Kentta.Tuentatapaukset.TukiaKolmeLSivuVapaa;
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia2PL_LSivuMomentti, 0, "2.1");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia2PL_LSivuMomentti, 1, "2.3");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia2PP_LSivuMomentti, 2, "2.5");
                    } else {
                        kentta.Tuentatapaus = Kentta.Tuentatapaukset.TukiaKolmePSivuVapaa;
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia2PL_PSivuMomentti, 0, "2.1");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia2PL_PSivuMomentti, 1, "2.3");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia2LL_PSivuMomentti, 2, "2.5");
                    }
                    break;
                case 3:
                    kentta = new Kentta(i, double.Parse(tbVaaka_3_6.Text), double.Parse(tbPysty_1_2_3.Text));
                    kentta.Tuentatapaus = Kentta.Tuentatapaukset.TukiaKaksiVierekkain;
                    kentta.VasenAlanurkka = new Point(mittaVaaka_1_4 + mittaVaaka_2_5, mittaPysty_4_6);
                    if (double.Parse(tbVaaka_1_4.Text) <= double.Parse(tbPysty_1_2_3.Text)) {
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiL_PSivuMomentti, 0, "3.2");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiP_LSivuMomentti, 1, "3.6");
                    } else {
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiP_LSivuMomentti, 0, "3.2");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiL_PSivuMomentti, 1, "3.6");
                    }
                    break;
                case 4:
                    kentta = new Kentta(i, double.Parse(tbVaaka_1_4.Text), double.Parse(tbPysty_4_6.Text), true);
                    kentta.Tuentatapaus = Kentta.Tuentatapaukset.TukiaKaksiVierekkain;
                    kentta.TuentatapaustenSuhde = double.Parse(tbPysty_5.Text) / double.Parse(tbPysty_4_6.Text);
                    kentta.VasenAlanurkka = new Point(0, 0);
                    if (double.Parse(tbVaaka_1_4.Text) <= double.Parse(tbPysty_4_6.Text)) {
                        kentta.ToinenTuenta = Kentta.Tuentatapaukset.TukiLyhyellaSivulla;
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiP_LSivuMomentti, 0, "4.1");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiL_PSivuMomentti, 1, "4.5");
                        kentta.AsetaToissijaisetJaykistysTapaukset(Kentta.Jaykistystapaukset.EiTukia_LSivuMomentti, 0);
                    } else {
                        kentta.ToinenTuenta = Kentta.Tuentatapaukset.TukiPitkallaSivulla;
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiL_PSivuMomentti, 0, "4.1");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiP_LSivuMomentti, 1, "4.5");
                        kentta.AsetaToissijaisetJaykistysTapaukset(Kentta.Jaykistystapaukset.EiTukia_PSivuMomentti, 0);
                    }
                    break;
                case 5:
                    kentta = new Kentta(i, double.Parse(tbVaaka_2_5.Text), double.Parse(tbPysty_5.Text), true);
                    kentta.Tuentatapaus = Kentta.Tuentatapaukset.TukiaNelja;
                    kentta.VasenAlanurkka = new Point(mittaVaaka_1_4, mittaPysty_7);
                    if (double.Parse(tbVaaka_2_5.Text) <= double.Parse(tbPysty_5.Text)) {
                        kentta.ToinenTuenta = Kentta.Tuentatapaukset.TukiaKolmeLSivuVapaa;
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia3_LSivuMomentti, 0, "5.2");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia3_PSivuMomentti, 1, "5.4");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia3_PSivuMomentti, 2, "5.6");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia3_LSivuMomentti, 3, "5.7");
                        kentta.AsetaToissijaisetJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia2PP_LSivuMomentti, 0);
                        kentta.AsetaToissijaisetJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia2PL_PSivuMomentti, 1);
                        kentta.AsetaToissijaisetJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia2PL_PSivuMomentti, 2);
                    } else {
                        kentta.ToinenTuenta = Kentta.Tuentatapaukset.TukiaKolmePSivuVapaa;
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia3_PSivuMomentti, 0, "5.2");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia3_LSivuMomentti, 1, "5.4");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia3_LSivuMomentti, 2, "5.6");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia3_PSivuMomentti, 3, "5.7");
                        kentta.AsetaToissijaisetJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia2LL_PSivuMomentti, 0);
                        kentta.AsetaToissijaisetJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia2PL_LSivuMomentti, 1);
                        kentta.AsetaToissijaisetJaykistysTapaukset(Kentta.Jaykistystapaukset.Tukia2PL_LSivuMomentti, 2);
                    }
                    kentta.TuentatapaustenSuhde = double.Parse(tbVaaka_7.Text) / double.Parse(tbVaaka_2_5.Text);
                    break;
                case 6:
                    kentta = new Kentta(i, double.Parse(tbVaaka_3_6.Text), double.Parse(tbPysty_4_6.Text));
                    kentta.Tuentatapaus = Kentta.Tuentatapaukset.TukiaKaksiVierekkain;
                    kentta.VasenAlanurkka = new Point(mittaVaaka_1_4 + mittaVaaka_2_5, 0);
                    if (double.Parse(tbVaaka_3_6.Text) <= double.Parse(tbPysty_4_6.Text)) {
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiP_LSivuMomentti, 0, "6.3");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiL_PSivuMomentti, 1, "6.7");
                    } else {
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiL_PSivuMomentti, 0, "6.3");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiP_LSivuMomentti, 1, "6.7");
                    }
                    break;
                case 7:
                    kentta = new Kentta(i, double.Parse(tbVaaka_7.Text), double.Parse(tbPysty_7.Text));
                    kentta.Tuentatapaus = Kentta.Tuentatapaukset.TukiaKaksiVierekkain;
                    kentta.VasenAlanurkka = new Point(mittaVaaka_1_4 + mittaVaaka_2_5 - mittaVaaka_7, 0);
                    if (double.Parse(tbVaaka_7.Text) <= double.Parse(tbPysty_7.Text)) {
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiP_LSivuMomentti, 0, "7.5");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiL_PSivuMomentti, 1, "7.6");
                    } else {
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiL_PSivuMomentti, 0, "7.5");
                        kentta.AsetaJaykistysTapaukset(Kentta.Jaykistystapaukset.TukiP_LSivuMomentti, 1, "7.6");
                    }
                    break;
            }
            kentat[--i] = kentta;
            return kentta;
        }

        private Laskija HaeLaskelmat(int i) {
            Kentta kentta = null;
            double hyotykuorma;
            try {
                kentta = HaeKentta(i);
            } catch (FormatException) {
				throw new FormatException("Mitat ei kelpaa! Tekstilaatikot saavat sisältää vain numeroarvoja");
			}
            try {
                kuormat = double.Parse(tbKuorma.Text);
                hyotykuorma = double.Parse(tbHyoty.Text);
            } catch (FormatException) {
                throw new FormatException("Kuormia ei annettu tai virheellinen syöte!");
            }
            if (kentta == null) {
                throw new Exception();
            }
            return new Laskija(kentta, kuormat, hyotykuorma);
        }

        private void MainWindow_Load(object sender, EventArgs e) {
            rectangles[0] = new Rectangle(65, 43, 156, 158);
            rectangles[1] = new Rectangle(221, 43, 152, 158);
            rectangles[2] = new Rectangle(373, 43, 122, 158);
            rectangles[3] = new Rectangle(65, 200, 156, 189);
            rectangles[4] = new Rectangle(221, 200, 152, 90);
            rectangles[5] = new Rectangle(373, 200, 122, 189);
            rectangles[6] = new Rectangle(285, 290, 86, 100);
            mitatVaaka = new TextBox[] { tbVaaka_1_4, tbVaaka_2_5, tbVaaka_3_6, tbVaaka_7 };
            mitatPysty = new TextBox[] { tbPysty_1_2_3, tbPysty_4_6 };
        }
        
        private void bNollaa_Click(object sender, EventArgs e) {
            foreach (TextBox t in mitatVaaka) {
                t.Text = "";
            }
            foreach (TextBox t in mitatPysty) {
                t.Text = "";
            }
            tbPysty_5.Text = "";
            tbPysty_7.Text = "";
            tbKuorma.Text = "";
            tbHyoty.Text = "";
            l_horizontal_reika.Text = "";
            l_horizontal_3_6.Text = "";
            l_vertical_1_2_3.Text = "";
            pictureBox1.Refresh();
            TyhjennaKentat();
            tasauksetLaskettu = false;
            asetukset.AsetaAlkuperaiset();
        }

        private void bEsimerkki_Click(object sender, EventArgs e) {
            tbVaaka_1_4.Text = "6,0";
            tbVaaka_2_5.Text = "6,0";
            tbVaaka_3_6.Text = "4,8";
            tbVaaka_7.Text = "3,3";
            tbPysty_1_2_3.Text = "6,0";
            tbPysty_4_6.Text = "7,2";
            tbKuorma.Text = "12,4";
            tbHyoty.Text = "4,0";
        }

        private void bOmatArvot_Click(object sender, EventArgs e) {
            tbVaaka_1_4.Text = "5,0";
            tbVaaka_2_5.Text = "6,0";
            tbVaaka_3_6.Text = "4,0";
            tbVaaka_7.Text = "4,0";
            tbPysty_1_2_3.Text = "6,0";
            tbPysty_4_6.Text = "8,0";
            tbKuorma.Text = "11,4";
            tbHyoty.Text = "4,5";
        }

        private void bTasausarvot_Click(object sender, EventArgs e) {

            pictureBox1.Refresh();
            #region AlustaMomentit
            // Laskee ja lataa momentit ja jäykkyysarvot muistiin
            Dictionary<int, ArrayList> kentatTemp = new Dictionary<int, ArrayList>();
            for (int i = 0; i < 7; i++) {
                Kentta kenttaTemp = null;
                try {
                    kenttaTemp = HaeKentta(i);
                } catch (Exception) {
                    lIlmoitus.Text = "Mitat ei kelpaa! Tekstilaatikot saavat sisältää vain numeroarvoja";
                    return;
                }
                double hyotykuormat = 0;
                try {
                    kuormat = double.Parse(tbKuorma.Text);
                    hyotykuormat = double.Parse(tbHyoty.Text);
                } catch (Exception) {
                    lIlmoitus.Text = "Kuormia ei annettu tai virheellinen syöte!";
                    return;
                }
                Laskija l = new Laskija(kenttaTemp, kuormat, hyotykuormat);
                try {
                    l.AsetaArvot();
                } catch (Exception ex) {
                    lIlmoitus.Text = ex.Message;
                    return;
                }
                
                kentatTemp.Add(i, new ArrayList());
                for (int j = 0; j < 4; j++) {
                    kentatTemp[i].Add(l.M_Tekstit[j]);
                }
                for (int j = 0; j < 4; j++) {
                    kentatTemp[i].Add(l.K_Tekstit[j]);
                }
                l.Dispose();
            }
            #endregion

            Graphics g = pictureBox1.CreateGraphics();


            double deltaM;
            double[] kenttaM1 = new double[9];
            double[] kenttaM2 = new double[9];
            double[] deltaM1 = new double[9];
            double[] deltaM2 = new double[9];
            double[] m = new double[18];
            Kentta kentta1 = HaeKentta(0);
            int toinenKentta = int.Parse(kentta1.HaeJaykistysTapausNimi(0).Substring(kentta1.HaeJaykistysTapausNimi(0).Length - 1, 1));
            Kentta kentta2 = HaeKentta(--toinenKentta);

            // 1.2
            kenttaM1[0] = HaeKenttaMVaaka(0, tbVaaka_1_4.Text, tbPysty_1_2_3.Text, kentatTemp);
            kenttaM2[0] = HaeKenttaMVaaka(1, tbVaaka_2_5.Text, tbPysty_1_2_3.Text, kentatTemp);
            // 1.4
            kenttaM1[1] = HaeKenttaMPysty(0, tbVaaka_1_4.Text, tbPysty_1_2_3.Text, kentatTemp);
            kenttaM2[1] = HaeKenttaMPysty(3, tbVaaka_1_4.Text, tbPysty_4_6.Text, kentatTemp);
            // 2.3
            kenttaM1[2] = HaeKenttaMVaaka(1, tbVaaka_2_5.Text, tbPysty_1_2_3.Text, kentatTemp);
            kenttaM2[2] = HaeKenttaMVaaka(2, tbVaaka_3_6.Text, tbPysty_1_2_3.Text, kentatTemp);
            // 4.5
            kenttaM1[3] = HaeKenttaMVaaka(3, tbVaaka_1_4.Text, tbPysty_4_6.Text, kentatTemp);
            kenttaM2[3] = HaeKenttaMVaaka(4, tbVaaka_2_5.Text, tbPysty_5.Text, kentatTemp);
            // 2.5
            kenttaM1[4] = HaeKenttaMPysty(1, tbVaaka_2_5.Text, tbPysty_1_2_3.Text, kentatTemp);
            kenttaM2[4] = HaeKenttaMPysty(4, tbVaaka_2_5.Text, tbPysty_5.Text, kentatTemp);
            // 3.6
            kenttaM1[5] = HaeKenttaMPysty(2, tbVaaka_3_6.Text, tbPysty_1_2_3.Text, kentatTemp);
            kenttaM2[5] = HaeKenttaMPysty(5, tbVaaka_3_6.Text, tbPysty_4_6.Text, kentatTemp);
            // 5.6
            kenttaM1[6] = HaeKenttaMVaaka(4, tbVaaka_2_5.Text, tbPysty_5.Text, kentatTemp);
            kenttaM2[6] = HaeKenttaMVaaka(5, tbVaaka_3_6.Text, tbPysty_4_6.Text, kentatTemp);
            // 6.7
            kenttaM1[7] = HaeKenttaMVaaka(5, tbVaaka_3_6.Text, tbPysty_4_6.Text, kentatTemp);
            kenttaM2[7] = HaeKenttaMVaaka(6, tbVaaka_7.Text, tbPysty_7.Text, kentatTemp);
            // 5.7 (koska ei ole viidettä m:n arvoa laskettu, se pitää laskea erikseen
            #region 5.7mxs2
            double leveys = double.Parse(tbVaaka_2_5.Text);
            double korkeus = double.Parse(tbPysty_5.Text);
            double sivusuhde = Math.Max(leveys, korkeus) / Math.Min(leveys, korkeus);
            double pyoristysYlos = Math.Ceiling(sivusuhde * 10) / 10;
            double pyoristysAlas = Math.Floor(sivusuhde * 10) / 10;
            double interpolointikerroin = sivusuhde * 100 % 100 / 100;
            Laskija l1 = new Laskija(HaeKentta(4), kuormat);
            double alfa = l1.LaskeAlfa(pyoristysYlos, pyoristysAlas, interpolointikerroin, 1);
            double kuorma = kuormat * Math.Pow(Math.Min(leveys, korkeus), 2);
            kenttaM1[8] = alfa / 10000 * kuorma;
            #endregion
            kenttaM2[8] = HaeKenttaMPysty(6, tbVaaka_7.Text, tbPysty_7.Text, kentatTemp);

            #region Kentta1-2_2-1      
            deltaM = kenttaM1[0] - kenttaM2[0];

            // Haetaan kenttien k-arvot kentat-listasta (k-arvot alkaa neljännestä alkiosta ja päättyy seitsemänteen)
            double kentta1_k = double.Parse((string)kentatTemp[0][4]);
            double kentta2_k = double.Parse((string)kentatTemp[1][4]);
            // Otetaan kaikki deltaM1 ja deltaM2 arvot taulukoihin eri laskuista aloittaen nollasta ja lisäten yhdellä joka askeleella
            // (käytetään deltaM2 arvoa laskiessa toisarvoista momenttia)
            deltaM1[0] = (kentta1_k / (kentta1_k + kentta2_k)) * deltaM;
            deltaM2[0] = deltaM - deltaM1[0];
            // m taulukko sisältää kaikille ominaiset momentit omilla alkioillaan
            m[0] = kenttaM1[0] - deltaM1[0];
            m[1] = kenttaM2[0] + deltaM2[0];
            // Asettaa kentälle tasauksen (0 = vasen, 1 = ylös, 2 = oikea, 3 = alas)
            this.kentat[0].AsetaTasaus(deltaM1[0], 2);
            this.kentat[1].AsetaTasaus(deltaM2[0] * (-1), 0);

            // Piirretään momentti 1.2
            // Point on tekstin sijainti joka on neliön oikeassa reunassa ja Y-suunnassa keskellä

            if (kenttaM1[0] > kenttaM2[0]) {
                // grafiikkaolio, kenttaM1, deltaM1, m-taulukko, ensisijaisen kentän momentin indeksi, deltaM1 ja deltaM2 tämänhetkinen indeksi, suorakulmion indeksi johon teksti piirretään
                PiirraTekstitOikea(g, kenttaM1, deltaM1, m, 0, 0, 0, false);

                // Piirretään momentti 2.1 (Laskettu deltaM2:lla ensimmäisessä laskussa, siksi deltaM2[0])
                PiirraTekstitVasen(g, kenttaM2, deltaM2, m, 1, 0, 1, true);
            } else {
                PiirraTekstitOikea(g, kenttaM1, deltaM1, m, 0, 0, 0, true);
                PiirraTekstitVasen(g, kenttaM2, deltaM2, m, 1, 0, 1, false);
            }

            #endregion

            #region Kentta__1-4_4-1
            kentta1 = HaeKentta(0);
            toinenKentta = int.Parse(kentta1.HaeJaykistysTapausNimi(1).Substring(kentta1.HaeJaykistysTapausNimi(1).Length - 1, 1));
            kentta2 = HaeKentta(--toinenKentta);

            deltaM = kenttaM1[1] - kenttaM2[1];
            kentta1_k = double.Parse((string)kentatTemp[0][5]);
            kentta2_k = double.Parse((string)kentatTemp[3][4]);
            deltaM1[1] = kentta1_k / (kentta1_k + kentta2_k) * deltaM;
            deltaM2[1] = deltaM - deltaM1[1];
            m[2] = kenttaM1[1] - deltaM1[1];
            m[3] = kenttaM2[1] + deltaM2[1];
            this.kentat[0].AsetaTasaus(deltaM1[1], 3);
            this.kentat[3].AsetaTasaus(deltaM2[1] * (-1), 1);
            if (kenttaM1[1] > kenttaM2[1]) {
                PiirraTekstitAlas(g, kenttaM1, deltaM1, m, 2, 1, 0, false);
                PiirraTekstitYlos(g, kenttaM2, deltaM2, m, 3, 1, 3, true);
            } else {
                PiirraTekstitAlas(g, kenttaM1, deltaM1, m, 2, 1, 0, true);
                PiirraTekstitYlos(g, kenttaM2, deltaM2, m, 3, 1, 3, false);
            }

            #endregion

            #region Kentta__2-3_3-2
            kentta1 = HaeKentta(1);
            toinenKentta = int.Parse(kentta1.HaeJaykistysTapausNimi(1).Substring(kentta1.HaeJaykistysTapausNimi(1).Length - 1, 1));
            kentta2 = HaeKentta(--toinenKentta);

            deltaM = kenttaM1[2] - kenttaM2[2];
            kentta1_k = double.Parse((string)kentatTemp[1][5]);
            kentta2_k = double.Parse((string)kentatTemp[2][4]);
            deltaM1[2] = kentta1_k / (kentta1_k + kentta2_k) * deltaM;
            deltaM2[2] = deltaM - deltaM1[2];
            m[4] = kenttaM1[2] - deltaM1[2];
            m[5] = kenttaM2[2] + deltaM2[2];
            this.kentat[1].AsetaTasaus(deltaM1[2], 2);
            this.kentat[2].AsetaTasaus(deltaM2[2] * (-1), 0);
            if (kenttaM1[2] > kenttaM2[2]) {
                PiirraTekstitOikea(g, kenttaM1, deltaM1, m, 4, 2, 1, false);
                PiirraTekstitVasen(g, kenttaM2, deltaM2, m, 5, 2, 2, true);
            } else {
                PiirraTekstitOikea(g, kenttaM1, deltaM1, m, 4, 2, 1, true);
                PiirraTekstitVasen(g, kenttaM2, deltaM2, m, 5, 2, 2, false);
            }

            #endregion

            #region Kentta__4-5_5-4
            kentta1 = HaeKentta(3);
            toinenKentta = int.Parse(kentta1.HaeJaykistysTapausNimi(1).Substring(kentta1.HaeJaykistysTapausNimi(1).Length - 1, 1));
            kentta2 = HaeKentta(--toinenKentta);

            deltaM = kenttaM1[3] - kenttaM2[3];
            kentta1_k = double.Parse((string)kentatTemp[3][5]);
            kentta2_k = double.Parse((string)kentatTemp[4][5]);
            deltaM1[3] = kentta1_k / (kentta1_k + kentta2_k) * deltaM;
            deltaM2[3] = deltaM - deltaM1[3];
            m[6] = kenttaM1[3] - deltaM1[3];
            m[7] = kenttaM2[3] + deltaM2[3];
            this.kentat[3].AsetaTasaus(deltaM1[3], 2);
            this.kentat[4].AsetaTasaus(deltaM2[3] * (-1), 0);
            if (kenttaM1[3] > kenttaM2[3]) {
                PiirraTekstitOikea(g, kenttaM1, deltaM1, m, 6, 3, 3, false, 50);
                PiirraTekstitVasen(g, kenttaM2, deltaM2, m, 7, 3, 4, true);
            } else {
                PiirraTekstitOikea(g, kenttaM1, deltaM1, m, 6, 3, 3, true, 50);
                PiirraTekstitVasen(g, kenttaM2, deltaM2, m, 7, 3, 4, false);
            }
            #endregion

            #region Kentta__2-5_5-2
            kentta1 = HaeKentta(1);
            toinenKentta = int.Parse(kentta1.HaeJaykistysTapausNimi(2).Substring(kentta1.HaeJaykistysTapausNimi(2).Length - 1, 1));
            kentta2 = HaeKentta(--toinenKentta);

            deltaM = kenttaM1[4] - kenttaM2[4];
            kentta1_k = double.Parse((string)kentatTemp[1][6]);
            kentta2_k = double.Parse((string)kentatTemp[4][4]);
            deltaM1[4] = kentta1_k / (kentta1_k + kentta2_k) * deltaM;
            deltaM2[4] = deltaM - deltaM1[4];
            m[8] = kenttaM1[4] - deltaM1[4];
            m[9] = kenttaM2[4] + deltaM2[4];
            this.kentat[1].AsetaTasaus(deltaM1[4], 3);
            this.kentat[4].AsetaTasaus(deltaM2[4] * (-1), 1);
            if (kenttaM1[4] > kenttaM2[4]) {
                PiirraTekstitAlas(g, kenttaM1, deltaM1, m, 8, 4, 1, false);
                PiirraTekstitYlos(g, kenttaM2, deltaM2, m, 9, 4, 4, true, 10);
            } else {
                PiirraTekstitAlas(g, kenttaM1, deltaM1, m, 8, 4, 1, true);
                PiirraTekstitYlos(g, kenttaM2, deltaM2, m, 9, 4, 4, false, 10);
            }
            #endregion

            #region Kentta__3-6_6-3
            kentta1 = HaeKentta(2);
            toinenKentta = int.Parse(kentta1.HaeJaykistysTapausNimi(1).Substring(kentta1.HaeJaykistysTapausNimi(1).Length - 1, 1));
            kentta2 = HaeKentta(--toinenKentta);

            deltaM = kenttaM1[5] - kenttaM2[5];
            kentta1_k = double.Parse((string)kentatTemp[2][5]);
            kentta2_k = double.Parse((string)kentatTemp[5][4]);
            deltaM1[5] = kentta1_k / (kentta1_k + kentta2_k) * deltaM;
            deltaM2[5] = deltaM - deltaM1[5];
            m[10] = kenttaM1[5] - deltaM1[5];
            m[11] = kenttaM2[5] + deltaM2[5];
            this.kentat[2].AsetaTasaus(deltaM1[5], 3);
            this.kentat[5].AsetaTasaus(deltaM2[5] * (-1), 1);
            if (kenttaM1[5] > kenttaM2[5]) {
                PiirraTekstitAlas(g, kenttaM1, deltaM1, m, 10, 5, 2, false);
                PiirraTekstitYlos(g, kenttaM2, deltaM2, m, 11, 5, 5, true);
            } else {
                PiirraTekstitAlas(g, kenttaM1, deltaM1, m, 10, 5, 2, true);
                PiirraTekstitYlos(g, kenttaM2, deltaM2, m, 11, 5, 5, false);
            }
            #endregion

            #region Kentta__5-6_6-5
            kentta1 = HaeKentta(4);
            toinenKentta = int.Parse(kentta1.HaeJaykistysTapausNimi(2).Substring(kentta1.HaeJaykistysTapausNimi(2).Length - 1, 1));
            kentta2 = HaeKentta(--toinenKentta);

            deltaM = kenttaM1[6] - kenttaM2[6];
            kentta1_k = double.Parse((string)kentatTemp[4][6]);
            kentta2_k = double.Parse((string)kentatTemp[5][5]);
            deltaM1[6] = kentta1_k / (kentta1_k + kentta2_k) * deltaM;
            deltaM2[6] = deltaM - deltaM1[6];
            m[12] = kenttaM1[6] - deltaM1[6];
            m[13] = kenttaM2[6] + deltaM2[6];
            this.kentat[4].AsetaTasaus(deltaM1[6], 2);
            this.kentat[5].AsetaTasaus(deltaM2[6] * (-1), 0);
            if (kenttaM1[6] > kenttaM2[6]) {
                PiirraTekstitOikea(g, kenttaM1, deltaM1, m, 12, 6, 4, false);
                PiirraTekstitVasen(g, kenttaM2, deltaM2, m, 13, 6, 5, true, 50);
            } else {
                PiirraTekstitOikea(g, kenttaM1, deltaM1, m, 12, 6, 4, true);
                PiirraTekstitVasen(g, kenttaM2, deltaM2, m, 13, 6, 5, false, 50);
            }
            #endregion

            #region Kentta__6-7_7-6
            kentta1 = HaeKentta(5);
            toinenKentta = int.Parse(kentta1.HaeJaykistysTapausNimi(1).Substring(kentta1.HaeJaykistysTapausNimi(1).Length - 1, 1));
            kentta2 = HaeKentta(--toinenKentta);

            deltaM = kenttaM1[7] - kenttaM2[7];
            kentta1_k = double.Parse((string)kentatTemp[5][5]);
            kentta2_k = double.Parse((string)kentatTemp[6][5]);
            deltaM1[7] = kentta1_k / (kentta1_k + kentta2_k) * deltaM;
            deltaM2[7] = deltaM - deltaM1[7];
            m[14] = kenttaM1[7] - deltaM1[7];
            m[15] = kenttaM2[7] + deltaM2[7];
            this.kentat[5].AsetaToissijainenTasaus(deltaM1[7], 0);
            this.kentat[6].AsetaTasaus(deltaM2[7] * (-1), 2);

            if (kenttaM1[7] > kenttaM2[7]) {
                PiirraTekstitVasen(g, kenttaM1, deltaM1, m, 14, 7, 5, false, -45);
                PiirraTekstitOikea(g, kenttaM2, deltaM2, m, 15, 7, 6, true);
            } else {
                PiirraTekstitVasen(g, kenttaM1, deltaM1, m, 14, 7, 5, true, -45);
                PiirraTekstitOikea(g, kenttaM2, deltaM2, m, 15, 7, 6, false);
            }
            #endregion

            #region Kentta__5-7_7-5
            kentta1 = HaeKentta(4);
            toinenKentta = int.Parse(kentta1.HaeJaykistysTapausNimi(3).Substring(kentta1.HaeJaykistysTapausNimi(3).Length - 1, 1));
            kentta2 = HaeKentta(--toinenKentta);

            deltaM = kenttaM1[8] - kenttaM2[8];
            kentta1_k = double.Parse((string)kentatTemp[4][7]);
            kentta2_k = double.Parse((string)kentatTemp[6][4]);
            deltaM1[8] = kentta1_k / (kentta1_k + kentta2_k) * deltaM;
            deltaM2[8] = deltaM - deltaM1[8];
            m[16] = kenttaM1[8] - deltaM1[8];
            m[17] = kenttaM2[8] + deltaM2[8];
            this.kentat[4].AsetaTasaus(deltaM1[8], 3);
            this.kentat[6].AsetaTasaus(deltaM2[8] * -1, 1);
            if (kenttaM1[8] > kenttaM2[8]) {
                PiirraTekstitAlas(g, kenttaM1, deltaM1, m, 16, 8, 4, false, 10);
                PiirraTekstitYlos(g, kenttaM2, deltaM2, m, 17, 8, 6, true, -10);
            } else {
                PiirraTekstitAlas(g, kenttaM1, deltaM1, m, 16, 8, 4, true, 10);
                PiirraTekstitYlos(g, kenttaM2, deltaM2, m, 17, 8, 6, false, -10);
            }
            #endregion

            #region virheenkorjausT1
            if (valikkoObjTasausarvot.Checked &&
                    MessageBox.Show("Näytetäänkö kaikki momenttien tasauksien arvot?",
                    "Momenttien tasaus",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question).
                    Equals(DialogResult.OK)) {
                #region virheenKorjaus0
                if (valikkoObjTasausarvot.Checked) {
                    MessageBox.Show("m1: " + kenttaM1[0] + "\nm2: " + kenttaM2[0] + "\nk1: " +
                        kentta1_k + "\nk2: " + kentta2_k + "\nDelta M: " + deltaM + "\nDelta M1: " +
                        deltaM1[0] + "\nDelta M2: " + deltaM2[0] + "\nm1.2: " + m[0] + "\nm2.1: " + m[1], "1-2");
                }
                #endregion virheenKorjaus
                #region virheenKorjaus1
                if (valikkoObjTasausarvot.Checked) {
                    MessageBox.Show("m1: " + kenttaM1[1] + "\nm2: " + kenttaM2[1] + "\nk1: " +
                        kentta1_k + "\nk2: " + kentta2_k + "\nDelta M: " + deltaM + "\nDelta M1: " +
                        deltaM1[1] + "\nDelta M2: " + deltaM2[1] + "\nm1.2: " + m[2] + "\nm2.1: " + m[3], "1-4");
                }
                #endregion virheenKorjaus
                #region virheenKorjaus2
                if (valikkoObjTasausarvot.Checked) {
                    MessageBox.Show("m1: " + kenttaM1[2] + "\nm2: " + kenttaM2[2] + "\nk1: " +
                        kentta1_k + "\nk2: " + kentta2_k + "\nDelta M: " + deltaM + "\nDelta M1: " +
                        deltaM1[2] + "\nDelta M2: " + deltaM2[2] + "\nm1.2: " + m[4] + "\nm2.1: " + m[5], "2-3");
                }
                #endregion virheenKorjaus
                #region virheenKorjaus3
                if (valikkoObjTasausarvot.Checked) {
                    MessageBox.Show("m1: " + kenttaM1[3] + "\nm2: " + kenttaM2[3] + "\nk1: " +
                        kentta1_k + "\nk2: " + kentta2_k + "\nDelta M: " + deltaM + "\nDelta M1: " +
                        deltaM1[3] + "\nDelta M2: " + deltaM2[3] + "\nm1.2: " + m[6] + "\nm2.1: " + m[7], "4-5");
                }
                #endregion virheenKorjaus

                #region virheenKorjaus4
                if (valikkoObjTasausarvot.Checked) {
                    MessageBox.Show("m1: " + kenttaM1[4] + "\nm2: " + kenttaM2[4] + "\nk1: " +
                        kentta1_k + "\nk2: " + kentta2_k + "\nDelta M: " + deltaM + "\nDelta M1: " +
                        deltaM1[4] + "\nDelta M2: " + deltaM2[4] + "\nm1.2: " + m[8] + "\nm2.1: " + m[9], "2-5");
                }
                #endregion virheenKorjaus
                #region virheenKorjaus5
                if (valikkoObjTasausarvot.Checked) {
                    MessageBox.Show("m1: " + kenttaM1[5] + "\nm2: " + kenttaM2[5] + "\nk1: " +
                        kentta1_k + "\nk2: " + kentta2_k + "\nDelta M: " + deltaM + "\nDelta M1: " +
                        deltaM1[5] + "\nDelta M2: " + deltaM2[5] + "\nm1.2: " + m[10] + "\nm2.1: " + m[11], "3-6");
                }
                #endregion virheenKorjaus
                #region virheenKorjaus6
                if (valikkoObjTasausarvot.Checked) {
                    MessageBox.Show("m1: " + kenttaM1[6] + "\nm2: " + kenttaM2[6] + "\nk1: " +
                        kentta1_k + "\nk2: " + kentta2_k + "\nDelta M: " + deltaM + "\nDelta M1: " +
                        deltaM1[6] + "\nDelta M2: " + deltaM2[6] + "\nm1.2: " + m[12] + "\nm2.1: " + m[13], "5-6");
                }
                #endregion virheenKorjaus
                #region virheenKorjaus7
                if (valikkoObjTasausarvot.Checked) {
                    MessageBox.Show("m1: " + kenttaM1[7] + "\nm2: " + kenttaM2[7] + "\nk1: " +
                        kentta1_k + "\nk2: " + kentta2_k + "\nDelta M: " + deltaM + "\nDelta M1: " +
                        deltaM1[7] + "\nDelta M2: " + deltaM2[7] + "\nm1.2: " + m[14] + "\nm2.1: " + m[15], "6-7");
                }
                #endregion virheenKorjaus
                #region virheenKorjaus8
                if (valikkoObjTasausarvot.Checked) {
                    MessageBox.Show("m1: " + kenttaM1[8] + "\nm2: " + kenttaM2[8] + "\nk1: " +
                        kentta1_k + "\nk2: " + kentta2_k + "\nDelta M: " + deltaM + "\nDelta M1: " +
                        deltaM1[8] + "\nDelta M2: " + deltaM2[8] + "\nm1.2: " + m[16] + "\nm2.1: " + m[17] +
                        "\n\tmxs2\nSivusuhde: " + sivusuhde + "\nPyöristys Ylös: " + pyoristysYlos + "\nPyöristys alas: " + pyoristysAlas +
                        "\nInterpolointikerroin: " + interpolointikerroin + "\nAlfa: " + alfa + "\nKuorma: " + kuorma, "5-7");
                }
                #endregion virheenKorjaus
            }
            #endregion

            AsetaTukimomentitKentille(m);
            tasauksetLaskettu = true;

            // Tehdään laskelmat uudelleen, että saadaan myös lopulliset kenttämomentit laskettua kun deltat on paikoillaan
            for (int i = 0; i < kentat.Length; i++) {
                try {
                    HaeLaskelmat(i).AsetaArvot();
                } catch (Exception ex) {
                    lIlmoitus.Text = ex.Message;
                }
                
            }
            lIlmoitus.Text = "Arvot laskettu!";

            #region virheenkorjausT2
            if (valikkoObjTasausarvot.Checked &&
                    MessageBox.Show("Näytetäänkö jokaisen kentän momentit ja jäykkyydet?",
                    "",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question).Equals(DialogResult.OK)) {

                int laskuri = 1;
                foreach (ArrayList a in kentatTemp.Values) {
                    MessageBox.Show(
                        "mys: " + a[0] +
                        "\nmxs: " + a[1] +
                        "\nmyf: " + a[2] +
                        "\nmxf: " + a[3] +
                        "\nk1: " + a[4] +
                        "\nk2: " + a[5] +
                        "\nk3: " + a[6] +
                        "\nk4: " + a[7],
                        laskuri.ToString());
                    laskuri++;
                }
            }
            #endregion virheenkorjausT
        }

        /// <summary>
        /// Asettaa tasatut tukimomentit kentille oikeisiin suuntiin
        /// </summary>
        /// <param name="m">Tukimomenttien arvotaulukko</param>
        private void AsetaTukimomentitKentille(double[] m) {
            for (int i = 0; i < kentat.Length; i++) {
                kentat[i].HaeTasatutTukimomentit().Clear();
            }
            // Kenttä 1 - oikealle
            kentat[0].AsetaTasattuTukimomentti("oikea", m[0]);
            // Kenttä 1 - alas
            kentat[0].AsetaTasattuTukimomentti("alas", m[2]);

            // KENTTÄ 2
            // Kenttä 2 - vasen
            kentat[1].AsetaTasattuTukimomentti("vasen", m[1]);
            // Kenttä 2 - oikea
            kentat[1].AsetaTasattuTukimomentti("oikea", m[4]);
            // Kenttä 2 - alas
            kentat[1].AsetaTasattuTukimomentti("alas", m[8]);

            // KENTTÄ 3
            // Kenttä 3 - vasen
            kentat[2].AsetaTasattuTukimomentti("vasen", m[5]);
            // Kenttä 3 - alas
            kentat[2].AsetaTasattuTukimomentti("alas", m[10]);

            // KENTTÄ 4
            // Kenttä 4 - ylös
            kentat[3].AsetaTasattuTukimomentti("ylos", m[3]);
            // Kenttä 4 - oikea
            kentat[3].AsetaTasattuTukimomentti("oikea", m[6]);

            // KENTTÄ 5
            // Kenttä 5 - vasen
            kentat[4].AsetaTasattuTukimomentti("vasen", m[7]);
            // Kenttä 5 - ylös
            kentat[4].AsetaTasattuTukimomentti("ylos", m[9]);
            // Kenttä 5 - oikea
            kentat[4].AsetaTasattuTukimomentti("oikea", m[12]);
            // Kenttä 5 - alas
            kentat[4].AsetaTasattuTukimomentti("alas", m[16]);

            // KENTTÄ 6
            // Kenttä 6 - vasen
            kentat[5].AsetaTasattuTukimomentti("vasen", m[13]);
            // Kenttä 6 - ylös
            kentat[5].AsetaTasattuTukimomentti("ylos", m[11]);
            // Kenttä 6 - vasen (alempi, 6->7)
            kentat[5].AsetaTasattuTukimomentti("oikea_toissijainen", m[14]);

            // KENTTÄ 7
            // Kenttä 7 - ylös
            kentat[6].AsetaTasattuTukimomentti("ylos", m[17]);
            // Kenttä 7 - oikea
            kentat[6].AsetaTasattuTukimomentti("oikea", m[15]);
        }

        #region Tekstien piirto
        /// <summary>
        /// Piirtää momenttien tasaukset kuvaan annetun suorakulmion oikealle reunalle
        /// </summary>
        /// <param name="g">Kuvan grafiikkaolio</param>
        /// <param name="kenttaM">Kentän alkup. momentti</param>
        /// <param name="deltaM">Kentältä miinustettava arvo</param>
        /// <param name="m">Kentän lopullinen momentti</param>
        /// <param name="mIndx">Kentän momentin indeksi (esim. 1.2 = 0, 1.4 = 1 jne.)</param>
        /// <param name="indeksi">Monesko kenttä kyseessä (esim 1.2 & 1.4 = 0, 2.3 & 2.5 = 1 jne.)</param>
        /// <param name="rectIndx">Piirrettävän suorakulmion indeksi</param>
        private void PiirraTekstitOikea(Graphics g, double[] kenttaM, double[] deltaM, double[] m, int mIndx, int indeksi, int rectIndx, bool plus, int lisays = 0) {
            g.DrawString(Math.Round(kenttaM[indeksi], 2).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + rectangles[rectIndx].Width - 37, rectangles[rectIndx].Y + rectangles[rectIndx].Height / 2 - 13 - lisays));
            if (!plus) {
                g.DrawString("-" + Math.Abs(Math.Round(-1 * deltaM[indeksi], 2)).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + rectangles[rectIndx].Width - 37, rectangles[rectIndx].Y + rectangles[rectIndx].Height / 2 - lisays));
            } else {
                g.DrawString("+" + Math.Abs(Math.Round(-1 * deltaM[indeksi], 2)).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + rectangles[rectIndx].Width - 37, rectangles[rectIndx].Y + rectangles[rectIndx].Height / 2 - lisays));
            }
            g.DrawString(Math.Round(m[mIndx], 2).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + rectangles[rectIndx].Width - 37, rectangles[rectIndx].Y + rectangles[rectIndx].Height / 2 + 13 - lisays));
        }

        /// <summary>
        /// Piirtää momenttien tasaukset kuvaan annetun suorakulmion oikealle reunalle
        /// </summary>
        /// <param name="g">Kuvan grafiikkaolio</param>
        /// <param name="kenttaM">Kentän alkup. momentti</param>
        /// <param name="deltaM">Kentältä miinustettava arvo</param>
        /// <param name="m">Kentän lopullinen momentti</param>
        /// <param name="mIndx">Kentän momentin indeksi (esim. 1.2 = 0, 1.4 = 1 jne.)</param>
        /// <param name="indeksi">Monesko kenttä kyseessä (esim 1.2 & 1.4 = 0, 2.3 & 2.5 = 1 jne.)</param>
        /// <param name="rectIndx">Piirrettävän suorakulmion indeksi</param>
        private void PiirraTekstitYlos(Graphics g, double[] kenttaM, double[] deltaM, double[] m, int mIndx, int indeksi, int rectIndx, bool plus, int lisays = 0) {
            g.DrawString(Math.Round(kenttaM[indeksi], 2).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + rectangles[rectIndx].Width / 2 - 10 + lisays, rectangles[rectIndx].Y + 3));
            if (!plus) {
                g.DrawString("-" + Math.Abs(Math.Round(-1 * deltaM[indeksi], 2)).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + rectangles[rectIndx].Width / 2 - 10 + lisays, rectangles[rectIndx].Y + 16));
            } else {
                g.DrawString("+" + Math.Abs(Math.Round(-1 * deltaM[indeksi], 2)).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + rectangles[rectIndx].Width / 2 - 10 + lisays, rectangles[rectIndx].Y + 16));
            }
            g.DrawString(Math.Round(m[mIndx], 2).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + rectangles[rectIndx].Width / 2 - 10 + lisays, rectangles[rectIndx].Y + 29));
        }

        /// <summary>
        /// Piirtää momenttien tasaukset kuvaan annetun suorakulmion alareunalle
        /// </summary>
        /// <param name="g">Kuvan grafiikkaolio</param>
        /// <param name="kenttaM">Kentän alkup. momentti</param>
        /// <param name="deltaM">Kentältä miinustettava arvo</param>
        /// <param name="m">Kentän lopullinen momentti</param>
        /// <param name="mIndx">Kentän momentin indeksi (esim. 1.2 = 0, 1.4 = 1 jne.)</param>
        /// <param name="indeksi">Monesko kenttä kyseessä (esim 1.2 & 1.4 = 0, 2.3 & 2.5 = 1 jne.)</param>
        /// <param name="rectIndx">Piirrettävän suorakulmion indeksi</param>
        /// <param name="plus">Jos arvo on true, laitetaan plus-merkki deltaM arvon eteen, muuten miinus-merkki</param>
        private void PiirraTekstitAlas(Graphics g, double[] kenttaM, double[] deltaM, double[] m, int mIndx, int indeksi, int rectIndx, bool plus, int lisays = 0) {
            g.DrawString(Math.Round(kenttaM[indeksi], 2).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + rectangles[rectIndx].Width / 2 - 10 + lisays, rectangles[rectIndx].Y + rectangles[rectIndx].Height - 40));
            if (!plus) {
                g.DrawString("-" + Math.Abs(Math.Round(-1 * deltaM[indeksi], 2)).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + rectangles[rectIndx].Width / 2 - 10 + lisays, rectangles[rectIndx].Y + rectangles[rectIndx].Height - 27));
            } else {
                g.DrawString("+" + Math.Abs(Math.Round(-1 * deltaM[indeksi], 2)).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + rectangles[rectIndx].Width / 2 - 10 + lisays, rectangles[rectIndx].Y + rectangles[rectIndx].Height - 27));
            }
            g.DrawString(Math.Round(m[mIndx], 2).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + rectangles[rectIndx].Width / 2 - 10 + lisays, rectangles[rectIndx].Y + rectangles[rectIndx].Height - 14));
        }

        /// <summary>
        /// Piirtää momenttien tasaukset kuvaan annetun suorakulmion vasemmalle reunalle
        /// </summary>
        /// <param name="g">Kuvan grafiikkaolio</param>
        /// <param name="kenttaM">Kentän alkup. momentti</param>
        /// <param name="deltaM">Kentältä miinustettava arvo</param>
        /// <param name="m">Kentän lopullinen momentti</param>
        /// <param name="mIndx">Kentän momentin indeksi (esim. 1.2 = 0, 1.4 = 1 jne.)</param>
        /// <param name="indeksi">Monesko kenttä kyseessä (esim 1.2 & 1.4 = 0, 2.3 & 2.5 = 1 jne.)</param>
        /// <param name="rectIndx">Piirrettävän suorakulmion indeksi</param>
        private void PiirraTekstitVasen(Graphics g, double[] kenttaM, double[] deltaM, double[] m, int mIndx, int indeksi, int rectIndx, bool plus, int lisays = 0) {
            g.DrawString(Math.Round(kenttaM[indeksi], 2).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + 5, rectangles[rectIndx].Y + rectangles[rectIndx].Height / 2 - 13 - lisays));
            if (!plus) {
                g.DrawString("-" + Math.Abs(Math.Round(-1 * deltaM[indeksi], 2)).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + 5, rectangles[rectIndx].Y + rectangles[rectIndx].Height / 2 - lisays));
            } else {
                g.DrawString("+" + Math.Abs(Math.Round(-1 * deltaM[indeksi], 2)).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + 5, rectangles[rectIndx].Y + rectangles[rectIndx].Height / 2 - lisays));
            }
            g.DrawString(Math.Round(m[mIndx], 2).ToString(), Font, Brushes.Black, new Point(rectangles[rectIndx].X + 5, rectangles[rectIndx].Y + rectangles[rectIndx].Height / 2 + 13 - lisays));
        }
        #endregion

        /// <summary>
        /// Hakee kentän momentin vaakasuunnassa
        /// </summary>
        /// <param name="indeksi">Kyseessä olevan kentän indeksi</param>
        /// <param name="mittaLeveys">Kentän horisontaalinen mitta</param>
        /// <param name="mittaKorkeus">Kentän vertikaalinen mitta</param>
        /// <param name="kentat">Kentät-lista</param>
        /// <returns>Annetun indeksi kohdasta kentät-listalta kentän momentin</returns>
        private double HaeKenttaMVaaka(int indeksi, string mittaLeveys, string mittaKorkeus, Dictionary<int, ArrayList> kentat) {
            double palautus = 0;
            if (double.Parse(mittaLeveys) <= double.Parse(mittaKorkeus)) {
                palautus = double.Parse((string)kentat[indeksi][1]);
            } else {
                palautus = double.Parse((string)kentat[indeksi][0]);
            }
            return palautus;
        }

        /// <summary>
        /// Hakee kentän momentin pystysuunnassa
        /// </summary>
        /// <param name="indeksi">Kyseessä olevan kentän indeksi</param>
        /// <param name="mittaLeveys">Kentän horisontaalinen mitta</param>
        /// <param name="mittaKorkeus">Kentän vertikaalinen mitta</param>
        /// <param name="kentat">Kentät-lista</param>
        /// <returns>Annetun indeksi kohdasta kentät-listalta kentän momentin</returns>
        private double HaeKenttaMPysty(int indeksi, string mittaLeveys, string mittaKorkeus, Dictionary<int, ArrayList> kentat) {
            double palautus = 0;
            if (double.Parse(mittaLeveys) <= double.Parse(mittaKorkeus)) {
                palautus = double.Parse((string)kentat[indeksi][0]);
            } else {
                palautus = double.Parse((string)kentat[indeksi][1]);
            }
            return palautus;
        }

        private void bNeliot_Click(object sender, EventArgs e) {
            int laskuri = 1;
            foreach (Rectangle r in rectangles) {
                Graphics g = pictureBox1.CreateGraphics();
                g.FillRectangle(Brushes.Bisque, r);
                g.DrawRectangle(Pens.Black, r);
                g.DrawString(laskuri.ToString(), new Font(Font, FontStyle.Bold), Brushes.Red, new Point(r.X + r.Width / 2, r.Y + r.Height / 2));
                g.Dispose();
                laskuri++;
            }
        }

        private void bPyyhi_Click(object sender, EventArgs e) {
            pictureBox1.Refresh();
        }

        private void bMallinna_Click(object sender, EventArgs e) {
            if (!KentatTaytettyOikein()) {
                lIlmoitus.Text = "Aseta mitat ja laske arvot!";
                return;
            }
            try {
                double laatanLeveys = double.Parse(tbVaaka_1_4.Text) * 1000 + double.Parse(tbVaaka_2_5.Text) * 1000 + double.Parse(tbVaaka_3_6.Text) * 1000;
                double laatanKorkeus = double.Parse(tbPysty_1_2_3.Text) * 1000 + double.Parse(tbPysty_4_6.Text) * 1000;
                mallintaja.LaatanLeveys = laatanLeveys;
                mallintaja.LaatanKorkeus = laatanKorkeus;
                mallintaja.Mallinna();
                lIlmoitus.Text = "Mallinnettu!";
                bLuoPiirros.Enabled = true;
                bRaudoita.Enabled = true;
            } catch (Mallintaja.ModelConnectionException ex) {
                lIlmoitus.Text = ex.Message;
            } catch (FormatException) {
                lIlmoitus.Text = "Tarkista mitat, annetut kuormat ja laske arvot!";
            } catch (NullReferenceException ex) {
                lIlmoitus.Text = "Laske arvot";
                lokiKirjoittaja.KirjoitaLokiin("************bMallinna - NullReference******************\n" + ex.StackTrace + "\n******************************");
            } catch (Exception ex) {
                lIlmoitus.Text = "Muu virhe: " + ex.Message;
                lokiKirjoittaja.KirjoitaLokiin("******************************\n" + ex.StackTrace + "\n******************************");
            }
        }
        #region Päävalikon metodit
        private void asetuksetToolStripMenuItem_Click(object sender, EventArgs e) {
            AsetuksetForm asetuksetForm = new AsetuksetForm(asetukset);
            asetuksetForm.Show();
        }

        private void avaaToolStripMenuItem_Click(object sender, EventArgs e) {
            if (tiedostonAvaus.ShowDialog().Equals(DialogResult.OK)) {
                Tiedostonkasittelija t = new Tiedostonkasittelija(tiedostonAvaus.FileName);
                string[] rivit = t.LueTiedostosta();
                if (rivit.Length == 8) {
                    tbVaaka_1_4.Text = rivit[0];
                    tbVaaka_2_5.Text = rivit[1];
                    tbVaaka_3_6.Text = rivit[2];
                    tbVaaka_7.Text = rivit[3];
                    tbPysty_1_2_3.Text = rivit[4];
                    tbPysty_4_6.Text = rivit[5];
                    tbKuorma.Text = rivit[6];
                    tbHyoty.Text = rivit[7];
                    asetukset.AsetaAlkuperaiset();
                    tiedostonAvaus.FileName = "Mitat.rkl";
                    lIlmoitus.Text = "Tiedot tuotiin vanhan version tallennuksesta, asetukset pysyvät ennallaan!";
                    return;
                }
                if (rivit.Length != 17) {
                    MessageBox.Show("Tiedosto ei kelpaa, arvojen lukumäärä ei täsmää!");
                    return;
                }
                tbVaaka_1_4.Text = rivit[0];
                tbVaaka_2_5.Text = rivit[1];
                tbVaaka_3_6.Text = rivit[2];
                tbVaaka_7.Text = rivit[3];
                tbPysty_1_2_3.Text = rivit[4];
                tbPysty_4_6.Text = rivit[5];
                tbKuorma.Text = rivit[6];
                tbHyoty.Text = rivit[7];
                asetukset.ArvioituMittaYlapinnasta = double.Parse(rivit[8]);
                asetukset.BetoninLujuus = int.Parse(rivit[9]);
                asetukset.BetoniVarmKerroin = double.Parse(rivit[10]);
                asetukset.LaatanPaksuus = double.Parse(rivit[11]);
                asetukset.Rasitusluokka = rivit[12];
                asetukset.RasitusluokkaIndx = int.Parse(rivit[13]);
                asetukset.Suojabetoni = double.Parse(rivit[14]);
                asetukset.TeraksenKoko = int.Parse(rivit[15]);
                asetukset.TeraksenLujuus = int.Parse(rivit[16]);
                tiedostonAvaus.FileName = "Mitat.rkl";
            }
        }

        private void tallennaToolStripMenuItem_Click(object sender, EventArgs e) {
            if (KentatTaytettyOikein() && tiedostonTallennus.ShowDialog().Equals(DialogResult.OK)) {
                Tiedostonkasittelija t = new Tiedostonkasittelija(tiedostonTallennus.FileName);
                // t.KirjoitaTiedostoon(mitatVaaka, mitatPysty, new string[] { tbKuorma.Text, tbHyoty.Text });
                t.KirjoitaTiedostoon(mitatVaaka, mitatPysty, new string[] { tbKuorma.Text, tbHyoty.Text }, asetukset);
                tiedostonTallennus.FileName = "Mitat.rkl";
            }
        }

        private void lopetaToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }
        #endregion
        private bool TasauksetLaskettu() {
            if (!tasauksetLaskettu) {
                lIlmoitus.Text = "Arvoja ei laskettu!";
                return false;
            }
            return true;
        }

        private bool KentatAlustettu() {
            for (int i = 0; i < kentat.Length; i++) {
                if (kentat[i] == null) {
                    return false;
                }
            }
            return true;
        }

        private bool KentatTaytettyOikein() {
            try {
                foreach (TextBox t in mitatVaaka) {
                    double.Parse(t.Text);
                }
                foreach (TextBox t in mitatPysty) {
                    double.Parse(t.Text);
                }
                double.Parse(tbKuorma.Text);
                double.Parse(tbHyoty.Text);
                return true;
            } catch (Exception) {
                lIlmoitus.Text = "Tarkista, että kaikki arvot on annettu ja kentät sisältävät vain numeroarvoja!";
                return false;
            }
        }

        private void ohjeetToolStripMenuItem_Click(object sender, EventArgs e) {
            string ohjeetOsoite = "";
            if (File.Exists(HAKEMISTO + "\\Tiedostot\\Ohjeet\\Ohjeet.html")) {
                ohjeetOsoite = @"file://" + HAKEMISTO + "\\Tiedostot\\Ohjeet\\Ohjeet.html";
            } else if (File.Exists(HAKEMISTO + "\\Ohjeet.html")) {
                ohjeetOsoite = @"file://" + HAKEMISTO + "\\Ohjeet.html";
            } else {
                MessageBox.Show("Ohjetiedostoa ei löytynyt!", "Virhe", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            
            new Formit.Ohjeet(ohjeetOsoite).Show();
        }

        private void bRaudoitus_Click(object sender, EventArgs e) {
            int kentta = 0;
            if (((Button)sender).Equals(bRaud1)) {
                kentta = 0;
            } else if (((Button)sender).Equals(bRaud2)) {
                kentta = 1;
            } else if (((Button)sender).Equals(bRaud3)) {
                kentta = 2;
            } else if (((Button)sender).Equals(bRaud4)) {
                kentta = 3;
            } else if (((Button)sender).Equals(bRaud5)) {
                kentta = 4;
            } else if (((Button)sender).Equals(bRaud6)) {
                kentta = 5;
            } else if (((Button)sender).Equals(bRaud7)) {
                kentta = 6;
            }
            if (KentatTaytettyOikein() && TasauksetLaskettu()) {
                try {
                    Raudoittaja r = new Raudoittaja(kentat, asetukset);
                    if (tukiraudatToolStripMenuItem.Checked) {
                        r.VirheenkorjausTukiraudat = true;
                    }
                    if (kenttäraudatToolStripMenuItem.Checked) {
                        r.VirheenkorjausKenttaraudat = true;
                    }
                    r.LaskeTukiraudoituksetKentalle(kentat[kentta]);
                    r.LaskeRaudoitusLyhytSivu(kentta);
                } catch (Exception ex) {
                    lIlmoitus.Text = ex.Message;
                    lokiKirjoittaja.KirjoitaLokiin(ex.StackTrace);
                }
                
            }
        }

        private void bMallinnusRaudoita_Click(object sender, EventArgs e) {
            if (!KentatAlustettu() || !tasauksetLaskettu) {
                lIlmoitus.Text = "Arvoja ei laskettu!";
                return;
            }
            try {
                Raudoittaja raudoittaja = new Raudoittaja(kentat, asetukset);
                for (int i = 0; i < kentat.Length; i++) {
                    raudoittaja.LaskeRaudoitusLyhytSivu(i, false);
                }
                raudoittaja.LaskeTukiraudoituksetKaikilleKentille();
                mallintaja.MallinnaRaudoitukset();
                lIlmoitus.Text = "Raudoitettu!";
            } catch (Exception ex) {
                lIlmoitus.Text = ex.Message;
                lokiKirjoittaja.KirjoitaLokiin("************bRaudoita_Click******************\n" + ex.StackTrace + "\n******************************");
            }
            
        }

        private void bTekla_Click(object sender, EventArgs e) {
            try {
                mallintaja = new Mallintaja(this.kentat);
                mallintaja.Asetukset = asetukset;
                bMallinna.Enabled = true;
                bTyhjennaTekla.Enabled = true;
                ajastinTekla.Start();
                lIlmoitus.Text = "Yhteys luotu!";
            } catch (Exception ex) {
                lIlmoitus.Text = ex.Message;
            }
            
        }

        private void ajastinTekla_Tick(object sender, EventArgs e) {
            if (!mallintaja.TarkistaYhteys()) {
                MessageBox.Show("EI");
                bMallinna.Enabled = false;
                bRaudoita.Enabled = false;
                lIlmoitus.Text = "Yhteys malliin katkesi";
                ajastinTekla.Stop();
            }
        }

        private void bNollaaTekla_Click(object sender, EventArgs e) {
            try {
                if (MessageBox.Show("Tyhjennetäänkö malli?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question).Equals(System.Windows.Forms.DialogResult.Cancel)) {
                    return;
                }
                mallintaja.TyhjennaMalli();                
                lIlmoitus.Text = "Malli tyhjennetty!";
            } catch (Exception ex) {
                lIlmoitus.Text = ex.Message;
            }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e) {
        }

        private void bLuoPiirros_Click(object sender, EventArgs e) {
            try {
                mallintaja.LuoPiirros();
                lIlmoitus.Text = "Piirros luotu";
            } catch (Exception ex) {
                lIlmoitus.Text = ex.Message;
            }
        }

        private void avaaLokiToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!File.Exists(HAKEMISTO + @"\Tiedostot\Loki.txt")) {
                MessageBox.Show("Lokitiedostoa ei löydy!");
                return;
            }
		    try {
				System.Diagnostics.Process.Start("notepad++", HAKEMISTO + @"\Tiedostot\loki.txt");
		    } catch (Exception) {
				System.Diagnostics.Process.Start("notepad", HAKEMISTO + @"\Tiedostot\loki.txt");
		    }
	        	
        }

        private void bKenttienRaudoitukset_Click(object sender, EventArgs e) {
            kentatLista = kentat.ToList();
            if (!KentatTaytettyOikein()) {
                return;
            }
            foreach (Kentta k in kentatLista) {
                if (k == null) {
                    lIlmoitus.Text = "Laske tasaukset!";
                    return;
                }
            }
            Formit.KaikkiRaudoitukset raudoitukset = new Formit.KaikkiRaudoitukset(kentatLista);
            Raudoittaja raudoittaja = new Raudoittaja(kentat, asetukset);
            raudoittaja.LaskeKaikkiRaudoitukset();
            raudoitukset.Show();
        }

        private void LaskeNurkkaRaudoitus(object sender, EventArgs e) {
            Kentta kentta = null;
            if ((sender as Button).Tag != null) {
                switch (((Button)sender).Tag.ToString()) {
                    case "kentta1":
                        kentta = kentat[0];
                        break;
                    case "kentta3":
                        kentta = kentat[2];
                        break;
                    case "kentta4":
                        kentta = kentat[3];
                        break;
                    case "kentta6":
                        kentta = kentat[5];
                        break;
                    case "kentta7":
                        kentta = kentat[6];
                        break;
                }
            } else {
                lIlmoitus.Text = "Virhe nurkkaraudoitusta laskiessa!";
                return;
            }
            if (kentta == null) {
                lIlmoitus.Text = "Virhe nurkkaraudoitusta laskiessa!";
                return;
            }
            Raudoittaja raudoittaja = new Raudoittaja(kentat, asetukset);
            MessageBox.Show(raudoittaja.LaskeNurkkaRaudoitus(kentta, kuormat).ToString());
        }
    }
}
