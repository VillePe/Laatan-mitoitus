using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Drawing = Tekla.Structures.Drawing;
using System.Collections;

namespace Laatan_mitoitus {
    class Mallintaja {

        private const double SEINAN_PAKSUUS = 100;

        Model malli;
        double laatanLeveys, laatanKorkeus;
        Point[] seinatAlku = new Point[12];
        Point[] seinatLoppu = new Point[12];
        Dictionary<int, Beam> seinat = new Dictionary<int, Beam>();
        ArrayList idNumerot = new ArrayList();
        Kentta[] kentat;
        Dictionary<int, string> betonimateriaalit;

        public Mallintaja(Kentta[] kentat) {
            malli = new Model();
            this.kentat = kentat;
            if (!malli.GetConnectionStatus()) {
                throw new ModelConnectionException("Malliin ei saada yhteyttä");
            }
            AlustaBetonimateriaalit();
        }

        public void TyhjennaMalli() {
            ModelObjectEnumerator objEnum = malli.GetModelObjectSelector().GetAllObjects();
            for (int i = 0; i < objEnum.GetSize(); i++) {
                if (objEnum.Current is ContourPlate) {
                    ((ContourPlate)objEnum.Current).Delete();
                } else if (objEnum.Current is Beam) {
                    ((Beam)objEnum.Current).Delete();
                }
                objEnum.MoveNext();
            }
            seinat.Clear();
            idNumerot.Clear();
            malli.CommitChanges();
        }

        public bool TarkistaYhteys() {
            return malli.GetConnectionStatus();
        }

        public Mallintaja(double laatanLeveys, double laatanKorkeus, Kentta[] kentat) : this(kentat) {
            this.laatanLeveys = laatanLeveys;
            this.laatanKorkeus = laatanKorkeus;
        }

        public double LaatanLeveys {
            set {
                laatanLeveys = value;
            }
            get {
                return laatanLeveys;
            }
        }
        public double LaatanKorkeus {
            set {
                laatanKorkeus = value;
            }
            get {
                return laatanKorkeus;
            }
        }
        public Asetukset Asetukset { get; set; }

        /// <summary>
        /// Mallintaa laatan ja seinät
        /// </summary>
        public void Mallinna() {
            AlustaSeinapisteet();
            MallinnaLaatta();
            MallinnaSeinat();
        }

