using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laatan_mitoitus {
    public class Asetukset {

        public int TeraksenKoko { get; set; }
        public double LaatanPaksuus { get; set; }
        public int BetoninLujuus { get; set; }
        public int TeraksenLujuus { get; set; }
        public double BetoniVarmKerroin { get; set; }
        public double ArvioituMittaYlapinnasta { get; set; }
        public string Rasitusluokka { get; set; }
        public int RasitusluokkaIndx { get; set; }
        public double Suojabetoni { get; set; }
        public int MinTerasKoko { get; set; }
        public int MinVerkkoTerasKoko { get; set; }
        public int HaluttuVerkkoSilmaVali { get; set; }
        public int TukiRaudJako { get; set; }


        public Asetukset() {

        }

        public void AsetaAlkuperaiset() {
            this.BetoninLujuus = 25;
            this.LaatanPaksuus = 180;
            this.TeraksenLujuus = 500;
            this.BetoniVarmKerroin = 1.5;
            this.ArvioituMittaYlapinnasta = 130;
            this.Rasitusluokka = "X0";
            this.RasitusluokkaIndx = 0;
            this.Suojabetoni = 35.0;
            this.MinTerasKoko = 5;
            this.MinVerkkoTerasKoko = 5;
            this.HaluttuVerkkoSilmaVali = 0;
            this.TukiRaudJako = 0;
        }
    }
}
