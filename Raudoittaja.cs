using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laatan_mitoitus {
    public class Raudoittaja {

        Kentta[] kentat;
        List<Kentta> kentatLista;
        Asetukset asetukset;
        public bool VirheenkorjausKenttaraudat { get; set; }
        public bool VirheenkorjausTukiraudat { get; set; }
        private ArrayList virheet = new ArrayList();
        Tiedostonkasittelija tKasittelija;

        #region Raudoituksen muuttujat
        double alfaCC = 0.85;
        double materOsaVarmBetoni = 1.5;             // GammaC
        double materOsaVarmTeras = 1.15;             // GammaY
        double toteutunutMittaYlapinnasta;           // dtot [mm]
        double betoninSuunnitteluLujuus;             // fcd [Mpa] = [N/mm2]
        double puristuslujuudenKeskiarvo;            // fcm
        double vetolujuudenKeskiarvo;                // fctm
        double ominaisVetolujuus;                    // fctk,0,05
        double teraksenMitoituslujuus;               // fyd [Mpa] = [N/mm2]
        double vahimmaisBetonipeite;                 // cmin [mm]
        double puristuspinnanSuhtKorkeus;            // Beta
        //double geometrinenRaudoitusSuhde;          // p ?
        //double mekaaninenRaudoitusSuhde;           // w ?
        double sisainenMomenttivarsi;                // z [mm]
        double suhteellinenMomentti;                 // µ
        double mitoitusleveys = 1000;                // Lasketaan metrin levystä "palkkia" laatasta [mm]
        // Tasapainoraudoitustaulukko
        double[] tasapRaudTarkTaulukko = new double[] { 0.372, 0.493, 0.353, 0.458, 0.336, 0.428 };
        double raudoitusVaadittuMaara;               // As [mm2]
        double raudoitusVahMaara;                    // Raudan määrän vähimmäistarve (reunat tai väh. raudoitus)
        #endregion

        private Dictionary<string, int> verkkotyypit;

        public Verkko VerkkoKeskella { get; private set; }
        public Verkko VerkkoYleensa { get; set; }
        public int VerkonSilmakoko { get; private set; }
        public int VerkonTeraksenKoko { get; private set; }
        public double RaudoitusVaadMaara {
            get { return raudoitusVaadittuMaara; }
            set { raudoitusVaadittuMaara = value; }
        }
        public double RaudoitusVahMaara {
            get {
                return raudoitusVahMaara;
            }
            set {
                raudoitusVahMaara = value;
            }
        }
        public double ToteutunutMittaYlapinnasta {
            private set {
                toteutunutMittaYlapinnasta = value;
            }
            get {
                return toteutunutMittaYlapinnasta;
            }
        }
        public double Suojabetoni {
            get {
                return vahimmaisBetonipeite;
            }
        }
        public double SilmakokoLeveys { get; set; }
        public double SilmakokoKorkeus { get; set; }


        public ArrayList Virheet {
            get {
                return virheet;
            }
        }

        public Raudoittaja(Kentta[] kentat, Asetukset asetukset) {
            tKasittelija = new Tiedostonkasittelija(Paaikkuna.HAKEMISTO);
            this.kentat = kentat;
            this.asetukset = asetukset;
            puristuslujuudenKeskiarvo = asetukset.BetoninLujuus + 8;
            if (asetukset.BetoninLujuus <= 50) {
                vetolujuudenKeskiarvo = 0.3 * Math.Pow(asetukset.BetoninLujuus, 2.0 / 3.0);
            } else {
                vetolujuudenKeskiarvo = 2.12 * Math.Log(1 + (puristuslujuudenKeskiarvo / 10));
                tKasittelija.KirjoitaLokiin("Betoninlujuus > 50 !");
            }
            ominaisVetolujuus = 0.7 * vetolujuudenKeskiarvo;
            VerkkoKeskella = new Verkko("Ei verkkoa", 0, 0, 0);
            AlustaVerkkotyypit();
        }

        /// <summary>
        /// Konstruktori, joka ottaa ensimmäiseksi parametrikseen List<> olion. HUOM! Tätä ei vielä testattu eikä välttämättä toimi oikein!
        /// </summary>
        /// <param name="kentat"></param>
        /// <param name="asetukset"></param>
        public Raudoittaja(List<Kentta> kentat, Asetukset asetukset) {
            tKasittelija = new Tiedostonkasittelija(Paaikkuna.HAKEMISTO);
            this.kentatLista = kentat;
            this.asetukset = asetukset;
            puristuslujuudenKeskiarvo = asetukset.BetoninLujuus + 8;
            if (asetukset.BetoninLujuus <= 50) {
                vetolujuudenKeskiarvo = 0.3 * Math.Pow(asetukset.BetoninLujuus, 2.0 / 3.0);
            } else {
                vetolujuudenKeskiarvo = 2.12 * Math.Log(1 + (puristuslujuudenKeskiarvo / 10));
                tKasittelija.KirjoitaLokiin("Betoninlujuus > 50 !");
            }
            ominaisVetolujuus = 0.7 * vetolujuudenKeskiarvo;
            VerkkoKeskella = new Verkko("Ei verkkoa", 0, 0, 0);
            AlustaVerkkotyypit();
        }

        /// <summary>
        /// Laskee ja asettaa kaikki raudoitukset kaikille kentille eikä avaa mitään ikkunoita
        /// </summary>
        public void LaskeKaikkiRaudoitukset() {
            for (int i = 0; i < kentat.Length; i++) {
                LaskeRaudoitusLyhytSivu(i, false);
            }
            LaskeTukiraudoituksetKaikilleKentille();
        }

        /// <summary>
        /// Laskee vähimmäisraudoituksen määrän annetulle momentille
        /// </summary>
        /// <param name="momentti">Mitoittava momentti</param>
        private void LaskeVahimmaisRaudoitus(double momentti) {
            double AsMin = Math.Max(0.26 * (vetolujuudenKeskiarvo / asetukset.TeraksenLujuus) * mitoitusleveys * asetukset.ArvioituMittaYlapinnasta,
                0.0013 * mitoitusleveys * asetukset.ArvioituMittaYlapinnasta);
            double puolikasMxf = LaskeVaadittuRaudoitusPintaAla(momentti / 2);
            raudoitusVahMaara = Math.Max(AsMin, puolikasMxf);
        }

        private double LaskeVahimmaisBetonipeite() {

            double cmindur = 10;
            switch (asetukset.Rasitusluokka) {
                case "X0":
                case "XC1":
                    cmindur = 10;
                    break;
                case "XC2":
                    cmindur = 20;
                    break;
                case "XC3":
                case "XC4":
                    cmindur = 25;
                    break;
                case "XD1":
                    cmindur = 30;
                    break;
                case "XD2":
                    cmindur = 35;
                    break;
                case "XD3":
                    cmindur = 40;
                    break;
                case "XS1":
                    cmindur = 30;
                    break;
                case "XS2":
                case "XS3":
                    cmindur = 40;
                    break;

            }
            if (VerkkoKeskella == null) {
                return Math.Max(cmindur, 10);
            } else {
                return Math.Max(Math.Max(VerkkoKeskella.RaudanKoko, cmindur), 10);
            }
        }

        /// <summary>
        /// Laskee raudoitukset kentän x-suunnassa. Avaa ikkunan joka näyttää raudoituksen ja asettaa raudoituksen annettuun kenttäolioon
        /// </summary>
        /// <param name="kentanNumero">Laskettavan kentän numero -1</param>
        /// <param name="avaaForm">Avataanko ikkuna, jossa näytetään raudoitukset, vai lasketaanko vain raudoitus</param>
        public bool LaskeRaudoitusLyhytSivu(int kentanNumero, bool avaaForm = true) {
            // Tarkistetaan että leveys ei ole pienempi kuin mitoittava 1000mm
            if (Math.Min(kentat[kentanNumero].Leveys, kentat[kentanNumero].Korkeus) * 1000 < 1000) {
                mitoitusleveys = Math.Min(kentat[kentanNumero].Leveys, kentat[kentanNumero].Korkeus) * 1000;
                DebugViesti("Mitoitusleveys = " + mitoitusleveys);
            }

            // Lasketaan vähimmäisbetonipeite rasitusluokan avulla ja varmistetaan, että suojabetonia on tarpeeksi
            vahimmaisBetonipeite = LaskeVahimmaisBetonipeite();
            if (vahimmaisBetonipeite > asetukset.Suojabetoni) {
                throw new Exception("Suojabetonin arvo asetuksissa on pienempi kuin vähimmäisarvo!");
            } else {
                vahimmaisBetonipeite = asetukset.Suojabetoni;
            }
            double momentti = kentat[kentanNumero].Kenttamomentit[0] * 1000;
            LaskeVahimmaisRaudoitus(momentti);

            // Lasketaan kuinka paljon raudoituspinta-alaa tarvitaan momentin takia
            raudoitusVaadittuMaara = LaskeVaadittuRaudoitusPintaAla(momentti);
            // Lasketaan sopiva verkko minimiraudoituksen perusteella
            VerkkoYleensa = LaskeSopivinVerkko((int)raudoitusVahMaara);
            VerkkoKeskella = LaskeSopivinVerkko((int)RaudoitusVaadMaara, VerkkoYleensa.PintaAlaVaaka);
            ToteutunutMittaYlapinnasta = asetukset.LaatanPaksuus - vahimmaisBetonipeite - VerkkoYleensa.RaudanKoko / 2;
            if (ToteutunutMittaYlapinnasta < asetukset.ArvioituMittaYlapinnasta) {
                throw new Exception("Todellinen mitta yläpinnasta on pienempi kuin arvioitu mitta!");
            }
            VerkkoYleensa.SuojaBetoni = asetukset.Suojabetoni;
            VerkkoKeskella.SuojaBetoni = asetukset.Suojabetoni + VerkkoYleensa.RaudanKoko * 2;
            kentat[kentanNumero].RaudoitusYleensa = this.VerkkoYleensa;
            kentat[kentanNumero].RaudoitusKeskella = this.VerkkoKeskella;
            kentat[kentanNumero].VaadittuRaudoitusKeskella = (int)raudoitusVaadittuMaara;
            kentat[kentanNumero].Minimiraudoitus = (int)raudoitusVahMaara;
            if (avaaForm) {
                RaudoitusForm rForm = new RaudoitusForm(this, kentat[kentanNumero]);
                rForm.Show();
            }
            return true;
        }

        /// <summary>
        /// Laskee tukiraudoitukset tietylle kentälle kaikkiin suuntiin
        /// </summary>
        /// <param name="kentta">Kenttä, jolle tukiraudoitukset lasketaan</param>
        public void LaskeTukiraudoituksetKentalle(Kentta kentta) {
            #region Virheenkorjaus
            string[] vaaditutRaudoitukset = new string[8];
            int i = 0;
            #endregion

            foreach (string suunta in kentta.HaeTasatutTukimomentit().Keys) {

                double momentti = kentta.HaeTasattuTukimomentti(suunta);
                int vaadittuRaudoitus = LaskeVaadittuRaudoitusPintaAla(momentti * 1000);
                HarjaterasRyhma sopivaRyhma = LaskeSopivinHarjaterasryhma(vaadittuRaudoitus);
                AsetaHarjaterasryhmalleAlkuJaLoppupiste(kentta, suunta, sopivaRyhma);
                sopivaRyhma.TankojenPituus = 1000;
                sopivaRyhma.Tyyppi = "A500HW";
                if (kentta.Tukiraudat.ContainsKey(suunta)) {
                    kentta.Tukiraudat[suunta] = sopivaRyhma;
                } else {
                    kentta.Tukiraudat.Add(suunta, sopivaRyhma);
                }
                #region Virheenkorjaus
                vaaditutRaudoitukset[i] = vaadittuRaudoitus.ToString();
                i++;
                #endregion
            }
            #region Virheenkorjaus
            if (VirheenkorjausTukiraudat) {
                StringBuilder debug = new StringBuilder();
                i = 0;
                foreach (string suunta in kentta.Tukiraudat.Keys) {
                    debug.Append("Suunta: " + suunta + "\n");
                    debug.Append("Koko: " + kentta.HaeTukiraudat(suunta).Koko + "\n");
                    debug.Append("Jako: " + kentta.HaeTukiraudat(suunta).Jako + "\n");
                    debug.Append("Raudoituksen pinta-ala metrille: " + (int)kentta.HaeTukiraudat(suunta).PintaAlaMetrille + "\n");
                    debug.Append("Vaadittu pinta-ala: " + vaaditutRaudoitukset[i] + "\n\n");
                    i++;

                }
                System.Windows.Forms.MessageBox.Show(debug.ToString(), "Kenttä " + kentta.Numero);
            }
            #endregion
        }

        public void LaskeTukiraudoituksetKaikilleKentille() {
            for (int i = 0; i < kentat.Length; i++) {
                LaskeTukiraudoituksetKentalle(kentat[i]);
            }
        }

        public Verkko LaskeNurkkaRaudoitus(Kentta kentta, double kuorma) {
            double momentti = (kuorma * Math.Min(kentta.Leveys, kentta.Korkeus) * Math.Max(kentta.Leveys, kentta.Korkeus)) / 40;
            System.Windows.Forms.MessageBox.Show("Momentti: " + momentti.ToString() + " kNm");
            int vaadittuRaudoitus = LaskeVaadittuRaudoitusPintaAla(momentti);
            return LaskeSopivinVerkko(vaadittuRaudoitus, false);
        }

        /// <summary>
        /// Laskee annetun momentin ja asetus-olion perusteella vaaditun raudoitus pinta-alan
        /// </summary>
        /// <param name="momentti">Mitoittava momentti newtoneissa</param>
        /// <returns></returns>
        private int LaskeVaadittuRaudoitusPintaAla(double momentti) {
            betoninSuunnitteluLujuus = alfaCC * asetukset.BetoninLujuus / materOsaVarmBetoni;
            teraksenMitoituslujuus = asetukset.TeraksenLujuus / materOsaVarmTeras;
            suhteellinenMomentti = momentti * 1000 / (betoninSuunnitteluLujuus * mitoitusleveys * Math.Pow(asetukset.ArvioituMittaYlapinnasta, 2));
            if (suhteellinenMomentti > tasapRaudTarkTaulukko[0]) {
                virheet.Add("Suhteellinen momentti ylittää tasapainorajan");
            }
            puristuspinnanSuhtKorkeus = 1 - Math.Sqrt(1 - 2 * suhteellinenMomentti);
            sisainenMomenttivarsi = asetukset.ArvioituMittaYlapinnasta - (1 - (puristuspinnanSuhtKorkeus / 2));
            #region virheenkorjaus
            if (VirheenkorjausKenttaraudat) {
                System.Windows.Forms.MessageBox.Show(
                    "\nMomentti = " + momentti.ToString() +
                    "\nFyd = " + teraksenMitoituslujuus.ToString() +
                    "\nd = " + asetukset.ArvioituMittaYlapinnasta.ToString() +
                    "\nµ = " + suhteellinenMomentti.ToString() +
                    "\nbeta = " + puristuspinnanSuhtKorkeus.ToString() +
                    "\nz = " + sisainenMomenttivarsi.ToString() +
                    "\nAs = " + raudoitusVaadittuMaara.ToString()
                    );
            }
            #endregion
            return (int)(momentti * 1000 / (sisainenMomenttivarsi * teraksenMitoituslujuus));

        }

        /// <summary>
        /// Hakee listalta sopivan verkon vaaditun teräspinta-alan perusteella. Vähennyksen avulla voidaan hakea useampia verkkoja
        /// </summary>
        /// <param name="vaadittuTeras">Vaaditun teräsmäärän pinta-ala</param>
        /// <param name="vahennys">Olemassa olevan teräsmäärän p-ala</param>
        public Verkko LaskeSopivinVerkko(int vaadittuTeras, int vahennys = 0) {
            return LaskeSopivinVerkko(vaadittuTeras, true, vahennys);
        }

        public Verkko LaskeSopivinVerkko(int vaadittuTeras, bool kaytaHaluttuaVerkkoa, int vahennys = 0) {
            vaadittuTeras -= vahennys;
            if (vaadittuTeras < 0) {
                return new Verkko("Ei verkkoa", 0, 0, 0);
            }
            string text = "";
            foreach (string s in verkkotyypit.Keys) {
                text += s + "\n";
                if (verkkotyypit[s] > vaadittuTeras) {
                    if (s.Length == 11) {
                        string tyyppi = s.Substring(0, 5);
                        int koko = int.Parse(s.Substring(6, 1));
                        double silma = double.Parse(s.Substring(8, 3));
                        if (koko < asetukset.MinVerkkoTerasKoko && vahennys == 0) continue;
                        if (asetukset.HaluttuVerkkoSilmaVali != 0 && silma != asetukset.HaluttuVerkkoSilmaVali && vahennys == 0 && kaytaHaluttuaVerkkoa) continue;
                        return new Verkko(tyyppi, koko, silma, silma);
                    } else {
                        string tyyppi = s.Substring(0, 5);
                        int koko = int.Parse(s.Substring(6, 2));
                        double silma = double.Parse(s.Substring(9, 3));
                        if (koko < asetukset.MinVerkkoTerasKoko && vahennys == 0) continue;
                        if (asetukset.HaluttuVerkkoSilmaVali != 0 && silma != asetukset.HaluttuVerkkoSilmaVali && vahennys == 0 && kaytaHaluttuaVerkkoa) continue;
                        return new Verkko(tyyppi, koko, silma, silma);
                    }
                }
            }
            return new Verkko("Ei sopivaa", 0, 0, 0);
        }

        public HarjaterasRyhma LaskeSopivinHarjaterasryhma(int vaadittuTeras) {
            HarjaterasRyhma palautus = new HarjaterasRyhma();
            foreach (int i in Enum.GetValues(typeof(HarjaterasRyhma.TerasKoot))) {
                if (i < asetukset.MinTerasKoko) continue;
                palautus.Koko = i;
                palautus.Jako = 200;
                
                if (palautus.PintaAlaMetrille > vaadittuTeras) {
                    if (asetukset.TukiRaudJako == 150) continue;
                    return palautus;
                }
                palautus.Jako = 150;
                if (palautus.PintaAlaMetrille > vaadittuTeras) {
                    if (asetukset.TukiRaudJako == 200) continue;
                    return palautus;
                }
            }
            return null;
        }

        /// <summary>
        /// Asettaa ryhmälle alku- ja loppupisteet suunnan ja kentän mittojen perusteella
        /// </summary>
        ///  <param name="kentta">Kenttä, jonka mittojen mukaan pisteet asetetaan</param>
        /// <param name="suunta">Mihin suuntaan tuki osoittaa kenttään nähden</param>
        /// <param name="ryhma">Harjateräsryhmä, johon suuntapisteet asetetaan</param>
        private void AsetaHarjaterasryhmalleAlkuJaLoppupiste(Kentta kentta, string suunta, HarjaterasRyhma ryhma) {
            switch (suunta) {
                case "vasen":
                    ryhma.SijaintiAlku = kentta.VasenAlanurkka;
                    ryhma.SijaintiLoppu = new System.Drawing.Point(kentta.VasenAlanurkka.X, kentta.VasenAlanurkka.Y + (int)(kentta.Korkeus * 1000));
                    break;
                case "ylos":
                    ryhma.SijaintiAlku = new System.Drawing.Point(kentta.VasenAlanurkka.X, kentta.VasenAlanurkka.Y + (int)(kentta.Korkeus * 1000));
                    ryhma.SijaintiLoppu = new System.Drawing.Point(kentta.VasenAlanurkka.X + (int)(kentta.Leveys * 1000), kentta.VasenAlanurkka.Y + (int)(kentta.Korkeus * 1000));
                    break;
                case "oikea":
                    ryhma.SijaintiAlku = new System.Drawing.Point(kentta.VasenAlanurkka.X + (int)(kentta.Leveys * 1000), kentta.VasenAlanurkka.Y + (int)(kentta.Korkeus * 1000));
                    ryhma.SijaintiLoppu = new System.Drawing.Point(kentta.VasenAlanurkka.X + (int)(kentta.Leveys * 1000), kentta.VasenAlanurkka.Y);
                    break;
                case "alas":
                    ryhma.SijaintiLoppu = new System.Drawing.Point(kentta.VasenAlanurkka.X + (int)(kentta.Leveys * 1000), kentta.VasenAlanurkka.Y);
                    ryhma.SijaintiAlku = kentta.VasenAlanurkka;
                    break;
            }
        }

        private void AlustaVerkkotyypit() {
            verkkotyypit = new Dictionary<string, int>();
            verkkotyypit.Add("B500K-4-150", (int)(Math.PI * (2 * 2) * 6)); // 1000/150 == 6,66666666 ~~ 6

            verkkotyypit.Add("B500K-5-200", (int)(Math.PI * 2.5 * 2.5 * 5));  // 1000/200 == 5
            verkkotyypit.Add("B500K-5-150", (int)(Math.PI * 2.5 * 2.5 * 6));
            
            verkkotyypit.Add("B500K-6-200", (int)(Math.PI * 3 * 3 * 5));
            verkkotyypit.Add("B500K-6-150", (int)(Math.PI * 3 * 3 * 6));
            
            verkkotyypit.Add("B500K-8-200", (int)(Math.PI * 4 * 4 * 5));
            verkkotyypit.Add("B500K-8-150", (int)(Math.PI * 4 * 4 * 6));
            
            verkkotyypit.Add("B500K-10-200", (int)(Math.PI * 5 * 5 * 5));
            verkkotyypit.Add("B500K-10-150", (int)(Math.PI * 5 * 5 * 6));
            
        }

        /// <summary>
        /// Heittää esiin windows dialogin, jossa teksi s
        /// </summary>
        /// <param name="s"></param>
        private void DebugViesti(string s) {
            System.Windows.Forms.MessageBox.Show(s, "Debug");
        }


    }
}
