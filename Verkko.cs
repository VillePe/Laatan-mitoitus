using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laatan_mitoitus {
    public class Verkko {

        public double SilmaKokoVaaka { get; set; }
        public double SilmaKokoPysty { get; set; }
        public int RaudanKoko { get; set; }
        public string Tyyppi { get; set; }
        public int pintaAlaVaaka;
        public int pintaAlaPysty;
        public double SuojaBetoni { get; set; }

        public Verkko(string vTyyppi, int koko, double sVaaka, double sPysty) {
            SilmaKokoVaaka = sVaaka;
            SilmaKokoPysty = sPysty;
            RaudanKoko = koko;
            Tyyppi = vTyyppi;
            if (sVaaka != 0 && sPysty != 0) {
                pintaAlaVaaka = (int)(Math.PI * Math.Pow((double)RaudanKoko / 2, 2) * ((int)(1000 / SilmaKokoVaaka)));
                pintaAlaPysty = (int)(Math.PI * Math.Pow((double)RaudanKoko / 2, 2) * ((int)(1000 / SilmaKokoPysty)));
            } else {
                pintaAlaVaaka = 0;
                pintaAlaPysty = 0;
            }
        }

        public int PintaAlaVaaka {
            get {
                return pintaAlaVaaka;
            }
        }
        public int PintaAlaPysty {
            get {
                return pintaAlaPysty;
            }
        }

        public override string ToString() {
            if (Tyyppi == "Ei verkkoa" || Tyyppi == "Ei sopivaa") {
                return Tyyppi;
            }
            return Tyyppi + "-" + RaudanKoko + "-" + SilmaKokoVaaka;
        }
    }
}
