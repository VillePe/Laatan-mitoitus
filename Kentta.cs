using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Tekla.Structures;

namespace Laatan_mitoitus {
    public class Kentta {

        private double leveys;
        private double korkeus;
        private int numero;
        private bool useampiTuentatapaus = false;
        private Kentta.Tuentatapaukset tuentatapaus;
        private Kentta.Tuentatapaukset toinenTuenta;
        private Kentta.Jaykistystapaukset[] jaykistystapaukset = new Jaykistystapaukset[4];
        private Kentta.Jaykistystapaukset[] jaykistystapauksetToissijaiset = new Jaykistystapaukset[4];
        private string[] jaykistysTapausNimet = new string[4];
        private double[] tasaukset = new double[4];
        private double[] toissijaisetTasaukset = new double[4];
        private double[] tukimomentit = new double[4];
        private Dictionary<string, double> tasatutTukimomentit = new Dictionary<string, double>();
        private Dictionary<string, double> tukimomentitDict = new Dictionary<string, double>();
        private double[] toissijaisetTukimomentit = new double[4];
        private double[] kenttamomentit = new double[2];
        private Point vasenAlanurkka;

        public Verkko RaudoitusYleensa { get; set; }
        public Verkko RaudoitusKeskella { get; set; }
        public int Minimiraudoitus { get; set; }
        public Dictionary<string, HarjaterasRyhma> Tukiraudat = new Dictionary<string, HarjaterasRyhma>();
        public Tekla.Structures.Geometry3d.Point VasenAlanurkkaTekla { get; private set; }
        public double TuentatapaustenSuhde {
            set {
                tuentatapaustenSuhde = value;
            }
            get {
                return tuentatapaustenSuhde;
            }
        }
        /// <summary>
        /// Kentän leveys metreissä
        /// </summary>
        public double Leveys {
            get {
                return leveys;
            }
        }
        /// <summary>
        /// Kentän korkeus metreissä
        /// </summary>
        public double Korkeus {
            get {
                return korkeus;
            }
        }
        public int Numero {
            get {
                return numero;
            }
        }
        public bool UseampiTuenta {
            get {
                return useampiTuentatapaus;
            }
        }
        public Tuentatapaukset Tuentatapaus {
            set {
                tuentatapaus = value;
            }
            get {
                return tuentatapaus;
            }
        }
        public Tuentatapaukset ToinenTuenta {
            set {
                toinenTuenta = value;
            }
            get {
                return toinenTuenta;
            }
        }
        public int VaadittuRaudoitusKeskella { get; set; }

        // Ensimmäisen tuentatapauksen suhde kokonaispituuteen
        private double tuentatapaustenSuhde;

        public Kentta(int numero, double leveys, double korkeus, bool useampiTuentatapaus = false) {
            this.leveys = leveys;
            this.korkeus = korkeus;
            this.numero = numero;
            this.useampiTuentatapaus = useampiTuentatapaus;
            jaykistystapauksetToissijaiset = new Jaykistystapaukset[4];

            // Alustaa toissijaiset jäykistystapaukset "EiTapausta" tilaan
            for (int i = 0; i < 4; i++) {
                jaykistystapauksetToissijaiset[i] = Jaykistystapaukset.EiTapausta;
            }
        }

        public double[] Tukimomentit {
            set {
                tukimomentit = value;
            }
            get {
                return tukimomentit;
            }
        }

        /// <summary>
        /// Vasen alanurkka millimetrin tarkkuudella
        /// </summary>
        public Point VasenAlanurkka {
            get {
                return vasenAlanurkka;
            }
            set {
                vasenAlanurkka = value;
                Tekla.Structures.Geometry3d.Point x = new Tekla.Structures.Geometry3d.Point(VasenAlanurkka.X, VasenAlanurkka.Y, 3000);
                VasenAlanurkkaTekla = x;
            }
        }
        
        public double[] Kenttamomentit {
            get {
                return kenttamomentit;
            }
            set {
                kenttamomentit = value;
            }
        }
        
        public void AsetaKenttamomentti(int i, double m) {
            kenttamomentit[i] = m; 
        }

        public void AsetaKenttamomentti(VoimienSuunnat i, double m) {
            kenttamomentit[(int)i] = m;
        }

        public void AsetaTukimomentti(int i, double m) {
            tukimomentit[i] = m;
        }

        public void AsetaTukimomentti(VoimienSuunnat i, double m) {
            tukimomentit[(int)i] = m;
        }

        public void AsetaTasattuTukimomentti(string suunta, double momentti) {
            tasatutTukimomentit.Add(suunta, momentti);
        }

        public void AsetaToissijainenTukimomentti(int i, double m) {
            toissijaisetTukimomentit[i] = m;
        }

        /// <summary>
        /// Asettaa kentälle momenttitasauksen annettuun indeksiin 
        /// </summary>
        /// <param name="tasaus"></param>
        /// <param name="i">0 = vasen, 1 = ylös, 2 = oikea, 3 = alas</param>
        public void AsetaTasaus(double tasaus, int i) {
            tasaukset[i] = tasaus;
        }

