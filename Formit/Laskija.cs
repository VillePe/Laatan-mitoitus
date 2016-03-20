using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Laatan_mitoitus {
    public partial class Laskija : Form {

        private Kentta kentta;
        double[,] jaykistysTaulukko;
        double[,] momenttiTaulukko;
        double[,] momenttikertoimetTaulukko;
        double[,] kenttamomenttienKorjauskertoimetTaulukko;
        double kuormat;
        double hyotykuormat;
        double sivujenSuhde;
        double pyoristysYlos;
        double pyoristysAlas;
        double interpolointikerroin;
        int taulukonIndeksi1;
        int taulukonIndeksi2;
        double ays;
        double axs;
        double ayf;
        double axf;
        double[] b_arvot;
        TextBox[] b_tekstit = new TextBox[4];
        TextBox[] k_textBox = new TextBox[4];
        string[] k_tekstit = new string[4];
        string[] m_tekstit = new string[4];
        Label[] b_labelit = new Label[4];
        Label[] k_labelit = new Label[4];
        int jaykkyysApuLaskuri = 0;

        // Tekstilaatikoiden siirtoa varten tallennetut pisteet
        Point[] mxx = new Point[8];
        Point[] axx = new Point[8];
        bool kaannetty = false;
        bool virheenkorjausMomentti = false;
        bool virheenkorjausJaykistys = false;

        public Laskija(Kentta kentta, double kuormat) {
            InitializeComponent();
            lKentta.Text = "Kenttä " + kentta.Numero;
            this.kentta = kentta;
            this.kuormat = kuormat * Math.Pow(Math.Min(kentta.Leveys, kentta.Korkeus), 2);
            b_arvot = new double[4];
            AlustaMomenttiTaulukko();
            AlustaJaykistysTaulukko();
            AlustaMomenttikertoimet();
            AlustaKenttamomenttienKertoimet();
        }

        public Laskija(Kentta kentta, double kuormat, bool verbose) : this(kentta, kuormat) {
            this.virheenkorjausMomentti = true;
            this.virheenkorjausJaykistys = true;
        }

        public Laskija(Kentta kentta, double kuormat, double hyotykuormat) : this(kentta, kuormat) {
            this.hyotykuormat = hyotykuormat;
        }

        public string[] K_Tekstit {
            get {
                return k_tekstit;
            }
        }

        public string[] M_Tekstit {
            get {
                return m_tekstit;
            }
        }

        public bool VirheenkorjausMomentti {
            set {
                virheenkorjausMomentti = value;
            }
        }
        public bool VirheenkorjausJaykistys {
            set {
                virheenkorjausJaykistys = value;
            }
        }
        public double Hyotykuormat {
            get {
                return hyotykuormat;
            }
            set {
                hyotykuormat = value;
            }
        }

        private void Field_Load(object sender, EventArgs e) {
            //AsetaArvot();
        }

        public void AsetaArvot() {
            double lengthX = Math.Min(kentta.Korkeus, kentta.Leveys);
            double lengthY = Math.Max(kentta.Korkeus, kentta.Leveys);
            sivujenSuhde = lengthY / lengthX;
            if (sivujenSuhde > 2) {
                throw new Exception("Sivujen suhde on liian suuri! (Taulukosta loppuu arvot kesken)");
            }
            lLyperLx.Text = "Ly/Lx = " + Math.Round(sivujenSuhde, 2);
            this.Text = "Kenttä " + kentta.Numero;

            pyoristysYlos = Math.Ceiling((sivujenSuhde * 10)) / 10;
            pyoristysAlas = Math.Floor((sivujenSuhde * 10)) / 10;
            interpolointikerroin = ((sivujenSuhde * 1000) % 100) / 100;

            AlustaTaulukonIndeksit();
            if (!kentta.UseampiTuenta) {
                LaskeMomentti();
            } else {
                LaskeUseanTapauksenMomentti();
            }
            AsetaMomenttiTekstit();
            AsetaAlfaTekstit();
            AlustaPaikanVaihdot();
            AlustaBJaK();
            LaskeJaykkyys();
            LaskeLopullisetKenttamomentit();
        }

        /// <summary>
        /// Alustaa taulukosta haettavat indeksit (esim sivusuhteesta 1.666 halutaan interpoloida arvo 1.6 ja 1.7 antamat arvot,
        /// haetaan taulukosta 6:nen ja 7:nen indeksin kohdalta arvot)
        /// </summary>
        private void AlustaTaulukonIndeksit() {
            // Laskee taulukon indeksin sivusuhteiden pyöristysten perusteella
            if (pyoristysYlos >= 2) {
                taulukonIndeksi1 = (int)(pyoristysAlas * 10 % 10 + 10);             // Sama kuin olisi 'taulukonIndeksi1 = 10;'
            } else {
                taulukonIndeksi1 = (int)(pyoristysAlas * 10 % 10);
            }
            if (pyoristysAlas >= 2) {
                taulukonIndeksi2 = (int)(pyoristysYlos * 10 % 10 + 10);
            } else {
                taulukonIndeksi2 = (int)(pyoristysYlos * 10 % 10);
            }
        }

        /// <summary>
        /// Alustaa B:n ja K:n taulukot
        /// </summary>
        private void AlustaBJaK() {
            b_tekstit[0] = tb_b1;
            b_tekstit[1] = tb_b2;
            b_tekstit[2] = tb_b3;
            b_tekstit[3] = tb_b4;
            k_tekstit[0] = tb_k1.Text;
            k_tekstit[1] = tb_k2.Text;
            k_tekstit[2] = tb_k3.Text;
            k_tekstit[3] = tb_k4.Text;

            k_textBox[0] = tb_k1;
            k_textBox[1] = tb_k2;
            k_textBox[2] = tb_k3;
            k_textBox[3] = tb_k4;

            b_labelit[0] = lb1;
            b_labelit[1] = lb2;
            b_labelit[2] = lb3;
            b_labelit[3] = lb4;
            k_labelit[0] = lk1;
            k_labelit[1] = lk2;
            k_labelit[2] = lk3;
            k_labelit[3] = lk4;
        }

        private void AlustaPaikanVaihdot() {
            mxx[0] = tbMys.Location;
            mxx[1] = tbMxs.Location;
            mxx[2] = tbMyf.Location;
            mxx[3] = tbMxf.Location;
            mxx[4] = lMys.Location;
            mxx[5] = lMxs.Location;
            mxx[6] = lMyf.Location;
            mxx[7] = lMxf.Location;

            axx[0] = lAys.Location;
            axx[1] = lAxs.Location;
            axx[2] = lAyf.Location;
            axx[3] = lAxf.Location;
            axx[4] = tbAys.Location;
            axx[5] = tbAxs.Location;
            axx[6] = tbAyf.Location;
            axx[7] = tbAxf.Location;
        }


        /// <summary>
        /// Perustapausten momenttien laskenta interpoloinnin avulla
        /// </summary>
        private void LaskeMomentti() {

            // Pyöristetään sivujen suhde ylös ja alas ja haetaan taulukosta molemmille arvot
            // Lasketaan arvojen erotus, kerrotaan interpolointikertoimella, ja lisätään alaspäin 
            // pyöristetyllä arvolla
            // Esim. 1.25 -> Haetaan taulukosta 4a kohdista 1.2 ja 1.3 alfan arvot 462 ja 479
            // Lasketaan erotus 479 - 462 = 17 ja kerrotaan interpolointikertoimella 0.5 joka saadaan seuraavalla tavalla:
            // 1.25 * 1000 = 1250, otetaan jakojäännös 100:sta 1250 % 100 = 50 ja jaetaan sadalla -> 0.5
            // lopuksi lisätään alkuperäiseen 462:een 17*0.5 -> 462 + 17 * 0.5 = 470.5

            ays = LaskeAlfa(pyoristysYlos, pyoristysAlas, interpolointikerroin, 0);
            axs = LaskeAlfa(pyoristysYlos, pyoristysAlas, interpolointikerroin, 1);
            ayf = LaskeAlfa(pyoristysYlos, pyoristysAlas, interpolointikerroin, 2);
            axf = LaskeAlfa(pyoristysYlos, pyoristysAlas, interpolointikerroin, 3);
        }

        private void LaskeUseanTapauksenMomentti() {


            double ays1 = LaskeAlfa(pyoristysYlos, pyoristysAlas, interpolointikerroin, 0);
            double ays2 = LaskeAlfa(kentta.ToinenTuenta, pyoristysYlos, pyoristysAlas, interpolointikerroin, 0);
            if (ays1 == 0 || ays2 == 0) {
                ays = ays1;
            } else {
                ays = ays1 * kentta.TuentatapaustenSuhde + ays2 * (1 - kentta.TuentatapaustenSuhde);
            }

            double axs1 = LaskeAlfa(pyoristysYlos, pyoristysAlas, interpolointikerroin, 1);
            double axs2 = LaskeAlfa(kentta.ToinenTuenta, pyoristysYlos, pyoristysAlas, interpolointikerroin, 1);
            if (axs1 == 0 || axs2 == 0) {
                axs = axs1;
            } else {
                axs = axs1 * kentta.TuentatapaustenSuhde + axs2 * (1 - kentta.TuentatapaustenSuhde);
            }

            double ayf1 = LaskeAlfa(pyoristysYlos, pyoristysAlas, interpolointikerroin, 2);
            double ayf2 = LaskeAlfa(kentta.ToinenTuenta, pyoristysYlos, pyoristysAlas, interpolointikerroin, 2);
            ayf = ayf1 * kentta.TuentatapaustenSuhde + ayf2 * (1 - kentta.TuentatapaustenSuhde);

            double axf1 = LaskeAlfa(pyoristysYlos, pyoristysAlas, interpolointikerroin, 3);
            double axf2 = LaskeAlfa(kentta.ToinenTuenta, pyoristysYlos, pyoristysAlas, interpolointikerroin, 3);
            axf = axf1 * kentta.TuentatapaustenSuhde + axf2 * (1 - kentta.TuentatapaustenSuhde);
        }

        /// <summary>
        /// Hakee taulukosta ylös- ja alaspäin pyöristetyt arvot sivujen suhteesta ja laskee interpoloimalla 
        /// alfan arvon. Tuentatapauksena käytetään kentälle annettua alkuperäistä tuentatapausta
        /// </summary>
        /// <param name="pyoristysYlos">Sivujen suhde pyöristettynä yhden desimaalin tarkkuuteen ylöspäin</param>
        /// <param name="pyoristysAlas">Sivujen suhde pyöristettynä yhden desimaalin tarkkuuteen alaspäin</param>
        /// <param name="interpolointikerroin">Etukäteen laskettu toinen desimaali sivujen suhteesta</param>
        /// <param name="taulukkoindeksi">ays = 0, axs = 1, ayf = 2, axf = 3</param>
        /// <returns>Alfan interpoloituna double tietotyypissä</returns>
        public double LaskeAlfa(double pyoristysYlos, double pyoristysAlas, double interpolointikerroin, int taulukkoindeksi) {
            double palautus = LaskeAlfa(kentta.Tuentatapaus, pyoristysYlos, pyoristysAlas, interpolointikerroin, taulukkoindeksi);
            return palautus;
        }

        /// <summary>
        /// Hakee taulukosta ylös- ja alaspäin pyöristetyt arvot sivujen suhteesta ja laskee interpoloimalla 
        /// alfan arvon
        /// </summary>
        /// <param name="tapaus"></param>
        /// <param name="pyoristysYlos">Sivujen suhde pyöristettynä yhden desimaalin tarkkuuteen ylöspäin</param>
        /// <param name="pyoristysAlas">Sivujen suhde pyöristettynä yhden desimaalin tarkkuuteen alaspäin</param>
        /// <param name="interpolointikerroin">Etukäteen laskettu toinen desimaali sivujen suhteesta</param>
        /// <param name="taulukkoindeksi">ays = 0, axs = 1, ayf = 2, axf = 3</param>
        /// <returns>Alfan interpoloituna double tietotyypissä</returns>
        private double LaskeAlfa(Kentta.Tuentatapaukset tapaus, double pyoristysYlos, double pyoristysAlas, double interpolointikerroin, int taulukkoindeksi) {
            double ays1 = HaeTaulukostaAlfa(tapaus, taulukkoindeksi, pyoristysYlos);
            double ays2 = HaeTaulukostaAlfa(tapaus, taulukkoindeksi, pyoristysAlas);
            if (virheenkorjausMomentti) {
                MessageBox.Show("\tAlfa taulukosta" +
                    "\n\nTapaus: " + tapaus +
                    "\nTaulukkoindeksi (ys = 0, xs = 1, yf = 2, xf = 3): " + taulukkoindeksi +
                    "\nSivujen suhde: " + sivujenSuhde +
                    "\nPyöristys alas alfa: " + ays2 +
                    "\nPyöristys ylös alfa: " + ays1 +
                    "\nInterpolointikerroin: " + interpolointikerroin,
                    "Momentti");
            }
            return Math.Abs((ays2 - ays1)) * interpolointikerroin + Math.Min(ays1, ays2);
        }

        /// <summary>
        /// Asettaa tekstit momenttiarvoille asetetuille tekstilaatikoille
        /// </summary>
        private void AsetaMomenttiTekstit() {
            m_tekstit[0] = ((ays / 10000) * kuormat).ToString();
            m_tekstit[1] = ((axs / 10000) * kuormat).ToString();
            m_tekstit[2] = ((ayf / 10000) * kuormat).ToString();
            m_tekstit[3] = ((axf / 10000) * kuormat).ToString();
            tbMys.Text = Math.Round(((ays / 10000) * kuormat), 2).ToString();
            tbMxs.Text = Math.Round(((axs / 10000) * kuormat), 2).ToString();
            tbMyf.Text = Math.Round(((ayf / 10000) * kuormat), 2).ToString();
            tbMxf.Text = Math.Round(((axf / 10000) * kuormat), 2).ToString();
        }

        /// <summary>
        /// Asettaa arvot alfan arvoille asetetuille tekstilaatikoille
        /// </summary>
        private void AsetaAlfaTekstit() {
            tbAys.Text = Math.Round(ays, 0).ToString();
            tbAxs.Text = Math.Round(axs, 0).ToString();
            tbAyf.Text = Math.Round(ayf, 0).ToString();
            tbAxf.Text = Math.Round(axf, 0).ToString();
        }

        /// <summary>
        /// Hakee alfan taulukosta tuentatapauksen, sarakelisän ja sivujen suhteen avulla
        /// </summary>
        /// <param name="tapaus">Tuentatapaus</param>
        /// <param name="taulukonSarakeLisa">ays = 0, axs = 1, ayf = 2, axf = 3</param>
        /// <param name="sivujenSuhde">Kentän sivujen suhde</param>
        /// <returns>Palauttaa taulukosta alfan</returns>
        private double HaeTaulukostaAlfa(Kentta.Tuentatapaukset tapaus, int taulukonSarakeLisa, double sivujenSuhde) {
            double palautus = -1;
            int taulukonIndeksi = 0;
            if (sivujenSuhde >= 2) {
                taulukonIndeksi = (int)(sivujenSuhde * 10) % 10 + 10;
            } else {
                taulukonIndeksi = (int)(sivujenSuhde * 10) % 10;
            }
            switch (tapaus) {
                case Kentta.Tuentatapaukset.EiTukia:
                    palautus = momenttiTaulukko[(int)Kentta.Tuentatapaukset.EiTukia + taulukonSarakeLisa, taulukonIndeksi];
                    break;
                case Kentta.Tuentatapaukset.TukiPitkallaSivulla:
                    palautus = momenttiTaulukko[(int)Kentta.Tuentatapaukset.TukiPitkallaSivulla + taulukonSarakeLisa, taulukonIndeksi];
                    break;
                case Kentta.Tuentatapaukset.TukiLyhyellaSivulla:
                    palautus = momenttiTaulukko[(int)Kentta.Tuentatapaukset.TukiLyhyellaSivulla + taulukonSarakeLisa, taulukonIndeksi];
                    break;
                case Kentta.Tuentatapaukset.TukiaKaksiVierekkain:
                    palautus = momenttiTaulukko[(int)Kentta.Tuentatapaukset.TukiaKaksiVierekkain + taulukonSarakeLisa, taulukonIndeksi];
                    break;
                case Kentta.Tuentatapaukset.TukiaKaksiVastakkainLSivulla:
                    palautus = momenttiTaulukko[(int)Kentta.Tuentatapaukset.TukiaKaksiVastakkainLSivulla + taulukonSarakeLisa, taulukonIndeksi];
                    break;
                case Kentta.Tuentatapaukset.TukiaKaksiVastakkainPSivulla:
                    palautus = momenttiTaulukko[(int)Kentta.Tuentatapaukset.TukiaKaksiVastakkainPSivulla + taulukonSarakeLisa, taulukonIndeksi];
                    break;
                case Kentta.Tuentatapaukset.TukiaKolmeLSivuVapaa:
                    palautus = momenttiTaulukko[(int)Kentta.Tuentatapaukset.TukiaKolmeLSivuVapaa + taulukonSarakeLisa, taulukonIndeksi];
                    break;
                case Kentta.Tuentatapaukset.TukiaKolmePSivuVapaa:
                    palautus = momenttiTaulukko[(int)Kentta.Tuentatapaukset.TukiaKolmePSivuVapaa + taulukonSarakeLisa, taulukonIndeksi];
                    break;
                case Kentta.Tuentatapaukset.TukiaNelja:
                    palautus = momenttiTaulukko[(int)Kentta.Tuentatapaukset.TukiaNelja + taulukonSarakeLisa, taulukonIndeksi];
                    break;


            }
            return palautus;
        }

        /// <summary>
        /// Laskee jäykkyysarvot kentälle
        /// </summary>
        private void LaskeJaykkyys() {



            double b1 = 0;
            double b2 = 0;

            for (int i = 0; i < b_arvot.Length; i++) {
                if (kentta.HaeJaykistysTapausNimi(i) == null) {
                    break;
                }

                b1 = jaykistysTaulukko[(int)kentta.HaeJaykistysTapaus(i), taulukonIndeksi2];
                b2 = jaykistysTaulukko[(int)kentta.HaeJaykistysTapaus(i), taulukonIndeksi1];
                b_arvot[i] = -1 * Math.Abs(b2 - b1) * interpolointikerroin + Math.Max(b1, b2);



                // Jos kentällä on toissijainen jäykistystapaus, pitää hakea b:n arvo toiselle tapaukselle myös
                // ja lisätä alkuperäiseen kertomalla suhteiden avulla.
                // Tässä toteutettu laskentatapa osaa odottaa kaikkia suhdekertoimia, eli ei vain puolikkaita tapauksia
                if (kentta.HaeToissijainenJaykistysTapaus(i) != Kentta.Jaykistystapaukset.EiTapausta) {
                    double b2_1 = jaykistysTaulukko[(int)kentta.HaeToissijainenJaykistysTapaus(i), taulukonIndeksi2];
                    double b2_2 = jaykistysTaulukko[(int)kentta.HaeToissijainenJaykistysTapaus(i), taulukonIndeksi1];
                    double bToissijainen = (b2_1 - b2_2) * interpolointikerroin + b2_2;
                    b_arvot[i] = b_arvot[i] * kentta.TuentatapaustenSuhde + bToissijainen * (1 - kentta.TuentatapaustenSuhde);
                }
                if (virheenkorjausJaykistys) {
                    MessageBox.Show("Suunta: " + kentta.HaeJaykistysTapausNimi(jaykkyysApuLaskuri) +
                        "\nSivujen suhde: " + sivujenSuhde +
                        "\nb1: " + b1 +
                        "\nb2: " + b2 +
                        "\nPyöristys alas: " + pyoristysAlas +
                        "\nPyöristys ylös: " + pyoristysYlos +
                        "\nInterpolointikerroin: " + interpolointikerroin,
                        "Jäykistys ");
                    jaykkyysApuLaskuri++;
                }
                b_tekstit[i].Text = b_arvot[i].ToString();
                b_labelit[i].Text = "b" + kentta.HaeJaykistysTapausNimi(i);
                k_labelit[i].Text = "k" + kentta.HaeJaykistysTapausNimi(i);
                k_tekstit[i] = (b_arvot[i] / Math.Min(kentta.Korkeus, kentta.Leveys)).ToString();
                k_textBox[i].Text = k_tekstit[i];
            }
        }

        /// <summary>
        /// Laskee kentälle lopulliset kenttämomentit
        /// </summary>
        private void LaskeLopullisetKenttamomentit() {
            double hyotykuormaKentta = hyotykuormat * Math.Pow(Math.Min(kentta.Leveys, kentta.Korkeus), 2);
            double mxf = double.Parse(m_tekstit[3]);
            double myf = double.Parse(m_tekstit[2]);
            
            mxf = LaskeKenttamomenttiMxf(mxf, hyotykuormaKentta);
            myf = LaskeKenttamomenttiMyf(myf, hyotykuormaKentta);
            kentta.AsetaKenttamomentti(0, mxf);
            kentta.AsetaKenttamomentti(1, myf);
            tbLMxf.Text = Math.Round(mxf, 2).ToString();
            tbLMyf.Text = Math.Round(myf, 2).ToString();

        }

        /// <summary>
        /// Laskee lopullisen kenttämomentin mxf kentälle
        /// </summary>
        /// <param name="mxf"></param>
        /// <param name="hyotykuormaKentta"></param>
        /// <returns></returns>
        private double LaskeKenttamomenttiMxf(double mxf, double hyotykuormaKentta) {
            double xAlfa = HaeAlfaKenttamomentinLaskemistaVarten("x");
            double lisakerroin = 1;
            double[] deltaTasaus = new double[4];
            double korjauskerroin = 0;
            string virheenkorjausTeksti = "";
            for (int i = 0; i < 4; i++) {
                if (kentta.HaeTasaus(i) == 0) {
                    continue;
                }
                korjauskerroin = HaeKorjauskerroin(i, 1);
                lisakerroin = HaeLisakerroin(i);
                if (kentta.HaeToissijainenTasaus(i) != 0) {
                    deltaTasaus[i] = (kentta.HaeTasaus(i) + kentta.HaeToissijainenTasaus(i)) / 2 * korjauskerroin / 1000 * lisakerroin;
                } else {
                    deltaTasaus[i] = kentta.HaeTasaus(i) * korjauskerroin / 1000 * lisakerroin;
                }
                virheenkorjausTeksti += "\n\nSuunta " + i;
                virheenkorjausTeksti += "\nKorjauskerroin: " + korjauskerroin.ToString();
                virheenkorjausTeksti += "\nTasaus: " + kentta.HaeTasaus(i).ToString();
                virheenkorjausTeksti += "\nLisäkerroin: " + lisakerroin.ToString();

            }
            #region Virheenkorjaus
            if (virheenkorjausMomentti) {
                MessageBox.Show("" +
                    "\nMyf: " + mxf +
                    "\nInterpoloitu Alfa: " + xAlfa +
                    virheenkorjausTeksti +
                    "\nHyötykuorma * alfa: " + hyotykuormaKentta * xAlfa / 1000
                    , "Lopulliset kenttämomentit xf");
            }
            #endregion
            return mxf = mxf + xAlfa / 10000 * hyotykuormaKentta + deltaTasaus[0] + deltaTasaus[1] + deltaTasaus[2] + deltaTasaus[3];
        }

        /// <summary>
        /// Laskee lopullisen kenttämomentin myf kentälle
        /// </summary>
        /// <param name="myf"></param>
        /// <param name="hyotykuormaKentta"></param>
        /// <returns></returns>
        private double LaskeKenttamomenttiMyf(double myf, double hyotykuormaKentta) {
            double yAlfa = HaeAlfaKenttamomentinLaskemistaVarten("y");
            double lisakerroin = 1;
            double[] deltaTasaus = new double[4];
            double korjauskerroin = 0;
            string virheenkorjausTeksti = "";
            for (int i = 0; i < 4; i++) {
                if (kentta.HaeTasaus(i) == 0) {
                    continue;
                }
                korjauskerroin = HaeKorjauskerroin(i);
                
                lisakerroin = HaeLisakerroin(i);
                if (kentta.HaeToissijainenTasaus(i) != 0) {
                    deltaTasaus[i] = (kentta.HaeTasaus(i) + kentta.HaeToissijainenTasaus(i)) / 2 * korjauskerroin / 1000 * lisakerroin;
                } else {
                    deltaTasaus[i] = kentta.HaeTasaus(i) * korjauskerroin / 1000 * lisakerroin;
                }
                virheenkorjausTeksti += "\n\nSuunta " + i;
                virheenkorjausTeksti += "\nKorjauskerroin: " + korjauskerroin.ToString();
                virheenkorjausTeksti += "\nTasaus: " + kentta.HaeTasaus(i).ToString();
                virheenkorjausTeksti += "\nLisäkerroin: " + lisakerroin.ToString();

            }
            #region Virheenkorjaus
            if (virheenkorjausMomentti) {
                MessageBox.Show("" +
                    "\nMyf: " + myf +
                    "\nInterpoloitu Alfa: " + yAlfa +

                    virheenkorjausTeksti +
                    "\n\nHyötykuorma * alfa: " + hyotykuormaKentta * yAlfa / 1000
                    , "Lopulliset kenttämomentit yf");
            }
            #endregion
            return myf = myf + yAlfa / 10000 * hyotykuormaKentta + deltaTasaus[0] + deltaTasaus[1] + deltaTasaus[2] + deltaTasaus[3];
        }



        /// <summary>
        /// Hakee deltakertoimen taulukosta
        /// </summary>
        /// <param name="i"></param>
        /// <param name="pariton">0 = mys, 1 = mxs. Hakee arvon taulukosta lisäämällä annetun arvon perusteella</param>
        /// <returns>Palauttaa deltakertoimen</returns>
        private double HaeKorjauskerroin(int i, int pariton = 0) {
            double korjauskerroin = 0;
            double korjauskerroin1 = 0;
            double korjauskerroin2 = 0;
            if (Math.Min(kentta.Leveys, kentta.Korkeus) == kentta.Leveys) {
                if (i == 0 || i == 2) {
                    korjauskerroin1 = kenttamomenttienKorjauskertoimetTaulukko[2 + pariton, taulukonIndeksi1];
                    korjauskerroin2 = kenttamomenttienKorjauskertoimetTaulukko[2 + pariton, taulukonIndeksi2];
                    korjauskerroin = (korjauskerroin2 - korjauskerroin1) * interpolointikerroin + korjauskerroin1;
                } else {
                    korjauskerroin1 = kenttamomenttienKorjauskertoimetTaulukko[0 + pariton, taulukonIndeksi1];
                    korjauskerroin2 = kenttamomenttienKorjauskertoimetTaulukko[0 + pariton, taulukonIndeksi2];
                    korjauskerroin = (korjauskerroin2 - korjauskerroin1) * interpolointikerroin + korjauskerroin1;
                }
            } else {
                if (i == 0 || i == 2) {
                    korjauskerroin1 = kenttamomenttienKorjauskertoimetTaulukko[0 + pariton, taulukonIndeksi1];
                    korjauskerroin2 = kenttamomenttienKorjauskertoimetTaulukko[0 + pariton, taulukonIndeksi2];
                    korjauskerroin = (korjauskerroin2 - korjauskerroin1) * interpolointikerroin + korjauskerroin1;
                } else {
                    korjauskerroin1 = kenttamomenttienKorjauskertoimetTaulukko[2 + pariton, taulukonIndeksi1];
                    korjauskerroin2 = kenttamomenttienKorjauskertoimetTaulukko[2 + pariton, taulukonIndeksi2];
                    korjauskerroin = (korjauskerroin2 - korjauskerroin1) * interpolointikerroin + korjauskerroin1;
                }
            }
            return korjauskerroin;
        }

        /// <summary>
        /// Hakee lisäkertoimen kentän tuentatapausten perusteella
        /// </summary>
        /// <param name="i">Tasauksen sijainnin indeksi (0 = vasen, 1 = ylös, 2 = oikea, 3 = alas</param>
        /// <returns></returns>
        private double HaeLisakerroin(int i) {
            double lisakerroin = 1;
            if (kentta.HaeTasaus(i) < 0) {
                switch (kentta.Tuentatapaus) {
                    case Kentta.Tuentatapaukset.TukiLyhyellaSivulla:
                    case Kentta.Tuentatapaukset.TukiPitkallaSivulla:
                        lisakerroin = 0.5;
                        break;
                    case Kentta.Tuentatapaukset.TukiaKaksiVastakkainLSivulla:
                    case Kentta.Tuentatapaukset.TukiaKaksiVastakkainPSivulla:
                    case Kentta.Tuentatapaukset.TukiaKaksiVierekkain:
                        lisakerroin = 0.6;
                        break;
                    case Kentta.Tuentatapaukset.TukiaKolmeLSivuVapaa:
                    case Kentta.Tuentatapaukset.TukiaKolmePSivuVapaa:
                        lisakerroin = 0.75;
                        break;
                    case Kentta.Tuentatapaukset.TukiaNelja:
                        lisakerroin = 0.9;
                        break;
                }
            } else {
                lisakerroin = 1;
            }
            return lisakerroin;
        }

        /// <summary>
        /// Hakee alfan kenttämomentin laskemista varten taulukosta annetun suunnan mukaan
        /// </summary>
        /// <param name="suunta">Momentin suunta ("x" tai "y")</param>
        /// <returns></returns>
        private double HaeAlfaKenttamomentinLaskemistaVarten(string suunta) {
            // Apumuuttuja, joka määrittää otetaanko taulukosta x- vai y-suuntaa ilmoittava kerroin
            int lisa = 0;
            if (suunta == "x") {
                lisa = 1;
            } else {
                lisa = 0;
            }

            // Momenttikerrointaulukosta haetut kertoimet taulukkosijainnin mukaan.            
            double xAlfa1 = momenttikertoimetTaulukko[((int)kentta.Tuentatapaus - 4) / 2 + lisa, taulukonIndeksi1];
            double xAlfa2 = momenttikertoimetTaulukko[((int)kentta.Tuentatapaus - 4) / 2 + lisa, taulukonIndeksi2];
            double xAlfa = (xAlfa2 - xAlfa1) * interpolointikerroin + xAlfa1;
            double xAlfaToissijainen = 0;

            // Otetaan huomioon alfan laskennassa kentän mahdollinen toissijainen tuenta
            if (kentta.ToinenTuenta != Kentta.Tuentatapaukset.EiTukia) {
                double xAlfaToissijainen1 = momenttikertoimetTaulukko[((int)kentta.ToinenTuenta - 4) / 2 + lisa, taulukonIndeksi1];
                double xAlfaToissijainen2 = momenttikertoimetTaulukko[((int)kentta.ToinenTuenta - 4) / 2 + lisa, taulukonIndeksi2];
                xAlfaToissijainen = (xAlfaToissijainen2 - xAlfaToissijainen1) * interpolointikerroin + xAlfaToissijainen1;
                xAlfa = (xAlfa * kentta.TuentatapaustenSuhde + (xAlfaToissijainen * (1 - kentta.TuentatapaustenSuhde)));
            }
            return xAlfa;
        }

        /// <summary>
        /// BY202 taulukon arvot jäykistyskertoimille
        /// </summary>
        private void AlustaJaykistysTaulukko() {
            jaykistysTaulukko = new double[,] {
                { 6.43, 5.91, 5.51, 5.18, 4.91, 4.69, 4.50, 4.34, 4.21, 4.09, 3.98 },       // 2
                { 6.43, 6.38, 6.34, 6.32, 6.30, 6.29, 6.28, 6.28, 6.28, 6.28, 6.28 },       // 3
                { 7.20, 7.10, 7.01, 6.94, 6.87, 6.82, 6.77, 6.72, 6.68, 6.65, 6.62 },       // 4a
                { 7.20, 6.61, 6.15, 5.73, 5.40, 5.11, 4.88, 4.68, 4.50, 4.34, 4.19 },       // 4b
                { 6.65, 6.24, 5.90, 5.63, 5.40, 5.22, 5.08, 4.96, 4.86, 4.77, 4.71 },       // 5
                { 6.65, 6.54, 6.46, 6.39, 6.35, 6.32, 6.30, 6.29, 6.28, 6.28, 6.28 },       // 6
                { 7.78, 7.63, 7.49, 7.37, 7.26, 7.16, 7.07, 6.99, 6.91, 6.85, 6.79 },       // 7a
                { 7.35, 6.80, 6.35, 5.99, 5.70, 5.47, 5.28, 5.13, 5.00, 4.90, 4.82 },       // 7b
                { 7.35, 7.23, 7.12, 7.03, 6.95, 6.88, 6.82, 6.76, 6.71, 6.67, 6.63 },       // 8a
                { 7.78, 7.17, 6.67, 6.21, 5.84, 5.51, 5.24, 5.00, 4.78, 4.58, 4.40 },       // 8b
                { 7.88, 7.73, 7.59, 7.46, 7.34, 7.23, 7.13, 7.04, 6.96, 6.88, 6.81 },       // 9a
                { 7.88, 7.27, 6.78, 6.35, 6.00, 5.72, 5.49, 5.31, 5.15, 5.02, 4.91 }        // 9b
            };
        }

        /// <summary>
        /// // BY202 taulukon arvot momenttikertoimille
        /// </summary>
        private void AlustaMomenttiTaulukko() {
            momenttiTaulukko = new double[,] {
                { 0,0,0,0,0,0,0,0,0,0,0},                               // 1 ays Täytekenttä
                { 0,0,0,0,0,0,0,0,0,0,0},                               // 1 axs Täytekenttä
                { 555,564,564,555,542,526,508,492,478,469,464 },        // 1 ayf Kentta.Tuentatapaukset.EiTukia + 0
                { 555,628,694,754,807,854,894,928,958,982,996 },        // 1 axf Kentta.Tuentatapaukset.EiTukia + 1
                { 0,0,0,0,0,0,0,0,0,0,0},                               // 2 ays Täytekenttä
                { 575,626,672,714,752,786,816,842,864,884,900 },        // 2 axs Kentta.Tuentatapaukset.TukiPitkallaSivulla + 0
                { 368,364,355,343,331,320,310,301,293,286,280 },        // 2 ayf Kentta.Tuentatapaukset.TukiPitkallaSivulla + 1
                { 429,471,505,536,560,577,592,602,610,620,632 },        // 2 axf ... + 2
                { 575,632,684,728,761,786,800,807,807,804,800 },        // 3 ays Kentta.Tuentatapaukset.TukiLyhyellaSivulla + 0
                { 0,0,0,0,0,0,0,0,0,0,0},                               // 3 axs Täytekenttä
                { 429,456,472,480,478,469,456,442,430,426,426 },        // 3 ayf ... + 1
                { 368,435,494,548,597,643,686,726,762,796,828 },        // 3 axf ... + 2
                { 415,440,462,479,492,503,511,517,520,521,520 },        // 4 ays Kentta.Tuentatapaukset.TukiaKaksiVierekkain + 0
                { 415,470,522,571,617,661,702,740,775,808,840 },        // 4 axs ... + 1
                { 314,317,316,311,304,294,283,272,261,253,250 },        // 4 ayf ... + 2
                { 314,357,392,422,448,471,489,505,516,529,538 },        // 4 axf ... + 3
                { 0,0,0,0,0,0,0,0,0,0,0},                               // 5 ays Täytekenttä
                { 479,510,537,558,577,594,609,622,635,648,660 },        // 5 axs Kentta.Tuentatapaukset.TukiaKaksiVastakkainPSivulla + 0
                { 232,224,217,211,206,201,196,190,184,177,168 },        // 5 ayf ... + 1
                { 310,330,347,361,374,385,394,400,405,409,414 },        // 5 axf ... + 2
                { 479,536,588,635,673,703,729,751,770,785,790 },        // 6 ays Kentta.Tuentatapaukset.TukiaKaksiVastakkainLSivulla + 0       
                { 0,0,0,0,0,0,0,0,0,0,0},                               // 6 axs Täytekenttä
                { 310,339,364,383,397,405,409,409,405,397,388 },        // 6 ayf ... + 1
                { 232,276,322,369,417,465,511,555,595,631,664 },        // 6 axf ... + 2
                { 308,311,314,316,318,319,320,320,320,320,320 },        // 7 ays Kentta.Tuentatapaukset.TukiaKolmeLSivuVapaa + 0
                { 367,406,441,473,500,525,546,565,580,591,600 },        // 7 axs ... + 1
                { 212,206,201,196,190,184,479,175,171,167,162 },        // 7 ayf ... + 2
                { 247,268,288,305,320,332,342,350,358,365,370 },        // 7 axf ... + 3
                { 367,396,420,440,454,465,473,480,487,494,500 },        // 8 ays Kentta.Tuentatapaukset.TukiaKolmePSivuVapaa + 0
                { 308,367,424,477,529,579,628,675,718,760,800 },        // 8 axs ... + 1
                { 247,258,265,268,268,264,258,250,241,230,224 },        // 8 ayf ... + 2
                { 212,247,280,310,336,360,379,396,410,422,430 },        // 8 axf ... + 3
                { 292,300,305,308,310,310,310,310,310,310,310 },        // 9 ays Kentta.Tuentatapaukset.TukiaNelja + 0
                { 292,337,378,414,445,470,491,509,525,543,560 },        // 9 axs ... + 1
                { 167,168,168,167,164,160,156,152,148,146,146 },        // 9 ayf ... + 2
                { 167,194,214,231,246,259,272,284,294,303,310 }         // 9 axf ... + 3
            };
        }

        private void AlustaMomenttikertoimet() {
            momenttikertoimetTaulukko = new double[,] {
                { 60,60,59,58,57,56,54,53,52,51,50 },
                { 14,30,45,60,76,94,114,134,153,172,190 },
                { 14,0,0,0,0,0,0,0,0,0,0},
                { 60,60,59,58,56,54,51,48,45,43,42},
                { 58,49,45,42,41,41,42,43,45,47,50},
                { 58,73,88,103,117,130,142,153,162,171,180},
                { 105,104,101,97,94,91,88,84,81,78,75},
                { 42,71,98,125,150,170,190,210,230,250,270},
                { 42,22,9,0,0,0,0,0,0,0,0},
                { 105,105,105,104,102,99,96,93,90,87,84},
                { 110,98,91,86,82,78,76,75,75,75,75},
                { 85,100,120,142,163,183,203,222,241,258,275},
                { 85,76,71,68,67,66,67,68,70,72,75},
                { 110,125,140,154,167,179,190,200,210,220,230},
                { 97,88,81,76,74,72,71,71,72,73,75},
                { 97,114,133,153,172,191,211,231,248,264,280},
            };
        }

        private void AlustaKenttamomenttienKertoimet() {
            kenttamomenttienKorjauskertoimetTaulukko = new double[,] {
                { 280,220,172,135,110,94,83,74,66,60,55 },
                { 380,356,338,325,315,305,295,285,274,258,238},
                { 380,374,364,350,331,310,289,272,258,251,248},
                { 280,314,344,373,398,421,443,461,473,481,484},
            };
        }

        /// <summary>
        /// Kääntää momenttiarvot esimerkkitapauksen järjestykseen tarkistusta helpottamaan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e) {
            if (kaannetty) {
                tbMys.Location = mxx[0];
                tbMxs.Location = mxx[1];
                tbMyf.Location = mxx[2];
                tbMxf.Location = mxx[3];
                lMys.Location = mxx[4];
                lMxs.Location = mxx[5];
                lMyf.Location = mxx[6];
                lMxf.Location = mxx[7];

                lAys.Location = axx[0];
                lAxs.Location = axx[1];
                lAyf.Location = axx[2];
                lAxf.Location = axx[3];
                tbAys.Location = axx[4];
                tbAxs.Location = axx[5];
                tbAyf.Location = axx[6];
                tbAxf.Location = axx[7];

                kaannetty = false;
                bJarjestys.Text = "Aseta arvot esimerkin järjestykseen";
            } else {
                tbMys.Location = mxx[1];
                tbMxs.Location = mxx[0];
                tbMyf.Location = mxx[3];
                tbMxf.Location = mxx[2];
                lMys.Location = mxx[5];
                lMxs.Location = mxx[4];
                lMyf.Location = mxx[7];
                lMxf.Location = mxx[6];

                lAys.Location = axx[1];
                lAxs.Location = axx[0];
                lAyf.Location = axx[3];
                lAxf.Location = axx[2];
                tbAys.Location = axx[5];
                tbAxs.Location = axx[4];
                tbAyf.Location = axx[7];
                tbAxf.Location = axx[6];

                kaannetty = true;
                bJarjestys.Text = "Vaihda takaisin";
            }
        }

        private void label3_Click(object sender, EventArgs e) {

        }

        private void label2_Click(object sender, EventArgs e) {

        }
    }
}
