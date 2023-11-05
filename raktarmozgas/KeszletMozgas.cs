using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raktarmozgas
{
    internal class KeszletMozgas //Egy készletmozgást reprezentáló osztály
    {
        byte id;
        byte ora;
        byte perc;
        string termek = "";
        float mennyiseg;
        double egysegAr;
        MozgasTipus tipus;
        string partner = "";

        public float Mennyiseg { get => mennyiseg; }
        public double EgysegAr { get => egysegAr; }
        public MozgasTipus Tipus { get => tipus; }
        public string Partner { get => partner; }

        public static List<KeszletMozgas> mozgasok = new(); //A készletmozgásokat reprezentáló objektumok listája

        public static int termekFajta = 10; //Termékféleségek aktuális darabszáma
        public static int mozgasFajta = 4; //Készletmozgás lehetséges típusainak száma

        /*Feldarabolja a beolvasott sorokat a megadott elválasztó jel mentén, az értékeket az objektum változóiba tölti.*/
        public static KeszletMozgas CreateKeszletMozgas(string input)
        {
            string[] data = input.Split(";");
            string[] ido = data[1].Split(":");

            KeszletMozgas keszletMozgas = new()
            {
                id = byte.Parse(data[0]),
                ora = byte.Parse(ido[0]),
                perc = byte.Parse(ido[1]),
                termek = data[2],
                mennyiseg = float.Parse(data[3]),
                egysegAr = double.Parse(data[4]),
                tipus = (MozgasTipus)Enum.Parse(typeof(MozgasTipus), data[5]),
                partner = data[6]
            };

            return keszletMozgas;
        }

        /*Létrehoz egy tömböt a különböző féle termékek számára.*/
        public static string[] GetProducts(List<KeszletMozgas> mozgas)
        {
            int i = 0;
            string[] termekek = new string[termekFajta];

            foreach (var item in mozgas)
            {
                if (!termekek.Contains(item.termek))
                {
                    termekek[i] = item.termek;
                    i++;
                }
            }
            return termekek;
        }

        /*Létrehoz egy kétdimenziós tömböt a beszállított és eladott termékek mennyiségei számára (2. dimenzió a mozgástípus értéke).*/
        public static float[,] GetProductAmounts(List<KeszletMozgas> mozgas)
        {
            string[] termekek = GetProducts(mozgas);
            float[,] mennyisegek = new float[termekFajta, mozgasFajta];

            foreach (var item
                in mozgas)
            {
                int j = 0;
                while (j < termekek.Length && !(item.termek == termekek[j])) j++;
                mennyisegek[j, (int)item.tipus] += item.mennyiseg;
            }
            return mennyisegek;
        }

        /*Létrehoz egy kétdimenziós tömböt a termékek beszerzési és eladási ára számára (2. dimenzió a mozgástípus értéke).*/
        public static double[,] GetProductPrices(List<KeszletMozgas> mozgas)
        {
            string[] termekek = GetProducts(mozgas);
            double[,] termekArak = new double[termekFajta, mozgasFajta];

            foreach (var item in mozgas)
            {
                int j = 0;
                while (j < termekek.Length && !(item.termek == termekek[j])) j++;
                termekArak[j, (int)item.tipus] = item.egysegAr;
            }
            return termekArak;
        }

        /*Létrehoz egy tömböt az egyes termékek beszállítói számára (2. dimenzió a mozgástípus értéke)*/
        public static string[,] GetProductShippers(List<KeszletMozgas> mozgas)
        {
            string[] termekek = GetProducts(mozgas);
            string[,] beszallitok = new string[termekFajta, mozgasFajta];

            foreach (var item in mozgas)
            {
                int j = 0;
                while (j < termekek.Length && !(item.termek == termekek[j])) j++;
                beszallitok[j, (int)item.tipus] = item.Partner;
            }
            return beszallitok;
        }

        /*Óránként összegzi a ki- és bemenő forgalmakat.*/
        public static int[,] SalesPerHour(List<KeszletMozgas> mozgas)
        {
            int[,] forgalmak = new int[24, mozgasFajta];

            foreach (var item in mozgas)
            {
                forgalmak[item.ora, (int)item.tipus]++;
            }
            return forgalmak;
        }

        /*Kiszámolja mely termékek készlete esett 50% alá, és visszaadja azon termékeket, melyekből rendelést kell leadni.*/
        public static string[] OutOfStock(List<KeszletMozgas> mozgas)
        {
            string[] termekek = GetProducts(mozgas);
            float[,] mennyisegek = GetProductAmounts(mozgas);
            string[] kifogyoTermekek = new string[termekFajta];
            int j = 0;

            for (int i = 0; i < termekek.Length; i++)
            {
                if (mennyisegek[i, (int)MozgasTipus.ELADAS] > mennyisegek[i, (int)MozgasTipus.BESZERZES] / 2)
                {
                    kifogyoTermekek[j] = termekek[i];
                    j++;
                }
            }
            return kifogyoTermekek;
        }
    }
}
