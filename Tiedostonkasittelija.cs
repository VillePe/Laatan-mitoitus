using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Laatan_mitoitus {
    class Tiedostonkasittelija {

        private StringBuilder sBuilder;
        private string hakemisto;

        public Tiedostonkasittelija(string hakemisto) {
            sBuilder = new StringBuilder();
            this.hakemisto = hakemisto;
        }

        public string[] LueTiedostosta() {
            return File.ReadAllLines(hakemisto);
        }

        public void KirjoitaTiedostoon(TextBox[] vaakaTekstit, TextBox[] pystyTekstit, string[] kuormat) {
            foreach (TextBox t in vaakaTekstit) {
                sBuilder.AppendLine(t.Text);
            }
            foreach(TextBox t in pystyTekstit) {
                sBuilder.AppendLine(t.Text);
            }
            foreach(string s in kuormat) {
                sBuilder.AppendLine(s);
            }
            File.WriteAllText(hakemisto, sBuilder.ToString());
        }

        public void KirjoitaTiedostoon(TextBox[] vaakaTekstit, TextBox[] pystyTekstit, string[] kuormat, Asetukset asetukset) {
            foreach (TextBox t in vaakaTekstit) {
                sBuilder.AppendLine(t.Text);
            }
            foreach (TextBox t in pystyTekstit) {
                sBuilder.AppendLine(t.Text);
            }
            foreach (string s in kuormat) {
                sBuilder.AppendLine(s);
            }
            sBuilder.AppendLine(asetukset.ArvioituMittaYlapinnasta.ToString());
            sBuilder.AppendLine(asetukset.BetoninLujuus.ToString());
            sBuilder.AppendLine(asetukset.BetoniVarmKerroin.ToString());
            sBuilder.AppendLine(asetukset.LaatanPaksuus.ToString());
            sBuilder.AppendLine(asetukset.Rasitusluokka.ToString());
            sBuilder.AppendLine(asetukset.RasitusluokkaIndx.ToString());
            sBuilder.AppendLine(asetukset.Suojabetoni.ToString());
            sBuilder.AppendLine(asetukset.TeraksenKoko.ToString());
            sBuilder.AppendLine(asetukset.TeraksenLujuus.ToString());

            File.WriteAllText(hakemisto, sBuilder.ToString());
        }

        public void KirjoitaLokiin(string s, bool lisaa = true) {
            if (!Directory.Exists(hakemisto + @"\Tiedostot")) {
                Directory.CreateDirectory(hakemisto + @"\Tiedostot\");
            }
            if (lisaa) {
                File.AppendAllText(hakemisto + @"\Tiedostot\loki.txt", s + "\n");
            } else {
                File.WriteAllText(hakemisto + @"\Tiedostot\loki.txt", s + "\n");
            }
        }
    }
}
