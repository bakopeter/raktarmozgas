﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raktarmozgas
{
    internal class Partner : KeszletMozgas //Egy ügyfelet reprezentáló osztály
    {
        int id = 0;
        string nev = "";
        MozgasTipus tipus = 0;
        float mennyiseg = 0;
        double ertek = 0;

        public string Nev { get => nev; }
        public float Mennyiseg { get => mennyiseg; }
        public double Ertek { get => ertek; }

        public static List<Partner> partnerek = new(); //Az ügyfeleket reprezentáló objektumok listája

        static Partner maxertek;
        public static Partner Maxertek
        {
            get => maxertek;
            set => maxertek = value;
        }
        static Partner maxmenny;
        public static Partner Maxmenny
        {
            get => maxmenny;
            set => maxmenny = value;
        }
        public Partner() { }

        /*A konstruktor metódus overloadjában létrehoz két Partner objektumot, melyek az adatok betöltése után visszaadják a legnagyobb
         beszállítók adatait. (Az ügyfelek listájába nem töltődnek be!)*/
        Partner(List<Partner> partnerek, string arg = "mennyiseg")
        {
            Partner max = (arg == "ertek") ? partnerek.MaxBy(e => e.ertek) : partnerek.MaxBy(m => m.mennyiseg);
            id = max.id;
            nev = max.nev;
            MozgasTipus tipus = max.tipus;
            mennyiseg = max.mennyiseg;
            ertek = max.ertek;
        }

        /*Létrehozza az üzleti partnereket reprezentáló objektumot.*/
        static Partner CreatePartner(List<KeszletMozgas> mozgas, int index, int id)
        {
            Partner partner = new()
            {
                id = id + 1,
                nev = mozgas[index].Partner,
                tipus = mozgas[index].Tipus,
                mennyiseg = mozgas[index].Mennyiseg,
                ertek = mozgas[index].EgysegAr * mozgas[index].Mennyiseg
            };
            return partner;
        }

        /*Ha ugyanazon partnerhez kapcsolódó eseményt talál, összegzi a partnerhez kötődő árumogás mennyiségét és értékét, majd
         frissíti a partnert reprezentáló objektumot az aktuális értékekkel.*/
        static Partner UpdatePartner(List<KeszletMozgas> mozgas, int index, int id)
        {
            Partner partner = new()
            {
                id = id,
                nev = mozgas[index].Partner,
                tipus = mozgas[index].Tipus,
                mennyiseg = partnerek[id].mennyiseg + mozgas[index].Mennyiseg,
                ertek = partnerek[id].ertek + mozgas[index].EgysegAr * mozgas[index].Mennyiseg
            };
            return partner;
        }

        /*Partnerek szerint csoportosítja a készletmozgást, összegzi az egy üzleti partnerre vetített áruk mennyiségét és értékét.
         Ezután a konstruktor overload metódusának meghívásával kiválasztja a legnagyobb mennyiségben és értékben szállító partnereket.*/
        public static List<Partner> CreatePartnerList(List<KeszletMozgas> mozgas)
        {
            partnerek.Add(CreatePartner(mozgas, 0, 0));

            for (int i = 1; i < mozgas.Count; i++)
            {
                int j = 0;
                while (j < partnerek.Count && partnerek[j].nev != mozgas[i].Partner) j++;
                if (j == partnerek.Count) { partnerek.Add(CreatePartner(mozgas, i, j)); }
                else { partnerek.Add(UpdatePartner(mozgas, i, j)); partnerek.RemoveAt(j); }
            }

            maxertek = new Partner(partnerek, "ertek");
            maxmenny = new Partner(partnerek, "mennyiseg");

            return partnerek;
        }

        /*Kiszámolja az össz napi termékforgalom mennyiségét.*/
        public static float[] SumTradeFlow(List<Partner> partnerek)
        {
            float[] osszmenny = new float[mozgasFajta];

            foreach (var item in partnerek)
            {
                osszmenny[(int)item.tipus] += item.mennyiseg;
            }
            return osszmenny;
        }
        /*Kiszámolja az össz napi pénzforgalom értékét.*/
        public static double[] SumCashFlow(List<Partner> partnerek)
        {
            double[] osszertek = new double[mozgasFajta];

            foreach (var item in partnerek)
            {
                osszertek[(int)item.tipus] += item.ertek;
            }
            return osszertek;
        }

        /*Kiírja a legnagyobb értékben és mennyiségben szállító partner nevét. (A komment zárójelek eltávolításával, és a Partner struktura 
         * konstruktorának kikommentelésével itt is meg lehet valósítani a max/min kiválasztás műveletét.)*/
        public static void DisplayMaxTransport(List<Partner> partnerek, Partner Maxmenny, Partner Maxertek)
        {
            //Partner.partnerek = Partner.CreatePartnerList(mozgas);

            //Partner maxM = Partner.partnerek.MaxBy(m => m.mennyiseg);
            //var maxE = Partner.partnerek.MaxBy(m => m.ertek);

            Console.WriteLine($"\tLegnagyobb mennyiség:\t{Maxmenny.Nev}\t{Maxmenny.Mennyiseg} kg\t{Maxmenny.Ertek} Ft.");
            Console.WriteLine($"\tLegnagyobb érték:\t{Maxertek.Nev}\t{Maxertek.Mennyiseg} kg\t{Maxertek.Ertek} Ft.");
        }
    }
}