        public void AsetaToissijainenTasaus(double tasaus, int i) {
            toissijaisetTasaukset[i] = tasaus;
        }

        /// <summary>
        /// Asettaa kentälle jäykistystapaukset taulukkoon annetun taulukon perusteella
        /// </summary>
        /// <param name="tapaukset">Neljä alkiota jäykistystapauksia sisältävä taulukko</param>
        public void AsetaJaykistysTapaukset(Jaykistystapaukset[] tapaukset) {
            this.jaykistystapaukset = tapaukset;
        }

        /// <summary>
        /// Asettaa kentälle jäykistystapaukset taulukkoon annetun tapauksen, paikan ja nimen perusteella
        /// </summary>
        /// <param name="tapaus">Kenttään sopiva jäykistystapaus</param>
        /// <param name="i">Mones kentän jäykistetapaus (max 4)</param>
        /// <param name="nimi">Mistä mihin on jäykistetapauksen momentti (Esim. 1->2 tai 1.2)</param>
        /// <returns>Palauttaa totta jos lisääminen onnistui</returns>
        public bool AsetaJaykistysTapaukset(Jaykistystapaukset tapaus, int i, string nimi) {
            if (i > 4 || i < 0) {
                return false;
            }
            try {
                jaykistystapaukset[i] = tapaus;
                jaykistysTapausNimet[i] = nimi;
                return true;
            } catch (Exception) {
                return false;
            }

        }

        /// <summary>
        /// Asettaa kentälle toissijaiset jäykistystapaukset taulukkoon annetun tapauksen ja paikan perusteella
        /// </summary>
        /// <param name="tapaus">Kenttään sopiva jäykistystapaus</param>
        /// <param name="i">Mones kentän jäykistetapaus (max 4)</param>
        /// <returns>Palauttaa totta jos lisääminen onnistui</returns>
        public bool AsetaToissijaisetJaykistysTapaukset(Jaykistystapaukset tapaus, int i) {
            if (i > 4 || i < 0) {
                return false;
            }
            try {
                jaykistystapauksetToissijaiset[i] = tapaus;
                return true;
            } catch (Exception) {
                return false;
            }

        }
        
        public double HaeTasattuTukimomentti(string suunta) {
            return tasatutTukimomentit[suunta];
        }

        public Dictionary<string, double> HaeTasatutTukimomentit() {
            return tasatutTukimomentit;
        }
        
        public double HaeTasaus(int i) {
            return tasaukset[i];
        }
        
        public double HaeToissijainenTasaus(int i) {
            return toissijaisetTasaukset[i];
        }
        
        public Jaykistystapaukset[] HaeJaykistysTapaukset() {
            return jaykistystapaukset;
        }

        public Jaykistystapaukset HaeJaykistysTapaus(int i) {
            return jaykistystapaukset[i];
        }

        public Jaykistystapaukset[] HaeToissijaisetJaykistysTapaukset() {
            return jaykistystapauksetToissijaiset;
        }

        public Jaykistystapaukset HaeToissijainenJaykistysTapaus(int i) {
            return jaykistystapauksetToissijaiset[i];
        }

        public string HaeJaykistysTapausNimi(int i) {
            return jaykistysTapausNimet[i];
        }
        
        public HarjaterasRyhma HaeTukiraudat(string suunta) {
            return Tukiraudat[suunta];
        }

        /// <summary>
        /// Ensimmäisen tuentatapauksen suhde kokonaispituuteen
        /// </summary>

        public enum Tuentatapaukset {
            EiTukia = 0,
            TukiPitkallaSivulla = 4,
            TukiLyhyellaSivulla = 8,
            TukiaKaksiVierekkain = 12,
            TukiaKaksiVastakkainPSivulla = 16,
            TukiaKaksiVastakkainLSivulla = 20,
            TukiaKolmeLSivuVapaa = 24,
            TukiaKolmePSivuVapaa = 28,
            TukiaNelja = 32        }

        public enum Jaykistystapaukset {
            EiTukia_PSivuMomentti = 0,
            EiTukia_LSivuMomentti = 1,
            TukiP_LSivuMomentti = 2,
            TukiL_PSivuMomentti = 3,
            TukiP_PSivuMomentti = 4,
            TukiL_LSivuMomentti = 5,
            Tukia2PP_LSivuMomentti = 6,
            Tukia2PL_PSivuMomentti = 7,
            Tukia2PL_LSivuMomentti = 8,
            Tukia2LL_PSivuMomentti = 9,
            Tukia3_LSivuMomentti = 10,
            Tukia3_PSivuMomentti = 11,
            EiTapausta = -1
        }

        public enum VoimienSuunnat {
            Vasen = 0,
            Ylos = 1,
            Oikea = 2,
            Alas = 3
        }

    }
}
