using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Laatan_mitoitus {
    public class HarjaterasRyhma {

        private int koko;
        private double tankojenVali;

        public int Maara { get; set; }
        public int Koko {
            get {
                return koko;
            }
            set {
                if (tankojenVali != 0) {
                    PintaAlaMetrille = Math.PI * Math.Pow((double)value / 2, 2) * ((int)(1000 / tankojenVali));
                }
                koko = value;
            }
        }
        public double Jako {
            get {
                return tankojenVali;
            }
            set {
                if (koko != 0) {
                    PintaAlaMetrille = Math.PI * Math.Pow((double)koko / 2, 2) * ((int)(1000 / value));
                }
                tankojenVali = value;
            }
        }
        public double TankojenPituus { get; set; }
        public Point SijaintiAlku { get; set; }
        public Point SijaintiLoppu { get; set; }
        public double PintaAlaMetrille { get; private set; }
        public string Tyyppi { get; set; }


        public HarjaterasRyhma() {
            koko = 0;
            tankojenVali = 0;
        }

        public override string ToString() {
            return Koko + " - k" + Jako;
        }

        public enum TerasKoot {
            O_6 = 6,
            O_8 = 8,
            O_10 = 10,
            O_12 = 12
        }

    }
}