        public void MallinnaRaudoitukset() {
            ContourPlate laatta = (ContourPlate)HaeLaattaMallista();
            if (laatta == null) {
                System.Windows.Forms.MessageBox.Show("Laatan raudoitus epäonnistui");
                return;
            }

            PoistaRaudoitus(laatta);
            MallinnaKenttaraudoitusVerkot(laatta);
            try {
                MallinnaTukiraudoitukset(laatta);
            } catch (Exception ex) {
                Tiedostonkasittelija tKasittelija = new Tiedostonkasittelija(Paaikkuna.HAKEMISTO);
                tKasittelija.KirjoitaLokiin(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void MallinnaKenttaraudoitusVerkot(ContourPlate laatta) {
            for (int i = 0; i < kentat.Length; i++) {
                Polygon polygon = new Polygon();
                Point p1 = kentat[i].VasenAlanurkkaTekla;
                Point p2 = new Point(kentat[i].VasenAlanurkkaTekla.X, kentat[i].VasenAlanurkkaTekla.Y + kentat[i].Korkeus * 1000, 3000);
                Point p3 = new Point(kentat[i].VasenAlanurkkaTekla.X + kentat[i].Leveys * 1000, kentat[i].VasenAlanurkkaTekla.Y + kentat[i].Korkeus * 1000, 3000);
                Point p4 = new Point(kentat[i].VasenAlanurkkaTekla.X + kentat[i].Leveys * 1000, kentat[i].VasenAlanurkkaTekla.Y, 3000);
                polygon.Points.Add(p1);
                polygon.Points.Add(p2);
                polygon.Points.Add(p3);
                polygon.Points.Add(p4);

                Verkko raudoitus = kentat[i].RaudoitusYleensa;
                if (raudoitus.Tyyppi == "Ei sopivaa") {
                    continue;
                }
                double silmavali = raudoitus.SilmaKokoVaaka;
                string verkonKoko = raudoitus.RaudanKoko.ToString();

                RebarMesh rMesh = new RebarMesh();
                rMesh.StartPoint = p1;
                rMesh.EndPoint = p2;
                rMesh.CatalogName = "Vähimmäisraudoitus";
                rMesh.LongitudinalSize = verkonKoko;
                rMesh.CrossSize = verkonKoko;
                rMesh.LeftOverhangCross = 100.0;
                rMesh.LeftOverhangLongitudinal = 100.0;
                rMesh.RightOverhangCross = 100.0;
                rMesh.RightOverhangLongitudinal = 100.0;
                rMesh.LongitudinalSpacingMethod = RebarMesh.RebarMeshSpacingMethodEnum.SPACING_TYPE_SAME_DISTANCE;
                rMesh.LongitudinalDistances.Add(silmavali);
                rMesh.CrossDistances.Add(silmavali);
                rMesh.Class = 7;
                rMesh.NumberingSeries.StartNumber = 0;
                rMesh.NumberingSeries.Prefix = "Mesh";
                rMesh.Grade = raudoitus.Tyyppi;
                rMesh.OnPlaneOffsets.Add(raudoitus.SuojaBetoni);
                rMesh.FromPlaneOffset = -raudoitus.SuojaBetoni;
                rMesh.StartPointOffsetType = Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_COVER_THICKNESS;
                rMesh.StartPointOffsetValue = raudoitus.SuojaBetoni;
                rMesh.StartFromPlaneOffset = raudoitus.SuojaBetoni;

                rMesh.Name = "Kentta " + kentat[i].Numero;
                rMesh.MeshType = RebarMesh.RebarMeshTypeEnum.POLYGON_MESH;
                rMesh.Polygon = polygon;

                rMesh.CrossBarLocation = RebarMesh.RebarMeshCrossBarLocationEnum.LOCATION_TYPE_ABOVE;

                rMesh.EndFromPlaneOffset = raudoitus.SuojaBetoni;
                rMesh.CutByFatherPartCuts = true;
                rMesh.Father = laatta;
                rMesh.Insert();
                malli.CommitChanges();
                RaudoitaKeskelle(kentat[i].RaudoitusKeskella, laatta, new Point[] { p1, p2, p3, p4 }, kentat[i]);
            }
        }

        private void MallinnaTukiraudoitukset(ContourPlate laatta) {
            for (int i = 0; i < kentat.Length; i++) {
                foreach (string suunta in kentat[i].Tukiraudat.Keys) {
                    if (Skippaa(kentat[i], suunta)) {
                        continue;
                    }
                    RebarGroup rGroup = new RebarGroup();
                    rGroup.Class = 10;
                    rGroup.Name = "Tukiraudoitus " + kentat[i].Numero + " " + suunta;
                    rGroup.Size = kentat[i].Tukiraudat[suunta].Koko.ToString();
                    rGroup.Grade = kentat[i].Tukiraudat[suunta].Tyyppi;
                    rGroup.RadiusValues.Add(30.0);
                    rGroup.OnPlaneOffsets.Add(Asetukset.ArvioituMittaYlapinnasta);
                    rGroup.FromPlaneOffset = (Asetukset.LaatanPaksuus - Asetukset.ArvioituMittaYlapinnasta);
                    rGroup.StartPointOffsetValue = -1 * kentat[i].Tukiraudat[suunta].TankojenPituus;
                    rGroup.EndPointOffsetValue = -1 * kentat[i].Tukiraudat[suunta].TankojenPituus;
                    rGroup.StartPointOffsetType = Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_COVER_THICKNESS;
                    rGroup.EndPointOffsetType = Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_COVER_THICKNESS;
                    rGroup.SpacingType = BaseRebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_EXACT_SPACE_FLEX_AT_BOTH;
                    rGroup.Spacings.Add(kentat[i].Tukiraudat[suunta].Jako);
                    rGroup.Father = laatta;
                    rGroup.Polygons.Add(HaeTukiraudoituksenPolygon(kentat[i], suunta, rGroup));
                    rGroup.ExcludeType = BaseRebarGroup.ExcludeTypeEnum.EXCLUDE_TYPE_NONE;
                    rGroup.Insert();
                    malli.CommitChanges();
                }
            }
        }

        /// <summary>
        /// Skipataan tietyt kentät
        /// </summary>
        /// <param name="kentta"></param>
        /// <param name="suunta"></param>
        /// <returns></returns>
        private bool Skippaa(Kentta kentta, string suunta) {
            if (kentta.Numero == 2 || kentta.Numero == 4 || kentta.Numero == 6) {
                return true;
            }
            if (kentta.Numero == 5 && suunta == "alas") {
                return true;
            }
            return false;
        }

        private Polygon HaeTukiraudoituksenPolygon(Kentta kentta, string suunta, RebarGroup rGroup) {
            Polygon polygon = new Polygon();
            switch (suunta) {
                case "vasen":
                    rGroup.StartPoint = new Point(kentta.Tukiraudat[suunta].SijaintiAlku.X, kentta.Tukiraudat[suunta].SijaintiAlku.Y, 3000);
                    rGroup.EndPoint = new Point(kentta.Tukiraudat[suunta].SijaintiLoppu.X, kentta.Tukiraudat[suunta].SijaintiLoppu.Y, 3000);
                    polygon.Points.Add(new Point(kentta.VasenAlanurkka.X - 100, kentta.VasenAlanurkka.Y, 3000));
                    polygon.Points.Add(new Point(kentta.VasenAlanurkka.X + 100, kentta.VasenAlanurkka.Y, 3000));
                    break;
                case "ylos":
                    rGroup.StartPoint = new Point(kentta.Tukiraudat[suunta].SijaintiAlku.X, kentta.Tukiraudat[suunta].SijaintiAlku.Y, 3000);
                    rGroup.EndPoint = new Point(kentta.Tukiraudat[suunta].SijaintiLoppu.X, kentta.Tukiraudat[suunta].SijaintiLoppu.Y, 3000);
                    polygon.Points.Add(new Point(kentta.VasenAlanurkka.X, kentta.VasenAlanurkka.Y + kentta.Korkeus * 1000 + 100, 3000));
                    polygon.Points.Add(new Point(kentta.VasenAlanurkka.X, kentta.VasenAlanurkka.Y + kentta.Korkeus * 1000 - 100, 3000));
                    break;
                case "oikea":
                    rGroup.EndPoint = new Point(kentta.Tukiraudat[suunta].SijaintiAlku.X, kentta.Tukiraudat[suunta].SijaintiAlku.Y, 3000);
                    rGroup.StartPoint = new Point(kentta.Tukiraudat[suunta].SijaintiLoppu.X, kentta.Tukiraudat[suunta].SijaintiLoppu.Y, 3000);
                    polygon.Points.Add(new Point(kentta.VasenAlanurkka.X + kentta.Leveys * 1000 - 100, kentta.VasenAlanurkka.Y, 3000));
                    polygon.Points.Add(new Point(kentta.VasenAlanurkka.X + kentta.Leveys * 1000 + 100, kentta.VasenAlanurkka.Y, 3000));
                    break;
                case "alas":
                    rGroup.StartPoint = new Point(kentta.Tukiraudat[suunta].SijaintiAlku.X, kentta.Tukiraudat[suunta].SijaintiAlku.Y, 3000);
                    rGroup.EndPoint = new Point(kentta.Tukiraudat[suunta].SijaintiLoppu.X, kentta.Tukiraudat[suunta].SijaintiLoppu.Y, 3000);
                    polygon.Points.Add(new Point(kentta.VasenAlanurkka.X, kentta.VasenAlanurkka.Y + 100, 3000));
                    polygon.Points.Add(new Point(kentta.VasenAlanurkka.X, kentta.VasenAlanurkka.Y - 100, 3000));
                    break;
            }
            return polygon;
        }

        private void PoistaRaudoitus(ModelObject laatta) {
            ModelObjectEnumerator rautaEnum = malli.GetModelObjectSelector().GetAllObjects();
            for (int j = 0; j < rautaEnum.GetSize(); j++) {
                if (rautaEnum.Current != null && rautaEnum.Current is RebarMesh) {
                    ((RebarMesh)rautaEnum.Current).Delete();
                }
                rautaEnum.MoveNext();
            }
        }

        private void RaudoitaKeskelle(Verkko verkko, ModelObject father, Point[] p, Kentta kentta) {
            if (verkko.Tyyppi == "Ei verkkoa") {
                return;
            }
            double kenttaX = Math.Min(kentta.Leveys, kentta.Korkeus);
            double kenttaNeljasosa = kenttaX / 4 * 1000;
            Polygon polygon = new Polygon();
            Point p1 = new Point(p[0].X + kenttaNeljasosa + 10, p[0].Y + kenttaNeljasosa, 3000);
            Point p2 = new Point(p[1].X + kenttaNeljasosa + 10, p[1].Y - kenttaNeljasosa, 3000);
            Point p3 = new Point(p[2].X - kenttaNeljasosa + 10, p[2].Y - kenttaNeljasosa, 3000);
            Point p4 = new Point(p[3].X - kenttaNeljasosa + 10, p[3].Y + kenttaNeljasosa, 3000);
            polygon.Points.Add(p1);
            polygon.Points.Add(p2);
            polygon.Points.Add(p3);
            polygon.Points.Add(p4);

            RebarMesh rMesh = new RebarMesh();
            rMesh.StartPoint = p1;
            rMesh.EndPoint = p2;
            rMesh.CatalogName = "Lisäverkko";
            rMesh.LongitudinalSize = verkko.RaudanKoko.ToString();
            rMesh.CrossSize = verkko.RaudanKoko.ToString();
            rMesh.LeftOverhangCross = 100.0;
            rMesh.LeftOverhangLongitudinal = 100.0;
            rMesh.RightOverhangCross = 100.0;
            rMesh.RightOverhangLongitudinal = 100.0;
            rMesh.LongitudinalSpacingMethod = RebarMesh.RebarMeshSpacingMethodEnum.SPACING_TYPE_SAME_DISTANCE;
            rMesh.LongitudinalDistances.Add(verkko.SilmaKokoVaaka);
            rMesh.CrossDistances.Add(verkko.SilmaKokoPysty);
            rMesh.Class = 7;
            rMesh.NumberingSeries.StartNumber = 0;
            rMesh.NumberingSeries.Prefix = "Mesh";
            rMesh.Grade = verkko.Tyyppi;
            rMesh.OnPlaneOffsets.Add(verkko.SuojaBetoni);
            rMesh.FromPlaneOffset = -verkko.SuojaBetoni;
            rMesh.StartPointOffsetType = Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_COVER_THICKNESS;
            rMesh.StartPointOffsetValue = verkko.SuojaBetoni;
            rMesh.StartFromPlaneOffset = verkko.SuojaBetoni;

            rMesh.Name = "Kentta " + kentta.Numero + " lisäraudoitus";
            rMesh.MeshType = RebarMesh.RebarMeshTypeEnum.POLYGON_MESH;
            rMesh.Polygon = polygon;

            rMesh.CrossBarLocation = RebarMesh.RebarMeshCrossBarLocationEnum.LOCATION_TYPE_ABOVE;

            rMesh.EndFromPlaneOffset = verkko.SuojaBetoni;
            rMesh.CutByFatherPartCuts = true;
            rMesh.Father = father;
            rMesh.Insert();
            malli.CommitChanges();
        }

        private void AlustaSeinapisteet() {
            // Vasen pystysuuntainen ulkoseinä
            seinatAlku[0] = new Point(0 + SEINAN_PAKSUUS / 2, 0 + SEINAN_PAKSUUS);
            seinatLoppu[0] = new Point(0 + SEINAN_PAKSUUS / 2, laatanKorkeus - SEINAN_PAKSUUS);
            // Ylempi vaakasuuntainen ulkoseinä
            seinatAlku[1] = new Point(0, laatanKorkeus - SEINAN_PAKSUUS / 2);
            seinatLoppu[1] = new Point(laatanLeveys, laatanKorkeus - SEINAN_PAKSUUS / 2);
            // Oikea pystysuuntainen ulkoseinä
            seinatAlku[2] = new Point(laatanLeveys - SEINAN_PAKSUUS / 2, 0 + SEINAN_PAKSUUS);
            seinatLoppu[2] = new Point(laatanLeveys - SEINAN_PAKSUUS / 2, laatanKorkeus - SEINAN_PAKSUUS);
            // Alempi vaakasuuntainen ulkoseinä
            seinatAlku[3] = new Point(0, 0 + SEINAN_PAKSUUS / 2);
            seinatLoppu[3] = new Point(laatanLeveys, 0 + SEINAN_PAKSUUS / 2);
            //
            //SISÄSEINÄT
            //
            // Keskimmäinen vaakasuuntainen
            seinatAlku[4] = new Point(0 + SEINAN_PAKSUUS, kentat[3].Korkeus * 1000);
            seinatLoppu[4] = new Point(laatanLeveys - SEINAN_PAKSUUS, kentat[3].Korkeus * 1000);
            // Alempi vaakasuuntainen
            seinatAlku[5] = new Point(kentat[3].Leveys * 1000 + SEINAN_PAKSUUS / 2, kentat[6].Korkeus * 1000);
            seinatLoppu[5] = new Point(kentat[3].Leveys * 1000 + kentat[4].Leveys * 1000 - SEINAN_PAKSUUS / 2, kentat[6].Korkeus * 1000);
            // Vasen pystysuuntainen
            seinatAlku[6] = new Point(kentat[3].Leveys * 1000, 0 + SEINAN_PAKSUUS);
            seinatLoppu[6] = new Point(kentat[3].Leveys * 1000, laatanKorkeus - SEINAN_PAKSUUS);
            // Oikea pystysuuntainen
            seinatAlku[7] = new Point(kentat[3].Leveys * 1000 + kentat[4].Leveys * 1000, 0 + SEINAN_PAKSUUS);
            seinatLoppu[7] = new Point(kentat[3].Leveys * 1000 + kentat[4].Leveys * 1000, laatanKorkeus - SEINAN_PAKSUUS);
            // Keskimmäinen pystysuuntainen
            seinatAlku[8] = new Point(kentat[3].Leveys * 1000 + kentat[4].Leveys * 1000 - kentat[6].Leveys * 1000, 0 + SEINAN_PAKSUUS);
            seinatLoppu[8] = new Point(kentat[3].Leveys * 1000 + kentat[4].Leveys * 1000 - kentat[6].Leveys * 1000, kentat[6].Korkeus * 1000 - SEINAN_PAKSUUS / 2);
        }

        public void PaivitaPiirrost() {

        }

        public void LuoPiirros() {
            if (laatanLeveys == 0 || laatanKorkeus == 0) {
                throw new NullReferenceException("Laske tasaukset!");
            }
            Drawing.DrawingHandler dHandler = new Drawing.DrawingHandler();
            Drawing.Drawing drawing = new Drawing.GADrawing("standard");

            CoordinateSystem cSystem = new CoordinateSystem(new Point(), new Vector(1, 0, 0), new Vector(0, 1, 0));
            Drawing.View view = new Drawing.View(drawing.GetSheet(), cSystem, cSystem, new AABB(new Point(-500, -500), new Point(laatanLeveys, laatanKorkeus, 3300)));
            if (!view.Attributes.LoadAttributes(Paaikkuna.HAKEMISTO + @"\Tiedostot\kantavat_laatat.vi")) {
                view.Attributes.LoadAttributes("standard");
            }
            view.Attributes.Scale = 50.0;
            view.Attributes.Shortening.CutParts = false;
            drawing.PlaceViews();
            drawing.Insert();
            dHandler.SetActiveDrawing(drawing);
            view.Insert();
        }

        private void MallinnaSeinat() {
            for (int i = 0; i < 9; i++) {
                if (idNumerot.Count > i && idNumerot[i] != null) {
                    seinat[(int)idNumerot[i]].StartPoint = seinatAlku[i];
                    seinat[(int)idNumerot[i]].EndPoint = seinatLoppu[i];
                    seinat[(int)idNumerot[i]].Modify();

                } else {
                    Beam seina = new Beam();
                    seina.Name = "SEINÄ";
                    seina.Profile.ProfileString = "3000*" + SEINAN_PAKSUUS;
                    seina.Material.MaterialString = "Concrete_Undefined";
                    seina.Position.Depth = Position.DepthEnum.FRONT;
                    seina.Name = i + " SEINÄ";
                    seina.StartPoint = seinatAlku[i];
                    seina.EndPoint = seinatLoppu[i];
                    seina.Insert();
                    seinat.Add(seina.Identifier.ID, seina);
                    idNumerot.Add(seina.Identifier.ID);
                }
            }
            malli.CommitChanges();
        }

        private void MallinnaLaatta() {
            ModelObjectSelector mSelector = malli.GetModelObjectSelector();
            ModelObjectEnumerator enumer = mSelector.GetAllObjects();
            for (int i = 0; i < enumer.GetSize(); i++) {
                ModelObject obj = enumer.Current;
                if (obj is ContourPlate) {
                    if (((ContourPlate)obj).Name == "LASKETTU_LAATTA") {
                        ContourPlate laattaTemp = (ContourPlate)obj;
                        laattaTemp.Contour.ContourPoints[0] = new ContourPoint(new Point(0, 0, 3000), null);
                        laattaTemp.Contour.ContourPoints[1] = new ContourPoint(new Point(0, laatanKorkeus, 3000), null);
                        laattaTemp.Contour.ContourPoints[2] = new ContourPoint(new Point(laatanLeveys, laatanKorkeus, 3000), null);
                        laattaTemp.Contour.ContourPoints[3] = new ContourPoint(new Point(laatanLeveys, 0, 3000), null);
                        PoistaRaudoitus(laattaTemp);
                        laattaTemp.Modify();
                        malli.CommitChanges();
                        return;
                    }
                }
                enumer.MoveNext();
            }
            ContourPlate laatta = new ContourPlate();

            laatta.Name = "LASKETTU_LAATTA";
            ContourPoint vasenAlanurkka = new ContourPoint(new Point(0, 0, 3000), null);
            ContourPoint vasenYlanurkka = new ContourPoint(new Point(0, laatanKorkeus, 3000), null);
            ContourPoint oikeaYlanurkka = new ContourPoint(new Point(laatanLeveys, laatanKorkeus, 3000), null);
            ContourPoint oikeaAlanurkka = new ContourPoint(new Point(laatanLeveys, 0, 3000), null);

            laatta.AddContourPoint(vasenAlanurkka);
            laatta.AddContourPoint(vasenYlanurkka);
            laatta.AddContourPoint(oikeaYlanurkka);
            laatta.AddContourPoint(oikeaAlanurkka);

            laatta.Profile.ProfileString = Asetukset.LaatanPaksuus.ToString();
            if (!string.IsNullOrWhiteSpace(Asetukset.BetoninLujuus.ToString())) {
                laatta.Material.MaterialString = betonimateriaalit[Asetukset.BetoninLujuus];
            } else {
                laatta.Material.MaterialString = "Concrete_Undefined";
            }
            laatta.Position.Depth = Position.DepthEnum.FRONT;

            laatta.Insert();
            malli.CommitChanges();

        }

        private ModelObject HaeLaattaMallista() {
            ModelObjectEnumerator objEnum = malli.GetModelObjectSelector().GetAllObjects();
            ModelObject palautus = null;
            for (int i = 0; i < objEnum.GetSize(); i++) {
                if (objEnum.Current is ContourPlate && ((ContourPlate)objEnum.Current).Name == "LASKETTU_LAATTA") {
                    palautus = (ModelObject)objEnum.Current;
                    break;
                }
                objEnum.MoveNext();
            }
            return palautus;
        }

        private bool OnkoSeinaJoMallinnettu(Point p1, Point p2) {
            ModelObjectEnumerator objEnum = malli.GetModelObjectSelector().GetAllObjects();
            for (int i = 0; i < objEnum.GetSize(); i++) {
                if (objEnum.Current is Beam) {
                    Beam seinaTemp = (Beam)objEnum.Current;
                    if (seinaTemp.StartPoint == p1 && seinaTemp.EndPoint == p2) {
                        return true;
                    }
                }
                objEnum.MoveNext();
            }
            return false;
        }

        public void LuetteleObjektit() {
            ModelObjectEnumerator objEnum = malli.GetModelObjectSelector().GetAllObjects();
            string objektit = "";
            for (int i = 0; i < objEnum.GetSize(); i++) {
                if (objEnum.Current != null) {
                    objektit += objEnum.Current.ToString() + "\n";
                }
                objEnum.MoveNext();
            }
            System.Windows.Forms.MessageBox.Show(objektit);
        }

        public class ModelConnectionException : Exception {

            private string message = "Could not connect to Tekla Structures Model!";

            public ModelConnectionException() {

            }

            public ModelConnectionException(string s) {
                message = s;
            }

            public override string Message {
                get {
                    return message;
                }
            }
        }

        private void AlustaBetonimateriaalit() {
            betonimateriaalit = new Dictionary<int, string>();
            betonimateriaalit.Add(12, "C12/15");
            betonimateriaalit.Add(16, "C16/20");
            betonimateriaalit.Add(20, "C20/25");
            betonimateriaalit.Add(25, "C25/30");
            betonimateriaalit.Add(30, "C30/37");
            betonimateriaalit.Add(32, "C32/40");
            betonimateriaalit.Add(35, "C35/45");
            betonimateriaalit.Add(40, "C40/50");
            betonimateriaalit.Add(45, "C45/55");

        }
    }

}
